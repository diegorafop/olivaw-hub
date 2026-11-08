"""Pesquisa comercial revisável; não toca canais, etapas, mensagens nem campanhas."""
from copy import deepcopy
from datetime import datetime, timezone
from uuid import UUID
from types import SimpleNamespace

from ms_trevize.domain.commercial import assessment_for, opening, strategy_for

EXAMPLE_ID = "1178e525-487f-58d8-a3bc-624338770a8b"


class LeadResearch:
  def __init__(self, api, website, ai):
    self.api, self.website, self.ai = api, website, ai

  def run(self, identity, apply=False, simulation=None):
    identity = str(UUID(identity))
    current = self.api.request("/leads/" + identity)
    snapshot = current.get("snapshot") or {}
    qualification = deepcopy(snapshot.get("qualification") or {})
    if qualification.get("schemaVersion") != 1: raise ValueError("Qualificação existente é necessária.")
    if apply and self.api.request("/leads?pageSize=1").get("commercialResearchVersion") != 1:
      raise ValueError("Publique a API v1.1.3 antes de salvar o diagnóstico.")
    if simulation is not None:
      if identity != EXAMPLE_ID or "TESTE" not in snapshot.get("business", "").upper():
        raise ValueError("Simulação permitida somente no lead fictício do teste controlado.")
      if simulation.get("basis") != "simulation": raise ValueError("Exemplo precisa declarar simulação.")
      assessment = deepcopy(simulation)
      assessment["assessedAt"] = datetime.now(timezone.utc).isoformat()
      assessment["strategy"] = strategy_for(assessment)
      qualification["websiteAssessment"] = assessment
      qualification["commercialObservation"] = ""  # Nenhuma visita real foi realizada.
    else:
      existing = qualification.get("websiteAssessment")
      if existing and existing.get("basis") in ("manual","conversation"):
        return {"leadId":identity,"status":"human-assessment-preserved","contactPerformed":False,"applied":False}
      evidence = self.website.analyze(qualification.get("websiteUrl", ""))
      if evidence.get("status") != "checked":
        return {"leadId":identity,"status":"inconclusive","contactPerformed":False,"applied":False}
      # Dados públicos já cadastrados; sem nova coleta de leads ou alteração de contatos.
      candidate = SimpleNamespace(name=snapshot.get("business", ""),category=qualification.get("segment", ""))
      analysis = self.ai.analyze(candidate,evidence,qualification.get("region", ""))
      qualification["websiteAssessment"] = analysis.get("websiteAssessment") or assessment_for({},evidence)
      qualification["restrictedAreaUrl"] = evidence.get("restrictedAreaUrl", "")
      qualification["commercialObservation"] = analysis.get("commercialObservation", "")
    if qualification.get("approachStatus") != "reviewed":
      qualification["approach"] = (analysis.get("approach") if simulation is None else "") or opening(snapshot.get("business", ""),qualification,snapshot.get("email", ""))
      qualification["approachStatus"] = "draft"
    if apply:
      updated = self.api.request("/leads/"+identity+"/qualification",{"revision":snapshot["revision"],"qualification":qualification},"PUT")
      if updated.get("conflict"): raise ValueError("Ficha alterada; revisar antes de repetir.")
      saved = self.api.request("/leads/"+identity).get("snapshot",{}).get("qualification") or {}
      for key in ("metrics","basis","strategy"):
        if saved.get("websiteAssessment",{}).get(key) != qualification["websiteAssessment"].get(key):
          raise ValueError("Diagnóstico não confirmado; conferir versão da API e revisão.")
    return {"leadId":identity,"status":"saved" if apply else "preview","qualification":qualification,"applied":apply,"contactPerformed":False}
