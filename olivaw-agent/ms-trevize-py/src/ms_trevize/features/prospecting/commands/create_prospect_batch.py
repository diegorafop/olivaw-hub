"""Um lote serial: descobrir, qualificar e confirmar entrega, sem contato comercial."""
from datetime import datetime, timezone
import time
from ms_trevize.core.prospecting import BatchState, Collector, LeadStore
from ms_trevize.domain.candidate import web_url


def qualification_for(candidate, evidence, settings, ai):
  checked = evidence.get("status") == "checked"
  presence = "unknown"
  if not candidate.website: presence = "not-located"
  elif evidence.get("status") == "social-only": presence = "social-only"
  elif checked: presence = "website"
  facts = "Atividade e contato exibidos na fonte: " + candidate.category + ". Telefone não confirma WhatsApp."
  positives, observations = "", "Presença digital pendente de revisão individual."
  if checked:
    facts += " Leitura HTML da página retornou HTTP 200. Título: " + evidence.get("title", "")
    positives = "Página respondeu à leitura e apresenta um título." if evidence.get("title") else "Página respondeu à leitura."
    checks = []
    if not evidence.get("hasDescription"): checks.append("Meta description não localizada no HTML lido.")
    if not evidence.get("hasViewport"): checks.append("Meta viewport não localizada; experiência móvel ainda precisa de teste visual.")
    if evidence.get("imagesWithoutAlt"): checks.append("Há imagens sem texto alternativo no HTML; verificar quais são decorativas.")
    observations = " ".join(checks) or "Nenhum problema relevante confirmado nesta leitura básica."
  questions = "O site e a tecnologia atendem à operação? Há rotinas em planilhas que gostariam de facilitar?"
  approach = "Olá, pessoal da " + candidate.name + ". Somos da Olivaw, uma nova empresa de desenvolvimento de sistemas. Estamos conhecendo empresas de eventos da região de Curitiba para construir parcerias. Hoje, a tecnologia atende bem à rotina de vocês ou existe algo que gostariam de facilitar?"
  if ai.get("status") == "completed":
    positives, observations, questions, approach = (ai[key] for key in ("positives","observations","questions","approach"))
  source = "Fonte: " + candidate.source + ". Verificado em " + datetime.now(timezone.utc).isoformat()
  return {"schemaVersion":1,"segment":candidate.category or settings["search"]["segment"],"region":settings["search"]["region"],
    "websiteUrl":candidate.website,"phone":candidate.phone,"whatsapp":evidence.get("whatsapp","") if checked else "",
    "contactFormUrl":web_url(evidence.get("url","")) if evidence.get("hasForm") else "",
    "sourceUrl":candidate.source,"presence":presence,"priority":"unassessed","confidence":"pending",
    "tags":["eventos","prospecção","revisão pendente"],"portfolioReference":"LTMJ — avaliar pertinência",
    "facts":(facts+" "+source)[:1500],"positives":positives[:1500],
    "observations":(observations+" Limite: "+evidence.get("limitations",evidence.get("reason","verificação incompleta")))[:1500],
    "questions":questions[:1500],"approach":approach[:3000],"approachStatus":"draft"}


class CollectionFailure(RuntimeError):
  """Código e etapa seguros; nunca serializar a mensagem da dependência."""
  def __init__(self, phase):
    super().__init__("collection-failed")
    self.phase = phase


class CreateProspectBatchHandler:
  def __init__(self, runtime: BatchState, collector: Collector, website, ai, api: LeadStore):
    self.runtime, self.collector, self.website, self.ai, self.api = runtime, collector, website, ai, api

  def handle(self):
    runtime, settings = self.runtime, self.runtime.settings
    runtime.prepare()
    seen = runtime.seen()
    deadline = time.monotonic() + settings["execution"]["timeoutSeconds"]
    report = {"schemaVersion":1,"agentId":"trevize","startedAt":datetime.now(timezone.utc).isoformat(),
              "target":settings["search"]["limit"],"collected":0,"delivered":0,"duplicates":0,"failed":0,"aiCompleted":0,"aiFailed":0,"items":[]}
    if not settings["execution"]["enabled"]: return {**report,"status":"disabled"}
    if settings["leadApi"]["enabled"]:
      try:
        self.api.preflight()
      except Exception as error:
        raise CollectionFailure("api-preflight") from error
    try:
      candidates = self.collector.collect(set(seen))
    except Exception as error:
      raise CollectionFailure("maps-collection") from error
    report["collected"] = len(candidates)
    calls = 0
    for candidate in candidates:
      if time.monotonic() >= deadline - 100: break
      if runtime.stopping(): break
      evidence = self.website.analyze(candidate.website)
      result = {"status":"disabled"}
      if settings["ai"]["enabled"] and calls < settings["ai"]["maxCallsPerCycle"]:
        calls += 1
        try:
          result = self.ai.analyze(candidate, evidence, settings["search"]["region"])
          report["aiCompleted"] += result.get("status") == "completed"
        except (OSError, ValueError, KeyError, TimeoutError):
          report["aiFailed"] += 1
          result = {"status":"failed"}
      qualification = qualification_for(candidate,evidence,settings,result)
      outcome = "preview"
      if settings["leadApi"]["enabled"]:
        try:
          outcome=self.api.publish(candidate,qualification)
          if outcome=="delivered": report["delivered"]+=1
          else: report["duplicates"]+=1
          runtime.remember(seen,candidate.key)
        except (OSError, ValueError, KeyError):
          outcome="delivery-not-confirmed"; report["failed"]+=1
          report["items"].append({"externalId":candidate.external_id,"outcome":outcome,"analysis":evidence.get("status"),"ai":result.get("status")})
          break
      report["items"].append({"externalId":candidate.external_id,"outcome":outcome,"analysis":evidence.get("status"),"ai":result.get("status")})
    report["completedAt"]=datetime.now(timezone.utc).isoformat()
    report["status"]="completed" if report["failed"]==0 and len(report["items"])==report["target"] else "partial"
    report["contactPerformed"]=False
    # Só metadados e IDs locais; a ficha detalhada fica na API.
    runtime.save(report)
    return report
