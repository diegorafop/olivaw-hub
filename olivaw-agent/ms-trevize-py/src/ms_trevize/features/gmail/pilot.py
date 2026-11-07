"""Comandos e consultas do piloto; não executa campanha nem responde por IA."""
from datetime import datetime, timezone

from ms_trevize.data.local_runtime import atomic_json, now
from ms_trevize.domain.mail import address, build_message, header, mailbox, message_view, reply_context


class GmailPilot:
  def __init__(self, store, api):
    self.store, self.api = store, api

  def profile(self):
    config = self.store.config()
    result = self.api.profile()
    if address(result["emailAddress"]) != config["sender"]:
      raise ValueError("Conta Gmail diverge do remetente configurado. Nenhum envio permitido.")
    return {"sender":config["sender"],"status":"connected","autoSend":False}

  def inbox(self, limit=10):
    self.profile()
    if type(limit) is not int or not 1 <= limit <= 20:
      raise ValueError("Consulta limitada a 1–20 mensagens.")
    return {"messages":[message_view(self.api.message(item["id"])) for item in self.api.inbox(limit)]}

  def thread(self, identity, include_body=False):
    self.profile()
    return {"messages":[message_view(item, include_body) for item in self.api.thread(identity).get("messages", [])]}

  def prepare(self, key, body, recipient=None, subject=None, reply_id=None, lead_id=None):
    config = self.store.config()
    content = {"sender":config["sender"],"body":body}
    if lead_id:
      from uuid import UUID
      content["leadId"] = str(UUID(lead_id))
    with self.store.lock():
      if reply_id:
        self.profile()
        original = message_view(self.api.message(reply_id))
        content.update(reply_context(original, config["sender"]))
        messages = self.api.thread(content["threadId"]).get("messages", [])
        if not messages or messages[-1]["id"] != reply_id:
          raise ValueError("Conversa possui mensagem mais recente; revise antes de responder.")
        # Uma mensagem recebida não pode gerar duas respostas com chaves diferentes.
        for intent in self.store.intents():
          if intent["content"].get("replyMessageId") == reply_id and intent["key"] != key:
            raise ValueError("Já existe uma intenção de resposta para esta mensagem.")
      else:
        content.update(to=address(recipient), subject=header(subject, 200))
      if content["to"] not in config["testRecipients"]:
        raise ValueError("Destinatário fora da lista de teste controlado.")
      build_message({**content,"mimeId":"<validation@trevize.invalid>"})
      return self.store.create(key, content)

  def send(self, identity, confirmed=False):
    if confirmed is not True:
      raise ValueError("Envio exige --confirm após revisar a intenção preparada.")
    with self.store.lock():
      intent = self.store.get(identity)
      if intent["state"] != "prepared":
        return {"id":identity,"state":intent["state"],"repeated":False,
          "message":"Nenhum novo envio. Use reconcile se o resultado estiver pendente."}
      self.profile()
      config, content = self.store.config(), intent["content"]
      if content["sender"] != config["sender"] or content["to"] not in config["testRecipients"]:
        raise ValueError("Remetente ou destinatário não autorizado para o piloto.")
      if content.get("replyMessageId"):
        original = message_view(self.api.message(content["replyMessageId"]))
        context = reply_context(original, config["sender"])
        if any(content.get(key) != value for key,value in context.items()):
          raise ValueError("Dados da resposta divergentes da mensagem original.")
        messages = self.api.thread(content["threadId"]).get("messages", [])
        if not messages or messages[-1]["id"] != content["replyMessageId"]:
          raise ValueError("Houve intervenção na conversa após a preparação; envio cancelado.")
      day = datetime.now(timezone.utc).date().isoformat()
      attempts = sum(item.get("attemptedAt", "").startswith(day) for item in self.store.intents())
      if attempts >= config["dailyLimit"]:
        raise ValueError("Limite diário do piloto atingido; aguarde o próximo dia UTC.")
      payload = build_message({**content,"mimeId":intent["mimeId"]})
      intent.update(state="confirmation-pending", attemptedAt=now())
      # Intenção durável antes de enviar. Crash neste ponto exige reconciliação, nunca retry automático.
      self.store.save(intent)
      try:
        result = self.api.send(payload)
        from ms_trevize.domain.mail import identifier
        intent.update(gmailMessageId=identifier(result["id"]), threadId=identifier(result["threadId"]))
        self.store.save(intent)
        return self._confirm(intent, intent["gmailMessageId"])
      except Exception:
        return {"id":identity,"state":"confirmation-pending","message":"Operação não confirmada; não reenviar. Execute reconcile."}

  def _confirm(self, intent, message_id):
    view = message_view(self.api.message(message_id))
    if "SENT" not in view["labels"] or view["messageId"] != intent["mimeId"] or mailbox(view["from"]) != intent["content"]["sender"] or mailbox(view["to"]) != intent["content"]["to"]:
      raise ValueError("Não foi possível confirmar a mensagem específica em Enviados.")
    intent.update(state="sent", gmailMessageId=view["id"], threadId=view["threadId"], confirmedAt=now())
    self.store.save(intent)
    return {"id":intent["id"],"state":"sent","gmailMessageId":view["id"],"threadId":view["threadId"],
      "message":"Presença em Enviados confirmada; entrega e leitura pelo destinatário não comprovadas."}

  def reconcile(self, identity):
    with self.store.lock():
      intent = self.store.get(identity)
      if intent["state"] == "prepared":
        return {"id":identity,"state":"prepared","message":"Nenhum envio foi iniciado."}
      self.profile()
      matches = self.api.find_sent(intent["mimeId"])
      if len(matches) != 1:
        return {"id":identity,"state":intent["state"],"message":"Mensagem não localizada ou ambígua; manter para revisão, sem reenviar."}
      return self._confirm(intent, matches[0]["id"])

  def replies(self):
    """Consulta somente fios conhecidos; não altera lido/não lido e não envia respostas."""
    with self.store.lock():
      self.profile()
      path = self.store.root / "received.json"
      from ms_trevize.data.gmail_store import private_json
      state = private_json(path) if path.exists() else {"seen":[]}
      seen = set(state["seen"])
      intents = [i for i in self.store.intents() if i["state"] == "sent"]
      own_ids = {i["gmailMessageId"] for i in intents}
      threads = {}
      for intent in intents:
        threads.setdefault(intent["threadId"], []).append(intent)
      incoming = []
      for thread_id, contacts in threads.items():
        messages = self.api.thread(thread_id).get("messages", [])
        if len(messages) > 100:
          raise ValueError("Conversa excede o limite do piloto; revisar manualmente.")
        known = {i["content"]["to"] for i in contacts}
        started = min(i["gmailMessageId"] for i in contacts)
        # Não depender de labels UNREAD: leitura manual não deve ocultar resposta.
        active = False
        human = False
        collected = []
        for message in messages:
          if message["id"] == started or message["id"] in own_ids:
            active = True
          if not active:
            continue
          view = message_view(message, True)
          if "SENT" in view["labels"]:
            human |= view["id"] not in own_ids
            continue
          if view["id"] in seen:
            continue
          sender = mailbox(view["from"])
          automatic = view["autoSubmitted"].lower() not in ("", "no") or view["precedence"].lower() in ("bulk", "list", "junk")
          disposition = "received" if sender in known and not automatic else "review-required"
          view.update(disposition=disposition, leadIds=sorted({i["content"]["leadId"] for i in contacts if i["content"].get("leadId")}))
          collected.append(view)
          seen.add(view["id"])
        if human:
          for view in collected:
            view["disposition"] = "human-takeover"
        incoming.extend(collected)
      if len(seen) > 1000:
        raise ValueError("Limite de histórico do piloto atingido; não descartar IDs para repetir processamento.")
      # Conteúdo fica somente na saída explícita; estado guarda IDs, não corpos da caixa.
      atomic_json(path, {"seen":sorted(seen)})
      return {"messages":incoming,"autoReply":False,"message":"Consulta de teste; nenhuma resposta enviada e nenhum Lead atualizado."}
