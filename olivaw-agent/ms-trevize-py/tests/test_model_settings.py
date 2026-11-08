import json
from pathlib import Path
import subprocess
from tempfile import TemporaryDirectory
from types import SimpleNamespace
import unittest
from unittest.mock import Mock

from ms_trevize.configuration import load_settings
from ms_trevize.data.ai_qualification import AIQualification
from ms_trevize.data.reply_advisor import ReplyAdvisor
from ms_trevize.data.gmail_store import GmailStore, private_json
from ms_trevize.data.local_runtime import atomic_json

ROOT=Path(__file__).resolve().parents[1]


class ModelSettingsTests(unittest.TestCase):
  def runner(self, payload):
    return Mock(return_value=subprocess.CompletedProcess([],0,json.dumps({'type':'item.completed','item':{'type':'agent_message','text':json.dumps(payload)}})))

  def test_collection_and_conversation_pin_sol_and_low_without_global_config(self):
    settings=load_settings(ROOT/'setting.json')['ai']
    self.assertEqual('gpt-6.1-sol',settings['model']);self.assertEqual('low',settings['reasoningEffort'])
    qualification={'positives':'','observations':'','questions':'','approach':'Rascunho','commercialObservation':'',
      'ratings':{key:{'score':None,'evidence':''} for key in ('modernity','content','ui','ux','management')}}
    first=self.runner(qualification)
    AIQualification(settings,'Sem ferramentas.',first).analyze(SimpleNamespace(name='Exemplo',category='Serviços'),{},'')
    second=self.runner({'classification':'qualify','questionId':'need','summary':'Entender publicação.',
      'reply':'Como vocês publicam os serviços no site?','interestSignal':'neutral','interestEvidence':''})
    ReplyAdvisor(settings,second).analyze({'bodyAvailable':True,'body':'O que vocês oferecem?'},{},set())
    for runner in (first,second):
      args=runner.call_args.args[0]
      self.assertEqual('gpt-6.1-sol',args[args.index('--model')+1])
      self.assertIn('model_reasoning_effort="low"',args)
      self.assertIn('--ignore-user-config',args)
      self.assertNotIn('model_reasoning_effort',runner.call_args.kwargs['env'])

  def test_invalid_reasoning_and_non_boolean_authorization_fail_before_execution(self):
    with TemporaryDirectory() as directory:
      path=Path(directory)/'setting.json';source=json.loads((ROOT/'setting.json').read_text())
      source['ai']['reasoningEffort']='unlimited';path.write_text(json.dumps(source))
      with self.assertRaises(ValueError):load_settings(path)
      source['ai']['reasoningEffort']='low';source['outreach']['automaticReplies']='true';path.write_text(json.dumps(source))
      with self.assertRaises(ValueError):load_settings(path)

  def test_production_policy_does_not_silently_promote_controlled_config(self):
    with TemporaryDirectory() as directory:
      store=GmailStore(Path(directory)/'gmail');store.prepare()
      base={'version':1,'sender':'olivaw@example.test','testRecipients':['operator@example.test'],'dailyLimit':5,'sendingEnabled':False}
      atomic_json(store.root/'config.json',base)
      self.assertEqual(5,store.config()['dailyLimit'])
      atomic_json(store.root/'config.json',{**base,'mode':'commercial','dailyLimit':500})
      with self.assertRaises(ValueError):store.config()
      atomic_json(store.root/'config.json',{**base,'mode':'commercial','dailyLimit':500,'policyApprovedAt':'2026-11-08T00:00:00Z'})
      self.assertEqual(500,store.config()['dailyLimit'])
      atomic_json(store.root/'received.json',{'seen':['a'*40]*4000})
      self.assertEqual(4000,len(private_json(store.root/'received.json')['seen']))
      atomic_json(store.root/'token.json',{'unexpected':'a'*150000})
      with self.assertRaises(ValueError):private_json(store.root/'token.json')

  def test_due_followup_factory_uses_real_cancellation_adapter(self):
    from ms_trevize.bootstrap import run_followup
    from unittest.mock import patch
    runtime=SimpleNamespace(state=Path('/unused-state'),settings={'outreach':{'enabled':True}})
    service,store=Mock(),Mock()
    settings={'outreach':{'enabled':True}}
    with patch('ms_trevize.bootstrap.commercial_context',return_value=(runtime,settings,store,None,service,'')), patch('ms_trevize.features.leads.followup.FollowUpSpecialist') as worker:
      worker.return_value.run.return_value={'status':'completed'}
      self.assertEqual('completed',run_followup('unused')['status'])
      self.assertTrue(callable(worker.call_args.kwargs['stopping']))
