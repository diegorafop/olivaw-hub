"""Subprocessos com tempo, saída e grupo de processos limitados."""
import os
import selectors
import signal
import subprocess
import time


def bounded_run(args, *, input, timeout, maximum=262144, cwd=None, env=None, **unused):
  process = subprocess.Popen(args, stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                             stderr=subprocess.DEVNULL, cwd=cwd, env=env, start_new_session=True)
  output = bytearray()
  pending = memoryview(input.encode())
  selector = selectors.DefaultSelector()
  os.set_blocking(process.stdin.fileno(), False)
  os.set_blocking(process.stdout.fileno(), False)
  selector.register(process.stdin, selectors.EVENT_WRITE)
  selector.register(process.stdout, selectors.EVENT_READ)
  deadline = time.monotonic() + timeout
  try:
    while selector.get_map():
      remaining = deadline - time.monotonic()
      if remaining <= 0: raise subprocess.TimeoutExpired(args, timeout)
      for key, mask in selector.select(min(remaining, 0.5)):
        if key.fileobj is process.stdin:
          if pending:
            try:
              count = os.write(process.stdin.fileno(), pending[:4096])
              pending = pending[count:]
            except BrokenPipeError:
              pending = memoryview(b'')
          if not pending:
            selector.unregister(process.stdin); process.stdin.close()
        else:
          data = os.read(process.stdout.fileno(), 8192)
          if data:
            output.extend(data)
            if len(output) > maximum: raise ValueError("Saída de processo excedeu limite.")
          else:
            selector.unregister(process.stdout); process.stdout.close()
    code = process.wait(timeout=max(0.01, deadline-time.monotonic()))
    return subprocess.CompletedProcess(args, code, output.decode('utf-8', errors='replace'))
  finally:
    selector.close()
    if process.poll() is None:
      os.killpg(process.pid, signal.SIGKILL)
    process.wait()
    for stream in (process.stdin, process.stdout):
      if not stream.closed: stream.close()
