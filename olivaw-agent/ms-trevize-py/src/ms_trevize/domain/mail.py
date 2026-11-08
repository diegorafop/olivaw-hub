"""Mensagens simples, destinatários únicos e correlação MIME de respostas."""
import base64
from email import policy
from email.message import EmailMessage
from email.utils import getaddresses
import re


def address(value):
  if not isinstance(value, str) or len(value) > 254 or any(c in value for c in "\r\n\x00"):
    raise ValueError("Endereço de e-mail inválido.")
  # Endereços simples e únicos; sem display name, listas ou cabeçalhos adicionais.
  if not re.fullmatch(r"[A-Za-z0-9.!#$%&'*+/=?^_`{|}~-]+@[A-Za-z0-9](?:[A-Za-z0-9.-]*[A-Za-z0-9])?\.[A-Za-z]{2,63}", value):
    raise ValueError("Informe um único endereço de e-mail, sem nome ou lista.")
  return value.lower()


def identifier(value):
  if not isinstance(value, str) or not re.fullmatch(r"[A-Za-z0-9_-]{1,200}", value):
    raise ValueError("Identificador Gmail inválido.")
  return value


def header(value, maximum=998):
  if not isinstance(value, str) or not value.strip() or len(value) > maximum or any(c in value for c in "\r\n\x00"):
    raise ValueError("Cabeçalho de mensagem inválido.")
  return value.strip()


def mailbox(value):
  parsed = getaddresses([value])
  if len(parsed) != 1:
    raise ValueError("Remetente ambíguo.")
  return address(parsed[0][1])


def message_view(message, include_body=False, body_limit=12000):
  payload = message.get("payload", {})
  headers = {}
  for item in payload.get("headers", []):
    name = item.get("name", "").lower()
    if name not in {"from", "to", "subject", "message-id", "date", "references", "reply-to", "auto-submitted", "precedence"}:
      continue
    if name in headers:
      raise ValueError("Cabeçalho duplicado na mensagem.")
    headers[name] = item.get("value", "")
  result = {"id": identifier(message["id"]), "threadId": identifier(message["threadId"]),
    "labels": message.get("labelIds", []), "date": headers.get("date", ""),
    "from": headers.get("from", ""), "to": headers.get("to", ""),
    "subject": headers.get("subject", ""), "messageId": headers.get("message-id", ""),
    "references": headers.get("references", ""), "replyTo": headers.get("reply-to", ""),
    "autoSubmitted": headers.get("auto-submitted", ""), "precedence": headers.get("precedence", "")}
  if include_body:
    texts = []
    truncated = False
    def visit(part, depth=0):
      nonlocal truncated
      if depth > 12 or part.get("filename"):
        return
      if part.get("mimeType") == "text/plain":
        data = part.get("body", {}).get("data", "")
        if len(data) <= 131072:
          try:
            text = base64.urlsafe_b64decode(data + "=" * (-len(data) % 4)).decode("utf-8", errors="replace")
            text = text.replace("\r\n","\n").replace("\r","\n")
            truncated |= len(text) > body_limit
            texts.append(text[:body_limit])
          except (ValueError, TypeError):
            raise ValueError("Corpo de mensagem inválido.") from None
        else: truncated = True
      for child in part.get("parts", [])[:50]:
        visit(child, depth+1)
    visit(payload)
    body = "\n".join(texts)
    result["body"] = body[:body_limit]
    result["bodyTruncated"] = truncated or len(body) > body_limit
    result["bodyAvailable"] = bool(texts)
  return result


def reply_context(original, sender):
  if mailbox(original["from"]) == address(sender) or "SENT" in original["labels"]:
    raise ValueError("Não responder automaticamente à própria mensagem.")
  if original.get("autoSubmitted", "").lower() not in ("", "no") or original.get("precedence", "").lower() in ("bulk", "list", "junk"):
    raise ValueError("Mensagem automática ou de lista: exige revisão humana.")
  target = mailbox(original["from"])
  if original.get("replyTo") and mailbox(original["replyTo"]) != target:
    raise ValueError("Reply-To divergente: exige revisão humana.")
  message_id = header(original["messageId"])
  if not re.fullmatch(r"<[^<>\s]+@[^<>\s]+>", message_id):
    raise ValueError("Message-ID ausente ou inválido para resposta.")
  subject = header(original["subject"], 200)
  references = original.get("references", "")
  if references:
    references = header(references, 700)
    if not re.fullmatch(r"(?:<[^<>\s]+@[^<>\s]+>\s*)+", references):
      raise ValueError("References inválido.")
  return {"to": target, "subject": subject, "threadId": identifier(original["threadId"]),
    "inReplyTo": message_id, "references": (references + " " + message_id).strip(), "replyMessageId": original["id"]}


def build_message(intent):
  body = intent["body"]
  if not isinstance(body, str) or not body.strip() or len(body.encode("utf-8")) > 32000 or "\x00" in body:
    raise ValueError("Corpo obrigatório, até 32 KB, sem bytes nulos.")
  message = EmailMessage(policy=policy.SMTP)
  message["From"] = address(intent["sender"])
  message["To"] = address(intent["to"])
  message["Subject"] = header(intent["subject"], 200)
  message["Message-ID"] = header(intent["mimeId"])
  if intent.get("inReplyTo"):
    message["In-Reply-To"] = header(intent["inReplyTo"])
    message["References"] = header(intent["references"])
  message.set_content(body)
  result = {"raw": base64.urlsafe_b64encode(message.as_bytes()).decode("ascii")}
  if intent.get("threadId"):
    result["threadId"] = identifier(intent["threadId"])
  return result
