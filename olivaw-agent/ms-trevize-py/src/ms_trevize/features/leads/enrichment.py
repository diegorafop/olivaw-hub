"""Enriquecimento limitado, com preview e preservação da qualificação humana."""
from copy import deepcopy
from uuid import UUID
from datetime import datetime, timezone

CONTACT_FIELDS = ("publicEmails", "contactPageUrl", "contactFormUrl", "contactFormStatus", "contactCheckedAt")


class LeadEnrichment:
  def __init__(self, api, website):
    self.api, self.website = api, website

  def select(self, identities, limit):
    if type(limit) is not int or not 1 <= limit <= 100:
      raise ValueError("Selecione de 1 a 100 leads.")
    if identities:
      return list(dict.fromkeys(str(UUID(item)) for item in identities))[:limit]
    result = []
    # A lista ordena por criação, que não é alterada pelo enriquecimento.
    for page in range(1, 51):
      from urllib.parse import urlencode
      batch = self.api.request("/leads?" + urlencode({"page":page,"pageSize":100,"entry":"prospecting"}))
      items = batch.get("items", [])
      result.extend(item["externalId"] for item in items)
      if len(result) >= limit or not items or len(items) < 100: break
    return list(dict.fromkeys(result))[:limit]

  def run(self, identities=(), limit=3, apply=False):
    result = []
    for identity in self.select(identities, limit):
      current = self.api.request("/leads/" + identity)
      snapshot = current.get("snapshot")
      if not isinstance(snapshot, dict):
        result.append({"id":identity,"status":"unavailable"})
        continue
      original = snapshot.get("qualification")
      if not isinstance(original, dict) or original.get("schemaVersion", 1) != 1:
        result.append({"id":identity,"status":"qualification-required"})
        continue
      evidence = self.website.analyze(original.get("websiteUrl", ""))
      if evidence.get("status") != "checked":
        if apply:
          self.api.request("/leads/" + identity + "/contact-discovery", {
            "revision":snapshot["revision"],"followUpRevision":current.get("followUpRevision",0),
            "researchCompleted":False,"contacts":{"contactFormStatus":"inconclusive",
              "contactCheckedAt":datetime.now(timezone.utc).isoformat()}}, "PUT")
        result.append({"id":identity,"status":"inconclusive","analysis":evidence.get("status")})
        continue
      merged = deepcopy(original)
      existing = merged.get("publicEmails", [])
      emails = list(existing)
      for item in evidence.get("publicEmails", []):
        if len(emails) < 5 and not any(e["email"].casefold() == item["email"].casefold() for e in emails):
          emails.append(item)
      merged["publicEmails"] = emails
      # Um link revisado nunca é apagado por uma leitura parcial ou negativa.
      for field, source in (("contactPageUrl", "contactPage"), ("contactFormUrl", "contactFormUrl")):
        if not merged.get(field): merged[field] = evidence.get(source, "")
      merged["contactFormStatus"] = "found" if merged.get("contactFormUrl") else evidence["contactFormStatus"]
      merged["contactCheckedAt"] = evidence["contactCheckedAt"]
      item = {"id":identity,"status":"preview","contacts":{key:merged.get(key) for key in CONTACT_FIELDS}}
      if apply:
        updated = self.api.request("/leads/" + identity + "/contact-discovery",
          {"revision":snapshot["revision"],"followUpRevision":current.get("followUpRevision",0),
            "researchCompleted":evidence["contactFormStatus"] in ("found","not-detected"),
            "contacts":{key:merged.get(key) for key in CONTACT_FIELDS}}, "PUT")
        if updated.get("conflict"): raise ValueError("Contato alterado durante enriquecimento.")
        confirmed = self.api.request("/leads/" + identity).get("snapshot", {}).get("qualification", {})
        if any(confirmed.get(key) != merged.get(key) for key in CONTACT_FIELDS if key != "contactCheckedAt"):
          raise ValueError("Enriquecimento não confirmado; verifique versão ou concorrência da API.")
        item["status"] = "enriched"
      result.append(item)
    return {"items":result,"applied":apply,"contactPerformed":False}
