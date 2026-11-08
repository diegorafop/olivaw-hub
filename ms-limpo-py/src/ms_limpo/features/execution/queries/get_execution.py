from dataclasses import dataclass
from uuid import UUID

from ms_limpo.core.execution_repository import ExecutionRepository
from ms_limpo.domain.execution import Execution


@dataclass(frozen=True)
class GetExecutionQuery:
    execution_id: UUID


class GetExecutionHandler:
    def __init__(self, repository: ExecutionRepository):
        self.repository = repository

    def handle(self, query: GetExecutionQuery) -> Execution | None:
        return self.repository.get(query.execution_id)
