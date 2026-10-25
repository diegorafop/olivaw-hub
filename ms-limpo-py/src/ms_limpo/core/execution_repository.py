from typing import Protocol
from uuid import UUID

from ms_limpo.domain.execution import Execution


class ExecutionRepository(Protocol):
    def add(self, execution: Execution) -> None: ...

    def get(self, execution_id: UUID) -> Execution | None: ...
