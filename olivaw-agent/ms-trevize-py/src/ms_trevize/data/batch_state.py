"""Estado local privado; mantém IDs para deduplicar e apenas o último resumo."""
from datetime import datetime, timezone
import json
import time
from ms_trevize.data.local_runtime import atomic_json, read_state


class BatchState:
  def __init__(self, runtime):
    self.runtime = runtime
    self.settings = runtime.settings

  def prepare(self):
    self.runtime.prepare()
    path=self.runtime.state/"last-cycle.json"
    if path.exists() and time.time()-path.stat().st_mtime > self.settings["localState"]["retentionDays"]*86400:
      path.unlink()

  def seen(self):
    path=self.runtime.state/"seen.json"
    if not path.exists():return {}
    if path.is_symlink() or path.stat().st_size>1048576:raise ValueError("Índice local inválido.")
    value=json.loads(path.read_text())
    if not isinstance(value,dict) or len(value)>5000:raise ValueError("Índice local inválido.")
    return value

  def remember(self,seen,key):
    seen[key]=datetime.now(timezone.utc).isoformat()
    limited=dict(sorted(seen.items(),key=lambda item:item[1])[-5000:])
    maximum=min(1048576,self.settings["localState"]["maxBytes"]-65536)
    while len(json.dumps(limited).encode())>maximum:
      del limited[next(iter(limited))]
    atomic_json(self.runtime.state/"seen.json",limited)

  def stopping(self):
    stop=read_state(self.runtime.state/"stop.json")
    status=read_state(self.runtime.state/"status.json")
    return bool(stop.get("instanceId") and stop.get("instanceId")==status.get("instanceId"))

  def save(self,report):
    atomic_json(self.runtime.state/"last-cycle.json",report)
