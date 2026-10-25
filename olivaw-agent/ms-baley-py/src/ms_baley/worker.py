"""Supervisor com execução serial, timer monotônico e parada cooperativa."""
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
             "state": "scheduled" if runtime.settings["execution"]["enabled"] else "waiting-configuration",
             "analysisEnabled": runtime.settings["execution"]["enabled"],
             "intervalHours": runtime.settings["schedule"]["intervalHours"],
             "lastAnalysis": None}
    heartbeat = 0.0
    next_analysis = time.monotonic()
    import concurrent.futures
    from ms_baley.data.audit_service import AuditService
    from ms_baley.features.audit.commands.run_audit import RunAuditHandler
    executor = concurrent.futures.ThreadPoolExecutor(max_workers=1)
    future = None
    try:
      while not stopping:
        if read_state(runtime.state / "stop.json").get("instanceId") == args.token:
          break
        if future is not None and future.done():
          try:
            state["lastAnalysis"] = future.result()
          except Exception:
            state["lastAnalysis"] = {"executionStatus": "failed", "error": "audit-cycle-failed"}
          future = None
          next_analysis = time.monotonic() + runtime.settings["schedule"]["intervalHours"] * 3600
          from datetime import datetime, timezone, timedelta
          state["nextAnalysisAt"] = (datetime.now(timezone.utc) + timedelta(hours=runtime.settings["schedule"]["intervalHours"])).isoformat()
          state["state"] = "scheduled" if runtime.settings["schedule"]["enabled"] else "idle"
        if future is None and state["analysisEnabled"] and time.monotonic() >= next_analysis:
          if runtime.settings["schedule"]["enabled"] or state["lastAnalysis"] is None:
            future = executor.submit(RunAuditHandler(runtime, AuditService(runtime)).handle)
            state["state"] = "auditing"
            state["nextAnalysisAt"] = None
        if time.monotonic() >= heartbeat:
          state["heartbeatAt"] = now()
          atomic_json(runtime.state / "status.json", state)
          heartbeat = time.monotonic() + 2
        time.sleep(0.2)
    finally:
      executor.shutdown(wait=True, cancel_futures=True)
      state.update(state="stopped", stoppedAt=now())
      atomic_json(runtime.state / "status.json", state)
  return 0


if __name__ == "__main__":
  raise SystemExit(main())
