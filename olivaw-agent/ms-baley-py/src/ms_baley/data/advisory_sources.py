"""Leitura limitada de feeds primários, sem redirecionamentos ou código remoto."""
from datetime import datetime, timedelta, timezone, date
import json
import re
from urllib.parse import urlencode
from urllib.request import build_opener, HTTPRedirectHandler, HTTPSHandler, Request

from ms_baley.configuration import unique_keys

CISA = "https://www.cisa.gov/sites/default/files/feeds/known_exploited_vulnerabilities.json"
NVD = "https://services.nvd.nist.gov/rest/json/cves/2.0"
SOFTWARE = re.compile(r"\b(docker|containerd|runc|traefik|postgresql|rabbitmq|redis|openssh|amazon linux|linux kernel|angular|ngrx|npm|node\.js)\b", re.I)
CVE = re.compile(r"CVE-\d{4}-\d{4,}")


class NoRedirect(HTTPRedirectHandler):
  def redirect_request(self, req, fp, code, msg, headers, newurl):
    return None


class AdvisorySources:
  def __init__(self, fetch=None):
    self.fetch = fetch or self._fetch

  def _fetch(self, url, timeout, maximum):
    # URLs são construídas somente aqui, sem destino vindo do feed ou setting.
    if not (url == CISA or url.startswith(NVD + "?")):
      raise ValueError("Fonte não permitida.")
    opener = build_opener(NoRedirect(), HTTPSHandler())
    request = Request(url, headers={"User-Agent": "Olivaw-Baley/0.1", "Accept": "application/json"})
    with opener.open(request, timeout=timeout) as response:
      data = response.read(maximum + 1)
      if len(data) > maximum:
        raise ValueError("Fonte excede limite de resposta.")
      return json.loads(data.decode("utf-8"), object_pairs_hook=unique_keys)

  def collect(self, settings, current=None, enabled=None):
    current = current or datetime.now(timezone.utc)
    start = current - timedelta(days=settings["lookbackDays"])
    query = urlencode({"lastModStartDate": start.strftime("%Y-%m-%dT%H:%M:%S.000Z"),
                       "lastModEndDate": current.strftime("%Y-%m-%dT%H:%M:%S.000Z"), "resultsPerPage": 100})
    sources = []
    candidates = {}
    for name, url in (("cisa-kev", CISA), ("nvd", NVD + "?" + query)):
      if enabled is not None and name not in enabled:
        continue
      info = {"name": name, "url": url, "status": "failed"}
      try:
        data = self.fetch(url, settings["timeoutSeconds"], settings["maxResponseBytes"])
        if not isinstance(data, dict):
          raise ValueError("Fonte sem objeto JSON.")
        key = "vulnerabilities"
        rows = data.get(key)
        if not isinstance(rows, list):
          raise ValueError("Fonte sem lista de vulnerabilidades.")
        info.update(status="ok", returned=len(rows), truncated=name == "nvd" and data.get("totalResults", 0) > len(rows))
        for row in rows:
          if not isinstance(row, dict):
            continue
          if name == "cisa-kev":
            identifier = row.get("cveID", "")
            text = " ".join(str(row.get(k, "")) for k in ("vendorProject", "product", "vulnerabilityName"))
            added = row.get("dateAdded", "")
            try:
              if date.fromisoformat(added) < start.date():
                continue
            except (ValueError, TypeError):
              continue
          else:
            cve = row.get("cve", {})
            if not isinstance(cve, dict):
              continue
            identifier = cve.get("id", "")
            text = " ".join(d.get("value", "") for d in cve.get("descriptions", []) if isinstance(d, dict) and d.get("lang") == "en")
            added = cve.get("lastModified", "")
          if not isinstance(identifier, str) or not CVE.fullmatch(identifier) or not SOFTWARE.search(text):
            continue
          candidate = candidates.setdefault(identifier, {"id": identifier, "title": text[:500],
              "applicability": "not-assessed", "sources": [], "modifiedOrAddedAt": added})
          candidate["sources"].append({"name": name, "url": "https://nvd.nist.gov/vuln/detail/" + identifier})
      except Exception as error:
        info["status"] = "failed"
        info["error"] = type(error).__name__
        if hasattr(error, "code"):
          info["httpStatus"] = error.code
      sources.append(info)
    all_candidates = sorted(candidates.values(), key=lambda row: row["id"])
    return {"sources": sources, "candidates": all_candidates[:settings["maxCandidates"]],
            "candidatesTruncated": len(all_candidates) > settings["maxCandidates"],
            "collectedAt": current.isoformat(), "windowStartedAt": start.isoformat()}
