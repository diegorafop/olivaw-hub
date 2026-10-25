"""Busca pública de fontes candidatas; nunca acessa os links encontrados."""
import re
from urllib.parse import urlencode, urlsplit, urlunsplit
import xml.etree.ElementTree as ET

from ms_baley.data.research_http import public_url

SEARCH_URL = "https://www.bing.com/search"
PRIMARY_DOMAINS = {"cisa.gov", "nist.gov", "docker.com", "postgresql.org", "rabbitmq.com", "redis.io", "aws.amazon.com", "ngrx.io", "angular.dev"}
RELEVANCE = re.compile(r"secur|vulnerab|advis|\bcve\b|\bghsa\b|hardening|exploit|seguran", re.I)
CYBER_CONTEXT = re.compile(r"cyber|ciber|vulnerab|advis|\bcve\b|\bghsa\b|docker|containerd|ngrx|angular|nist|cisa|infosec|appsec|owasp|\bnvd\b|\bnpm\b|malware|ransomware|\bcwe\b|hardening|exploit", re.I)
COMMUNITY_DOMAINS = {"reddit.com", "stackoverflow.com", "news.ycombinator.com"}


def domain_matches(host, domains):
  return any(host == domain or host.endswith("." + domain) for domain in domains)


class WebDiscovery:
  def __init__(self, http):
    self.http = http

  def search(self, settings, known_urls):
    if settings.get("provider") != "bing-rss":
      raise ValueError("Provedor de busca não suportado.")
    queries = settings.get("queries")
    if not isinstance(queries, list) or not 1 <= len(queries) <= 3 or any(not isinstance(q, str) or not 1 <= len(q) <= 180 for q in queries):
      raise ValueError("Consultas de discovery inválidas.")
    cap = settings.get("maxResults", 10)
    if type(cap) is not int or not 1 <= cap <= 30:
      raise ValueError("Limite de discovery inválido.")
    known = set(known_urls)
    items, searches = {}, []
    for query in queries:
      url = SEARCH_URL + "?" + urlencode({"q": query, "format": "rss"})
      info = {"query": query, "url": url, "status": "failed"}
      try:
        body = self.http.fetch(url, settings["timeoutSeconds"], 1048576)
        if b"<!DOCTYPE" in body.upper() or b"<!ENTITY" in body.upper():
          raise ValueError("DTD não permitido no feed de busca.")
        root = ET.fromstring(body)
        if root.tag != "rss":
          raise ValueError("Busca não retornou RSS.")
        rows = root.findall("./channel/item")
        info.update(status="ok", returned=len(rows), truncated=len(rows) >= cap)
        info["irrelevantResultsSkipped"] = 0
        for row in rows:
          link = (row.findtext("link") or "").strip()
          try:
            parsed = public_url(link)
          except (ValueError, TypeError):
            continue
          title = (row.findtext("title") or "")[:250]
          if not RELEVANCE.search(title + " " + link) or not CYBER_CONTEXT.search(title + " " + link):
            info["irrelevantResultsSkipped"] += 1
            continue
          normalized = urlunsplit(("https", parsed.netloc.lower(), parsed.path.rstrip("/") or "/", parsed.query, ""))
          if normalized in known or any(normalized.rstrip("/") == k.rstrip("/") for k in known):
            continue
          if normalized in items:
            items[normalized]["queries"].append(query)
            continue
          if len(items) >= cap * len(queries):
            info["truncated"] = True
            continue
          host = parsed.hostname.lower()
          kind = "community" if domain_matches(host, COMMUNITY_DOMAINS) else "primary-publisher-candidate" if domain_matches(host, PRIMARY_DOMAINS) else "unknown-publisher"
          items[normalized] = {"url": normalized, "title": title,
              "publisherType": kind, "trustStatus": "not-verified", "operatorApprovalRequired": True,
              "discoveredBy": "web-search", "queries": [query],
              "reviewChecks": ["Confirmar autoria e domínio", "Conferir datas e referências primárias", "Revisar qualidade e histórico editorial", "Corroborar relatos de fórum antes de usar"]}
      except Exception as error:
        info["error"] = type(error).__name__
        if hasattr(error, "code"):
          info["httpStatus"] = error.code
      searches.append(info)
    # Intercala as buscas para que a primeira não esgote toda a lista.
    selected = []
    pools = [[item for item in items.values() if query in item["queries"]] for query in queries]
    while len(selected) < cap and any(pools):
      for pool in pools:
        if pool and len(selected) < cap:
          item = pool.pop(0)
          if item not in selected:
            selected.append(item)
    if len(items) > len(selected):
      for info in searches:
        info["truncated"] = True
    return {"items": selected, "searches": searches}
