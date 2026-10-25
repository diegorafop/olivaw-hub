"""Lifecycle Linux, lock de processo e parada cooperativa, sem rede."""
from contextlib import contextmanager
from datetime import datetime, timezone
import json
import os
from pathlib import Path
import subprocess
import sys
import time
from uuid import uuid4

from ms_baley.configuration import load_settings


def now():
  return datetime.now(timezone.utc).isoformat()


def atomic_json(path, value):
  temporary = path.with_name(path.name + "." + uuid4().hex + ".tmp")
  descriptor = os.open(temporary, os.O_WRONLY | os.O_CREAT | os.O_EXCL, 0o600)
  try:
    with os.fdopen(descriptor, "w", encoding="utf-8") as target:
      json.dump(value, target, ensure_ascii=False)
      target.flush()
      os.fsync(target.fileno())
    os.replace(temporary, path)
  finally:
    temporary.unlink(missing_ok=True)


def read_state(path):
  if path.is_symlink():
    raise ValueError("Estado não pode ser link simbólico.")
  try:
    if path.stat().st_size > 65536:
      raise ValueError("Estado local excede limite.")
    value = json.loads(path.read_text(encoding="utf-8"))
    return value if isinstance(value, dict) else {}
  except (FileNotFoundError, json.JSONDecodeError, RecursionError):
    return {}


@contextmanager
def exclusive_lock(path, blocking=False):
  if os.name != "posix":
    raise ValueError("Lifecycle disponível inicialmente em Linux/POSIX.")
  import fcntl
  descriptor = os.open(path, os.O_RDWR | os.O_CREAT | os.O_NOFOLLOW, 0o600)
  try:
    fcntl.flock(descriptor, fcntl.LOCK_EX | (0 if blocking else fcntl.LOCK_NB))
    yield
  finally:
    os.close(descriptor)


class LocalRuntime:
  def __init__(self, settings_path):
    self.path = Path(settings_path).resolve(strict=True)
    self.settings = load_settings(self.path)
    if self.settings["agent"].get("id") != "baley" or self.settings["agent"].get("module") != "ms_baley":
      raise ValueError("Configuração não pertence ao Baley.")
    location = Path(self.settings["localState"]["directory"])
    if location.is_absolute() or ".." in location.parts or location == Path("."):
      raise ValueError("Estado deve ficar em subdiretório do projeto.")
    self.state = self.path.parent / location
    for parent in (self.state, *self.state.parents):
      if parent == self.path.parent:
        break
      if parent.is_symlink():
        raise ValueError("Diretório de estado não pode ser link.")

  def prepare(self):
    self.state.mkdir(mode=0o700, parents=True, exist_ok=True)
    if self.state.stat().st_uid != os.getuid():
      raise ValueError("Estado deve pertencer ao usuário atual.")
    self.state.chmod(0o700)

  def status(self):
    if os.name != "posix":
      return {"agentId": "baley", "running": False, "state": "unsupported-platform"}
    import fcntl
    running = False
    lock = self.state / "worker.lock"
    if lock.is_symlink():
      raise ValueError("Lock não pode ser link.")
    if lock.exists():
      descriptor = os.open(lock, os.O_RDWR | os.O_NOFOLLOW)
      try:
        try:
          fcntl.flock(descriptor, fcntl.LOCK_EX | fcntl.LOCK_NB)
        except BlockingIOError:
          running = True
      finally:
        os.close(descriptor)
    state = read_state(self.state / "status.json")
    state.update(agentId="baley", running=running)
    if not running:
      state["state"] = "stopped"
    elif "state" not in state:
      state["state"] = "starting"
    return state

  def start(self):
    self.prepare()
    with exclusive_lock(self.state / "launch.lock", blocking=True):
      current = self.status()
      if current["running"]:
        return current
      token = uuid4().hex
      allowed = {"PATH", "HOME", "LANG", "LC_ALL", "LC_CTYPE", "TZ", "PYTHONIOENCODING"}
      environment = {key: value for key, value in os.environ.items() if key in allowed}
      environment["PYTHONPATH"] = str(Path(__file__).resolve().parents[2])
      process = subprocess.run(
          [sys.executable, "-m", "ms_baley.worker", "--settings", str(self.path), "--token", token, "--detach"],
          env=environment, cwd=self.path.parent, stdin=subprocess.DEVNULL,
          stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, start_new_session=True, timeout=5)
      if process.returncode:
        raise ValueError("Worker não iniciou; confira configuração e permissões locais.")
      for _ in range(100):
        state = self.status()
        if state.get("running") and state.get("instanceId") == token:
          return state
        time.sleep(0.05)
      atomic_json(self.state / "stop.json", {"instanceId": token})
      raise ValueError("Timeout ao iniciar worker.")

  def stop(self):
    if not self.state.exists():
      return self.status()
    # Mesmo lock de start evita solicitar parada de uma instância que está nascendo.
    with exclusive_lock(self.state / "launch.lock", blocking=True):
      state = self.status()
      if not state["running"]:
        return state
      if not state.get("instanceId"):
        raise ValueError("Worker sem identidade; não é seguro solicitar parada.")
      atomic_json(self.state / "stop.json", {"instanceId": state["instanceId"]})
      for _ in range(100):
        state = self.status()
        if not state["running"]:
          return state
        time.sleep(0.05)
      raise ValueError("Worker não confirmou parada; nenhum PID arbitrário foi encerrado.")
