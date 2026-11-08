import base64
from contextlib import redirect_stderr, redirect_stdout
from email import policy
from email.parser import BytesParser
from io import StringIO
import json
import os
from pathlib import Path
from tempfile import TemporaryDirectory
import unittest
from unittest.mock import Mock, patch

from ms_trevize.cli import main
from ms_trevize.data.gmail_client import GoogleGmail, GmailError
from ms_trevize.data.gmail_store import GmailStore, private_json
from ms_trevize.domain.mail import build_message, message_view
from ms_trevize.features.gmail.cli import parser_for
from ms_trevize.features.gmail.pilot import GmailPilot


def remote_message(identity, sender="contact@example.test", recipient="olivaw@example.test", thread="thread1", labels=None, **extra):
  headers = {"From":sender,"To":recipient,"Subject":"Parceria Olivaw","Message-ID":"<"+identity+"@example.test>",**extra}
  return {"id":identity,"threadId":thread,"labelIds":labels or ["INBOX"],
    "payload":{"mimeType":"text/plain","headers":[{"name":k,"value":v} for k,v in headers.items()],
      "body":{"data":base64.urlsafe_b64encode("Olá, gostaria de saber mais.".encode()).decode()}}}


class FakeGmail:
  def __init__(self):
    self.messages = {}
    self.sent = []
    self.account = "olivaw@example.test"
    self.timeout = False

  def profile(self): return {"emailAddress":self.account}
  def inbox(self, limit): return [{"id":k} for k in list(self.messages)[:limit]]
  def message(self, identity, full=False): return self.messages[identity]
  def thread(self, identity): return {"messages":[m for m in self.messages.values() if m["threadId"]==identity]}
  def send(self, payload):
    self.sent.append(payload)
    parsed = BytesParser(policy=policy.default).parsebytes(base64.urlsafe_b64decode(payload["raw"]))
    identity = "sent"+str(len(self.sent))
    self.messages[identity] = remote_message(identity, str(parsed["From"]), str(parsed["To"]),
      payload.get("threadId", "thread1"), ["SENT"], **{"Message-ID":str(parsed["Message-ID"]),"Subject":str(parsed["Subject"])})
    if self.timeout: raise TimeoutError("SECRET TOKEN must not reach output")
    return {"id":identity,"threadId":self.messages[identity]["threadId"]}
  def find_sent(self, mime_id):
    return [{"id":m["id"]} for m in self.messages.values() if "SENT" in m["labelIds"] and message_view(m)["messageId"]==mime_id]


class GmailTests(unittest.TestCase):
  def setUp(self):
    self.temp = TemporaryDirectory(); self.addCleanup(self.temp.cleanup)
    self.root = Path(self.temp.name)
    client = self.root/"download.json"
    client.write_text(json.dumps({"installed":{"client_id":"test-client","client_secret":"PRIVATE",
      "auth_uri":"https://accounts.google.com/o/oauth2/auth","token_uri":"https://oauth2.googleapis.com/token"}}))
    self.store = GmailStore(self.root/"gmail")
    self.store.configure("olivaw@example.test",["contact@example.test"],client)
    # Somente o transporte FakeGmail dos testes pode enviar nesta fixture.
    from ms_trevize.data.local_runtime import atomic_json
    config = self.store.config(); config['sendingEnabled'] = True
    atomic_json(self.store.root/'config.json', config)
    self.api = FakeGmail(); self.pilot = GmailPilot(self.store,self.api)

  def prepare(self, key="first", **options):
    return self.pilot.prepare(key,"Olá! Somos a Olivaw.","contact@example.test","Parceria Olivaw",**options)

  def test_status_does_not_start_or_connect(self):
    with patch("ms_trevize.data.gmail_client.GoogleGmail.connect") as connect:
      result = self.store.status()
      self.assertFalse(result["autoSend"]); self.assertFalse(result["authorized"])
      connect.assert_not_called()

  def test_default_directory_is_agent_state_not_source_tree(self):
    location = parser_for().parse_args(["status"]).directory
    self.assertEqual(location,Path(__file__).resolve().parents[1]/".state"/"gmail")

  def test_imported_client_config_intent_are_private(self):
    intent = self.prepare()
    for file in ["client.json","config.json",intent["id"]+".json"]:
      self.assertEqual((self.store.root/file).stat().st_mode & 0o777,0o600)
    self.assertEqual(self.store.root.stat().st_mode & 0o777,0o700)

  def test_symlink_and_world_readable_credentials_are_rejected(self):
    config = self.store.root/"config.json"
    config.chmod(0o644)
    with self.assertRaises(ValueError): self.store.config()
    config.chmod(0o600)
    link = self.root/"link.json";link.symlink_to(config)
    with self.assertRaises(ValueError): private_json(link)
    link_dir = self.root/"linked";link_dir.symlink_to(self.store.root)
    with self.assertRaises(ValueError): GmailStore(link_dir)

  def test_non_google_oauth_endpoints_are_rejected(self):
    file = self.root/"bad-client.json"
    file.write_text(json.dumps({"installed":{"client_id":"x","client_secret":"x","auth_uri":"https://evil.test","token_uri":"https://evil.test"}}))
    with self.assertRaises(ValueError): self.store.configure("olivaw@example.test",["contact@example.test"],file)

  def test_header_injection_or_multiple_recipients_blocked(self):
    for value in ["contact@example.test\r\nBcc:other@example.test","a@example.test,b@example.test"]:
      with self.assertRaises(ValueError): self.pilot.prepare("bad","body",value,"subject")
    with self.assertRaises(ValueError): self.pilot.prepare("bad","body","contact@example.test","Olá\nBcc: x")
    self.assertEqual(self.api.sent,[])

  def test_recipient_allowlist_and_wrong_account_block_send(self):
    with self.assertRaises(ValueError): self.pilot.prepare("bad","body","stranger@example.test","subject")
    intent = self.prepare();self.api.account="wrong@example.test"
    with self.assertRaises(ValueError): self.pilot.send(intent["id"],True)
    self.assertEqual(self.store.get(intent["id"])["state"],"prepared")
    self.assertEqual(self.api.sent,[])

  def test_prepare_does_not_need_gmail_or_token(self):
    self.pilot.api=None
    intent=self.prepare()
    self.assertEqual(intent["state"],"prepared")
    self.assertFalse((self.store.root/"token.json").exists())

  def test_stable_key_cannot_change_content(self):
    first=self.prepare();self.assertEqual(first,self.prepare())
    with self.assertRaises(ValueError): self.pilot.prepare("first","Changed","contact@example.test","Parceria Olivaw")

  def test_send_requires_confirmation_and_confirms_exact_sent_message(self):
    intent=self.prepare()
    with self.assertRaises(ValueError): self.pilot.send(intent["id"])
    result=self.pilot.send(intent["id"],True)
    self.assertEqual(result["state"],"sent")
    self.assertEqual(self.store.get(intent["id"])["gmailMessageId"],"sent1")
    self.pilot.send(intent["id"],True)
    self.assertEqual(len(self.api.sent),1)

  def test_timeout_after_acceptance_never_retries_and_can_reconcile(self):
    self.api.timeout=True;intent=self.prepare()
    self.assertEqual(self.pilot.send(intent["id"],True)["state"],"confirmation-pending")
    self.pilot.send(intent["id"],True)
    self.assertEqual(len(self.api.sent),1)
    self.assertEqual(self.pilot.reconcile(intent["id"])["state"],"sent")

  def test_crash_reservation_is_not_retried_even_if_no_message_found(self):
    intent=self.prepare();intent.update(state="confirmation-pending",attemptedAt="2026-01-01T00:00:00Z");self.store.save(intent)
    self.pilot.send(intent["id"],True)
    self.assertEqual(self.pilot.reconcile(intent["id"])["state"],"confirmation-pending")
    self.assertEqual(self.api.sent,[])

  def test_sent_confirmation_rejects_wrong_message(self):
    intent=self.prepare()
    original=self.api.message
    def mismatch(identity,full=False):
      result=original(identity,full);result["labelIds"]=["DRAFT"];return result
    self.api.message=mismatch
    self.assertEqual(self.pilot.send(intent["id"],True)["state"],"confirmation-pending")

  def test_daily_limit_counts_ambiguous_attempts(self):
    self.api.timeout=True
    for i in range(5): self.pilot.send(self.prepare("attempt"+str(i))["id"],True)
    with self.assertRaises(ValueError):self.pilot.send(self.prepare("sixth")["id"],True)
    self.assertEqual(len(self.api.sent),5)

  def test_reply_keeps_thread_mime_references_and_subject(self):
    self.api.messages["received1"]=remote_message("received1")
    intent=self.pilot.prepare("answer","Pode nos contar sua necessidade?",reply_id="received1")
    result=self.pilot.send(intent["id"],True)
    self.assertEqual(result["threadId"],"thread1")
    payload=self.api.sent[0]
    parsed=BytesParser(policy=policy.default).parsebytes(base64.urlsafe_b64decode(payload["raw"]))
    self.assertEqual(parsed["In-Reply-To"],"<received1@example.test>")
    self.assertEqual(parsed["References"],"<received1@example.test>")
    self.assertEqual(parsed["Subject"],"Parceria Olivaw")

  def test_human_message_after_preparation_blocks_reply(self):
    self.api.messages["received1"]=remote_message("received1")
    intent=self.pilot.prepare("answer","body",reply_id="received1")
    self.api.messages["human1"]=remote_message("human1","olivaw@example.test","contact@example.test",labels=["SENT"])
    with self.assertRaises(ValueError): self.pilot.send(intent["id"],True)
    self.assertEqual(self.api.sent,[])

  def test_cannot_answer_same_message_with_another_key(self):
    self.api.messages["received1"]=remote_message("received1")
    self.pilot.prepare("answer","body",reply_id="received1")
    with self.assertRaises(ValueError):self.pilot.prepare("new-key","body",reply_id="received1")

  def test_auto_message_self_reply_and_reply_to_divergence_blocked(self):
    for message in [remote_message("auto",**{"Auto-Submitted":"auto-replied"}),
        remote_message("self","olivaw@example.test",labels=["SENT"]),
        remote_message("other",**{"Reply-To":"different@example.test"})]:
      self.api.messages={message["id"]:message}
      with self.assertRaises(ValueError):self.pilot.prepare("response","body",reply_id=message["id"])

  def test_duplicate_unimportant_headers_are_allowed_but_from_is_not(self):
    message=remote_message("r1")
    message["payload"]["headers"].extend([{"name":"Received","value":"a"},{"name":"Received","value":"b"}])
    self.assertEqual(message_view(message)["id"],"r1")
    message["payload"]["headers"].append({"name":"From","value":"x@example.test"})
    with self.assertRaises(ValueError):message_view(message)

  def test_bodies_are_explicit_and_attachments_are_not_read(self):
    message=remote_message("r1")
    self.assertNotIn("body",message_view(message))
    message["payload"]["parts"]=[{"filename":"secret.txt","mimeType":"text/plain","body":{"data":base64.urlsafe_b64encode(b'SECRET').decode()}}]
    self.assertNotIn("SECRET",message_view(message,True)["body"])

  def test_replies_consult_known_threads_and_do_not_depend_on_unread(self):
    intent=self.prepare();self.pilot.send(intent["id"],True)
    incoming=remote_message("inbound1",labels=["INBOX"])
    self.api.messages["inbound1"]=incoming
    self.api.messages["unrelated"]=remote_message("unrelated",thread="another-thread")
    result=self.pilot.replies()
    self.assertEqual([i['id'] for i in result['messages']],["inbound1"])
    self.assertFalse(result["autoReply"])
    self.assertEqual(self.pilot.replies()["messages"],[])
    self.assertEqual(len(self.api.sent),1)
    self.assertNotIn("body",(self.store.root/"received.json").read_text())

  def test_manual_intervention_is_reported(self):
    intent=self.prepare();self.pilot.send(intent["id"],True)
    self.api.messages["human"]=remote_message("human","olivaw@example.test","contact@example.test",labels=["SENT"])
    self.api.messages["inbound1"]=remote_message("inbound1")
    self.assertEqual(self.pilot.replies()["messages"][0]["disposition"],"human-takeover")

  def test_google_adapter_does_not_retry_writes_or_expose_secrets(self):
    service=Mock();request=service.users.return_value.messages.return_value.send.return_value
    request.execute.side_effect=RuntimeError("PRIVATE token")
    with self.assertRaises(GmailError) as error:GoogleGmail(service, writes_enabled=True).send({"raw":"test"})
    self.assertNotIn("PRIVATE",str(error.exception))
    request.execute.assert_called_once_with(num_retries=0)

  def test_reconcile_accepts_provider_rewritten_message_id_only_with_exact_returned_id_and_body(self):
    intent = self.prepare()
    self.pilot.send(intent['id'],True)
    saved = self.store.get(intent['id']); saved['state'] = 'confirmation-pending'; self.store.save(saved)
    remote = self.api.messages[saved['gmailMessageId']]
    for entry in remote['payload']['headers']:
      if entry['name'].lower() == 'message-id': entry['value'] = '<provider-generated@example.test>'
    remote['payload']['body']['data'] = base64.urlsafe_b64encode((intent['content']['body']+'\r\n').encode()).decode()
    self.assertEqual([],self.api.find_sent(intent['mimeId']))
    self.assertEqual('sent',self.pilot.reconcile(intent['id'])['state'])
    self.assertEqual(1,len(self.api.sent))
    self.assertEqual('<provider-generated@example.test>',self.store.get(intent['id'])['gmailMimeId'])

  def test_rewritten_message_id_with_different_content_or_without_send_response_remains_unconfirmed(self):
    intent = self.prepare()
    self.pilot.send(intent['id'],True)
    saved = self.store.get(intent['id']); saved['state'] = 'confirmation-pending'; self.store.save(saved)
    remote = self.api.messages[saved['gmailMessageId']]
    for entry in remote['payload']['headers']:
      if entry['name'].lower() == 'message-id': entry['value'] = '<provider-generated@example.test>'
    with self.assertRaises(ValueError): self.pilot.reconcile(intent['id'])
    self.assertEqual('confirmation-pending',self.store.get(intent['id'])['state'])
    saved.pop('gmailMessageId'); self.store.save(saved)
    self.assertEqual('confirmation-pending',self.pilot.reconcile(intent['id'])['state'])
    self.assertEqual(1,len(self.api.sent))

  def test_gmail_cli_routes_without_collecting_or_starting(self):
    stdout,stderr=StringIO(),StringIO()
    with redirect_stdout(stdout),redirect_stderr(stderr),patch('ms_trevize.cli.LocalRuntime') as runtime:
      result=main(['gmail','--directory',str(self.store.root),'status'])
    self.assertEqual(result,0);runtime.assert_not_called()
    self.assertFalse(json.loads(stdout.getvalue())["autoSend"])

  def test_cli_sanitizes_sdk_exceptions(self):
    output=StringIO()
    with redirect_stderr(output),patch('ms_trevize.data.gmail_client.GoogleGmail.connect',side_effect=ValueError('SECRET')):
      result=main(['gmail','--directory',str(self.store.root),'profile'])
    self.assertEqual(result,1);self.assertNotIn('SECRET',output.getvalue())

  def test_reviewed_purpose_and_origin_questions_are_accepted_by_transport(self):
    for question in ('purpose','contact-origin'):
      intent = self.prepare('review-'+question,question_id=question)
      self.assertEqual(question,intent['content']['questionId'])
      self.assertEqual('prepared',intent['state'])
    with self.assertRaises(ValueError): self.prepare('forbidden',question_id='negotiate-price')
    self.assertEqual([],self.api.sent)


if __name__ == "__main__": unittest.main()
