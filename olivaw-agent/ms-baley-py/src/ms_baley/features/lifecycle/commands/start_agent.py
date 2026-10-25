from ms_baley.core.runtime import AgentRuntime


class StartAgentHandler:
  def __init__(self, runtime: AgentRuntime):
    self.runtime = runtime

  def handle(self):
    return self.runtime.start()
