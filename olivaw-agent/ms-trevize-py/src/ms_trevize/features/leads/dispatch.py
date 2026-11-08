"""Primeiro contato serial: fila pública qualificada e ritmo persistente de dois por hora."""
from datetime import datetime, timezone, timedelta
from urllib.parse import urlencode, urlsplit
from uuid import UUID

from ms_trevize.data.gmail_store import private_json
from ms_trevize.data.local_runtime import atomic_json, exclusive_lock
from ms_trevize.domain.commercial import contact_source
from ms_trevize.domain.mail import address
from ms_trevize.features.leads.outreach import paused
from ms_trevize.features.leads.research import LeadResearch, EXAMPLE_ID


class OutboundDispatcher:
  def __init__(self, crm, store, outreach, website=None, ai=None, settings=None, clock=None):
    self.crm, self.store, self.outreach = crm, store, outreach
    self.website, self.ai = website, ai
    self.settings = settings or {}
    self.clock = clock or (lambda: datetime.now(timezone.utc))

  def _rate(self, intents, now):
    attempts = []
    for intent in intents:
      if intent["content"].get("replyMessageId"): continue
      recorded = intent.get("attemptedAt") or intent.get("confirmedAt")
      if not recorded:
        if intent["state"] in ("sent", "confirmation-pending"):
          return {"status":"paused","reason":"initial-attempt-time-unknown","sent":0}
        continue
      moment = datetime.fromisoformat(recorded.replace("Z", "+00:00"))
      if moment.tzinfo is None: raise ValueError("Tentativa sem fuso horário; revisar diário.")
      attempts.append(moment)
    attempts.sort(reverse=True)
    due = now
    if attempts: due = max(due,attempts[0]+timedelta(minutes=30))
    recent = [moment for moment in attempts if moment > now-timedelta(hours=1)]
    if len(recent) >= 2: due = max(due,recent[1]+timedelta(hours=1))
    if due > now:
      return {"status":"scheduled","reason":"initial-rate-limit","nextInitialAt":due.isoformat(),"sent":0}
    return None

  @staticmethod
  def _recipient(lead):
    snapshot = lead.get("snapshot") or {}
    qualification = snapshot.get("qualification") or {}
    acquisition = snapshot.get("acquisition") or {}
    if snapshot.get("entry") != "prospecting" or acquisition.get("initiative") != "outbound": return None
    if acquisition.get("origin", "").casefold() != "google maps": return None
    if lead.get("externalId") == EXAMPLE_ID or lead.get("followUpStatus") != "new" or paused(snapshot): return None
    tags = {item.casefold() for item in qualification.get("tags", [])}
    if tags & {"teste", "test", "teste interno", "fake", "simulação", "simulation", "opt-out", "do-not-contact", "não contatar", "nao contatar"}: return None
    if (qualification.get("websiteAssessment") or {}).get("basis") == "simulation": return None
    if any(item.get("kind") in ("prepared", "sending", "sent", "received") for item in snapshot.get("communications") or []): return None
    try: emails = {address(item["email"]) for item in qualification.get("publicEmails", [])}
    except (ValueError, KeyError, TypeError): return None
    if len(emails) != 1: return None
    recipient = next(iter(emails))
    primary = snapshot.get("email", "").strip().casefold()
    if primary and primary != recipient: return None
    try:
      source = contact_source(qualification,recipient)
      website = urlsplit(qualification.get("websiteUrl", ""))
      public = urlsplit(source)
    except ValueError: return None
    if website.scheme not in ("https", "http") or website.username or website.password: return None
    if not website.hostname or not public.hostname: return None
    if website.hostname.lower().removeprefix("www.") != public.hostname.lower().removeprefix("www."): return None
    return recipient

  @staticmethod
  def _ready(qualification):
    if not isinstance(qualification.get("approach"),str) or not qualification["approach"].strip(): return False
    if qualification.get("approachStatus") == "reviewed": return True
    assessment = qualification.get("websiteAssessment") or {}
    if assessment.get("basis") not in ("html", "manual", "conversation") or not assessment.get("assessedAt"): return False
    return bool(qualification.get("commercialObservation")) or any(
      type(metric.get("score")) is int and metric.get("confidence") != "unknown" and metric.get("evidence")
      for metric in (assessment.get("metrics") or {}).values())

  def _candidates(self):
    rows = []
    for page in range(1,6):
      batch = self.crm.request("/leads?"+urlencode({"entry":"prospecting","followUpStatus":"new","page":page,"pageSize":100}))
      items = batch.get("items") or []
      rows.extend(items)
      if len(items) < 100: break
    return sorted(rows,key=lambda item:(item.get("createdAt", ""),item["externalId"]))

  def _research_due(self, identity, now):
    path = self.store.root/"dispatch-research.json"
    history = private_json(path) if path.exists() else {}
    if identity in history:
      last = datetime.fromisoformat(history[identity].replace("Z", "+00:00"))
      if last > now-timedelta(hours=24): return False
    if len(history) >= 1000 and identity not in history: return False
    # Reserva anterior à chamada impede repetir pesquisa paga após queda do processo.
    history[identity] = now.isoformat()
    atomic_json(path,history)
    return True

  def run(self):
    if not all(self.settings.get(key) is True for key in ("enabled", "sendingEnabled", "automaticInitials")):
      return {"status":"paused","reason":"initial-policy-disabled","sent":0}
    config = self.store.config()
    if not config.get("sendingEnabled"):
      return {"status":"paused","reason":"gmail-sending-disabled","sent":0}
    self.store.prepare()
    with exclusive_lock(self.store.root/"dispatch.lock"):
      now = self.clock()
      intents = self.store.intents()
      attempts_today = sum(bool(item.get("attemptedAt", "").startswith(now.date().isoformat())) for item in intents)
      if attempts_today >= config.get("dailyLimit",500):
        tomorrow = now.replace(hour=0,minute=0,second=0,microsecond=0)+timedelta(days=1)
        return {"status":"scheduled","reason":"daily-sending-limit","nextInitialAt":tomorrow.isoformat(),"sent":0}
      limited = self._rate(intents,now)
      if limited: return limited
      used = {item["content"]["to"] for item in intents
        if not item["content"].get("replyMessageId") and item["state"] != "cancelled"}
      used_leads = {item["content"].get("leadId") for item in intents
        if not item["content"].get("replyMessageId") and item["state"] != "cancelled"}
      researched = False
      checked = 0
      for row in self._candidates():
        identity = str(UUID(row["externalId"]))
        if identity in used_leads: continue
        try:
          lead = self.outreach.lead(identity)
          checked += 1
          recipient = self._recipient(lead)
          if not recipient or recipient in used: continue
          self.outreach.eligible(lead,recipient)
          qualification = lead["snapshot"]["qualification"]
          if not self._ready(qualification):
            if researched or self.website is None or self.ai is None: continue
            if not self._research_due(identity,now): continue
            researched = True
            LeadResearch(self.crm,self.website,self.ai).run(identity,apply=True)
            lead = self.outreach.lead(identity)
            if self._recipient(lead) != recipient: continue
            qualification = lead["snapshot"]["qualification"]
            if not self._ready(qualification): continue
          subject = "Uma ideia para o site da " + lead["snapshot"].get("business", "empresa")[:120]
          prepared = self.outreach.prepare(identity,recipient,subject,qualification["approach"],True)
          if self.store.get(prepared["id"])["state"] != "prepared":
            return {"status":"paused","reason":"initial-already-started","sent":0,"leadId":identity}
          result = self.outreach.send(prepared["id"],True)
          return {"status":"completed","sent":int(result["state"] == "sent"),"leadId":identity,
            "intentId":prepared["id"],"transportState":result["state"],"nextInitialAt":(self.clock()+timedelta(minutes=30)).isoformat()}
        except (OSError, ValueError, KeyError, TypeError):
          # Uma reserva/preparação pode ter sido aceita; não selecionar outro lead neste ciclo.
          return {"status":"partial","reason":"initial-not-confirmed-review-required","sent":0,"leadId":identity}
      return {"status":"idle","reason":"no-qualified-public-contact","sent":0,"checked":checked,"researched":researched}
