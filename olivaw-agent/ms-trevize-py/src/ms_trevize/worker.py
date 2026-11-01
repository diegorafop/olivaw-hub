"""Worker manual: primeiro ciclo imediato, demais 24h após o término anterior."""
import argparse
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone, timedelta
import os
import signal
import time
from ms_trevize.bootstrap import compose
from ms_trevize.domain.schedule import Schedule
from ms_trevize.data.local_runtime import LocalRuntime, atomic_json, exclusive_lock, now, read_state


def main(argv=None):
  parser=argparse.ArgumentParser()
  parser.add_argument("--settings",required=True);parser.add_argument("--token",required=True)
  parser.add_argument("--detach",action="store_true")
  args=parser.parse_args(argv)
  runtime=LocalRuntime(args.settings);runtime.prepare()
  if args.detach:
    if os.fork():return 0
    os.setsid()
    if os.fork():os._exit(0)
  stopping=False
  def stop_signal(signum,frame):
    nonlocal stopping
    stopping=True
    atomic_json(runtime.state/"stop.json",{"instanceId":args.token})
  signal.signal(signal.SIGTERM,stop_signal);signal.signal(signal.SIGINT,stop_signal)
  with exclusive_lock(runtime.state/"worker.lock"):
    state={"agentId":"trevize","instanceId":args.token,"startedAt":now(),"state":"scheduled",
      "autoStart":False,"intervalHours":runtime.settings["schedule"]["intervalHours"],"lastCollection":None}
    schedule=Schedule(runtime.settings["schedule"]["intervalHours"], time.monotonic(), runtime.settings["schedule"]["enabled"])
    heartbeat=0;future=None
    atomic_json(runtime.state/"status.json",state)
    executor=ThreadPoolExecutor(max_workers=1)
    try:
      while not stopping:
        if read_state(runtime.state/"stop.json").get("instanceId")==args.token:break
        if future is not None and future.done():
          try:state["lastCollection"]=future.result()
          except Exception:state["lastCollection"]={"status":"failed","error":"collection-failed-check-configuration"}
          future=None
          hours=runtime.settings["schedule"]["intervalHours"]
          schedule.finish(time.monotonic())
          state.update(state="scheduled",nextCollectionAt=(datetime.now(timezone.utc)+timedelta(hours=hours)).isoformat())
        enabled=runtime.settings["execution"]["enabled"]
        if future is None and enabled and schedule.due(time.monotonic()):
          future=executor.submit(compose(args.settings).handle)
          state.update(state="collecting",nextCollectionAt=None)
        if not enabled:state["state"]="waiting-configuration"
        if time.monotonic()>=heartbeat:
          state["heartbeatAt"]=now();atomic_json(runtime.state/"status.json",state);heartbeat=time.monotonic()+2
        time.sleep(.2)
    finally:
      state.update(state="stopping",stopRequested=True);atomic_json(runtime.state/"status.json",state)
      executor.shutdown(wait=True,cancel_futures=True)
      state.update(state="stopped",stoppedAt=now());atomic_json(runtime.state/"status.json",state)
  return 0


if __name__=="__main__":raise SystemExit(main())
