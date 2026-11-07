"""Cliente oficial Google carregado apenas nos comandos explícitos de Gmail."""
import json

from ms_trevize.data.gmail_store import SCOPES, private_json
from ms_trevize.data.local_runtime import atomic_json
from ms_trevize.domain.mail import identifier


class GmailError(ValueError):
  """Erro sanitizado; nunca imprimir resposta HTTP, tokens ou configuração OAuth."""


def credentials_for(store):
  from google.oauth2.credentials import Credentials
  from google.auth.transport.requests import Request
  token = private_json(store.root / "token.json")
  if token.get("token_uri") != "https://oauth2.googleapis.com/token" or set(token.get("scopes", [])) != set(SCOPES):
    raise GmailError("Token incompatível; autorize novamente somente os escopos do piloto.")
  try:
    credentials = Credentials.from_authorized_user_info(token, SCOPES)
    if not credentials.valid:
      if not credentials.refresh_token:
        raise GmailError("Autorização expirada; execute authorize novamente.")
      credentials.refresh(Request())
      atomic_json(store.root / "token.json", json.loads(credentials.to_json()))
    return credentials
  except Exception:
    raise GmailError("Autorização Gmail inválida, expirada ou indisponível; autorize novamente.") from None


def authorize(store):
  from google_auth_oauthlib.flow import InstalledAppFlow
  config = store.config()
  client = private_json(store.root / "client.json")
  flow = InstalledAppFlow.from_client_config(client, SCOPES, autogenerate_code_verifier=True)
  try:
    credentials = flow.run_local_server(host="127.0.0.1", port=0, timeout_seconds=180,
      open_browser=True, authorization_prompt_message="Autorize a conta no navegador: {url}",
      success_message="Autorização recebida. Você pode fechar esta aba e voltar ao terminal.",
      access_type="offline", prompt="consent select_account", login_hint=config["sender"])
    if not credentials.has_scopes(SCOPES) or (credentials.granted_scopes is not None and not set(SCOPES).issubset(credentials.granted_scopes)):
      raise GmailError("Permissões Gmail incompletas.")
    from googleapiclient.discovery import build
    client_api = GoogleGmail(build("gmail", "v1", credentials=credentials, cache_discovery=False))
    from ms_trevize.domain.mail import address
    if address(client_api.profile()["emailAddress"]) != config["sender"]:
      raise GmailError("Conta autorizada diverge do remetente configurado; token não foi salvo.")
    atomic_json(store.root / "token.json", json.loads(credentials.to_json()))
    return {"status":"authorized","sender":config["sender"],"autoSend":False}
  except GmailError:
    raise
  except Exception:
    raise GmailError("Não foi possível autorizar Gmail; confira cliente OAuth, conta de teste e consentimento.") from None


class GoogleGmail:
  def __init__(self, service, writes_enabled=False):
    self.users = service.users()
    self.writes_enabled = writes_enabled

  @classmethod
  def connect(cls, store):
    from googleapiclient.discovery import build
    return cls(build("gmail", "v1", credentials=credentials_for(store), cache_discovery=False),
      writes_enabled=store.config().get("sendingEnabled",False))

  def execute(self, request, write=False):
    try:
      # Sem retry de escrita: timeout pode ter ocorrido após o Gmail aceitar a mensagem.
      return request.execute(num_retries=0 if write else 2)
    except Exception:
      raise GmailError("Operação Gmail não confirmada; confira conexão, OAuth e limites. Escritas não são repetidas automaticamente.") from None

  def profile(self):
    return self.execute(self.users.getProfile(userId="me"))

  def inbox(self, limit=10):
    return self.execute(self.users.messages().list(userId="me", labelIds=["INBOX"], maxResults=limit)).get("messages", [])

  def message(self, identity, full=False):
    return self.execute(self.users.messages().get(userId="me", id=identifier(identity), format="full" if full else "metadata"))

  def thread(self, identity):
    return self.execute(self.users.threads().get(userId="me", id=identifier(identity), format="full"))

  def send(self, payload):
    if not self.writes_enabled: raise GmailError("Transporte de envio desabilitado.")
    return self.execute(self.users.messages().send(userId="me", body=payload), write=True)

  def find_sent(self, mime_id):
    return self.execute(self.users.messages().list(userId="me", q="in:sent rfc822msgid:" + mime_id,
      maxResults=10)).get("messages", [])
