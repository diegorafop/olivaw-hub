import unittest
from ms_trevize.domain.presentation import initial_presentation, GREETING, SERVICE


class PresentationTests(unittest.TestCase):
  def test_cached_old_draft_is_reformatted_without_name_or_lost_business_observation(self):
    old='Olá! Sou Trevize, do atendimento da Olivaw. Vi no site o portfólio de eventos. Encontrei este e-mail na página de vocês.\n\nA Olivaw ajuda empresas a atualizar seus sites. Faz sentido modernizar o site?'
    result=initial_presentation(old)
    self.assertTrue(result.startswith(GREETING+'\n\n'+SERVICE))
    self.assertNotIn('Trevize',result)
    self.assertIn('portfólio de eventos',result)
    self.assertIn('Encontrei este e-mail',result)
    self.assertTrue(result.endswith('Faz sentido modernizar o site?'))
    self.assertEqual(result,initial_presentation(result))
    self.assertTrue(all(len(p.split())<35 for p in result.split('\n\n')))

  def test_missing_context_or_excessive_copy_never_becomes_a_new_initial(self):
    for text in ('Olá! Sou Trevize, do atendimento da Olivaw.', 'Uma ideia '+('extensa '*150)):
      with self.assertRaises(ValueError):initial_presentation(text)
