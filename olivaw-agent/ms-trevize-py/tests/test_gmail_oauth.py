import json
from pathlib import Path
import sys
from tempfile import TemporaryDirectory
from types import ModuleType
import unittest
from unittest.mock import Mock, patch

from ms_trevize.data.gmail_client import authorize, credentials_for, GmailError
from ms_trevize.data.gmail_store import GmailStore, SCOPES
from ms_trevize.data.local_runtime import atomic_json


class OAuthTests(unittest.TestCase):
  def setUp(self):
    self.temp = TemporaryDirectory(); self.addCleanup(self.temp.cleanup)
    self.root = Path(self.temp.name)
    self.store = GmailStore(self.root/'gmail')
    source = self.root/'client.json'
    source.write_text(json.dumps({'installed':{'client_id':'fixture','client_secret':'fixture',
      'auth_uri':'https://accounts.google.com/o/oauth2/auth','token_uri':'https://oauth2.googleapis.com/token'}}))
    self.store.configure('olivaw@example.test',['operator@example.test'],source)
    self.credentials = Mock(valid=True, granted_scopes=list(SCOPES))
    self.credentials.has_scopes.return_value = True
    self.credentials.to_json.return_value = json.dumps({'token_uri':'https://oauth2.googleapis.com/token','scopes':list(SCOPES)})

  def modules(self):
    flow = ModuleType('google_auth_oauthlib.flow')
    flow.InstalledAppFlow = Mock()
    flow.InstalledAppFlow.from_client_config.return_value.run_local_server.return_value = self.credentials
    discovery = ModuleType('googleapiclient.discovery'); discovery.build = Mock()
    return {'google_auth_oauthlib.flow':flow,'googleapiclient.discovery':discovery}, flow

  def test_account_mismatch_never_persists_token_and_flow_uses_pkce_loopback(self):
    modules, flow = self.modules()
    with patch.dict(sys.modules,modules), patch('ms_trevize.data.gmail_client.GoogleGmail') as client:
      client.return_value.profile.return_value = {'emailAddress':'wrong@example.test'}
      with self.assertRaises(GmailError): authorize(self.store)
    self.assertFalse((self.store.root/'token.json').exists())
    self.assertTrue(flow.InstalledAppFlow.from_client_config.call_args.kwargs['autogenerate_code_verifier'])
    options = flow.InstalledAppFlow.from_client_config.return_value.run_local_server.call_args.kwargs
    self.assertEqual('127.0.0.1',options['host'])
    self.assertEqual(0,options['port'])
    self.assertEqual(180,options['timeout_seconds'])

  def test_expected_account_saves_private_token_and_incomplete_scopes_do_not(self):
    modules, _ = self.modules()
    with patch.dict(sys.modules,modules), patch('ms_trevize.data.gmail_client.GoogleGmail') as client:
      client.return_value.profile.return_value = {'emailAddress':'olivaw@example.test'}
      self.assertEqual('authorized',authorize(self.store)['status'])
    self.assertEqual(0o600,(self.store.root/'token.json').stat().st_mode & 0o777)
    self.credentials.has_scopes.return_value = False
    modules, _ = self.modules()
    with patch.dict(sys.modules,modules):
      with self.assertRaises(GmailError): authorize(self.store)

  def test_oauth_failure_does_not_expose_library_exception(self):
    modules, flow = self.modules()
    flow.InstalledAppFlow.from_client_config.return_value.run_local_server.side_effect = RuntimeError('SECRET_TOKEN')
    with patch.dict(sys.modules,modules):
      with self.assertRaises(GmailError) as error: authorize(self.store)
    self.assertNotIn('SECRET_TOKEN',str(error.exception))

  def test_refresh_failures_and_wrong_token_endpoint_are_rejected(self):
    module = ModuleType('google.oauth2.credentials'); module.Credentials = Mock()
    transport = ModuleType('google.auth.transport.requests'); transport.Request = Mock()
    modules = {'google.oauth2.credentials':module,'google.auth.transport.requests':transport}
    self.credentials.valid = False
    self.credentials.refresh.side_effect = RuntimeError('SECRET_REFRESH_TOKEN')
    module.Credentials.from_authorized_user_info.return_value = self.credentials
    atomic_json(self.store.root/'token.json',{'token_uri':'https://oauth2.googleapis.com/token','scopes':list(SCOPES)})
    with patch.dict(sys.modules,modules):
      with self.assertRaises(GmailError) as error: credentials_for(self.store)
      self.assertNotIn('SECRET_REFRESH_TOKEN',str(error.exception))
      atomic_json(self.store.root/'token.json',{'token_uri':'https://evil.test/token','scopes':list(SCOPES)})
      self.credentials.refresh.reset_mock()
      with self.assertRaises(GmailError): credentials_for(self.store)
      self.credentials.refresh.assert_not_called()
