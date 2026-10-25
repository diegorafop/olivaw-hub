import argparse
import json
from pathlib import Path
import sys

from ms_baley.bootstrap import compose
from ms_baley.presentation import review_text


def main(argv=None):
  parser = argparse.ArgumentParser(description="Baley — supervisor local e atualização revisável da skill safety-docker.")
  parser.add_argument("operation", choices=("start", "stop", "status", "update"))
  parser.add_argument("--settings", type=Path, default=Path(__file__).resolve().parents[2] / "setting.json")
  parser.add_argument("--format", choices=("json", "text"), default="json")
  args = parser.parse_args(argv)
  try:
    result = compose(args.settings)[args.operation].handle()
    print(review_text(result) if args.format == "text" else json.dumps(result, ensure_ascii=False, indent=2))
    return 1 if result.get("status") in ("failed", "partial") else 0
  except (OSError, ValueError, KeyError, TypeError) as error:
    # Não imprimir resposta remota, traceback ou conteúdo do setting.
    print(json.dumps({"agentId": "baley", "operation": args.operation,
                      "error": type(error).__name__, "message": "Operação não concluída; confira configuração, permissões e limites."}), file=sys.stderr)
    return 1
