"""Supervisor local aguardando o contrato de coleta; não executa auditoria/IA."""
import argparse
import os
import signal
import time

from ms_baley.data.local_runtime import LocalRuntime, atomic_json, exclusive_lock, now, read_state


def main(argv=None):
  parser = argparse.ArgumentParser()
  parser.add_argument("--settings", required=True)
  parser.add_argument("--token", required=True)
  parser.add_argument("--detach", action="store_true")
  args = parser.parse_args(argv)
  runtime = LocalRuntime(args.settings)
  if runtime.settings["execution"]["enabled"]:
    raise ValueError("Contrato de execução ainda pendente.")
  runtime.prepare()
  if args.detach:
    # O launcher é processo Python novo, sem threads; o chamador espera sua saída.
    if os.fork():
      return 0
    os.setsid()
    if os.fork():
      os._exit(0)
  stopping = False

  def stop_signal(signum, frame):
    nonlocal stopping
    stopping = True

  signal.signal(signal.SIGTERM, stop_signal)
  signal.signal(signal.SIGINT, stop_signal)
  with exclusive_lock(runtime.state / "worker.lock"):
    state = {"agentId": "baley", "instanceId": args.token, "startedAt": now(),
             "state": "waiting-configuration", "analysisEnabled": False,
             "intervalHours": runtime.settings["schedule"]["intervalHours"],
             "lastAnalysis": None}
    heartbeat = 0.0
    try:
      while not stopping:
        if read_state(runtime.state / "stop.json").get("instanceId") == args.token:
          break
        if time.monotonic() >= heartbeat:
          state["heartbeatAt"] = now()
          atomic_json(runtime.state / "status.json", state)
          heartbeat = time.monotonic() + 2
        time.sleep(0.2)
    finally:
      state.update(state="stopped", stoppedAt=now())
      atomic_json(runtime.state / "status.json", state)
  return 0


if __name__ == "__main__":
  raise SystemExit(main())
