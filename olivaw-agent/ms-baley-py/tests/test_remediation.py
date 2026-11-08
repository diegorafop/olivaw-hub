import copy
import json
import os
from pathlib import Path
from tempfile import TemporaryDirectory
import unittest
from unittest.mock import Mock, patch

from ms_baley.data.local_runtime import LocalRuntime
from ms_baley.data.remediation_service import RemediationService, same_snapshot
from ms_baley.data.remote_remediation import execute

ROOT = Path(__file__).resolve().parents[1]


def healthy():
  return {'healthy':True,'containers':[{'id':'a','name':'app','restarts':0,'startedAt':'fixed'}],'samples':1}


class RemediationTests(unittest.TestCase):
  def setUp(self):
    self.temp=TemporaryDirectory();self.addCleanup(self.temp.cleanup)
    self.base=Path(self.temp.name)
    settings=json.loads((ROOT/'setting.json').read_text())
    (self.base/'setting.json').write_text(json.dumps(settings))
    self.runtime=LocalRuntime(self.base/'setting.json');self.runtime.prepare()
    self.directory=self.runtime.state/'remediations';self.directory.mkdir()
    self.client=Mock();self.client.login.return_value='test'
    self.service=RemediationService(self.runtime,client=self.client,sleeper=lambda _:None)
    self.service.token='test'
    self.task={'externalId':'30000000-0000-0000-0000-000000000001','hostId':settings['audit']['hostId'],'planJson':json.dumps({'Action':'restrict-permissions','Target':'authorizedKeys','ExpectedMode':0o622})}
    self.receipt={'before':{'inode':1,'device':1,'owner':1,'mode':0o622},'appliedMode':0o600}

  def test_unhealthy_or_stopped_container_blocks_mutation_and_cleanup(self):
    self.service.remote=Mock(return_value={'healthy':False,'containers':[{'state':'exited'}]})
    result=self.service.process(self.task,self.directory)
    self.assertEqual('intervention',result['status'])
    self.assertEqual([{'operation':'health'}],[call.args[0] for call in self.service.remote.call_args_list])
    self.assertEqual('intervention',self.client.request.call_args_list[-1].args[1]['action'])

  def test_resolved_requires_two_healthy_samples_unchanged_snapshot_and_absent_finding(self):
    self.service.remote=Mock(side_effect=[healthy(),self.receipt,{'findingAbsent':True},healthy(),healthy(),{'findingAbsent':True}])
    result=self.service.process(self.task,self.directory)
    self.assertEqual('resolved',result['status'])
    calls=[call.args[1]['action'] for call in self.client.request.call_args_list]
    self.assertEqual(['claim','validating','resolved'],calls)
    proof=json.loads(self.client.request.call_args_list[-1].args[1]['resultJson'])
    self.assertEqual(2,proof['healthAfter']['samples'])
    self.assertTrue(proof['findingAbsent']);self.assertFalse(list(self.directory.glob('*.json')))

  def test_health_regression_rolls_back_and_never_claims_resolved(self):
    bad=healthy();bad['healthy']=False
    self.service.remote=Mock(side_effect=[healthy(),self.receipt,{},bad,healthy(),{'findingAbsent':True},{'rolledBack':True}])
    result=self.service.process(self.task,self.directory)
    self.assertEqual('intervention',result['status'])
    self.assertEqual('rollback',self.service.remote.call_args_list[-1].args[0]['operation'])
    payload=json.loads(self.client.request.call_args_list[-1].args[1]['resultJson'])
    self.assertTrue(payload['rollback']['confirmed'])

  def test_receipt_retry_does_not_repeat_mutation_after_network_failure(self):
    self.service.remote=Mock(side_effect=[healthy(),self.receipt,{},healthy(),healthy(),{'findingAbsent':True}])
    self.client.request.side_effect=[{}, {}, OSError('offline')]
    with self.assertRaises(OSError):self.service.process(self.task,self.directory)
    job=json.loads(next(self.directory.glob('*.json')).read_text())
    self.assertEqual('resolved',job['action'])
    self.client.request.side_effect=[{}, {'items':[]}]
    self.service.remote.reset_mock()
    self.assertEqual('idle',self.service.poll()['status'])
    self.service.remote.assert_not_called()
    self.assertFalse(list(self.directory.glob('*.json')))

  def test_interrupted_job_never_replays_and_unknown_plan_or_host_never_executes(self):
    (self.directory/'30000000-0000-0000-0000-000000000001.json').write_text(json.dumps({'externalId':'30000000-0000-0000-0000-000000000001','leaseToken':'token','phase':'changed'}))
    self.client.request.side_effect=[{}, {'items':[]}]
    self.service.remote=Mock()
    self.service.poll();self.service.remote.assert_not_called()
    self.assertEqual('intervention',self.client.request.call_args_list[0].args[1]['action'])
    task=dict(self.task);task['hostId']='other'
    with self.assertRaises(ValueError):self.service.process(task,self.directory)
    task=dict(self.task);task['planJson']=json.dumps({'Action':'docker-system-prune','Target':'all','ExpectedMode':0})
    with self.assertRaises(ValueError):self.service.process(task,self.directory)
    self.service.remote.assert_not_called()

  def test_permissions_are_restricted_and_rollback_preserves_original_without_content_access(self):
    with patch.dict(os.environ,{'HOME':str(self.base)}):
      ssh=self.base/'.ssh';ssh.mkdir(mode=0o700)
      key=ssh/'authorized_keys';key.write_text('public-key-fixture');key.chmod(0o622)
      op={'action':'restrict-permissions','target':'authorizedKeys','expectedMode':0o622}
      receipt=execute({**op,'operation':'prepare'})
      execute({**op,'operation':'apply','receipt':receipt})
      self.assertEqual(0o600,key.stat().st_mode&0o777)
      self.assertTrue(execute({**op,'operation':'verify','receipt':receipt})['findingAbsent'])
      execute({**op,'operation':'rollback','receipt':receipt})
      self.assertEqual(0o622,key.stat().st_mode&0o777)
      self.assertEqual('public-key-fixture',key.read_text())

  def test_remote_rejects_symlinks_arbitrary_targets_and_non_catalog_actions(self):
    with patch.dict(os.environ,{'HOME':str(self.base)}):
      ssh=self.base/'.ssh';ssh.mkdir();outside=self.base/'outside';outside.write_text('fixture')
      (ssh/'authorized_keys').symlink_to(outside)
      with self.assertRaises(ValueError):execute({'operation':'prepare','action':'restrict-permissions','target':'authorizedKeys','expectedMode':0})
      with self.assertRaises(ValueError):execute({'operation':'apply','action':'shell','target':'authorizedKeys'})
      with self.assertRaises(ValueError):execute({'operation':'prepare','action':'restrict-permissions','target':'/etc/passwd'})

  def test_container_restart_or_deployment_changes_prevent_success(self):
    before=healthy();after=healthy();after['containers'][0]['restarts']=1
    self.assertFalse(same_snapshot(before,after))

  def test_remote_health_blocks_cleanup_for_any_stopped_container_including_rollback(self):
    from ms_baley.data.remote_remediation import health, REQUIRED
    rows=[{'id':str(index+1)*64,'name':'/'+name,'state':'running','paused':False,'health':'healthy','restarts':0,'startedAt':'fixed'} for index,name in enumerate(REQUIRED)]
    rows.append({'id':'f'*64,'name':'/rollback','state':'exited','paused':False,'health':'none','restarts':0,'startedAt':'fixed'})
    with patch('ms_baley.data.remote_remediation.run',side_effect=['\n'.join(row['id'] for row in rows),*[json.dumps(row) for row in rows]]) as runner:
      result=health()
    self.assertFalse(result['healthy'])
    self.assertEqual([],result['checks'])
    self.assertEqual(len(rows)+1,runner.call_count)
