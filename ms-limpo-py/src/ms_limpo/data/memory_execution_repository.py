from uuid import UUID

from ms_limpo.domain.execution import Execution


class MemoryExecutionRepository:
    """Adaptador demonstrativo; os dados duram somente durante o processo."""

    def __init__(self):
        self.executions: dict[UUID, Execution] = {}

    def add(self, execution: Execution) -> None:
        if execution.id in self.executions:
            raise ValueError("Execução já cadastrada.")
        self.executions[execution.id] = execution

    def get(self, execution_id: UUID) -> Execution | None:
        return self.executions.get(execution_id)
