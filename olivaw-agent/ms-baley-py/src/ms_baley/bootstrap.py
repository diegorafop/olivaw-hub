from ms_baley.data.audit_service import AuditService
from ms_baley.features.audit.commands.run_audit import RunAuditHandler
from ms_baley.data.local_runtime import LocalRuntime
from ms_baley.data.update_review import UpdateReview
from ms_baley.features.lifecycle.commands.start_agent import StartAgentHandler
from ms_baley.features.lifecycle.commands.stop_agent import StopAgentHandler
from ms_baley.features.lifecycle.queries.read_status import ReadStatusHandler
from ms_baley.features.skill_update.commands.update_skill import UpdateSkillHandler


def compose(settings_path):
  runtime = LocalRuntime(settings_path)
  return {"start": StartAgentHandler(runtime), "stop": StopAgentHandler(runtime),
          "status": ReadStatusHandler(runtime),
          "audit": RunAuditHandler(runtime, AuditService(runtime)),
          "update": UpdateSkillHandler(UpdateReview(runtime))}
