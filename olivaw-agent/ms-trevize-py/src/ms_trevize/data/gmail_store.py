"""Estado privado do piloto: OAuth e intenções de transporte, nunca cadastro CRM."""
import hashlib
import json
import os
from pathlib import Path
import stat

from ms_trevize.data.local_runtime import atomic_json, exclusive_lock
from ms_trevize.domain.mail import address


SCOPES = ("https://www.googleapis.com/auth/gmail.send", "https://www.googleapis.com/auth/gmail.readonly")


def private_json(path):
  path = Path(path)
  if path.is_symlink():
    raise ValueError("Arquivo Gmail não pode ser link simbólico.")
  descriptor = os.open(path, os.O_RDONLY | os.O_NOFOLLOW)
  with os.fdopen(descriptor, "r", encoding="utf-8") as source:
    info = os.fstat(source.fileno())
    if not stat.S_ISREG(info.st_mode) or info.st_uid != os.getuid() or info.st_mode & 0o077 or info.st_size > (4194304 if path.name in ("received.json","approvals.json","retired-leads.json") else 131072):
      raise ValueError("Arquivo Gmail deve pertencer ao operador, respeitar o limite privado do tipo de arquivo e ter permissão 0600.")
    value = json.load(source)
  if not isinstance(value, dict):
    raise ValueError("Arquivo Gmail inválido.")
  return value


class GmailStore:
  def __init__(self, root):
    self.root = Path(root)
    for parent in (self.root, *self.root.parents):
      if parent.is_symlink():
        raise ValueError("Estado Gmail não pode atravessar links simbólicos.")

  def prepare(self):
    self.root.mkdir(parents=True, exist_ok=True, mode=0o700)
    if self.root.stat().st_uid != os.getuid():
      raise ValueError("Estado Gmail deve pertencer ao operador.")
    self.root.chmod(0o700)

  def lock(self):
    self.prepare()
    return exclusive_lock(self.root / "gmail.lock")

  def configure(self, sender, recipients, client_path):
    sender = address(sender)
    recipients = sorted(set(address(item) for item in recipients))
    if not recipients or len(recipients) > 5:
      raise ValueError("Configure de um a cinco destinatários controlados para o piloto.")
    source = Path(client_path)
    if source.is_symlink() or source.stat().st_size > 32768:
      raise ValueError("Arquivo de cliente OAuth inválido.")
    client = json.loads(source.read_text(encoding="utf-8"))
    installed = client.get("installed", {})
    if not installed.get("client_id") or not installed.get("client_secret") or installed.get("auth_uri") != "https://accounts.google.com/o/oauth2/auth" or installed.get("token_uri") != "https://oauth2.googleapis.com/token":
      raise ValueError("Use o JSON de cliente OAuth Google do tipo Aplicativo para computador.")
    with self.lock():
      config_path = self.root / "config.json"
      if config_path.exists() and private_json(config_path)["sender"] != sender:
        raise ValueError("Outra conta já configurada; use outro diretório privado de piloto.")
      atomic_json(self.root / "client.json", {"installed": installed})
      atomic_json(config_path, {"version":1,"sender":sender,"testRecipients":recipients,"dailyLimit":5,"sendingEnabled":False})
    return self.status()

  def config(self):
    config = private_json(self.root / "config.json")
    address(config["sender"])
    if config.get("version") != 1 or config.get("dailyLimit") != (500 if config.get("mode") == "commercial" else 5) or not isinstance(config.get("testRecipients"), list) or not 1 <= len(config["testRecipients"]) <= 5:
      raise ValueError("Configuração do piloto Gmail inválida.")
    if config.get("mode", "controlled") not in ("controlled","commercial"): raise ValueError("Modo Gmail inválido.")
    if config.get("mode") == "commercial" and not config.get("policyApprovedAt"): raise ValueError("Política comercial requer autorização registrada.")
    for item in config["testRecipients"]:
      address(item)
    if type(config.get("sendingEnabled", False)) is not bool: raise ValueError("Chave de envio inválida.")
    return config

  def status(self):
    if not self.root.exists() or not (self.root / "config.json").exists():
      return {"status":"setup-required","configured":False,"authorized":False,"autoSend":False}
    config = self.config()
    return {"status":"configured","configured":True,"authorized":(self.root / "token.json").is_file(),
      "sender":config["sender"],"testRecipients":config["testRecipients"],"dailyLimit":config["dailyLimit"],"mode":config.get("mode","controlled"),"autoSend":False,
      "sendingEnabled":config.get("sendingEnabled",False),
      "automaticPolicyEnabled":config.get("mode") == "commercial",
      "message":"Token presente não comprova autorização válida; use profile para verificar."}

  def intent_path(self, identity):
    if len(identity) != 64 or any(c not in "0123456789abcdef" for c in identity):
      raise ValueError("Identificador de intenção inválido.")
    return self.root / (identity + ".json")

  def get(self, identity):
    return private_json(self.intent_path(identity))

  def save(self, intent):
    if len(json.dumps(intent).encode()) > 131072:
      raise ValueError("Intenção excede o limite do estado privado.")
    atomic_json(self.intent_path(intent["id"]), intent)

  def approve(self, lead_id, recipient):
    from uuid import UUID
    lead_id, recipient = str(UUID(lead_id)), address(recipient)
    with self.lock():
      path = self.root / "approvals.json"
      approvals = private_json(path) if path.exists() else {}
      if any(identity != lead_id and email == recipient for identity,email in approvals.items()):
        raise ValueError("Destinatário já aprovado para outro lead; revisar duplicidade.")
      limit = 10000 if self.config().get("mode") == "commercial" else 5
      if lead_id not in approvals and len(approvals) >= limit:
        raise ValueError("Piloto comercial limitado a cinco leads aprovados.")
      if lead_id in approvals and approvals[lead_id] != recipient:
        raise ValueError("Lead já aprovado para outro destinatário; revisar manualmente.")
      approvals[lead_id] = recipient
      atomic_json(path, approvals)

  def retired_leads(self):
    path = self.root/"retired-leads.json"
    return set(private_json(path)) if path.exists() else set()

  def retire_lead(self, identity, note):
    from uuid import UUID
    from ms_trevize.data.local_runtime import now
    identity = str(UUID(identity))
    if not isinstance(note,str) or not note.strip() or len(note)>300: raise ValueError("Informe o motivo do encerramento.")
    with self.lock():
      path = self.root/"retired-leads.json"
      records = private_json(path) if path.exists() else {}
      if identity not in records:
        records[identity] = {"at":now(),"reason":note}
        atomic_json(path,records)

  def allowed(self, recipient, lead_id=None):
    config = self.config()
    if recipient in config["testRecipients"]: return True
    path = self.root / "approvals.json"
    return bool(lead_id and path.exists() and private_json(path).get(lead_id) == recipient)

  def intents(self):
    files = sorted(self.root.glob("[0-9a-f]" * 64 + ".json"))
    limit = 50000 if (self.root/"config.json").exists() and self.config().get("mode") == "commercial" else 500
    if len(files) > limit:
      raise ValueError("Piloto atingiu o limite de 500 intenções; não remover histórico para reenviar.")
    return [private_json(path) for path in files]

  def create(self, key, content):
    if not isinstance(key, str) or not 1 <= len(key) <= 160 or any(c in key for c in "\r\n\x00"):
      raise ValueError("Informe chave estável da operação, até 160 caracteres.")
    identity = hashlib.sha256((content["sender"] + "\n" + key).encode()).hexdigest()
    path = self.intent_path(identity)
    if path.exists():
      current = self.get(identity)
      if current["content"] != content:
        raise ValueError("Chave já utilizada com outro conteúdo; intenção original preservada.")
      return current
    limit = 50000 if (self.root/"config.json").exists() and self.config().get("mode") == "commercial" else 500
    if len(self.intents()) >= limit:
      raise ValueError("Limite do piloto atingido.")
    from ms_trevize.data.local_runtime import now
    intent = {"id":identity,"key":key,"state":"prepared","createdAt":now(),"content":content,
      "mimeId":"<trevize." + identity + "@" + content["sender"].split("@",1)[1] + ">"}
    self.save(intent)
    return intent
