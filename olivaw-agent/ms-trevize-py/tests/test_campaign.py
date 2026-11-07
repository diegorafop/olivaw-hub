from datetime import datetime, timezone, timedelta
import json
import subprocess
import unittest
from unittest.mock import Mock, patch

import test_lead_workflow as fixtures
from ms_trevize.data.gmail_client import GoogleGmail, GmailError
from ms_trevize.data.local_runtime import atomic_json
from ms_trevize.data.reply_advisor import ReplyAdvisor, QUESTIONS, classify, current_text
from ms_trevize.features.leads.campaign import Campaign
from test_gmail import remote_message


class CampaignTests(unittest.TestCase):
  setUp = fixtures.WorkflowTests.setUp
  prepare = fixtures.WorkflowTests.prepare

  def campaign(self, sending=False):
    self.settings = {'enabled':True,'sendingEnabled':sending,'maxTurns':1,'windowHours':72,'maxPerCycle':3}
    service = Campaign(self.store,self.service,self.settings)
    service.plan([fixtures.LEAD],'Parceria {business}','Olá, {business}. Somos da Olivaw.',True,True)
    service.activate(True)
    return service

  def test_default_transport_and_local_gate_cannot_send(self):
    client = Mock()
    with self.assertRaises(GmailError): GoogleGmail(client).send({'raw':'fixture'})
    client.users.return_value.messages.return_value.send.assert_not_called()
    intent = self.prepare()
    config = self.store.config(); config['sendingEnabled'] = False
    atomic_json(self.store.root/'config.json',config)
    with self.assertRaises(ValueError): self.service.send(intent['id'],True)
    self.assertEqual([],self.gmail.sent)
    self.assertEqual(['prepared'],[event['kind'] for event in self.crm.events])

  def test_campaign_approvals_and_disabled_sending_prepare_without_sending(self):
    campaign = self.campaign()
    result = campaign.run()
    self.assertEqual(0,result['sent'])
    self.assertEqual(1,result['prepared'])
    self.assertEqual([],self.gmail.sent)
    campaign.stop()
    self.assertEqual('paused',campaign.run()['status'])

  def test_campaign_requires_separate_approval_and_never_overwrites_plan(self):
    settings = {'enabled':False,'sendingEnabled':False,'maxTurns':1,'windowHours':72,'maxPerCycle':3}
    campaign = Campaign(self.store,self.service,settings)
    with self.assertRaises(ValueError): campaign.plan([fixtures.LEAD],'Oi','Oi')
    campaign.plan([fixtures.LEAD],'Oi','Oi',True)
    with self.assertRaises(ValueError): campaign.activate()
    with self.assertRaises(ValueError): campaign.plan([fixtures.LEAD],'Outro','Outro',True)
    self.assertEqual('paused',campaign.run()['status'])

  def test_cancel_and_revision_do_not_send_or_reuse_cancelled_intent(self):
    first = self.prepare()
    self.service.cancel(first['id']); self.service.cancel(first['id'])
    second = self.service.prepare(fixtures.LEAD,'contact@example.test','Assunto revisado','Texto revisado',True)
    self.assertNotEqual(first['id'],second['id'])
    self.assertEqual('cancelled',self.store.get(first['id'])['state'])
    self.assertEqual([],self.gmail.sent)
    self.service.send(second['id'],True)
    with self.assertRaises(ValueError): self.service.cancel(second['id'])

  def test_campaign_cycle_and_reply_turn_limit_with_fake_transport_only(self):
    campaign = self.campaign(True)
    self.assertEqual(1,campaign.run()['sent'])
    self.assertEqual(0,campaign.run()['sent'])
    self.gmail.messages['reply1'] = remote_message('reply1')
    self.assertEqual(1,campaign.run()['sent'])
    self.gmail.messages['reply2'] = remote_message('reply2')
    self.advisor.analyze.return_value = {'classification':'qualify','questionId':'current-system','summary':'Sistema atual.'}
    self.assertEqual(0,campaign.run()['sent'])
    self.assertEqual(2,len(self.gmail.sent))

  def test_campaign_time_window_blocks_old_thread_response(self):
    campaign = self.campaign(True); campaign.run()
    intent = self.store.intents()[0]
    intent['confirmedAt'] = (datetime.now(timezone.utc)-timedelta(hours=100)).isoformat()
    self.store.save(intent)
    self.gmail.messages['reply1'] = remote_message('reply1')
    self.assertEqual(0,campaign.run()['sent'])
    self.assertEqual(1,len(self.gmail.sent))

  def test_refusal_and_quote_requests_suspend_automation(self):
    self.service.advisor = ReplyAdvisor({'enabled':False})
    first = self.prepare(); self.service.send(first['id'],True)
    message = remote_message('reply1')
    import base64
    message['payload']['body']['data'] = base64.urlsafe_b64encode('Não temos interesse, remova nosso contato.'.encode()).decode()
    self.gmail.messages['reply1'] = message
    self.service.sync()
    self.assertEqual('suppressed',self.crm.events[-1]['kind'])
    with self.assertRaises(ValueError): self.service.prepare(fixtures.LEAD,'contact@example.test',None,'Oferta',True,'reply1')
    self.assertEqual('handoff',classify('Pode enviar um orçamento?')['classification'])

  def test_handoff_on_model_failure_is_durable_and_not_retried(self):
    first = self.prepare(); self.service.send(first['id'],True)
    self.advisor.analyze.side_effect = ValueError('SECRET')
    self.gmail.messages['reply1'] = remote_message('reply1')
    self.service.sync(); self.service.sync()
    self.assertEqual(1,self.advisor.analyze.call_count)
    self.assertEqual('handoff',self.service.decisions()[0]['classification'])
    self.assertNotIn('SECRET',json.dumps(self.service.decisions()))

  def test_ai_only_selects_approved_question_and_cannot_execute_tools(self):
    decision = {'classification':'qualify','questionId':'need','summary':'Entender a operação.'}
    runner = Mock(return_value=subprocess.CompletedProcess([],0,json.dumps({'type':'item.completed','item':{'type':'agent_message','text':json.dumps(decision)}})))
    advisor = ReplyAdvisor({'enabled':True,'timeoutSeconds':30},runner)
    result = advisor.analyze({'body':'Quero entender como funciona.','bodyAvailable':True},{'business':'Empresa'},set())
    self.assertEqual('need',result['questionId'])
    self.assertIn('shell_tool',runner.call_args.args[0])
    self.assertIn('read-only',runner.call_args.args[0])
    self.assertNotIn('reply',result)
    self.assertIn('necessidade',QUESTIONS[result['questionId']])
    self.assertEqual('handoff',advisor.analyze({'body':'Oi','bodyAvailable':True},{}, {'need'})['classification'])
    runner.return_value = subprocess.CompletedProcess([],0,json.dumps({'type':'item.completed','item':{'type':'command_execution'}}))
    with self.assertRaises(ValueError): advisor.analyze({'body':'Oi','bodyAvailable':True},{},set())

  def test_quote_footer_is_not_mistaken_for_a_new_refusal(self):
    self.assertIsNone(classify(current_text('Quero conhecer.\n> unsubscribe\n> Não tenho interesse')))
    self.assertIsNone(classify(current_text('Quero conhecer.\nEm sexta-feira escreveu:\nNão tenho interesse')))

  def test_disabled_worker_path_has_no_google_crm_or_ai_calls(self):
    from ms_trevize.bootstrap import run_outreach
    with patch('ms_trevize.bootstrap.LocalRuntime') as runtime, patch('ms_trevize.bootstrap.LeadApi') as api:
      runtime.return_value.settings = {'outreach':{'enabled':False}}
      self.assertEqual('outreach-disabled',run_outreach('unused')['reason'])
      api.assert_not_called()
