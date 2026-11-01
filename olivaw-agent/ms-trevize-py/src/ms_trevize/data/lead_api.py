"""Persistência autenticada e idempotente, sem acesso direto a banco."""
import json
import os
from pathlib import Path
import urllib.request
import urllib.error
from urllib.parse import urlencode
from ms_trevize.domain.candidate import web_url


class ApiError(ValueError):
  def __init__(self, status, allow=""):
    self.status = status
    self.allow = allow
    super().__init__("API não concluiu a operação: HTTP " + str(status))


class LeadApi:
  def __init__(self, settings, project, opener=urllib.request.urlopen):
    self.settings, self.project, self.opener = settings, Path(project), opener
    self.token = None

  def request(self, path, payload=None, method=None):
    headers={"Accept":"application/json"}
    if self.token: headers["Authorization"]="Bearer "+self.token
    if payload is not None: headers["Content-Type"]="application/json"
    request=urllib.request.Request(self.settings["baseUrl"].rstrip("/")+path,
        data=None if payload is None else json.dumps(payload).encode(), headers=headers, method=method)
    # Não seguir redirects com credenciais ou repetir automaticamente escritas.
    class NoRedirect(urllib.request.HTTPRedirectHandler):
      def redirect_request(self, req, fp, code, msg, hdrs, newurl): return None
    opener=self.opener
    if opener is urllib.request.urlopen: opener=urllib.request.build_opener(NoRedirect).open
    try:
      with opener(request,timeout=25) as response:
        raw=response.read(262145)
        if len(raw)>262144: raise ValueError("Resposta API excede limite.")
        return json.loads(raw) if raw else {}
    except urllib.error.HTTPError as error:
      raise ApiError(error.code, error.headers.get("Allow", "")) from None
    except (urllib.error.URLError, TimeoutError, json.JSONDecodeError):
      raise ValueError("Comunicação API indisponível; verificar confirmação antes de repetir.") from None

  def login(self):
    path=(self.project/self.settings["credentialsPath"]).resolve(strict=True)
    if path.stat().st_uid!=os.getuid() or path.stat().st_mode & 0o077 or path.stat().st_size>65536:
      raise ValueError("Credenciais exigem arquivo privado do usuário (0600).")
    credentials=json.loads(path.read_text())[self.settings["credentialsSection"]]
    result=self.request("/identity/authentication/login",{"login":credentials["login"],"password":credentials["password"]})
    self.token=result.get("accessToken")
    if not self.token: raise ValueError("Login sem token.")
    access=self.request("/identity/authentication/access")
    if access.get("isRoot") or "leads.manage" not in access.get("permissions",[]):
      self.token=None
      raise ValueError("Conta técnica deve ter leads.manage sem root.")

  def preflight(self):
    self.login()
    try:
      self.request("/leads/00000000-0000-0000-0000-000000000000/qualification",method="OPTIONS")
    except ApiError as error:
      if error.status == 405 and "PUT" in error.allow:
        return
      raise ValueError("Publique a API com a operação de qualificação antes de coletar.") from None

  def publish(self,candidate,qualification):
    if not self.token:self.login()
    identity=candidate.external_id
    try:
      current=self.request("/leads/"+identity)
    except ApiError as error:
      if error.status!=404:raise
      matches=self.request("/leads?"+urlencode({"search":candidate.name,"pageSize":100}))
      for item in matches.get("items",[]):
        if item.get("business","").casefold()==candidate.name.casefold():return "possible-duplicate-review"
      payload={"externalId":identity,"initiative":"outbound","origin":"Google Maps",
        "externalReference":candidate.key,"sourceUrl":candidate.source,"business":candidate.name,
        "title":("Presença digital — "+candidate.name)[:160],
        "requirements":"Candidato para revisão de presença digital. Atividade exibida: "+candidate.category+". Nenhum contato realizado.","email":"","whatsapp":""}
      try:self.request("/leads",payload)
      except ApiError as create_error:
        if create_error.status!=409:raise
      current=self.request("/leads/"+identity)
    snapshot=current.get("snapshot")
    if not isinstance(snapshot,dict):raise ValueError("Lead sem conteúdo legível.")
    if snapshot.get("qualification") is not None:return "already-registered"
    self.request("/leads/"+identity+"/qualification",{"revision":snapshot["revision"],"qualification":qualification},"PUT")
    verified=self.request("/leads/"+identity)
    if not verified.get("snapshot",{}).get("qualification"):raise ValueError("Qualificação não confirmada.")
    return "delivered"
