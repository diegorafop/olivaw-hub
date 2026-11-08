from ms_trevize.core.agent_runtime import AgentRuntime


class StartAgentHandler:
  def __init__(self, runtime: AgentRuntime):
    self.runtime = runtime

  def handle(self):
    return self.runtime.start()
