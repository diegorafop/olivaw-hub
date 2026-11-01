"""Composição explícita dos adaptadores de um ciclo."""
from ms_trevize.data.local_runtime import LocalRuntime
from ms_trevize.data.batch_state import BatchState
from ms_trevize.data.maps_collector import MapsCollector
from ms_trevize.data.website_review import WebsiteReview
from ms_trevize.data.ai_qualification import AIQualification
from ms_trevize.data.lead_api import LeadApi
from ms_trevize.features.prospecting.commands.create_prospect_batch import CreateProspectBatchHandler


def compose(path):
  runtime = LocalRuntime(path)
  settings = runtime.settings
  skill_path = (runtime.path.parent/settings["skill"]["path"]).resolve(strict=True)
  if skill_path.stat().st_size>32768: raise ValueError("Skill excede limite.")
  return CreateProspectBatchHandler(BatchState(runtime),
    MapsCollector(settings["search"], min(300,settings["execution"]["timeoutSeconds"])),
    WebsiteReview(settings["website"]), AIQualification(settings["ai"],skill_path.read_text()),
    LeadApi(settings["leadApi"],runtime.path.parent))
