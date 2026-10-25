"""Consulta e apresenta revisão; não cria estado nem altera Baley/skill."""
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
from hashlib import sha256
from html.parser import HTMLParser
import json
from pathlib import Path

from ms_baley.configuration import unique_keys
from ms_baley.data.advisory_sources import AdvisorySources, CISA, NVD
from ms_baley.data.research_http import ResearchHTTP, public_url
from ms_baley.data.web_discovery import WebDiscovery


class PageText(HTMLParser):
  def __init__(self):
    super().__init__(convert_charrefs=True)
    self.skip = 0
    self.parts = []

  def handle_starttag(self, tag, attrs):
    if tag in ("script", "style", "noscript"):
      self.skip += 1

  def handle_endtag(self, tag):
    if tag in ("script", "style", "noscript") and self.skip:
      self.skip -= 1

  def handle_data(self, text):
    if not self.skip:
      self.parts.append(text)


RULES = (
    ("rootless", "Avaliar containers rootless e registrar as limitações operacionais.", "Isolamento do Docker"),
    ("seccomp", "Verificar perfil seccomp e exceções concedidas aos containers.", "Isolamento do Docker"),
    ("apparmor", "Verificar AppArmor/SELinux e evidências de confinamento efetivo.", "Hardening do host"),
    ("ssrf", "Revisar proteção contra SSRF, destinos privados e redirecionamentos.", "Superfície das APIs"),
    ("supply chain", "Revisar procedência, versões e integridade das dependências/imagens.", "Dependências"),
    ("ngrx", "Verificar avisos de segurança dos pacotes NgRx e sua aplicabilidade às versões usadas.", "Dependências frontend"),
    ("access control", "Revisar controle de acesso e permissões efetivas dos serviços.", "Autorização"),
    ("tls", "Conferir política TLS e validação de certificados das conexões internas/externas.", "Transportes"),
)


def learning_items(advisories, pages, skill_text, maximum):
  result = []
  for row in advisories:
    result.append({"id": "advisory-" + row["id"],
        "action": "Investigar " + row["id"] + " e conferir produto, versão, backports e exposição antes de adicionar verificação à skill.",
        "basis": "advisory-candidate", "applicability": "not-assessed",
        "evidence": row["sources"], "suggestedSkillSection": "Vulnerabilidades e versões",
        "operatorDecision": "pending"})
  for term, action, section in RULES:
    matches = [page for page in pages if term in page["text"].lower()]
    if matches:
      result.append({"id": "control-" + term.replace(" ", "-"), "action": action,
          "basis": "control-mentioned-by-current-source", "matchedTerm": term,
          "skillCoverage": "mentioned" if term in skill_text.lower() else "not-mentioned",
          "evidence": [{"sourceId": page["id"], "url": page["url"]} for page in matches],
          "suggestedSkillSection": section, "operatorDecision": "pending"})
  return [{"number": index + 1, **row} for index, row in enumerate(result[:maximum])], len(result) > maximum


class UpdateReview:
  def __init__(self, runtime, http=None, discovery=None):
    self.runtime = runtime
    self.http = http or ResearchHTTP()
    self.discovery = discovery or WebDiscovery(self.http)

  def _registry(self, source):
    registry = source / "references/sources.json"
    if registry.is_symlink() or registry.stat().st_size > 65536:
      raise ValueError("Catálogo inválido.")
    document = json.loads(registry.read_text(), object_pairs_hook=unique_keys)
    rows = document.get("sources")
    if document.get("version") != 1 or not isinstance(rows, list) or not 1 <= len(rows) <= 12:
      raise ValueError("Catálogo deve conter até doze fontes.")
    seen = set()
    for row in rows:
      if not isinstance(row, dict) or not isinstance(row.get("id"), str) or row["id"] in seen or type(row.get("enabled")) is not bool:
        raise ValueError("Fonte duplicada ou inválida.")
      seen.add(row["id"])
      public_url(row["url"])
      if row.get("kind") not in ("feed", "page"):
        raise ValueError("Tipo de fonte inválido.")
      if row["kind"] == "feed" and (row["id"], row["url"]) not in (("cisa-kev", CISA), ("nvd", NVD)):
        raise ValueError("Feed sem adaptador registrado; use page ou implemente adaptador.")
    return rows

  def _page(self, row, settings):
    info = {"id": row["id"], "url": row["url"], "publisher": row.get("publisher"), "kind": "page", "status": "failed", "approved": True}
    try:
      body = self.http.fetch(row["url"], settings["timeoutSeconds"], min(settings["maxResponseBytes"], 1048576))
      parser = PageText()
      parser.feed(body.decode("utf-8", errors="replace"))
      text = " ".join(" ".join(parser.parts).split())
      info.update(status="ok", contentHash=sha256(body).hexdigest())
      return info, {"id": row["id"], "url": row["url"], "text": text}
    except Exception as error:
      info["error"] = type(error).__name__
      info["operatorSuggestion"] = "review-source-url-or-availability"
      if hasattr(error, "code"):
        info["httpStatus"] = error.code
      return info, None

  def update(self):
    settings = self.runtime.settings.get("update", {})
    for key, low, high in (("timeoutSeconds", 1, 15), ("maxResponseBytes", 1024, 8388608), ("maxCandidates", 1, 100), ("lookbackDays", 1, 30)):
      if type(settings.get(key)) is not int or not low <= settings[key] <= high:
        raise ValueError("Limite inválido: " + key)
    search_settings = settings.get("discovery", {})
    if type(search_settings.get("timeoutSeconds")) is not int or not 1 <= search_settings["timeoutSeconds"] <= 15:
      raise ValueError("Timeout de discovery inválido.")
    source = self.runtime.path.parent.parent / "skills/safety-docker"
    if source.is_symlink() or not source.is_dir():
      raise ValueError("Skill canônica ausente.")
    if (self.runtime.path.parent / self.runtime.settings["skill"]["path"]).resolve(strict=True) != (source / "SKILL.md").resolve(strict=True):
      raise ValueError("Skill configurada difere da fonte canônica.")
    texts = []
    digest = sha256()
    for path in sorted(source.rglob("*")):
      if path.is_symlink():
        raise ValueError("Skill não pode conter links.")
      if path.is_file():
        if path.stat().st_size > 1048576:
          raise ValueError("Skill excede limite.")
        body = path.read_bytes()
        digest.update(str(path.relative_to(source)).encode() + b"\0" + body)
        if path.suffix == ".md":
          texts.append(body.decode("utf-8"))
    rows = self._registry(source)
    enabled = [row for row in rows if row["enabled"]]
    feed_ids = [row["id"] for row in enabled if row["kind"] == "feed"]

    def json_fetch(url, timeout, maximum):
      return json.loads(self.http.fetch(url, timeout, maximum).decode("utf-8"), object_pairs_hook=unique_keys)

    collected = AdvisorySources(json_fetch).collect(settings, enabled=feed_ids)
    feed_results = {row["name"]: row for row in collected["sources"]}
    sources, pages = [], []
    for row in rows:
      if not row["enabled"]:
        sources.append({**row, "status": "disabled", "approved": True})
      elif row["kind"] == "feed":
        feed = feed_results[row["id"]]
        sources.append({**row, **feed, "id": row["id"], "approved": True})
    with ThreadPoolExecutor(max_workers=3) as pool:
      for info, text in pool.map(lambda row: self._page(row, settings), (row for row in enabled if row["kind"] == "page")):
        sources.append(info)
        if text:
          pages.append(text)
    discovery = self.discovery.search(search_settings, [row["url"] for row in rows])
    learn, learn_truncated = learning_items(collected["candidates"], pages, "\n".join(texts), settings["maxCandidates"])
    errors = any(row["status"] == "failed" or row.get("truncated") for row in sources)
    search_errors = any(row["status"] != "ok" or row.get("truncated") for row in discovery["searches"])
    status = "partial" if errors or search_errors or learn_truncated or collected["candidatesTruncated"] else "completed"
    if not any(row["status"] == "ok" for row in sources) and not any(row["status"] == "ok" for row in discovery["searches"]):
      status = "failed"
    return {"schemaVersion": "2.0", "operation": "update", "agentId": "baley", "status": status,
        "source": sources, "discovery": discovery["items"], "learn": learn,
        "research": {"analysisMethod": "evidence-rules", "searchProvider": search_settings["provider"],
            "searches": discovery["searches"], "windowStartedAt": collected["windowStartedAt"],
            "collectedAt": datetime.now(timezone.utc).isoformat(), "learnTruncated": learn_truncated,
            "limitations": ["Sugestões exigem revisão humana; não são alterações aplicadas.",
                "Resultados de busca não foram acessados nem aprovados como fontes confiáveis.",
                "Uma página de NVD não é cobertura completa de CVEs.",
                "Menção em documento não comprova vulnerabilidade no ambiente."]},
        "review": {"skillHash": digest.hexdigest(), "operatorDecisionRequired": True,
            "changesApplied": False, "localFilesWritten": False, "skillChanged": False, "agentChanged": False}}
