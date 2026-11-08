import unittest
from unittest.mock import Mock
from ms_trevize.data.email_domain import check_email_domain


class EmailDomainTests(unittest.TestCase):
  def test_nxdomain_and_null_mx_are_rejected_without_sending_or_more_queries(self):
    for answer in ({'Status':3},{'Status':0,'Answer':[{'type':15,'data':'0 .'}]}):
      fetch=Mock(return_value=answer)
      self.assertEqual('invalid',check_email_domain('contact@example.test',fetch)['status'])
      fetch.assert_called_once_with('example.test','MX')

  def test_mx_and_rfc_fallback_are_accepted_but_do_not_prove_mailbox(self):
    self.assertEqual('valid',check_email_domain('contact@example.test',Mock(return_value={'Status':0,'Answer':[{'type':15,'data':'10 mx.example.test.'}]}))['status'])
    fetch=Mock(side_effect=[{'Status':0},{'Status':0,'Answer':[{'type':1,'data':'203.0.113.1'}]}])
    self.assertEqual('smtp-address-fallback',check_email_domain('contact@example.test',fetch)['reason'])

  def test_temporary_dns_failure_is_unknown_not_a_permanent_invalid_contact(self):
    fetch=Mock(side_effect=TimeoutError())
    self.assertEqual('unknown',check_email_domain('contact@example.test',fetch)['status'])
