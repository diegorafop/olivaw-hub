from ms_baley.core.runtime import AgentRuntime


class ReadStatusHandler:
  def __init__(self, runtime: AgentRuntime):
    self.runtime = runtime

  def handle(self):
    return self.runtime.status()
