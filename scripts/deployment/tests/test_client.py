import contextlib
import io
import json
import os
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import patch, Mock
import urllib.request


class ClientTests(unittest.TestCase):
    def run_client(self, arguments, job_path, expected_payload, service_dir_name=None, login_name="test-account"):
        script = (Path(__file__).parents[1] / 'deploy.sh').read_text()
        source = script.split("<<'PYTHON'\n", 1)[1].rsplit('\nPYTHON', 1)[0]
        replies = [dict(accessToken='test-jwt'), dict(id='test-job', status='queued')]
        calls = []

        with tempfile.TemporaryDirectory() as temporary:
            repo_root = Path(temporary) / 'olivaw'
            repo_root.mkdir()
            service = service_dir_name or ("ms-devops-api" if arguments == ["--cleanup"] else arguments[0])
            if service == "lisierp":
                service_directory = Path(temporary) / 'lisifin'
                (repo_root / 'scripts' / 'deployment').mkdir(parents=True)
                (repo_root / 'scripts' / 'deployment' / 'external-services.json').write_text(
                    json.dumps({"lisierp": "../lisifin"}), encoding='utf-8')
            else:
                service_directory = repo_root / service
            credentials_path = service_directory / 'deployment' / 'olivaw-credentials.env'
            credentials_path.parent.mkdir(parents=True)
            credentials_path.write_text(
                f'OLIVAW_LOGIN={login_name}\nOLIVAW_PASSWORD=test-only-password\n', encoding='utf-8')

            def open_request(request, timeout):
                calls.append(request)
                reply = Mock()
                reply.__enter__ = Mock(return_value=io.BytesIO(json.dumps(replies.pop(0)).encode()))
                reply.__exit__ = Mock(return_value=False)
                return reply

            settings = dict(
                OLIVAW_API_URL='https://olivaw.test',
                DEVOPS_API_URL='https://devops.test',
                OLIVAW_REPO_ROOT=str(repo_root),
            )
            with patch.dict(os.environ, settings), patch.object(sys, 'argv', ['deploy.sh', *arguments, '--no-wait']):
                with patch.object(urllib.request, 'urlopen', side_effect=open_request), contextlib.redirect_stdout(io.StringIO()):
                    with self.assertRaises(SystemExit) as result:
                        exec(compile(source, 'deploy-client', 'exec'), {'__name__': '__main__'})
        self.assertEqual(0, result.exception.code)
        self.assertEqual('https://olivaw.test/identity/authentication/login', calls[0].full_url)
        self.assertEqual({'login': login_name, 'password': 'test-only-password'}, json.loads(calls[0].data))
        self.assertEqual('https://devops.test' + job_path, calls[1].full_url)
        self.assertEqual('Bearer test-jwt', calls[1].get_header('Authorization'))
        self.assertEqual(expected_payload, json.loads(calls[1].data))

    def test_deployment_reads_client_credentials_from_service_directory(self):
        self.run_client(['ms-ltmj-ng', 'prd', 'develop'], '/deployments',
                        dict(folder='olivaw/ms-ltmj-ng', environment='prd', branch='develop'),
                        login_name='ltmj')

    def test_deployment_resolves_standalone_lisierp_repository(self):
        self.run_client(['lisierp', 'prd', 'main'], '/deployments',
                        dict(folder='lisierp', environment='prd', branch='main'),
                        login_name='lisierp')

    def test_internal_service_uses_root_credentials_in_its_own_directory(self):
        self.run_client(['ms-devops-api', 'prd', 'develop'], '/deployments',
                        dict(folder='olivaw/ms-devops-api', environment='prd', branch='develop'),
                        login_name='root')

    def test_cleanup_uses_root_credentials_from_devops_directory(self):
        self.run_client(['--cleanup'], '/disk-cleanups', {}, login_name='root')


if __name__ == '__main__':
    unittest.main()
