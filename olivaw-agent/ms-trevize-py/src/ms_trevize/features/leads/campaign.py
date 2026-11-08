"""Campanha local explícita; ativa somente após aprovação, sem autostart ou envio por padrão."""
from datetime import datetime, timezone, timedelta
from uuid import UUID, uuid4

from ms_trevize.data.gmail_store import private_json
from ms_trevize.data.local_runtime import atomic_json, exclusive_lock, now
from ms_trevize.domain.mail import address, build_message


class Campaign:
  def __init__(self, store, outreach, settings):
    self.store, self.outreach, self.settings = store, outreach, settings
    self.path = store.root / "campaign.json"

  def plan(self, identities, subject, body, approved=False, automatic_replies=False):
    if not approved: raise ValueError("O lote e o roteiro precisam de --approve.")
    identities = list(dict.fromkeys(str(UUID(value)) for value in identities))
    if not 1 <= len(identities) <= 5: raise ValueError("Piloto limitado a 1–5 leads selecionados.")
    leads, recipients = [], set()
    for identity in identities:
      lead = self.outreach.lead(identity); snapshot = lead["snapshot"]
      emails = (snapshot.get("qualification") or {}).get("publicEmails", [])
      recipient = snapshot.get("email") or (emails[0]["email"] if len(emails) == 1 else "")
      recipient = address(recipient)
      self.outreach.eligible(lead, recipient)
      if recipient in recipients: raise ValueError("Lote possui destinatário duplicado.")
      recipients.add(recipient)
      business = snapshot.get("business", "")[:120]
      item = {"leadId":identity,"to":recipient,"subject":subject.replace("{business}",business),
        "body":body.replace("{business}",business)}
      if "{approach}" in item["body"]:
        qualification = snapshot.get("qualification") or {}
        if qualification.get("approachStatus") != "reviewed" or not qualification.get("approach"):
          raise ValueError("Abordagem individual precisa estar revisada na ficha antes de aprovar o lote.")
        item["body"] = item["body"].replace("{approach}",qualification["approach"])
      build_message({**item,"sender":self.store.config()["sender"],"mimeId":"<validation@trevize.invalid>"})
      leads.append(item)
    with self.store.lock():
      if self.path.exists(): raise ValueError("Já existe um plano neste diretório. Não substituir o histórico de campanha.")
      plan = {"id":str(uuid4()),"active":False,"approvedAt":now(),"leads":leads,
        "policy":{"automaticReplies":bool(automatic_replies),"maxTurns":self.settings["maxTurns"],"windowHours":self.settings["windowHours"]}}
      atomic_json(self.path, plan)
    return {"id":plan["id"],"active":False,"leads":leads,"policy":plan["policy"],"sendingEnabled":False}

  def activate(self, approved=False):
    if not approved: raise ValueError("Ativação exige --approve do lote congelado.")
    with self.store.lock():
      plan = private_json(self.path)
      plan.update(active=True, activatedAt=now())
      atomic_json(self.path,plan)
    return self.status()

  def stop(self):
    with self.store.lock():
      plan = private_json(self.path); plan.update(active=False,stoppedAt=now()); atomic_json(self.path,plan)
    return self.status()

  def status(self):
    plan = private_json(self.path) if self.path.exists() else {}
    counts = {}
    for intent in self.store.intents(): counts[intent["state"]] = counts.get(intent["state"],0)+1
    return {"planned":bool(plan),"active":plan.get("active",False),"id":plan.get("id"),"intents":counts,
      "sendingEnabled":self.settings["sendingEnabled"] and (self.store.config().get("sendingEnabled",False) if (self.store.root/"config.json").exists() else False),
      "automaticReplies":plan.get("policy",{}).get("automaticReplies",False)}

  def run(self):
    self.store.prepare()
    with exclusive_lock(self.store.root/"campaign.lock"):
      plan = private_json(self.path) if self.path.exists() else {}
      if not self.settings["enabled"] or not plan.get("active"):
        return {"status":"paused","reason":"campaign-disabled","sent":0}
      summary = {"status":"completed","sent":0,"prepared":0,"drafts":0,"paused":0,"failures":0}
      can_send = self.settings["sendingEnabled"] and self.store.config().get("sendingEnabled",False)
      attempted = 0
      for item in plan["leads"]:
        existing = [intent for intent in self.store.intents() if intent["content"].get("leadId") == item["leadId"] and not intent["content"].get("replyMessageId")]
        if existing and not any(intent["state"] == "prepared" for intent in existing): continue
        if existing and not can_send: continue
        if attempted >= self.settings["maxPerCycle"]: break
        attempted += 1
        try:
          prepared = self.outreach.prepare(item["leadId"],item["to"],item["subject"],item["body"],True)
          intent = self.store.get(prepared["id"])
          if intent["state"] != "prepared": continue
          summary["prepared"] += 1
          if can_send:
            result = self.outreach.send(intent["id"],True)
            summary["sent"] += result["state"] == "sent"
        except (OSError, ValueError, KeyError, TypeError): summary["failures"] += 1
      # Leitura pode ser habilitada separadamente; ausência de token não inicia consentimento no worker.
      try: self.outreach.sync(self.settings["maxPerCycle"])
      except (OSError, ValueError, KeyError, TypeError):
        summary["paused"] += 1
        summary["status"] = "partial"
        return summary
      if not plan["policy"]["automaticReplies"]: return summary
      remaining = self.settings["maxPerCycle"]-attempted
      attempted = 0
      for decision in self.outreach.decisions():
        if decision["classification"] != "qualify" or decision.get("campaignPaused"): continue
        roster = next((item for item in plan["leads"] if item["leadId"] == decision["leadId"]), None)
        if not roster: continue
        related = [item for item in self.store.intents() if item["content"].get("leadId") == decision["leadId"]]
        if any(item["content"].get("replyMessageId") == decision["messageId"] and item["state"] != "prepared" for item in related): continue
        if attempted >= remaining: break
        attempted += 1
        initial = next((item for item in related if not item["content"].get("replyMessageId") and item["state"] == "sent"), None)
        if not initial: continue
        turns = sum(bool(item["content"].get("replyMessageId")) and item["state"] in ("sent","confirmation-pending") for item in related)
        start = datetime.fromisoformat(initial.get("confirmedAt",initial["createdAt"]))
        limit = min(plan["policy"]["maxTurns"],self.settings["maxTurns"])
        window = min(plan["policy"]["windowHours"],self.settings["windowHours"])
        if turns >= limit or datetime.now(timezone.utc) > start+timedelta(hours=window):
          self.outreach.record(initial,"handoff",{"id":decision["messageId"],"threadId":decision["threadId"],
            "subject":initial["content"]["subject"],"from":roster["to"],"body":"Limite de turnos ou janela de atendimento atingido; encaminhar ao operador."})
          summary["paused"] += 1
          continue
        summary["drafts"] += 1
        # Texto livre contextual é revisável; o lote antigo não aprova respostas novas da IA.
        if decision.get("requiresReview"): continue
        if not can_send: continue
        try:
          prepared = self.outreach.prepare(decision["leadId"],roster["to"],None,decision["reply"],True,
            decision["messageId"],decision["questionId"])
          result = self.outreach.send(prepared["id"],True)
          summary["sent"] += result["state"] == "sent" and not result.get("repeated",False)
        except (OSError, ValueError, KeyError, TypeError):
          from ms_trevize.features.leads.outreach import digest
          with self.store.lock():
            decision["campaignPaused"] = True
            atomic_json(self.store.root/("decision-"+digest([decision["leadId"],decision["messageId"]])+".json"),decision)
          summary["failures"] += 1
      return summary
