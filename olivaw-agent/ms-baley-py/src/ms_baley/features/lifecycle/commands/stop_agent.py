from ms_baley.core.runtime import AgentRuntime


class StopAgentHandler:
  def __init__(self, runtime: AgentRuntime):
    self.runtime = runtime

  def handle(self):
    return self.runtime.stop()
