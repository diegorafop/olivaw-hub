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
