import argparse
import json

from ms_limpo.bootstrap import compose_example
from ms_limpo.features.execution.commands.create_execution import CreateExecutionCommand
from ms_limpo.features.execution.queries.get_execution import GetExecutionQuery


def main(argv=None):
    parser = argparse.ArgumentParser(description="Template de agentes Python com CQS.")
    commands = parser.add_subparsers(dest="command", required=True)
    example = commands.add_parser("example", help="Executa um exemplo em memória, sem serviços externos.")
    example.add_argument("--name", default="example")
    arguments = parser.parse_args(argv)
    create, get = compose_example()
    try:
        execution = create.handle(CreateExecutionCommand(arguments.name))
    except ValueError as error:
        parser.error(str(error))
    result = get.handle(GetExecutionQuery(execution.id))
    print(json.dumps({"id": str(result.id), "name": result.name,
                      "created_at": result.created_at.isoformat()}, ensure_ascii=False, indent=2))
    return 0
