from copy import deepcopy
from pathlib import Path
from tempfile import TemporaryDirectory
import unittest
from unittest.mock import Mock

from ms_trevize.features.leads.followup import FollowUpSpecialist


def message(identity, sender):
  return {'id':identity,'threadId':'thread1','labelIds':['INBOX'],'payload':{'headers':[
    {'name':'From','value':sender},{'name':'To','value':'olivaw@example.test'},
    {'name':'Subject','value':'Re: Site'},{'name':'Message-ID','value':'<'+identity+'@example.test>'}]}}


class Store:
  def __init__(self, root):
    self.root, self.items, self.enabled = root, [], True
  def prepare(self): self.root.mkdir(exist_ok=True)
  def config(self): return {'sendingEnabled':self.enabled}
  def intents(self): return deepcopy(self.items)
  def get(self, identity): return deepcopy(next(item for item in self.items if item['id'] == identity))


class FollowUpTests(unittest.TestCase):
  def setUp(self):
    self.temp = TemporaryDirectory(); self.addCleanup(self.temp.cleanup)
    self.store = Store(Path(self.temp.name))
    self.settings = {'enabled':True,'sendingEnabled':True,'aiEnabled':True,'automaticReplies':True}
    self.outreach = Mock()
    self.decisions, self.latest, self.sent = [], {}, []
    self.outreach.decisions.side_effect = lambda:deepcopy(self.decisions)
    self.outreach.gmail.thread.side_effect = lambda identity:{'messages':[self.latest[identity]]}
    self.outreach.lead.return_value = {'snapshot':{}}
    self.outreach.prepare.side_effect = self.prepare
    self.outreach.send.side_effect = self.send
    self.outreach.sync.return_value = {'items':[]}
    self.worker = FollowUpSpecialist(self.outreach,self.store,self.settings)

  def add(self, count):
    for number in range(count):
      identity, thread, email = 'm'+str(number),'thread'+str(number),'contact'+str(number)+'@example.test'
      lead = '00000000-0000-0000-0000-'+str(number+1).zfill(12)
      self.store.items.append({'id':'root'+str(number),'state':'sent','threadId':thread,
        'content':{'leadId':lead,'to':email,'body':'Primeiro contato'}})
      self.decisions.append({'intentId':'root'+str(number),'leadId':lead,'threadId':thread,'messageId':identity,
        'classification':'qualify','questionId':'need','reply':'Qual serviço quer publicar primeiro?',
        'requiresReview':True})
      self.latest[thread] = {**message(identity,email),'threadId':thread}
    return self.decisions

  def prepare(self, lead, email, subject, body, approved, reply, question):
    self.assertTrue(approved)
    identity = 'reply'+reply
    if not any(item['id'] == identity for item in self.store.items):
      self.store.items.append({'id':identity,'state':'prepared',
        'content':{'leadId':lead,'to':email,'body':body,'replyMessageId':reply}})
    return {'id':identity}

  def send(self, identity, approved):
    self.assertTrue(approved)
    current = next(item for item in self.store.items if item['id'] == identity)
    current['state'] = 'sent'
    self.sent.append(identity)
    return {'id':identity,'state':'sent'}

  def test_disabled_policy_has_no_external_calls(self):
    self.settings['enabled'] = False
    self.assertEqual('paused',self.worker.run()['status'])
    self.outreach.sync.assert_not_called()

  def test_reading_remains_enabled_but_no_automatic_reply_without_explicit_policy_and_both_gates(self):
    self.add(1)
    for disabled in ('automaticReplies','sendingEnabled','aiEnabled'):
      with self.subTest(disabled=disabled):
        self.settings[disabled] = False
        result = self.worker.run()
        self.assertEqual(1,result['pending'])
        self.assertEqual('disabled',result['sendStatus'])
        self.settings[disabled] = True
    self.store.enabled = False
    self.assertEqual(0,self.worker.run()['sent'])
    self.assertEqual(4,self.outreach.sync.call_count)
    self.outreach.prepare.assert_not_called()

  def test_drains_over_a_hundred_messages_and_responds_without_initial_contact_batch_cap(self):
    self.add(106)
    self.outreach.sync.side_effect = [{'items':[{'id':'m'+str(i),'status':'received'} for i in range(100)]},
      {'items':[{'id':'m'+str(i),'status':'received'} for i in range(100,106)]}]
    result = self.worker.run()
    self.assertEqual(106,result['read'])
    self.assertEqual(106,result['sent'])
    self.assertEqual(2,self.outreach.sync.call_count)
    self.assertEqual(106,len(self.sent))
    self.assertEqual(0,result['remaining'])
    self.outreach.sync.assert_called_with(limit=100,stopping=self.worker.stopping)

  def test_sent_pending_and_manually_cancelled_replies_are_never_sent_again(self):
    self.add(3)
    for number,state in enumerate(('sent','confirmation-pending','cancelled')):
      self.store.items.append({'id':'reply'+str(number),'state':state,
        'content':{'leadId':self.decisions[number]['leadId'],'to':'contact'+str(number)+'@example.test','replyMessageId':'m'+str(number)}})
    result = self.worker.run()
    self.assertEqual(0,result['sent'])
    self.assertEqual(1,result['pending'])
    self.assertEqual(2,result['skipped'])
    self.outreach.prepare.assert_not_called()
    self.outreach.send.assert_not_called()

  def test_only_latest_received_message_is_answered_and_other_sender_is_not_contacted(self):
    self.add(2)
    old = {**self.decisions[0],'messageId':'old'}
    self.decisions.insert(0,old)
    self.latest['thread1'] = {**message('m1','stranger@example.test'),'threadId':'thread1'}
    result = self.worker.run()
    self.assertEqual(1,result['sent'])
    self.assertEqual(['replym0'],self.sent)
    self.assertEqual(2,result['skipped'])
    self.assertEqual(2,self.outreach.gmail.thread.call_count)

  def test_handoff_optout_uncertain_and_reprocess_decisions_never_authorize_a_reply(self):
    self.add(5)
    for number,classification in enumerate(('handoff','optout','uncertain')):
      self.decisions[number]['classification'] = classification
    self.decisions[3]['reprocess'] = True
    self.decisions[4]['campaignPaused'] = True
    self.outreach.sync.return_value = {'items':[{'id':'m0','status':'received'},{'id':'m1','status':'received'},{'id':'m2','status':'received'}]}
    result = self.worker.run()
    self.assertEqual(3,result['handoffs'])
    self.assertEqual(0,result['drafts'])
    self.outreach.prepare.assert_not_called()

  def test_live_lead_pause_is_respected_and_not_automatically_resumed(self):
    self.add(1)
    self.outreach.eligible.side_effect = ValueError('Conversa assumida pelo operador.')
    result = self.worker.run()
    self.assertEqual('partial',result['status'])
    self.assertEqual(1,result['pending'])
    self.outreach.prepare.assert_not_called()
    self.outreach.resume.assert_not_called()

  def test_unknown_local_intent_or_wrong_lead_does_not_authorize_an_email(self):
    self.add(1)
    self.decisions[0]['intentId'] = 'unknown'
    self.assertEqual(0,self.worker.run()['sent'])
    self.outreach.prepare.assert_not_called()

  def test_transport_ambiguity_is_preserved_and_next_round_does_not_retry(self):
    self.add(1)
    def ambiguous(identity, approved):
      current = next(item for item in self.store.items if item['id']==identity)
      current['state'] = 'confirmation-pending'
      return {'state':'confirmation-pending'}
    self.outreach.send.side_effect = ambiguous
    self.assertEqual(1,self.worker.run()['pending'])
    self.assertEqual(1,self.worker.run()['pending'])
    self.assertEqual(1,self.outreach.send.call_count)

  def test_stop_between_drafts_keeps_remaining_visible(self):
    self.add(6)
    stopping = [False]
    self.worker.stopping = lambda:stopping[0]
    def send_and_stop(identity, approved):
      result = self.send(identity,approved)
      stopping[0] = True
      return result
    self.outreach.send.side_effect = send_and_stop
    result = self.worker.run()
    self.assertEqual('interrupted',result['status'])
    self.assertTrue(result['interrupted'])
    self.assertEqual(1,result['sent'])
    self.assertEqual(5,result['remaining'])

  def test_failed_sync_or_nonprogressing_batches_never_send_from_stale_decisions(self):
    self.add(1)
    self.outreach.sync.side_effect = ValueError('CRM fora do ar.')
    self.assertEqual('partial',self.worker.run()['status'])
    self.outreach.send.assert_not_called()
    batch = {'items':[{'id':'same'} for _ in range(100)]}
    self.outreach.sync.side_effect = None
    self.outreach.sync.return_value = batch
    self.assertEqual('partial',self.worker.run()['status'])
    self.outreach.send.assert_not_called()

  def test_one_lead_failure_does_not_block_other_known_replies(self):
    self.add(2)
    original = self.prepare
    def fail_one(*args):
      if args[0] == self.decisions[0]['leadId']: raise ValueError('Conflito CRM.')
      return original(*args)
    self.outreach.prepare.side_effect = fail_one
    result = self.worker.run()
    self.assertEqual(1,result['failures'])
    self.assertEqual(1,result['sent'])
    self.assertEqual('partial',result['status'])

  def test_stop_during_sync_does_not_claim_an_empty_backlog_or_send_stale_drafts(self):
    self.add(2)
    stopping = [False]
    self.worker.stopping = lambda:stopping[0]
    def stop_sync(**args):
      stopping[0] = True
      return {'items':[]}
    self.outreach.sync.side_effect = stop_sync
    result = self.worker.run()
    self.assertEqual('interrupted',result['status'])
    self.assertTrue(result['remainingUnknown'])
    self.outreach.send.assert_not_called()
