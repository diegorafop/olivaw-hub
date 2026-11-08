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


def commercial_focus(qualification):
  """Transforma notas em pauta comercial, sem convertê-las em fatos sobre o cliente."""
  assessment = qualification.get("websiteAssessment") or {}
  metrics = assessment.get("metrics") or {}
  opportunities = [key for key in DIMENSIONS if type((metrics.get(key) or {}).get("score")) is int
    and 0 <= metrics[key]["score"] < 8 and metrics[key].get("confidence") != "unknown"]
  opportunities.sort(key=lambda key:metrics[key]["score"])
  primary = opportunities[0] if opportunities else "discovery"
  question = QUESTIONS[primary]
  if primary in ("ui","ux") and all(key in opportunities for key in ("ui","ux")):
    question = "Vocês têm interesse em modernizar a apresentação do site e facilitar o acesso aos serviços e ao contato pelo celular?"
  elif primary == "ui":
    question = "Faz sentido para vocês modernizar a apresentação do site para valorizar melhor os serviços da empresa?"
  elif primary == "ux":
    question = "Vocês têm interesse em tornar mais simples encontrar os serviços e pedir atendimento pelo celular no site?"
  elif primary == "content":
    question = "Seria útil conseguir atualizar serviços e novidades diretamente no site, com mais autonomia?"
  optional = "Se fizer sentido explorar operação, use uma situação concreta sustentada pelo contexto do negócio e confirme como é feita hoje. Não escolher agenda, planilha ou outra ferramenta apenas pelo segmento."
  sources = [metric.get("sourceUrl", "") for metric in metrics.values() if metric and metric.get("score") is not None and metric.get("sourceUrl")]
  observed = assessment.get("basis") in ("manual","html") and bool(sources)
  return {"primary":primary,"dimensions":opportunities,"question":question,
    "prioritizeWebsite":bool(opportunities),"optionalHypothesis":optional,
    "researchClaim":"Usar apenas as observações e fontes efetivamente registradas; análise limitada não é auditoria completa." if observed else "Sem inspeção real comprovada: propor evolução como hipótese, sem afirmar que visitamos ou analisamos o site."}


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
  focus = commercial_focus(qualification)
  source = contact_source(qualification, recipient) if recipient else ""
  greeting = "Olá, pessoal da " + business + "!" if business else "Olá!"
  # Evidência curta escolhida na revisão, sem transformar meta tags em elogio comercial.
  observation = qualification.get("commercialObservation", "").strip()[:400]
  context = "Ao conhecer o site de vocês, chamou nossa atenção: " + observation if observation else ""
  origin = "Encontramos este e-mail na página pública de contato: " + source + "." if source else ""
  question = focus["question"]
  return "\n\n".join(part for part in [greeting,context,
    "Somos da Olivaw. Criamos sites e sistemas para facilitar a apresentação e a rotina dos negócios.",
    origin,question,"Abraços,\nEquipe Olivaw"] if part)
