from copy import deepcopy
import json
from pathlib import Path
from tempfile import TemporaryDirectory
import unittest
from unittest.mock import Mock

from ms_trevize.data.gmail_store import GmailStore
from ms_trevize.features.gmail.pilot import GmailPilot
from ms_trevize.features.leads.enrichment import LeadEnrichment
from ms_trevize.features.leads.outreach import LeadOutreach
from test_gmail import FakeGmail, remote_message

LEAD = '00000000-0000-0000-0000-000000000001'


class FakeCRM:
  def __init__(self):
    self.data = {'externalId':LEAD,'followUpStatus':'new','followUpRevision':0,
      'snapshot':{'revision':1,'email':'contact@example.test','acquisition':{'initiative':'outbound'},
        'qualification':{'schemaVersion':1,'websiteUrl':'https://example.test','approach':'Texto humano',
          'approachStatus':'reviewed','tags':[],'contactFormUrl':'https://example.test/revisado'}}}
    self.events, self.fail = [], None

  def request(self, path, payload=None, method=None):
    if path.startswith('/leads?'): return {'items':[{'externalId':LEAD}]}
    if path.endswith('/contact-discovery'):
      contacts = payload['contacts']
      qualification = self.data['snapshot']['qualification']
      for key in ('publicEmails','contactPageUrl','contactFormUrl','contactFormStatus','contactCheckedAt'):
        if key in contacts: qualification[key] = deepcopy(contacts[key])
      self.data['snapshot']['revision'] += 1
      return {'revision':self.data['snapshot']['revision']}
    if path.endswith('/qualification'):
      if payload['revision'] != self.data['snapshot']['revision']: raise ValueError('Conflict')
      self.data['snapshot']['qualification'] = deepcopy(payload['qualification'])
      self.data['snapshot']['revision'] += 1
      return {'revision':self.data['snapshot']['revision']}
    if path.endswith('/communications'):
      event = payload['event']
      if self.fail == event['kind']: raise ValueError('CRM unavailable')
      for previous in self.events:
        if previous['eventId'] == event['eventId']:
          if previous != event: raise ValueError('Changed content')
          return {'repeated':True}
      self.events.append(deepcopy(event))
      self.data['snapshot']['communications'] = deepcopy(self.events)
      self.data['snapshot']['revision'] += 1
      self.data['followUpRevision'] += 1
      if event['kind'] in ('sent','received') and self.data['followUpStatus'] == 'new':
        self.data['followUpStatus'] = 'conversation'
      return {'repeated':False}
    return deepcopy(self.data)


class WorkflowTests(unittest.TestCase):
  def setUp(self):
    self.temp = TemporaryDirectory(); self.addCleanup(self.temp.cleanup)
    self.root = Path(self.temp.name)
    client = self.root/'client.json'
    client.write_text(json.dumps({'installed':{'client_id':'fixture','client_secret':'fixture',
      'auth_uri':'https://accounts.google.com/o/oauth2/auth','token_uri':'https://oauth2.googleapis.com/token'}}))
    self.store = GmailStore(self.root/'state')
    self.store.configure('olivaw@example.test',['operator@example.test'],client)
    # Somente o transporte FakeGmail dos testes pode enviar nesta fixture.
    from ms_trevize.data.local_runtime import atomic_json
    config = self.store.config(); config['sendingEnabled'] = True
    atomic_json(self.store.root/'config.json', config)
    self.crm, self.gmail = FakeCRM(), FakeGmail()
    self.advisor = Mock()
    self.advisor.analyze.return_value = {'classification':'qualify','questionId':'need','summary':'Entender a necessidade.'}
    self.service = LeadOutreach(self.crm,self.store,self.gmail,self.advisor)

  def prepare(self):
    return self.service.prepare(LEAD,'contact@example.test','Parceria Olivaw','Olá, somos a Olivaw.',True)

  def test_enrichment_preview_and_apply_preserve_human_fields_and_existing_links(self):
    website = Mock()
    website.analyze.return_value = {'status':'checked','publicEmails':[{'email':'new@example.test','sourceUrl':'https://example.test'}],
      'contactPage':'https://example.test/contato','contactFormUrl':'','contactFormStatus':'not-detected',
      'contactCheckedAt':'2026-11-07T12:00:00Z'}
    service = LeadEnrichment(self.crm, website)
    original = deepcopy(self.crm.data)
    self.assertEqual('preview', service.run(limit=3)['items'][0]['status'])
    self.assertEqual(original, self.crm.data)
    self.assertEqual('enriched', service.run([LEAD],3,True)['items'][0]['status'])
    qualification = self.crm.data['snapshot']['qualification']
    self.assertEqual('Texto humano', qualification['approach'])
    self.assertEqual('reviewed', qualification['approachStatus'])
    self.assertEqual('https://example.test/revisado', qualification['contactFormUrl'])
    self.assertEqual('found', qualification['contactFormStatus'])
    self.assertEqual('new', self.crm.data['followUpStatus'])

  def test_inconclusive_does_not_erase_contacts(self):
    website = Mock(); website.analyze.return_value = {'status':'inconclusive'}
    original = deepcopy(self.crm.data)
    self.assertEqual('inconclusive', LeadEnrichment(self.crm,website).run([LEAD],1,True)['items'][0]['status'])
    self.assertEqual(original['snapshot']['email'], self.crm.data['snapshot']['email'])
    self.assertEqual(original['snapshot']['qualification']['approach'], self.crm.data['snapshot']['qualification']['approach'])
    self.assertEqual(original['snapshot']['qualification']['contactFormUrl'], self.crm.data['snapshot']['qualification']['contactFormUrl'])

  def test_preparation_requires_approval_and_known_recipient(self):
    with self.assertRaises(ValueError): self.service.prepare(LEAD,'contact@example.test','Oi','Oi')
    with self.assertRaises(ValueError): self.service.prepare(LEAD,'other@example.test','Oi','Oi',True)
    self.assertEqual([], self.gmail.sent)
    self.assertEqual([], self.store.intents())

  def test_new_lead_with_null_communications_can_be_prepared_without_sending(self):
    self.crm.data['snapshot']['communications'] = None
    prepared = self.prepare()
    self.assertEqual('prepared',prepared['state'])
    self.assertEqual(['prepared'],[entry['kind'] for entry in self.crm.events])
    self.assertEqual([],self.gmail.sent)

  def test_outbound_state_and_optout_block_approach(self):
    for changes in ({'followUpStatus':'delivered'},{'followUpStatus':'discontinued'}):
      self.crm.data.update(changes)
      with self.assertRaises(ValueError): self.prepare()
    self.crm.data['followUpStatus'] = 'new'
    self.crm.data['snapshot']['acquisition']['initiative'] = 'inbound'
    with self.assertRaises(ValueError): self.prepare()
    self.crm.data['snapshot']['acquisition']['initiative'] = 'outbound'
    self.crm.data['snapshot']['qualification']['tags'] = ['opt-out']
    with self.assertRaises(ValueError): self.prepare()

  def test_initial_approval_send_and_sync_are_idempotent(self):
    prepared = self.prepare()
    self.assertEqual('prepared', self.crm.events[0]['kind'])
    self.assertEqual([], self.gmail.sent)
    with self.assertRaises(ValueError): self.service.send(prepared['id'])
    self.assertEqual('sent', self.service.send(prepared['id'],True)['state'])
    self.service.send(prepared['id'],True)
    self.service.sync()
    self.assertEqual(['prepared','sending','sent'],[event['kind'] for event in self.crm.events])
    self.assertEqual('conversation',self.crm.data['followUpStatus'])
    self.assertEqual(1,len(self.gmail.sent))

  def test_direct_test_cli_cannot_send_a_commercial_intent(self):
    prepared = self.prepare()
    with self.assertRaises(ValueError): GmailPilot(self.store,self.gmail).send(prepared['id'],True)
    self.assertEqual([],self.gmail.sent)

  def test_timeout_and_reservation_crash_reconcile_without_resend(self):
    prepared = self.prepare(); self.gmail.timeout = True
    self.assertEqual('confirmation-pending', self.service.send(prepared['id'],True)['state'])
    self.assertEqual('sent', self.service.reconcile(prepared['id'])['state'])
    self.service.send(prepared['id'],True)
    self.assertEqual(1,len(self.gmail.sent))

  def test_crm_reservation_blocks_another_process_even_if_local_state_lost(self):
    prepared = self.prepare()
    self.service.reserve(self.store.get(prepared['id']))
    with self.assertRaises(ValueError): self.service.send(prepared['id'],True)
    self.assertEqual([],self.gmail.sent)
    self.assertEqual('confirmation-pending', self.service.reconcile(prepared['id'])['state'])
    self.assertEqual([],self.gmail.sent)

  def test_crm_failure_after_send_is_recoverable_by_sync(self):
    prepared = self.prepare(); self.crm.fail = 'sent'
    with self.assertRaises(ValueError): self.service.send(prepared['id'],True)
    self.assertEqual(1,len(self.gmail.sent))
    self.crm.fail = None; self.service.sync()
    self.assertEqual('sent',self.crm.events[-1]['kind'])
    self.assertEqual(1,len(self.gmail.sent))

  def test_incoming_not_acknowledged_before_crm_and_reply_requires_approval(self):
    prepared = self.prepare(); self.service.send(prepared['id'],True)
    self.gmail.messages['reply1'] = remote_message('reply1')
    self.crm.fail = 'received'
    with self.assertRaises(ValueError): self.service.sync()
    self.assertFalse((self.store.root/'received.json').exists())
    self.crm.fail = None
    self.assertEqual('received',self.service.sync()['items'][0]['status'])
    self.assertEqual([],self.service.sync()['items'])
    with self.assertRaises(ValueError): self.service.prepare(LEAD,'contact@example.test',None,'Resposta',False,'reply1')
    reply = self.service.prepare(LEAD,'contact@example.test',None,'Resposta revisada',True,'reply1')
    self.service.send(reply['id'],True)
    self.assertEqual(2,len(self.gmail.sent))
    self.assertEqual('thread1',self.gmail.sent[-1]['threadId'])

  def test_human_takeover_without_new_incoming_is_persisted_and_blocks_reply(self):
    prepared = self.prepare(); self.service.send(prepared['id'],True)
    self.gmail.messages['reply1'] = remote_message('reply1')
    self.service.sync()
    self.gmail.messages['human'] = remote_message('human','olivaw@example.test',labels=['SENT'])
    self.service.sync()
    self.assertEqual('human-takeover',self.crm.events[-1]['kind'])
    with self.assertRaises(ValueError): self.service.prepare(LEAD,'contact@example.test',None,'Resposta',True,'reply1')

  def test_test_inbox_consultation_does_not_consume_crm_reply(self):
    prepared = self.prepare(); self.service.send(prepared['id'],True)
    self.gmail.messages['reply1'] = remote_message('reply1')
    GmailPilot(self.store,self.gmail).replies()
    self.assertEqual('received',self.service.sync()['items'][0]['status'])

  def test_pilot_approval_limits_and_same_email_duplicate_are_blocked(self):
    self.store.approve(LEAD,'contact@example.test')
    with self.assertRaises(ValueError): self.store.approve('00000000-0000-0000-0000-000000000002','contact@example.test')
    for number in range(2,6):
      self.store.approve('00000000-0000-0000-0000-%012d' % number, 'contact%d@example.test' % number)
    with self.assertRaises(ValueError): self.store.approve('00000000-0000-0000-0000-000000000006','six@example.test')

  def test_unchanged_canonical_approach_reuses_intent_but_changed_text_is_rejected(self):
    first = self.prepare()
    self.assertEqual(first['id'],self.prepare()['id'])
    with self.assertRaises(ValueError): self.service.prepare(LEAD,'contact@example.test','Outro assunto','Outro texto',True)
    self.assertEqual(1,len(self.store.intents()))

  def test_prepared_reply_is_blocked_when_human_intervenes_before_send(self):
    prepared = self.prepare(); self.service.send(prepared['id'],True)
    self.gmail.messages['reply1'] = remote_message('reply1'); self.service.sync()
    reply = self.service.prepare(LEAD,'contact@example.test',None,'Resposta revisada',True,'reply1')
    self.gmail.messages['human'] = remote_message('human','olivaw@example.test',labels=['SENT'])
    with self.assertRaises(ValueError): self.service.send(reply['id'],True)
    self.assertEqual(1,len(self.gmail.sent))
