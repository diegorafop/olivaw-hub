"""Piloto comercial: aprovação humana, reserva CRM e reconciliação sem reenvio."""
import hashlib
import json
from uuid import UUID

from ms_trevize.domain.mail import address, build_message
from ms_trevize.features.gmail.pilot import GmailPilot


def digest(value):
  return hashlib.sha256(json.dumps(value, sort_keys=True, ensure_ascii=True).encode()).hexdigest()


class LeadOutreach:
  def __init__(self, crm, store, gmail):
    self.crm, self.store, self.gmail = crm, store, gmail
    self.pilot = GmailPilot(store, gmail, self.reserve)

  def lead(self, identity):
    identity = str(UUID(identity))
    result = self.crm.request("/leads/" + identity)
    if not isinstance(result.get("snapshot"), dict): raise ValueError("Lead indisponível.")
    return result

  def eligible(self, lead, recipient):
    snapshot = lead["snapshot"]
    if (snapshot.get("acquisition") or {}).get("initiative") != "outbound":
      raise ValueError("Piloto comercial somente para leads outbound.")
    if lead.get("followUpStatus", "new") not in ("new", "conversation"):
      raise ValueError("Etapa exige intervenção humana.")
    qualification = snapshot.get("qualification") or {}
    tags = {item.casefold() for item in qualification.get("tags", [])}
    if tags & {"opt-out", "do-not-contact", "não contatar", "nao contatar"}:
      raise ValueError("Lead bloqueado para contato.")
    known = {item["email"].lower() for item in qualification.get("publicEmails", [])}
    if snapshot.get("email"): known.add(snapshot["email"].lower())
    if recipient not in known: raise ValueError("Destinatário não consta nos contatos do lead.")
    if any(event["kind"] in ("human-takeover", "suppressed", "handoff") for event in snapshot.get("communications", [])):
      raise ValueError("Conversa assumida por uma pessoa; revisar no Gmail.")

  def event(self, intent, kind, message=None):
    content = intent["content"]
    record = {"eventId":digest([intent["id"], kind, (message or {}).get("id", "")]),
      "intentId":intent["id"],"kind":kind,"contentHash":digest(content),
      "email":content["to"],"subject":content["subject"],"bodyPreview":content["body"][:1500],
      "gmailMessageId":intent.get("gmailMessageId", "") if kind == "sent" else "",
      "threadId":intent.get("threadId", content.get("threadId", "")),
      "replyMessageId":content.get("replyMessageId", "")}
    record["questionId"] = content.get("questionId", "")
    if message:
      record.update(gmailMessageId=message["id"], threadId=message["threadId"],
        subject=message.get("subject", "")[:200],
        bodyPreview=("De: " + message.get("from", "") + "\n" + message.get("body", "[Sem texto simples disponível]"))[:1500])
    return record

  def record(self, intent, kind, message=None):
    identity = intent["content"]["leadId"]
    lead = self.lead(identity)
    event = self.event(intent, kind, message)
    if kind in ("prepared", "sending"): self.eligible(lead, intent["content"]["to"])
    result = self.crm.request("/leads/" + identity + "/communications",
      {"revision":lead["snapshot"]["revision"],"followUpRevision":lead.get("followUpRevision", 0),"event":event}, "PUT")
    if result.get("conflict"): raise ValueError("Conflito no CRM; conferir histórico antes de repetir.")
    verified = self.lead(identity)["snapshot"].get("communications", [])
    if not any(item["eventId"] == event["eventId"] for item in verified):
      raise ValueError("Evento não confirmado no CRM.")
    return result

  def prepare(self, identity, recipient, subject, body, approved=False, reply_id=None, question_id=""):
    if approved is not True: raise ValueError("Preparação exige --approve após revisar destinatário e texto.")
    identity, recipient = str(UUID(identity)), address(recipient)
    self.eligible(self.lead(identity), recipient)
    if reply_id:
      # O próprio adaptador verifica remetente, última mensagem e automações.
      from ms_trevize.domain.mail import message_view, reply_context
      view = message_view(self.gmail.message(reply_id))
      context = reply_context(view, self.store.config()["sender"])
      if context["to"] != recipient: raise ValueError("Resposta pertence a outro destinatário.")
      subject = context["subject"]
    build_message({"sender":self.store.config()["sender"],"to":recipient,"subject":subject,
      "body":body,"mimeId":"<validation@trevize.invalid>"})
    self.store.approve(identity, recipient)
    # Chave canônica impede repetir abordagem inicial com outra chave arbitrária.
    key = "lead:" + identity + (":reply:" + reply_id if reply_id else ":initial")
    previous = [item for item in self.store.intents() if item["key"] == key or item["key"].startswith(key+":v")]
    cancelled = [item for item in previous if item["state"] == "cancelled"]
    if cancelled: key += ":v" + str(len(cancelled))
    intent = self.pilot.prepare(key, body, recipient, subject, reply_id, identity, question_id)
    self.record(intent, "prepared")
    return {"id":intent["id"],"state":"prepared","to":recipient,"subject":intent["content"]["subject"],
      "body":body,"leadId":identity,"sent":False}

  def cancel(self, identity):
    with self.store.lock():
      intent = self.store.get(identity)
      if intent["state"] == "cancelled": return {"id":identity,"state":"cancelled"}
      if intent["state"] != "prepared": raise ValueError("Envio iniciado exige reconciliação, não cancelamento.")
      self.record(intent, "cancelled")
      intent.update(state="cancelled")
      self.store.save(intent)
      return {"id":identity,"state":"cancelled"}

  def reserve(self, intent):
    if not intent["content"].get("leadId"): raise ValueError("Intenção sem lead associado.")
    result = self.record(intent, "sending")
    if result.get("repeated"):
      raise ValueError("Reserva já utilizada; reconciliar sem enviar novamente.")

  def send(self, identity, confirmed=False):
    intent = self.store.get(identity)
    if not intent["content"].get("leadId"): raise ValueError("Intenção sem lead associado.")
    result = self.pilot.send(identity, confirmed)
    if result["state"] == "sent": self.record(self.store.get(identity), "sent")
    return result

  def reconcile(self, identity):
    intent = self.store.get(identity)
    if not intent["content"].get("leadId"): raise ValueError("Intenção sem lead associado.")
    # Uma queda após a reserva no CRM pode anteceder a atualização local.
    if intent["state"] == "prepared":
      events = self.lead(intent["content"]["leadId"])["snapshot"].get("communications", [])
      if any(item["intentId"] == identity and item["kind"] == "sending" for item in events):
        with self.store.lock():
          intent.update(state="confirmation-pending")
          self.store.save(intent)
    result = self.pilot.reconcile(identity)
    if result["state"] == "sent": self.record(self.store.get(identity), "sent")
    return result

  def sync(self):
    # Primeiro recuperar envios confirmados localmente e ainda não registrados no CRM.
    intents = [item for item in self.store.intents() if item["content"].get("leadId") and item["state"] == "sent"]
    for intent in intents: self.record(intent, "sent")
    batch = self.pilot.replies(acknowledge=False)
    incoming = batch["messages"]
    for message in batch["takeovers"]:
      contacts = [item for item in intents if item["threadId"] == message["threadId"]]
      if len({item["content"]["leadId"] for item in contacts}) == 1:
        self.record(contacts[0], "human-takeover", message)
    items = []
    for message in incoming:
      contacts = [item for item in intents if item["threadId"] == message["threadId"]]
      lead_ids = {item["content"]["leadId"] for item in contacts}
      if len(lead_ids) != 1:
        items.append({"id":message["id"],"status":"ambiguous-lead-review"})
        continue
      intent = contacts[0]
      self.record(intent, message["disposition"], message)
      # Só reconhecer após gravar no CRM; uma falha permite retomar sem perder a resposta.
      self.pilot.acknowledge([message["id"]])
      items.append({"id":message["id"],"leadId":intent["content"]["leadId"],"status":message["disposition"]})
    return {"items":items,"autoReply":False,"contactPerformed":False}
