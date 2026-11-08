"""Especialista de respostas: consulta fios conhecidos e usa o transporte auditado."""
from ms_trevize.data.local_runtime import exclusive_lock
from ms_trevize.domain.mail import mailbox, message_view


class FollowUpSpecialist:
  def __init__(self, outreach, store, settings=None, stopping=None):
    self.outreach, self.store, self.settings = outreach, store, settings or {}
    self.stopping = stopping or (lambda: False)

  def run(self):
    if not self.settings.get("enabled"):
      return {"status":"paused","reason":"outreach-disabled","read":0,"sent":0}
    self.store.prepare()
    with exclusive_lock(self.store.root/"followup.lock"):
      return self._run()

  def _run(self):
    result = {"status":"completed","read":0,"drafts":0,"sent":0,"handoffs":0,
      "pending":0,"skipped":0,"failures":0,"interrupted":False,"remaining":0}
    seen = set()
    while not self.stopping():
      try:
        batch = self.outreach.sync(limit=100,stopping=self.stopping)
        items = batch.get("items", [])
        if not isinstance(items,list): raise ValueError("Leitura incompatível.")
        identities = {item["id"] for item in items}
        if items and not identities.difference(seen): raise ValueError("Leitura sem progresso.")
        seen.update(identities)
        result["read"] += len(items)
        result["handoffs"] += sum(item.get("status") in ("review-required","human-takeover","ambiguous-lead-review") for item in items)
        if len(items) < 100: break
      except (OSError,ValueError,KeyError,TypeError):
        result.update(status="partial",failures=result["failures"]+1)
        return result  # Sem visão atualizada da thread, não arriscar uma resposta.
    if self.stopping():
      result.update(status="interrupted",interrupted=True,remainingUnknown=True)
      return result
    decisions = self.outreach.decisions()
    result["handoffs"] += sum(item.get("messageId") in seen and item.get("classification") in ("handoff","uncertain","optout") for item in decisions)
    retired = getattr(self.store,"retired_leads",lambda:set())()
    pending = [item for item in decisions if item.get("classification") == "qualify" and item.get("leadId") not in retired
      and not item.get("campaignPaused") and not item.get("reprocess") and isinstance(item.get("reply"),str) and item["reply"].strip()]
    result["drafts"] = len(pending)
    can_send = (self.settings.get("automaticReplies") is True and self.settings.get("aiEnabled") is True
      and self.settings.get("sendingEnabled") is True and self.store.config().get("sendingEnabled") is True)
    if not can_send:
      result.update(pending=len(pending),remaining=len(pending),sendStatus="disabled")
      return result
    intents = self.store.intents()
    threads = {}
    for index,decision in enumerate(pending):
      if self.stopping():
        result.update(status="interrupted",interrupted=True,remaining=len(pending)-index)
        break
      try:
        outcome = self._respond(decision,intents,threads)
        if outcome == "sent": result["sent"] += 1
        elif outcome == "pending": result["pending"] += 1
        else: result["skipped"] += 1
      except (OSError,ValueError,KeyError,TypeError):
        # O diário conserva qualquer envio ambíguo; a próxima rodada não o repete.
        result["failures"] += 1
        result["pending"] += 1
        result["status"] = "partial"
    result["remaining"] += result["pending"]
    return result

  def _respond(self, decision, intents, threads):
    lead_id, message_id, thread_id = (decision[key] for key in ("leadId","messageId","threadId"))
    roots = [item for item in intents if item["id"] == decision.get("intentId") and item["state"] == "sent"
      and item["content"].get("leadId") == lead_id and item.get("threadId") == thread_id]
    if len(roots) != 1: return "skipped"
    recipient = roots[0]["content"]["to"]
    attempts = [item for item in intents if item["content"].get("replyMessageId") == message_id]
    if any(item["content"].get("leadId") != lead_id or item["content"]["to"] != recipient for item in attempts):
      raise ValueError("Resposta vinculada a outro contato.")
    if any(item["state"] == "confirmation-pending" for item in attempts): return "pending"
    if any(item["state"] in ("sent","cancelled") for item in attempts): return "skipped"
    if thread_id not in threads:
      messages = self.outreach.gmail.thread(thread_id).get("messages", [])
      threads[thread_id] = message_view(messages[-1]) if messages else None
    latest = threads[thread_id]
    if not latest or latest["id"] != message_id or mailbox(latest["from"]) != recipient: return "skipped"
    self.outreach.eligible(self.outreach.lead(lead_id),recipient)
    # A aprovação explícita da política automática vale para rascunhos desta execução.
    # Campanhas antigas não passam por este especialista sem automaticReplies=True.
    prepared = self.outreach.prepare(lead_id,recipient,None,decision["reply"],True,message_id,decision.get("questionId", ""))
    result = self.outreach.send(prepared["id"],True)
    current = self.store.get(prepared["id"])
    intents[:] = [item for item in intents if item["id"] != current["id"]] + [current]
    if result.get("state") == "sent": return "skipped" if result.get("repeated") else "sent"
    return "pending"
