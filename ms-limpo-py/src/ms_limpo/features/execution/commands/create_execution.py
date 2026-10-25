from dataclasses import dataclass
from datetime import datetime, timezone
from uuid import uuid4

from ms_limpo.core.execution_repository import ExecutionRepository
from ms_limpo.domain.execution import Execution


@dataclass(frozen=True)
class CreateExecutionCommand:
    name: str


class CreateExecutionHandler:
    def __init__(self, repository: ExecutionRepository):
        self.repository = repository

    def handle(self, command: CreateExecutionCommand) -> Execution:
        execution = Execution(uuid4(), command.name.strip(), datetime.now(timezone.utc))
        self.repository.add(execution)
        return execution
