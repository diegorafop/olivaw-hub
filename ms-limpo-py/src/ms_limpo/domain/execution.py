from dataclasses import dataclass
from datetime import datetime
from uuid import UUID


@dataclass(frozen=True)
class Execution:
    id: UUID
    name: str
    created_at: datetime

    def __post_init__(self):
        if not self.name.strip():
            raise ValueError("O nome da execução é obrigatório.")
