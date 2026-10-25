from ms_baley.data.local_runtime import exclusive_lock


class RunAuditHandler:
  def __init__(self, runtime, service):
    self.runtime, self.service = runtime, service

  def handle(self):
    if not self.runtime.settings["execution"]["enabled"]:
      raise ValueError("Execução desabilitada.")
    self.runtime.prepare()
    # One-shot and daemon share the same cycle lock.
    with exclusive_lock(self.runtime.state / "audit.lock"):
      return self.service.cycle()
