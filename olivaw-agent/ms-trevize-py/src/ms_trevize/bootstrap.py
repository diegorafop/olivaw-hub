"""Composição explícita dos adaptadores de um ciclo."""
from ms_trevize.data.local_runtime import LocalRuntime
from ms_trevize.data.batch_state import BatchState
from ms_trevize.data.maps_collector import MapsCollector
from ms_trevize.data.website_review import WebsiteReview
from ms_trevize.data.ai_qualification import AIQualification
from ms_trevize.data.lead_api import LeadApi
from ms_trevize.features.prospecting.commands.create_prospect_batch import CreateProspectBatchHandler


def compose(path, preview_limit=None):
  runtime = LocalRuntime(path)
  settings = runtime.settings
  if preview_limit is not None:
    if type(preview_limit) is not int or not 1 <= preview_limit <= 5:
      raise ValueError("Prévia limitada a 1–5 candidatos.")
    settings["search"]["limit"] = preview_limit
    settings["leadApi"]["enabled"] = False
    settings["ai"]["enabled"] = False
    runtime.prepare()
  skill_path = (runtime.path.parent/settings["skill"]["path"]).resolve(strict=True)
  if skill_path.stat().st_size>32768: raise ValueError("Skill excede limite.")
  return CreateProspectBatchHandler(BatchState(runtime),
    MapsCollector(settings["search"], min(300,settings["execution"]["timeoutSeconds"])),
    WebsiteReview(settings["website"]), AIQualification(settings["ai"],skill_path.read_text()),
    LeadApi(settings["leadApi"],runtime.path.parent))


def run_outreach(path, directory=None):
  runtime = LocalRuntime(path)
  settings = runtime.settings
  if not settings["outreach"]["enabled"]:
    return {"status":"paused","reason":"outreach-disabled","sent":0}
  if not settings["leadApi"]["enabled"]:
    return {"status":"paused","reason":"crm-disabled","sent":0}
  from ms_trevize.data.gmail_store import GmailStore
  from ms_trevize.data.gmail_client import GoogleGmail
  from ms_trevize.data.reply_advisor import ReplyAdvisor
  from ms_trevize.features.leads.outreach import LeadOutreach
  from ms_trevize.features.leads.campaign import Campaign
  store = GmailStore(directory or runtime.path.parent/".state"/"gmail")
  if not (store.root/"campaign.json").exists() or not (store.root/"token.json").exists():
    return {"status":"paused","reason":"campaign-or-oauth-required","sent":0}
  from ms_trevize.data.gmail_store import private_json
  if not private_json(store.root/"campaign.json").get("active"):
    return {"status":"paused","reason":"campaign-disabled","sent":0}
  crm = LeadApi(settings["leadApi"],runtime.path.parent); crm.login()
  ai = ReplyAdvisor({**settings["ai"],"enabled":settings["outreach"]["aiEnabled"]})
  service = LeadOutreach(crm,store,GoogleGmail.connect(store),ai)
  return Campaign(store,service,settings["outreach"]).run()
