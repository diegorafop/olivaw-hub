import unittest
from unittest.mock import Mock
from ms_trevize.data.website_review import WebsiteReview
from ms_trevize.domain.candidate import Candidate
from ms_trevize.features.prospecting.commands.create_prospect_batch import qualification_for


class ContactDiscoveryTests(unittest.TestCase):
  def review(self, home, pages=None, rules=""):
    pages = pages or {}
    def fetch(url, *_):
      if url.endswith('/robots.txt'): return 200, 'text/plain', rules, url
      if url == 'https://example.test/': return 200, 'text/html', home, url
      if url in pages:
        value = pages[url]
        if isinstance(value, Exception): raise value
        return 200, 'text/html', value, url
      raise ValueError('Unexpected request')
    self.fetch = Mock(side_effect=fetch)
    return WebsiteReview({'enabled':True, 'timeoutSeconds':1, 'maxBytes':65536}, self.fetch).analyze('https://example.test/')

  def test_discovers_mailto_text_and_real_contact_form_with_sources(self):
    result = self.review('<a href="mailto:COMERCIAL@example.test?subject=Oi">Email</a><a href="/fale">Fale conosco</a>',
      {'https://example.test/fale':'<p>comercial@example.test e equipe@example.test.</p><form><input type="email"><textarea name="mensagem"></textarea></form>'})
    self.assertEqual([{'email':'comercial@example.test','sourceUrl':'https://example.test/'},
      {'email':'equipe@example.test','sourceUrl':'https://example.test/fale'}], result['publicEmails'])
    self.assertEqual('found', result['contactFormStatus'])
    self.assertEqual('https://example.test/fale', result['contactFormUrl'])
    self.assertEqual('https://example.test/fale', result['contactPage'])
    candidate = Candidate('a','Empresa','Eventos','','','','https://maps.example.test')
    qualification = qualification_for(candidate, result, {'search':{'segment':'Eventos','region':'Curitiba'}}, {})
    self.assertEqual(result['publicEmails'], qualification['publicEmails'])
    self.assertEqual(result['contactFormUrl'], qualification['contactFormUrl'])
    self.assertEqual(result['contactCheckedAt'], qualification['contactCheckedAt'])

  def test_search_newsletter_and_login_forms_are_not_contact_forms(self):
    result = self.review('<form><input name="search"></form><form><input type="email"></form>'
      '<form><input type="hidden" name="email"><input type="hidden" name="message"></form>'
      '<form><input type="password"><input type="email"><textarea></textarea></form>'
      '<script>secret@example.test</script><p>logo@2x.png</p>')
    self.assertTrue(result['hasForm'])
    self.assertEqual('not-detected', result['contactFormStatus'])
    self.assertEqual('', result['contactFormUrl'])
    self.assertEqual([], result['publicEmails'])

  def test_robots_and_failed_contact_page_preserve_home_email_with_uncertainty(self):
    for rules in ('User-agent: *\nDisallow: /contato', ''):
      with self.subTest(rules=rules):
        result = self.review('<p>oi@example.test</p><a href="/contato">Contato</a>',
          {'https://example.test/contato':TimeoutError()}, rules)
        self.assertEqual('checked', result['status'])
        self.assertEqual('inconclusive', result['contactFormStatus'])
        self.assertEqual('oi@example.test', result['publicEmails'][0]['email'])
        self.assertEqual(2 if rules else 3, self.fetch.call_count)

  def test_limits_pages_and_emails_and_never_follows_external_contact_links(self):
    home = '<a href="https://external.test/contact">Contato</a><a href="javascript:contact()">Contato</a>'
    home += ''.join('<a href="/contato%d">Contato</a>' % number for number in range(10))
    home += ' '.join('e%d@example.test' % number for number in range(10))
    result = self.review(home, {'https://example.test/contato%d' % number:'<p>Contato</p>' for number in range(3)})
    self.assertEqual(5, len(result['publicEmails']))
    self.assertEqual(5, self.fetch.call_count)
    self.assertTrue(all(call.args[0].startswith('https://example.test/') for call in self.fetch.call_args_list))

  def test_unchecked_does_not_assert_absence(self):
    candidate = Candidate('a','Empresa','Eventos','','','','')
    qualification = qualification_for(candidate, {'status':'not-checked'}, {'search':{'segment':'Eventos','region':'Curitiba'}}, {})
    self.assertEqual('not-checked', qualification['contactFormStatus'])
    self.assertIsNone(qualification['contactCheckedAt'])
