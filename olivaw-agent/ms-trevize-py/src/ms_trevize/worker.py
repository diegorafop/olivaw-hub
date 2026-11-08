"""Três especialistas paralelos, com início manual e agendas independentes."""
import argparse
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone, timedelta
import os
import signal
import time
from ms_trevize.bootstrap import compose, run_initial_outreach, run_followup
from ms_trevize.features.prospecting.commands.create_prospect_batch import CollectionFailure
from ms_trevize.data.local_runtime import LocalRuntime, atomic_json, exclusive_lock, now, read_state


class Specialists:
  """Uma tarefa por especialista; uma coleta lenta não segura leitura ou contato."""
  def __init__(self, executor, jobs, clock=time.monotonic, once=()):
    self.executor, self.jobs, self.clock = executor, jobs, clock
    self.pending = {}
    self.once, self.finished = set(once), set()
    self.next_due = {name:clock() for name in jobs}
    self.states = {name:{"state":"scheduled","nextAt":now()} for name in jobs}

  def tick(self):
    completed = {}
    for name,(operation,interval,enabled) in self.jobs.items():
      future = self.pending.get(name)
      if future is not None and future.done():
        try: outcome = future.result()
        except CollectionFailure as error: outcome = {"status":"failed","phase":error.phase}
        except Exception: outcome = {"status":"failed","reason":"check-configuration","sent":0}
        completed[name] = outcome
        self.finished.add(name)
        del self.pending[name]
        self.states[name].update(state="scheduled",lastResult=outcome)
      if not enabled or (name in self.once and name in self.finished):
        self.states[name]["state"] = "disabled"
        continue
      stamp = self.clock()
      if name not in self.pending and stamp >= self.next_due[name]:
        self.pending[name] = self.executor.submit(operation)
        self.next_due[name] = stamp+interval
        self.states[name].update(state="running",startedAt=now(),
          nextAt=(datetime.now(timezone.utc)+timedelta(seconds=interval)).isoformat())
    return completed


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
    settings = runtime.settings
    state={"agentId":"trevize","instanceId":args.token,"startedAt":now(),"state":"scheduled",
      "autoStart":False,"intervalHours":settings["schedule"]["intervalHours"],"lastCollection":None,
      "ai":{"model":settings["ai"].get("model"),"reasoningEffort":settings["ai"].get("reasoningEffort")}}
    executor=ThreadPoolExecutor(max_workers=3,thread_name_prefix="trevize-commercial")
    jobs = {
      "prospecting":(lambda:compose(args.settings).handle(),settings["schedule"]["intervalHours"]*3600,settings["execution"]["enabled"]),
      "first-contact":(lambda:run_initial_outreach(args.settings),settings["outreach"]["initialPollSeconds"],settings["outreach"]["enabled"] and settings["outreach"]["automaticInitials"]),
      "follow-up":(lambda:run_followup(args.settings),settings["outreach"]["pollSeconds"],settings["outreach"]["enabled"]),
    }
    specialists = Specialists(executor,jobs,once=() if settings["schedule"]["enabled"] else ("prospecting",))
    heartbeat=0
    atomic_json(runtime.state/"status.json",state)
    try:
      while not stopping:
        if read_state(runtime.state/"stop.json").get("instanceId")==args.token:break
        completed = specialists.tick()
        if "prospecting" in completed:
          state["lastCollection"] = completed["prospecting"]
          state["nextCollectionAt"] = specialists.states["prospecting"]["nextAt"]
        state["specialists"] = specialists.states
        state["outreach"] = {name:details.get("lastResult") for name,details in specialists.states.items() if name != "prospecting"}
        state["state"] = "collecting" if specialists.states["prospecting"]["state"] == "running" else "scheduled"
        if not settings["execution"]["enabled"] and not settings["outreach"]["enabled"]: state["state"]="waiting-configuration"
        if time.monotonic()>=heartbeat:
          state["heartbeatAt"]=now();atomic_json(runtime.state/"status.json",state);heartbeat=time.monotonic()+2
        time.sleep(.2)
    finally:
      state.update(state="stopping",stopRequested=True);atomic_json(runtime.state/"status.json",state)
      executor.shutdown(wait=True,cancel_futures=True)
      state.update(state="stopped",stoppedAt=now());atomic_json(runtime.state/"status.json",state)
  return 0


if __name__=="__main__":raise SystemExit(main())
