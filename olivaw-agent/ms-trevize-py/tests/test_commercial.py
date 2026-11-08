from copy import deepcopy
import json
from pathlib import Path
import subprocess
import unittest
from unittest.mock import Mock

from ms_trevize.data.reply_advisor import ReplyAdvisor
from ms_trevize.domain.commercial import assessment_for, contact_source, opening, strategy_for
from ms_trevize.features.leads.research import LeadResearch, EXAMPLE_ID
import test_lead_workflow as workflow


class CommercialTests(unittest.TestCase):
  def sample(self):
    return json.loads((Path(__file__).resolve().parents[1]/'examples/empresa-exemplo-assessment.json').read_text())

  def test_medium_ui_and_ux_select_identity_without_fabricating_research_or_exposing_scores(self):
    assessment = self.sample()
    self.assertEqual('ui',strategy_for(assessment))
    text = opening('Empresa Exemplo',{'websiteAssessment':assessment},'operator@example.test')
    self.assertIn('identidade',text)
    self.assertNotIn('5/10',text)
    self.assertNotIn('cores ruins',text)
    self.assertNotIn('encontramos este e-mail',text.lower())
    self.assertNotIn('conhecer o site',text)
    self.assertIsNone(assessment['metrics']['management']['score'])

  def test_html_can_score_quoted_content_but_cannot_infer_visual_or_internal_management(self):
    ratings = {key:{'score':5,'evidence':'Serviços e projetos'} for key in ('modernity','content','ui','ux','management')}
    assessment = assessment_for(ratings,{'status':'checked','url':'https://example.test','publicText':'Serviços e projetos'})
    self.assertEqual(5,assessment['metrics']['content']['score'])
    self.assertEqual('https://example.test',assessment['metrics']['content']['sourceUrl'])
    for key in ('modernity','ui','ux','management'): self.assertIsNone(assessment['metrics'][key]['score'])
    self.assertIsNone(assessment_for(ratings,{'status':'inconclusive'})['metrics']['content']['score'])
    ratings['content']['evidence'] = 'Texto que não existe'
    self.assertIsNone(assessment_for(ratings,{'status':'checked','publicText':'Serviços'})['metrics']['content']['score'])

  def test_origin_only_matches_actual_recipient_and_safe_url(self):
    qualification = {'publicEmails':[{'email':'contact@example.test','sourceUrl':'https://example.test/contato'}]}
    self.assertEqual('https://example.test/contato',contact_source(qualification,'CONTACT@example.test'))
    self.assertEqual('',contact_source(qualification,'other@example.test'))
    qualification['publicEmails'][0]['sourceUrl'] = 'https://user:pass@example.test'
    self.assertEqual('',contact_source(qualification,'contact@example.test'))

  def test_origin_question_answers_known_source_without_ai_but_unknown_requires_review(self):
    runner = Mock()
    advisor = ReplyAdvisor({'enabled':False},runner)
    lead = {'email':'contact@example.test','qualification':{'publicEmails':[{'email':'contact@example.test','sourceUrl':'https://example.test/contato'}]}}
    message = {'body':'Como encontraram meu e-mail?','bodyAvailable':True}
    decision = advisor.analyze(message,lead,set())
    self.assertEqual('qualify',decision['classification'])
    self.assertIn('https://example.test/contato',decision['reply'])
    self.assertTrue(decision['requiresReview'])
    self.assertEqual('neutral',decision['interestSignal'])
    self.assertEqual('handoff',advisor.analyze(message,{},set())['classification'])
    runner.assert_not_called()

  def test_ai_reply_is_reviewable_and_unsupported_interest_does_not_heat_lead(self):
    decision = {'classification':'qualify','questionId':'current-system','summary':'Entender como publica serviços.',
      'reply':'Vocês conseguem atualizar os serviços hoje?','interestSignal':'positive','interestEvidence':'Interesse inventado'}
    runner = Mock(return_value=subprocess.CompletedProcess([],0,json.dumps({'type':'item.completed','item':{'type':'agent_message','text':json.dumps(decision)}})))
    advisor = ReplyAdvisor({'enabled':True,'timeoutSeconds':10},runner,skill='Não depreciar sites.')
    result = advisor.analyze({'body':'Do que se trata?','bodyAvailable':True},{'business':'Exemplo'},set())
    self.assertEqual('neutral',result['interestSignal'])
    self.assertTrue(result['requiresReview'])
    self.assertIn('Não depreciar sites.',runner.call_args.kwargs['input'])
    self.assertEqual('handoff',advisor.analyze({'body':'Do que se trata?','bodyAvailable':True},{}, {'current-system'})['classification'])

  def test_complaint_is_not_hot_and_explicit_commercial_request_goes_to_operator(self):
    advisor = ReplyAdvisor({'enabled':False})
    complaint = advisor.analyze({'body':'Tenho uma reclamação sobre esse contato.','bodyAvailable':True},{},set())
    self.assertEqual('handoff',complaint['classification'])
    self.assertNotEqual('ready',complaint.get('interestSignal'))
    proposal = advisor.analyze({'body':'Quero um orçamento para meu site.','bodyAvailable':True},{},set())
    self.assertEqual('handoff',proposal['classification'])
    self.assertEqual('ready',proposal['interestSignal'])
    negative = advisor.analyze({'body':'Não preciso de orçamento agora.','bodyAvailable':True},{},set())
    self.assertNotEqual('ready',negative.get('interestSignal'))

  def test_simulated_research_preview_has_no_mutation_and_refuses_real_leads(self):
    crm = workflow.FakeCRM(); crm.data['snapshot']['business'] = 'Empresa Exemplo — TESTE INTERNO'
    before = deepcopy(crm.data)
    website, ai = Mock(), Mock()
    result = LeadResearch(crm,website,ai).run(EXAMPLE_ID,simulation=self.sample())
    self.assertEqual('preview',result['status']); self.assertFalse(result['contactPerformed'])
    self.assertEqual(before,crm.data)
    website.analyze.assert_not_called(); ai.analyze.assert_not_called()
    with self.assertRaises(ValueError): LeadResearch(crm,website,ai).run(workflow.LEAD,simulation=self.sample())
    with self.assertRaises(ValueError): LeadResearch(crm,website,ai).run(EXAMPLE_ID,True,simulation=self.sample())
    self.assertEqual(before,crm.data)  # API antiga bloqueada antes de qualquer PUT.

  def test_research_preserves_reviewed_draft_and_contacts_after_confirmation(self):
    crm = workflow.FakeCRM()
    original = crm.request
    crm.request = lambda path,payload=None,method=None: {'commercialResearchVersion':1} if path == '/leads?pageSize=1' else original(path,payload,method)
    crm.data['snapshot']['business'] = 'Empresa Exemplo — TESTE INTERNO'
    result = LeadResearch(crm,Mock(),Mock()).run(EXAMPLE_ID,True,simulation=self.sample())
    self.assertEqual('saved',result['status'])
    self.assertEqual('Texto humano',crm.data['snapshot']['qualification']['approach'])
    self.assertEqual('contact@example.test',crm.data['snapshot']['email'])
    self.assertEqual('new',crm.data['followUpStatus'])
    self.assertFalse(crm.events)

  def test_human_assessment_is_preserved_without_spending_new_ai_call(self):
    crm = workflow.FakeCRM()
    crm.data['snapshot']['qualification']['websiteAssessment'] = {**self.sample(),'basis':'manual'}
    website, ai = Mock(), Mock()
    result = LeadResearch(crm,website,ai).run(workflow.LEAD)
    self.assertEqual('human-assessment-preserved',result['status'])
    website.analyze.assert_not_called(); ai.analyze.assert_not_called()

  def test_confirmed_provided_contact_uses_normal_advisor_without_inventing_public_source(self):
    decision = {'classification':'qualify','questionId':'purpose','summary':'Origem informada pelo operador.',
      'reply':'Seu contato foi fornecido à equipe Olivaw. Como o site atende o negócio hoje?',
      'interestSignal':'neutral','interestEvidence':'Como conheceram meu e-mail?'}
    runner = Mock(return_value=subprocess.CompletedProcess([],0,json.dumps({'type':'item.completed','item':{'type':'agent_message','text':json.dumps(decision)}})))
    advisor = ReplyAdvisor({'enabled':True,'timeoutSeconds':10},runner)
    message = {'body':'Como conheceram meu e-mail?','bodyAvailable':True}
    lead = {'qualification':{'tags':['contact-provided'],'confidence':'confirmed','facts':'Fornecido à equipe pelo operador.'}}
    result = advisor.analyze(message,lead,set())
    self.assertEqual('qualify',result['classification'])
    self.assertTrue(result['requiresReview'])
    self.assertIn('Contato fornecido',runner.call_args.kwargs['input'])
    self.assertNotIn('site público',result['reply'])
    lead['qualification']['confidence'] = 'pending'
    self.assertEqual('handoff',advisor.analyze(message,lead,set())['classification'])
    self.assertEqual(1,runner.call_count)
