import json
import os
from pathlib import Path
from tempfile import TemporaryDirectory
import subprocess
import unittest
from unittest.mock import Mock, patch

from ms_baley.data.audit_service import AuditService, findings_for, private_file, ReportClient
from ms_baley.data.local_runtime import LocalRuntime
from ms_baley.data.remote_probe import access_summary
from ms_baley.data.process_io import bounded_run
from ms_baley.data.ai_review import AIReview
from ms_baley.features.audit.commands.run_audit import RunAuditHandler

ROOT = Path(__file__).resolve().parents[1]


class AuditTests(unittest.TestCase):
  def setUp(self):
    self.temp = TemporaryDirectory(); self.addCleanup(self.temp.cleanup)
    self.base = Path(self.temp.name)
    settings = json.loads((ROOT/'setting.json').read_text())
    settings['ai']['enabled'] = False
    settings['skill']['path'] = 'SKILL.md'
    settings['ssh']['privateKeyPath'] = 'key.pem'
    settings['ssh']['knownHostsPath'] = str(self.base/'known_hosts')
    (self.base/'SKILL.md').write_text('Auditoria somente leitura.')
    (self.base/'key.pem').write_text('fixture-not-a-private-key'); (self.base/'key.pem').chmod(0o600)
    (self.base/'known_hosts').write_text('fixture')
    (self.base/'setting.json').write_text(json.dumps(settings))
    self.runtime = LocalRuntime(self.base/'setting.json')

  def evidence(self):
    return {'host': {}, 'containers': [], 'access': {'events': 2}, 'coverage': {'docker': 'collected'}}

  def test_access_log_never_retains_query_headers_or_arbitrary_paths(self):
    raw = json.dumps({'RequestMethod':'GET','DownstreamStatus':401,'ClientHost':'1.2.3.4',
                      'RequestPath':'/.env?password=SECRET','Authorization':'Bearer SECRET'})
    data = access_summary(raw+'\nmalformed')
    self.assertEqual(1, data['suspiciousPatterns']['known-scanner-path'])
    self.assertEqual([('1.2.3.4',1)], data['scannerSources'])
    self.assertNotIn('SECRET', json.dumps(data)); self.assertNotIn('.env', json.dumps(data))

  def test_partial_and_failed_collection_never_claim_safe(self):
    service = AuditService(self.runtime)
    service.collect = Mock(side_effect=ValueError('SECRET'))
    report = service.audit()
    self.assertEqual('failed', report['executionStatus']); self.assertNotIn('SECRET', json.dumps(report))
    service.collect = Mock(return_value={'coverage':{'docker':'unavailable'}})
    self.assertEqual('partial', service.audit()['executionStatus'])

  def test_privileged_socket_port_and_logs_are_classified_without_mutation(self):
    findings = findings_for({'containers':[{'name':'test','privileged':True,'mounts':[{'dockerSocket':True}],
                'ports':{'5432/tcp':[{'HostIp':'0.0.0.0'}]},'log':{'type':'json-file'}}]})
    self.assertEqual({'docker.privileged','docker.socket','docker.published-service','docker.logs'}, {row['ruleId'] for row in findings})

  def test_ssh_uses_verified_host_no_shell_and_bounded_program(self):
    runner = Mock(return_value=subprocess.CompletedProcess([],0,json.dumps(self.evidence())))
    AuditService(self.runtime, runner=runner).collect()
    args = runner.call_args.args[0]
    self.assertIn('StrictHostKeyChecking=yes',args); self.assertIn('BatchMode=yes',args)
    self.assertNotIn('shell', runner.call_args.kwargs)
    source = runner.call_args.kwargs['input']
    self.assertNotIn('.Config.Env',source); self.assertNotIn('docker", "exec',source)
    self.runtime.settings['ssh']['host'] = '-oProxyCommand=bad'
    with self.assertRaises(ValueError): AuditService(self.runtime,runner=runner).collect()
    self.assertEqual(1,runner.call_count)

  def test_private_key_rejects_open_permissions_and_symlinks(self):
    key = self.base/'key.pem'; key.chmod(0o644)
    with self.assertRaises(ValueError): private_file(key)
    key.chmod(0o600); link=self.base/'alias';link.symlink_to(key)
    with self.assertRaises(ValueError): private_file(link)

  def test_delivery_reuses_run_id_after_failure_and_only_deletes_confirmed(self):
    client=Mock();client.send.side_effect=ValueError('offline')
    service=AuditService(self.runtime, client=client);service.collect=Mock(return_value=self.evidence())
    first=service.cycle(); self.assertEqual('pending',first['delivery']);self.assertEqual(1, first['pendingCount'])
    pending=json.loads(next((self.runtime.state/'outbox').glob('*.json')).read_text())
    self.assertEqual(first['runId'],pending['runId'])
    client.send.side_effect=None;client.send.return_value={'externalId':'fixture'}
    second=service.cycle()
    self.assertEqual(0,second['pendingCount']);self.assertEqual(2,second['deliveredCount'])
    self.assertIn(first['runId'],[call.args[0]['runId'] for call in client.send.call_args_list])

  def test_retention_removes_expired_reports_and_reports_loss_count(self):
    service=AuditService(self.runtime);service.prune()
    old=self.runtime.state/'outbox/old.json';old.write_text('{}');os.utime(old,(0,0))
    self.assertEqual(1,service.prune());self.assertFalse(old.exists())

  def test_audit_lock_prevents_overlapping_one_shot_and_worker(self):
    from ms_baley.data.local_runtime import exclusive_lock
    service=Mock();self.runtime.prepare()
    with exclusive_lock(self.runtime.state/'audit.lock'):
      with self.assertRaises(BlockingIOError):RunAuditHandler(self.runtime,service).handle()
    service.cycle.assert_not_called()

  def test_report_client_reads_only_baley_section_from_shared_admin_json(self):
    path = self.base/'admin.json'
    path.write_text(json.dumps({'cloud': {'pass': 'never-send'}, 'baley': {'login': 'baley-agent', 'password': 'fixture'}}))
    path.chmod(0o600)
    client = ReportClient({'credentialsPath': 'admin.json', 'credentialsSection': 'baley'}, self.base)
    client.request = Mock(side_effect=[{'accessToken': 'test'}, {'runId': 'run', 'externalId': 'id'}])
    client.send({'runId': 'run'})
    self.assertEqual({'login':'baley-agent','password':'fixture'},client.request.call_args_list[0].args[1])
    self.assertNotIn('never-send',str(client.request.call_args_list))

  def test_http_rejects_destination_change_before_sending_credentials(self):
    client=ReportClient({'baseUrl':'https://evil.example'},self.base)
    with self.assertRaises(ValueError):client.request('/login',{'password':'fixture'})

  def test_process_limits_output_and_runtime(self):
    import sys
    with self.assertRaises(ValueError):bounded_run([sys.executable,'-c','print("x"*10000)'],input='',timeout=2,maximum=100)
    with self.assertRaises(subprocess.TimeoutExpired):bounded_run([sys.executable,'-c','import time;time.sleep(5)'],input='',timeout=.1)

  def test_ai_preserves_read_only_flags_and_structured_output(self):
    output=json.dumps({'type':'item.completed','item':{'type':'agent_message','text':json.dumps({'summary':'Resumo','recommendations':['Revisar socket.']})}})
    runner=Mock(return_value=subprocess.CompletedProcess([],0,output))
    result=AIReview({'enabled':True,'provider':'codex-cli'},runner).analyze({'executionStatus':'completed','hostId':'fixture','result':{}},'Somente leitura')
    self.assertEqual('completed',result['status']);args=runner.call_args.args[0]
    self.assertIn('--ignore-user-config',args);self.assertIn('shell_tool',args);self.assertIn('--ephemeral',args)

  def test_host_rules_distinguish_sensitive_listener_loopback_and_expected_ports(self):
    findings = findings_for({'host': {'listeners': [
      {'protocol':'tcp','address':'0.0.0.0','port':5432},
      {'protocol':'tcp','address':'127.0.0.1','port':6379},
      {'protocol':'tcp','address':'::','port':443},
      {'protocol':'tcp','address':'0.0.0.0','port':8080}],
      'firewallIPv4': {'inputPolicy':'ACCEPT','inputRuleCount':0}, 'imdsTokenlessStatus':200,
      'permissions': {'dockerSocket': {'mode':0o666}, 'authorizedKeys': {'mode':0o622}},
      'kernelControls': {'protected_symlinks':0}}})
    rules = [row['ruleId'] for row in findings]
    for rule in ['host.sensitive-listener','host.unexpected-listener','host.firewall-input','host.imdsv1','host.socket-permissions','host.ssh-permissions','host.kernel-links']:
      self.assertIn(rule,rules)
    self.assertEqual(1,rules.count('host.sensitive-listener'))
    self.assertNotIn('invasão',json.dumps(findings))

  def test_ss_listener_parser_handles_ipv4_and_ipv6_without_process_arguments(self):
    from ms_baley.data.remote_probe import parse_listeners
    rows = parse_listeners('tcp LISTEN 0 128 0.0.0.0:22 0.0.0.0:*\ntcp LISTEN 0 128 [::]:443 [::]:*')
    self.assertEqual([22,443],[row['port'] for row in rows])
    self.assertEqual('::',rows[1]['address'])
