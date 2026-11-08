import contextlib
import io
import json
import os
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import Mock, patch
import urllib.error
import urllib.request


JOB = '7c73bdf6-781a-4e98-96a8-559adcc30b7f'


def http_failure(code):
    return urllib.error.HTTPError('https://devops.test/deployments/' + JOB, code, 'failure', {},
                                  io.BytesIO(b'{"title":"API temporarily unavailable"}'))


class StatusRecoveryTests(unittest.TestCase):
    def run_client(self, steps, arguments=None):
        source = (Path(__file__).parents[1] / 'deploy.sh').read_text()
        source = source.split("<<'PYTHON'\n", 1)[1].rsplit('\nPYTHON', 1)[0]
        arguments = arguments or ['ms-olivaw-api', 'prd', 'feature/issue031-test']
        calls, output = [], io.StringIO()
        remaining = list(steps)
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            service = 'ms-devops-api' if '--cleanup' in arguments else 'ms-olivaw-api'
            path = root / service / 'deployment' / 'olivaw-credentials.env'
            path.parent.mkdir(parents=True)
            path.write_text('OLIVAW_LOGIN=test-account\nOLIVAW_PASSWORD=private-test-password\n')

            def open_request(request, timeout):
                calls.append(request)
                if not remaining:
                    raise AssertionError('O cliente fez uma chamada não prevista, possivelmente duplicada.')
                step = remaining.pop(0)
                if isinstance(step, Exception):
                    raise step
                raw = step.encode() if isinstance(step, str) else json.dumps(step).encode()
                response = Mock()
                response.__enter__ = Mock(return_value=io.BytesIO(raw))
                response.__exit__ = Mock(return_value=False)
                return response

            environment = {'OLIVAW_API_URL':'https://olivaw.test', 'DEVOPS_API_URL':'https://devops.test',
                           'OLIVAW_REPO_ROOT':str(root)}
            with patch.dict(os.environ, environment), patch.object(sys, 'argv', ['deploy.sh', *arguments]):
                with patch.object(urllib.request, 'urlopen', side_effect=open_request), patch('time.sleep') as sleep:
                    with contextlib.redirect_stdout(output), contextlib.redirect_stderr(output):
                        with self.assertRaises(SystemExit) as exited:
                            exec(compile(source, 'deploy-client', 'exec'), {'__name__':'__main__'})
        self.assertNotIn('private-test-password', output.getvalue())
        self.assertNotIn('private-token', output.getvalue())
        return exited.exception.code, calls, output.getvalue(), sleep

    def test_network_timeout_invalid_json_and_503_recover_without_resubmitting(self):
        code, calls, output, sleep = self.run_client([
            {'accessToken':'private-token'}, {'id':JOB,'status':'queued'},
            urllib.error.URLError('temporary'), TimeoutError(), 'not-json', http_failure(503),
            {'id':JOB,'status':'succeeded','log':'Compose: exit code 0.'}])
        self.assertEqual(0, code)
        self.assertEqual(1, sum(call.full_url == 'https://devops.test/deployments' for call in calls))
        self.assertEqual(5, sum(call.full_url.endswith('/' + JOB) for call in calls))
        self.assertEqual([3,6,12,15], [call.args[0] for call in sleep.call_args_list])
        self.assertIn('Job concluído', output)

    def test_resume_only_logs_in_and_reads_the_existing_job(self):
        code, calls, _, _ = self.run_client([{'accessToken':'private-token'}, {'id':JOB,'status':'succeeded'}],
                                           ['ms-olivaw-api','--job',JOB])
        self.assertEqual(0, code)
        self.assertEqual(['POST','GET'], [call.get_method() for call in calls])
        self.assertEqual('https://devops.test/deployments/' + JOB, calls[1].full_url)

    def test_cleanup_resume_uses_same_job_without_creating_another_cleanup(self):
        code, calls, _, _ = self.run_client([{'accessToken':'private-token'}, {'id':JOB,'status':'succeeded'}],
                                           ['--cleanup','--job',JOB])
        self.assertEqual(0, code)
        self.assertEqual('https://devops.test/disk-cleanups/' + JOB,calls[1].full_url)
        self.assertEqual('GET',calls[1].get_method())

    def test_401_renews_login_once_and_reuses_job(self):
        code, calls, output, _ = self.run_client([
            {'accessToken':'private-token'}, {'id':JOB,'status':'queued'}, http_failure(401),
            {'accessToken':'private-token-new'}, {'id':JOB,'status':'succeeded'}])
        self.assertEqual(0,code)
        self.assertEqual(2,sum(call.full_url.endswith('/login') for call in calls))
        self.assertEqual(1,sum(call.full_url == 'https://devops.test/deployments' for call in calls))
        self.assertEqual('Bearer private-token-new',calls[-1].get_header('Authorization'))
        self.assertIn('Autenticação renovada',output)

    def test_403_is_terminal_and_does_not_retry_or_resubmit(self):
        code, calls, output, sleep = self.run_client([
            {'accessToken':'private-token'}, {'id':JOB,'status':'queued'}, http_failure(403)])
        self.assertEqual(2,code)
        self.assertEqual(3,len(calls))
        sleep.assert_not_called()
        self.assertIn('--job ' + JOB,output)

    def test_consecutive_failure_budget_stops_with_resume_command(self):
        code, calls, output, sleep = self.run_client([
            {'accessToken':'private-token'}, {'id':JOB,'status':'queued'},
            *[urllib.error.URLError('temporary') for _ in range(6)]])
        self.assertEqual(2,code)
        self.assertEqual(1,sum(call.full_url == 'https://devops.test/deployments' for call in calls))
        self.assertEqual(5,sleep.call_count)
        self.assertIn('seis falhas consecutivas',output)
        self.assertIn('ms-olivaw-api --job ' + JOB,output)

    def test_invalid_response_shape_retries_and_success_resets_failure_budget(self):
        code, calls, _, _ = self.run_client([
            {'accessToken':'private-token'}, {'id':JOB,'status':'queued'}, [], {},
            {'id':JOB,'status':'running'}, [], {}, {'id':JOB,'status':'succeeded'}])
        self.assertEqual(0,code)
        self.assertEqual(1,sum(call.full_url == 'https://devops.test/deployments' for call in calls))

    def test_submit_timeout_is_not_retried(self):
        code, calls, output, _ = self.run_client([{'accessToken':'private-token'},TimeoutError()])
        self.assertIsInstance(code,str)
        self.assertEqual(2,len(calls))
        self.assertEqual('POST',calls[-1].get_method())
        self.assertNotIn('Job solicitado',output)

    def test_invalid_resume_id_makes_no_requests(self):
        code, calls, _, _ = self.run_client([],['ms-olivaw-api','--job','../../other'])
        self.assertEqual(2,code)
        self.assertEqual([],calls)
