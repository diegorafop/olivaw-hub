"""CLI local; importar, consultar status ou instalar não inicia o agente."""
import argparse
import json
from pathlib import Path
import sys
from ms_trevize.data.local_runtime import LocalRuntime
from ms_trevize.features.lifecycle.commands.start_agent import StartAgentHandler
from ms_trevize.features.lifecycle.commands.stop_agent import StopAgentHandler
from ms_trevize.features.lifecycle.queries.read_status import ReadStatusHandler


def main(argv=None):
  arguments = list(sys.argv[1:] if argv is None else argv)
  if arguments and arguments[0] == "gmail":
    from ms_trevize.features.gmail.cli import main as gmail_main
    return gmail_main(arguments[1:])
  parser=argparse.ArgumentParser(description="Trevize — prospecção e qualificação de Leads")
  parser.add_argument("operation",choices=("start","stop","status","collect","audit","update"))
  parser.add_argument("--settings",type=Path,default=Path(__file__).resolve().parents[2]/"setting.json")
  args=parser.parse_args(argv)
  try:
    runtime=LocalRuntime(args.settings)
    if args.operation in ("start","stop","status"):
      result={"start":StartAgentHandler,"stop":StopAgentHandler,"status":ReadStatusHandler}[args.operation](runtime).handle()
    elif args.operation in ("update", "audit"):
      result={"agentId":"trevize","status":"not-applicable","message":"Operação não se aplica ao Trevize; nada foi alterado."}
    else:
      from ms_trevize.bootstrap import compose
      runtime.prepare()
      from ms_trevize.data.local_runtime import exclusive_lock
      with exclusive_lock(runtime.state/"worker.lock"):
        from uuid import uuid4
        from ms_trevize.data.local_runtime import atomic_json, now
        state={"agentId":"trevize","instanceId":uuid4().hex,"state":"collecting","autoStart":False,"startedAt":now()}
        atomic_json(runtime.state/"status.json",state)
        try:
          result=compose(args.settings).handle()
        finally:
          state.update(state="stopped",stoppedAt=now())
          atomic_json(runtime.state/"status.json",state)
    print(json.dumps(result,ensure_ascii=False,indent=2))
    return 1 if result.get("status") in ("failed","partial") else 0
  except (OSError,ValueError,KeyError,TypeError,ImportError):
    print(json.dumps({"agentId":"trevize","operation":args.operation,"status":"failed",
      "message":"Confira configuração, dependências, autenticação e disponibilidade das fontes; nenhum contato comercial foi realizado."},ensure_ascii=False,indent=2),file=sys.stderr)
    return 1
