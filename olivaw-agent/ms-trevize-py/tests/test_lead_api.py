import io
import json
from pathlib import Path
import unittest
from unittest.mock import Mock
from ms_trevize.data.lead_api import LeadApi,ApiError
from ms_trevize.domain.candidate import Candidate


class ApiTests(unittest.TestCase):
  def test_already_reviewed_qualification_is_not_overwritten(self):
    api=LeadApi({},Path('.'));api.token='fixture'
    api.request=Mock(return_value={'snapshot':{'revision':4,'qualification':{'approach':'Texto humano'}}})
    candidate=Candidate('a','Empresa','Eventos','','','','https://example.test')
    self.assertEqual('already-registered',api.publish(candidate,{'approach':'Novo texto'}))
    api.request.assert_called_once()

  def test_new_lead_uses_stable_id_then_revision_and_confirms_delivery(self):
    api=LeadApi({},Path('.'));api.token='fixture'
    candidate=Candidate('a','Empresa','Eventos','','(41) 99999-0000','','https://example.test')
    api.request=Mock(side_effect=[ApiError(404),{'items':[]},{'externalId':candidate.external_id},
      {'snapshot':{'revision':1}},{'revision':2},{'snapshot':{'qualification':{'approach':'Rascunho'}}}])
    self.assertEqual('delivered',api.publish(candidate,{'approach':'Rascunho'}))
    create=api.request.call_args_list[2].args[1]
    self.assertEqual(candidate.external_id,create['externalId']);self.assertEqual('',create['whatsapp'])
    update=api.request.call_args_list[4]
    self.assertEqual(1,update.args[1]['revision']);self.assertEqual('PUT',update.args[2])

  def test_ambiguous_same_name_requires_review_instead_of_merging(self):
    api=LeadApi({},Path('.'));api.token='fixture'
    api.request=Mock(side_effect=[ApiError(404),{'items':[{'business':'Empresa'}]}])
    self.assertEqual('possible-duplicate-review',api.publish(Candidate('a','Empresa','Eventos','','','',''),{}))
    self.assertEqual(2,api.request.call_count)

  def test_missing_qualification_route_blocks_before_collection_or_create(self):
    api=LeadApi({},Path('.'));api.login=Mock()
    api.request=Mock(side_effect=ApiError(404))
    with self.assertRaises(ValueError):api.preflight()
    api.request.assert_called_once_with('/leads/00000000-0000-0000-0000-000000000000/qualification',method='OPTIONS')
    api.request.side_effect=ApiError(405,'PUT')
    api.preflight()

  def test_primary_email_is_created_and_contact_evidence_requires_readback(self):
    candidate=Candidate('a','Empresa','Eventos','','','','https://example.test')
    qualification={'publicEmails':[{'email':'contato@example.test','sourceUrl':'https://example.test/contato'}],
      'contactFormStatus':'found','contactFormUrl':'https://example.test/contato'}
    api=LeadApi({},Path('.'));api.token='fixture'
    api.request=Mock(side_effect=[ApiError(404),{'items':[]},{},
      {'snapshot':{'revision':1}},{'revision':2},{'snapshot':{'qualification':qualification}}])
    self.assertEqual('delivered',api.publish(candidate,qualification))
    self.assertEqual('contato@example.test',api.request.call_args_list[2].args[1]['email'])
    api.request=Mock(side_effect=[{'snapshot':{'revision':1}},{'revision':2},
      {'snapshot':{'qualification':{'contactFormUrl':qualification['contactFormUrl']}}}])
    with self.assertRaises(ValueError):api.publish(candidate,qualification)
