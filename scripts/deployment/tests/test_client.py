import contextlib
import io
import json
import os
from pathlib import Path
import sys
import unittest
from unittest.mock import patch, Mock
import urllib.request


class ClientTests(unittest.TestCase):
    def run_client(self, arguments, job_path, expected_payload):
        script = (Path(__file__).parents[1] / 'deploy.sh').read_text()
        source = script.split("<<'PYTHON'\n", 1)[1].rsplit('\nPYTHON', 1)[0]
        replies = [dict(accessToken='test-jwt'), dict(id='test-job', status='queued')]
        calls = []

        def open_request(request, timeout):
            calls.append(request)
            reply = Mock()
            reply.__enter__ = Mock(return_value=io.BytesIO(json.dumps(replies.pop(0)).encode()))
            reply.__exit__ = Mock(return_value=False)
            return reply

        settings = dict(HUB_API_URL='https://hub.test', DEVOPS_API_URL='https://devops.test', HUB_ROOT_PASSWORD='test-only-password')
        with patch.dict(os.environ, settings), patch.object(sys, 'argv', ['deploy.sh', *arguments, '--no-wait']):
            with patch.object(urllib.request, 'urlopen', side_effect=open_request), contextlib.redirect_stdout(io.StringIO()):
                with self.assertRaises(SystemExit) as result:
                    exec(compile(source, 'deploy-client', 'exec'), {'__name__': '__main__'})
        self.assertEqual(0, result.exception.code)
        self.assertEqual('https://hub.test/identity/authentication/login', calls[0].full_url)
        self.assertEqual({'login': 'root', 'password': 'test-only-password'}, json.loads(calls[0].data))
        self.assertEqual('https://devops.test' + job_path, calls[1].full_url)
        self.assertEqual('Bearer test-jwt', calls[1].get_header('Authorization'))
        self.assertEqual(expected_payload, json.loads(calls[1].data))

    def test_deployment_authenticates_in_hub_and_submits_to_devops(self):
        self.run_client(['ms-ltmj-ng', 'prd', 'develop'], '/deployments',
                        dict(folder='olivaw-hub/ms-ltmj-ng', environment='prd', branch='develop'))

    def test_cleanup_uses_same_hub_authentication_without_deployment_parameters(self):
        self.run_client(['--cleanup'], '/disk-cleanups', {})


if __name__ == '__main__':
    unittest.main()
