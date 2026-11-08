"""Apresentação institucional curta, aplicada também a rascunhos já preparados."""
import re

GREETING = 'Olá, somos da equipe Olivaw.'
SERVICE = 'Somos uma empresa de TI que desenvolve sistemas e cuida da infraestrutura para mantê-los funcionando.'


def initial_presentation(body):
  text = body.replace('\r\n','\n').replace('\r','\n').strip()
  sentences = re.split(r'(?<=[.!?])\s+|\n\s*\n',text)
  content = []
  for sentence in sentences:
    sentence = sentence.strip()
    if not sentence: continue
    # Retira apenas apresentações antigas, preservando as observações e a pergunta.
    if re.match(r'^(?:ol[aá][!,]?\s*(?:pessoal|equipe)|sou\s+(?:o\s+)?trevize\b|aqui\s+[eé]\s+(?:o\s+)?trevize\b|me chamo\s+trevize\b)',sentence,re.I): continue
    if re.fullmatch(r'ol[aá][!.]?',sentence,re.I): continue
    sentence = re.sub(r'^ol[aá][!,]\s*','',sentence,flags=re.I)
    if re.match(r'^(?:sou\s+(?:o\s+)?trevize\b|me chamo\s+trevize\b)',sentence,re.I): continue
    sentence = re.sub(r'^Vi\b','Vimos',sentence,flags=re.I)
    if re.match(r'^(?:somos (?:da|a|uma)|a Olivaw (?:ajuda|desenvolve|cria|cuida)|desenvolvemos (?:sites|sistemas))\b',sentence,re.I): continue
    if sentence == SERVICE: continue
    content.append(sentence)
  result = '\n\n'.join([GREETING,SERVICE,*content])
  if re.search(r'\btrevize\b',result,re.I): raise ValueError('Revisar apresentação: nome interno no texto comercial.')
  if len(result.split())>130: raise ValueError('Apresentação extensa; revisar antes de enviar.')
  if not content: raise ValueError('Apresentação sem contexto ou pergunta comercial.')
  return result
