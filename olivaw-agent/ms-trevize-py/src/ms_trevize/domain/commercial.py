"""Diagnóstico interno e abordagem com origem verificável; nenhuma operação de envio."""
from datetime import datetime, timezone
from urllib.parse import urlsplit

DIMENSIONS = ("modernity", "content", "ui", "ux", "management")
QUESTIONS = {
  "modernity": "Vocês têm pensado em atualizar a apresentação do site para acompanhar a fase atual da empresa?",
  "content": "Hoje vocês conseguem atualizar os serviços e novidades do site com a facilidade que gostariam?",
  "ui": "A apresentação do site hoje transmite a identidade que vocês querem passar aos clientes?",
  "ux": "Quem chega pelo celular consegue encontrar o que precisa e entrar em contato do jeito que vocês gostariam?",
  "management": "Como vocês organizam as atualizações do site e os pedidos que chegam por ele hoje?",
  "discovery": "O site atende ao que vocês precisam hoje ou há algo que gostariam de facilitar?",
  "operations": "Há alguma rotina entre o contato pelo site e o atendimento que vocês gostariam de simplificar?",
}


def strategy_for(assessment):
  metrics = assessment.get("metrics", {})
  scored = [(key, item["score"]) for key,item in metrics.items()
    if key in DIMENSIONS and type(item.get("score")) is int]
  if not scored: return "discovery"
  weakest = min(scored,key=lambda item:item[1])
  return weakest[0] if weakest[1] < 8 else "operations"


def assessment_for(ratings, evidence):
  checked = evidence.get("status") == "checked"
  text = evidence.get("publicText", "")
  metrics = {}
  for key in DIMENSIONS:
    item = ratings.get(key, {})
    score, quote = item.get("score"), item.get("evidence", "")
    # A leitura HTML não observa aparência, navegação real ou operação interna.
    observable = key == "content" and checked and isinstance(quote,str) and bool(quote.strip()) and quote in text
    if not observable or type(score) is not int or not 0 <= score <= 10:
      metrics[key] = {"score":None,"evidence":"Não observado nesta leitura; confirmar em revisão ou conversa.","sourceUrl":"","confidence":"unknown"}
    else:
      metrics[key] = {"score":score,"evidence":quote[:400],"sourceUrl":evidence.get("url", ""),"confidence":"provisional"}
  result = {"schemaVersion":1,"assessedAt":datetime.now(timezone.utc).isoformat(),"basis":"html","metrics":metrics}
  result["strategy"] = strategy_for(result)
  return result


def contact_source(qualification, recipient):
  for item in qualification.get("publicEmails", []):
    if item.get("email", "").casefold() == recipient.casefold():
      url = item.get("sourceUrl", "")
      parsed = urlsplit(url)
      if parsed.scheme in ("http", "https") and parsed.hostname and not parsed.username and not parsed.password:
        return url
  return ""


def opening(business, qualification, recipient=""):
  assessment = qualification.get("websiteAssessment") or {}
  strategy = assessment.get("strategy", "discovery")
  source = contact_source(qualification, recipient) if recipient else ""
  greeting = "Olá, pessoal da " + business + "!" if business else "Olá!"
  # Evidência curta escolhida na revisão, sem transformar meta tags em elogio comercial.
  observation = qualification.get("commercialObservation", "").strip()[:400]
  context = "Ao conhecer o site de vocês, chamou nossa atenção: " + observation if observation else ""
  origin = "Encontramos este e-mail na página pública de contato: " + source + "." if source else ""
  question = QUESTIONS.get(strategy, QUESTIONS["discovery"])
  return "\n\n".join(part for part in [greeting,context,
    "Somos da Olivaw. Criamos sites e sistemas para facilitar a apresentação e a rotina dos negócios.",
    origin,question,"Abraços,\nEquipe Olivaw"] if part)
