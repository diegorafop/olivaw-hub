from ms_limpo.data.memory_execution_repository import MemoryExecutionRepository
from ms_limpo.features.execution.commands.create_execution import CreateExecutionHandler
from ms_limpo.features.execution.queries.get_execution import GetExecutionHandler


def compose_example():
    repository = MemoryExecutionRepository()
    return CreateExecutionHandler(repository), GetExecutionHandler(repository)
