from copy import deepcopy
from datetime import datetime, timezone, timedelta
from pathlib import Path
from tempfile import TemporaryDirectory
from unittest.mock import Mock, patch
import unittest

from ms_trevize.features.leads.dispatch import OutboundDispatcher

NOW = datetime(2026,11,8,16,0,tzinfo=timezone.utc)
FIRST = '00000000-0000-0000-0000-000000000010'
SECOND = '00000000-0000-0000-0000-000000000020'


def lead(identity=FIRST,recipient='contact@example.test'):
  return {'externalId':identity,'createdAt':'2026-11-01T12:00:00Z','followUpStatus':'new','snapshot':{
    'entry':'prospecting','business':'Eventos Aurora','email':recipient,
    'acquisition':{'initiative':'outbound','origin':'Google Maps'},
    'qualification':{'schemaVersion':1,'websiteUrl':'https://www.example.test','tags':[],
      'publicEmails':[{'email':recipient,'sourceUrl':'https://example.test/contato'}],
      'approachStatus':'draft','approach':'Olá, vi a apresentação dos seus serviços. Faz sentido modernizar o site?',
      'commercialObservation':'Projetos corporativos.',
      'websiteAssessment':{'basis':'html','assessedAt':'2026-11-08T12:00:00Z','metrics':{}}}}}


class FakeStore:
  def __init__(self,root): self.root=Path(root);self.rows=[];self.enabled=True
  def prepare(self): self.root.mkdir(exist_ok=True)
  def config(self): return {'sendingEnabled':self.enabled}
  def intents(self): return deepcopy(self.rows)
  def get(self,identity): return next(item for item in self.rows if item['id']==identity)


class DispatchTests(unittest.TestCase):
  def setUp(self):
    self.directory=TemporaryDirectory();self.addCleanup(self.directory.cleanup)
    self.store=FakeStore(self.directory.name)
    self.leads={FIRST:lead(),SECOND:lead(SECOND,'second@example.test')}
    self.crm=Mock()
    self.crm.request.side_effect=lambda path:{'items':[{'externalId':identity,'createdAt':item['createdAt']} for identity,item in self.leads.items()]}
    self.outreach=Mock()
    self.outreach.lead.side_effect=lambda identity:deepcopy(self.leads[identity])
    def prepare(identity,recipient,subject,body,approved):
      value={'id':identity,'state':'prepared','content':{'leadId':identity,'to':recipient,'subject':subject,'body':body}}
      self.store.rows.append(value)
      return deepcopy(value)
    def send(identity,confirmed):
      value=self.store.get(identity);value.update(state='sent',attemptedAt=self.clock.isoformat())
      return {'state':'sent'}
    self.outreach.prepare.side_effect=prepare;self.outreach.send.side_effect=send
    self.clock=NOW
    self.settings={'enabled':True,'sendingEnabled':True,'automaticInitials':True}
    self.service=OutboundDispatcher(self.crm,self.store,self.outreach,settings=self.settings,clock=lambda:self.clock)

  def test_one_initial_every_thirty_minutes_survives_recreation_and_does_not_count_replies(self):
    first=self.service.run()
    self.assertEqual(1,first['sent']);self.assertEqual(FIRST,first['leadId'])
    self.assertEqual('initial-rate-limit',self.service.run()['reason'])
    self.assertEqual(1,self.outreach.prepare.call_count)
    self.clock=NOW+timedelta(minutes=30)
    self.store.rows.append({'id':'reply','state':'sent','attemptedAt':self.clock.isoformat(),
      'content':{'leadId':FIRST,'to':'contact@example.test','replyMessageId':'received1'}})
    restarted=OutboundDispatcher(self.crm,self.store,self.outreach,settings=self.settings,clock=lambda:self.clock)
    self.assertEqual(1,restarted.run()['sent'])
    self.assertEqual(SECOND,self.outreach.prepare.call_args.args[0])
    self.assertEqual(2,self.outreach.send.call_count)

  def test_uncertain_initial_counts_toward_rate_and_is_never_resent(self):
    self.store.rows=[{'id':'uncertain','state':'confirmation-pending','attemptedAt':(NOW-timedelta(minutes=5)).isoformat(),
      'content':{'leadId':FIRST,'to':'contact@example.test'}}]
    self.assertEqual('initial-rate-limit',self.service.run()['reason'])
    self.outreach.prepare.assert_not_called();self.crm.request.assert_not_called()
    self.clock=NOW+timedelta(minutes=30)
    self.assertEqual(SECOND,self.service.run()['leadId'])
    self.assertEqual(SECOND,self.outreach.prepare.call_args.args[0])

  def test_two_rolling_hour_attempts_are_capped_even_if_spaced_too_close_previously(self):
    self.store.rows=[{'id':str(i),'state':'sent','attemptedAt':(NOW-timedelta(minutes=minutes)).isoformat(),
      'content':{'leadId':str(i),'to':str(i)+'@example.test'}} for i,minutes in enumerate((45,35))]
    result=self.service.run()
    self.assertEqual('initial-rate-limit',result['reason'])
    self.assertEqual((NOW+timedelta(minutes=15)).isoformat(),result['nextInitialAt'])
    self.crm.request.assert_not_called()

  def test_gates_never_fetch_leads_or_generate_ai(self):
    for setting in ('enabled','sendingEnabled','automaticInitials'):
      self.settings[setting]=False
      self.assertEqual('paused',self.service.run()['status'])
      self.settings[setting]=True
    self.store.enabled=False
    self.assertEqual('gmail-sending-disabled',self.service.run()['reason'])
    self.crm.request.assert_not_called();self.outreach.prepare.assert_not_called()

  def test_one_address_cannot_be_reused_via_another_lead_or_cancelled_crm_preparation(self):
    self.leads[SECOND]['snapshot']['email']='contact@example.test'
    self.leads[SECOND]['snapshot']['qualification']['publicEmails'][0]['email']='contact@example.test'
    self.store.rows=[{'id':'sent','state':'sent','attemptedAt':(NOW-timedelta(hours=2)).isoformat(),
      'content':{'leadId':FIRST,'to':'contact@example.test'}}]
    self.assertEqual('idle',self.service.run()['status'])
    self.outreach.prepare.assert_not_called()

  def test_missing_ambiguous_foreign_or_simulated_public_contact_is_skipped(self):
    changes=[lambda q:q.update(publicEmails=[]),
      lambda q:q['publicEmails'].append({'email':'other@example.test','sourceUrl':'https://example.test'}),
      lambda q:q['publicEmails'][0].update(sourceUrl='https://other.test'),
      lambda q:q.update(tags=['opt-out']),
      lambda q:q['websiteAssessment'].update(basis='simulation')]
    self.leads.pop(SECOND)
    for change in changes:
      self.leads[FIRST]=lead();change(self.leads[FIRST]['snapshot']['qualification'])
      self.assertEqual('idle',self.service.run()['status'])
    self.outreach.prepare.assert_not_called()

  def test_incomplete_research_is_reserved_and_not_repeated_every_poll(self):
    self.leads.pop(SECOND)
    self.leads[FIRST]['snapshot']['qualification'].pop('websiteAssessment')
    self.service.website=Mock();self.service.ai=Mock()
    with patch('ms_trevize.features.leads.dispatch.LeadResearch') as research:
      self.assertEqual('idle',self.service.run()['status'])
      self.assertEqual('idle',self.service.run()['status'])
      research.return_value.run.assert_called_once_with(FIRST,apply=True)
    self.outreach.prepare.assert_not_called()

  def test_research_ready_draft_is_used_and_send_failure_never_selects_another_lead(self):
    self.outreach.send.side_effect=ValueError('PRIVATE error')
    result=self.service.run()
    self.assertEqual('partial',result['status'])
    self.assertNotIn('PRIVATE',str(result))
    self.assertEqual(1,self.outreach.prepare.call_count)
    self.assertEqual(self.leads[FIRST]['snapshot']['qualification']['approach'],self.outreach.prepare.call_args.args[3])
    self.assertIs(True,self.outreach.prepare.call_args.args[4])

  def test_unknown_attempt_time_blocks_initials_for_review(self):
    self.store.rows=[{'id':'old','state':'confirmation-pending','content':{'leadId':FIRST,'to':'contact@example.test'}}]
    self.assertEqual('initial-attempt-time-unknown',self.service.run()['reason'])
    self.crm.request.assert_not_called()

  def test_daily_transport_budget_is_checked_before_research_or_preparation(self):
    self.store.config=lambda:{'sendingEnabled':True,'dailyLimit':1}
    self.store.rows=[{'id':'reply','state':'sent','attemptedAt':(NOW-timedelta(hours=2)).isoformat(),
      'content':{'leadId':FIRST,'to':'contact@example.test','replyMessageId':'received1'}}]
    self.assertEqual('daily-sending-limit',self.service.run()['reason'])
    self.crm.request.assert_not_called(); self.outreach.prepare.assert_not_called()
