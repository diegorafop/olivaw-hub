import importlib.util
import json
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import patch, Mock
import uuid

SPEC = importlib.util.spec_from_file_location('deploy', Path(__file__).parents[1] / 'deploy.py')
deploy = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(deploy)


class DeploymentTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='olivaw-executor-')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        (self.root / 'olivaw-hub/.git').mkdir(parents=True)
        (self.root / 'olivaw-hub/ms-devops-api/docker').mkdir(parents=True)
        self.config = dict(rootDirectory=str(self.root), stateDirectory=str(self.root / 'state'),
                           allowedFolders=['olivaw-hub/ms-devops-api'], stepTimeoutSeconds=1)
        self.executor = deploy.Executor(self.config)
        self.request = dict(folder='olivaw-hub/ms-devops-api', branch='develop', environment='dev')

    def job(self):
        job = dict(self.request, id=str(uuid.uuid4()), status='queued', log='Queued.')
        self.executor.save(job)
        return job

    def test_validate_rejects_missing_fields_injection_and_traversal(self):
        for field in self.request:
            for value in [None, '', ';id', 'a&&id', 'a|id', '../outside', '$(id)', 'a\nid']:
                with self.subTest(field=field, value=value):
                    with self.assertRaises(ValueError):
                        deploy.validate_request(dict(self.request, **{field: value}))
        for branch in ['-main', 'HEAD', 'a//b', 'a.lock', 'a/.hidden', 'main.']:
            with self.assertRaises(ValueError):
                deploy.validate_request(dict(self.request, branch=branch))

    def test_supported_environments_and_gitflow_branch(self):
        for environment in ['dev', 'qas', 'stg', 'rls', 'prd']:
            self.assertEqual(environment, deploy.validate_request(dict(self.request,
                             environment=environment, branch='release/1.2.3'))['environment'])

    def test_allowlist_and_symbolic_links_are_rejected(self):
        with self.assertRaises(ValueError):
            deploy.resolve_folder(self.config, dict(self.request, folder='olivaw-hub'))
        target = self.root / 'alias'
        target.symlink_to(self.root / 'olivaw-hub/ms-devops-api', target_is_directory=True)
        self.config['allowedFolders'].append('alias')
        with self.assertRaises(ValueError):
            deploy.resolve_folder(self.config, dict(self.request, folder='alias'))
        self.assertEqual(self.root / self.request['folder'], deploy.resolve_folder(self.config, self.request))

    @patch.object(deploy.subprocess, 'run')
    def test_submit_schedules_detached_systemd_worker_and_persists_job(self, run):
        result = self.executor.submit(self.request)
        self.assertEqual('queued', result['status'])
        command = run.call_args.args[0]
        self.assertEqual('/usr/bin/systemd-run', command[0])
        self.assertIn('--user', command)
        self.assertIn('--property=RuntimeMaxSec=1800', command)
        self.assertEqual(['run', result['id']], command[-2:])
        self.assertNotIn('shell', run.call_args.kwargs)
        self.assertEqual(self.request['folder'], self.executor.load(result['id'])['folder'])
        self.assertNotIn('folder', result)
        self.assertEqual(0, self.executor.path(result['id']).stat().st_mode & 0o077)

    @patch.object(deploy.subprocess, 'run', side_effect=OSError('private details'))
    def test_scheduling_failure_is_standardized_and_persisted(self, run):
        response = deploy.gateway(self.executor, json.dumps(dict(self.request, action='submit')))
        self.assertEqual({'success': False, 'error': 'executor_unavailable'}, response)
        self.assertEqual('failed', json.loads(next(self.executor.state.glob('*.json')).read_text())['status'])

    @patch.object(deploy, 'run_step', return_value=0)
    def test_worker_runs_ordered_commands_from_service_root(self, run):
        job = self.job()
        self.executor.run(job['id'])
        saved = self.executor.load(job['id'])
        self.assertEqual('succeeded', saved['status'])
        self.assertEqual(3, run.call_count)
        self.assertEqual(['/usr/bin/git', 'checkout', 'develop'], run.call_args_list[0].args[0])
        self.assertEqual(['/usr/bin/git', 'pull', '--ff-only'], run.call_args_list[1].args[0])
        self.assertIn('docker/docker-compose.dev.yml', run.call_args_list[2].args[0])
        self.assertIn('--build', run.call_args_list[2].args[0])
        self.assertEqual(self.root / self.request['folder'], run.call_args_list[2].args[1])
        self.executor.run(job['id'])
        self.assertEqual(3, run.call_count)

    def test_worker_stops_on_checkout_pull_or_docker_failure(self):
        for index in range(3):
            job = self.job()
            with patch.object(deploy, 'run_step', side_effect=[0] * index + [1]) as run:
                self.executor.run(job['id'])
                self.assertEqual(index + 1, run.call_count)
                self.assertEqual('failed', self.executor.load(job['id'])['status'])

    @patch.object(deploy, 'run_step', side_effect=subprocess.TimeoutExpired('git', 1))
    def test_worker_timeout_is_persisted_without_raw_error(self, run):
        job = self.job()
        self.executor.run(job['id'])
        saved = self.executor.load(job['id'])
        self.assertEqual('failed', saved['status'])
        self.assertNotIn('TimeoutExpired', saved['log'])
        self.assertEqual(1, run.call_count)

    @patch.object(deploy.subprocess, 'run', return_value=Mock(returncode=3, stdout='inactive\n'))
    def test_status_detects_interrupted_worker_without_mutating_record(self, run):
        job = self.job()
        result = self.executor.status(job['id'])
        self.assertEqual('failed', result['status'])
        self.assertEqual('queued', self.executor.load(job['id'])['status'])

    def test_gateway_validates_protocol_and_unknown_job(self):
        for raw in ['x' * 4097, 'not json', '[]', '{}', '{"action":"shell"}', '{"action":"status","id":"../file"}']:
            self.assertEqual({'success': False, 'error': 'invalid_request'}, deploy.gateway(self.executor, raw))
        self.assertEqual({'success': False, 'error': 'not_found'}, deploy.gateway(self.executor,
                         json.dumps(dict(action='status', id=str(uuid.uuid4())))))

    def test_compose_uses_each_environment_as_separate_arguments(self):
        for environment in ['dev', 'qas', 'stg', 'rls', 'prd']:
            command = deploy.commands('develop', environment)[2][1]
            self.assertIn(f'docker/.env.{environment}', command)
            self.assertIn(f'docker/docker-compose.{environment}.yml', command)
            self.assertEqual(['up', '-d', '--build'], command[-3:])

    def test_self_deployment_preserves_ssh_override(self):
        command = deploy.commands('develop', 'dev', include_ssh=True)[2][1]
        self.assertIn('docker/docker-compose.ssh.yml', command)
        self.assertNotIn('docker/docker-compose.ssh.yml', deploy.commands('develop', 'dev')[2][1])

    @patch.object(deploy.os, 'killpg')
    @patch.object(deploy.subprocess, 'Popen')
    def test_timeout_kills_process_group_and_reaps_child(self, popen, killpg):
        process = popen.return_value.__enter__.return_value
        process.pid = 42
        process.wait.side_effect = [subprocess.TimeoutExpired('git', 1), 0]
        with self.assertRaises(subprocess.TimeoutExpired):
            deploy.run_step(['/usr/bin/git', 'pull'], self.root, 1)
        killpg.assert_called_once_with(42, deploy.signal.SIGKILL)
        self.assertTrue(popen.call_args.kwargs['start_new_session'])
        self.assertEqual(subprocess.DEVNULL, popen.call_args.kwargs['stdin'])


if __name__ == '__main__':
    unittest.main()
