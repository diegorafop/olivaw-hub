"""Programa de coleta remoto: somente leitura, saída agregada e limitada, sem env."""
import collections
import datetime
import json
import re
import subprocess
import time
from ms_baley.data.process_io import bounded_run

WINDOW_HOURS = 6
MAX_LINES = 1200


def command(args, timeout=12):
  try:
    result = bounded_run(args, input="", timeout=timeout, maximum=2097152)
    if result.returncode:
      return {"status": "unavailable", "exitCode": result.returncode}
    # Comandos escolhidos possuem limite próprio; limite adicional de transporte.
    if len(result.stdout.encode()) > 2097152:
      return {"status": "limited"}
    return {"status": "collected", "text": result.stdout}
  except (OSError, ValueError, subprocess.TimeoutExpired):
    return {"status": "unavailable"}


def access_summary(text):
  counts = collections.Counter()
  sources = collections.Counter()
  suspicious = collections.Counter()
  denied = collections.Counter()
  parsed = invalid = 0
  scanner = re.compile(r"/(?:\.env|\.git|\.svn|wp-admin|wp-login|actuator|ecp/|phpmyadmin|vendor/phpunit|server-status|@vite|\.vscode)", re.I)
  for line in text.splitlines()[-MAX_LINES:]:
    try:
      row = json.loads(line)
      # Traefik general logs do not constitute an access event.
      if not isinstance(row, dict) or "RequestMethod" not in row or "DownstreamStatus" not in row:
        continue
      status = int(row["DownstreamStatus"])
      parsed += 1
      counts[str(status)] += 1
      source = str(row.get("ClientHost", "unknown"))
      # Never persist arbitrary URL paths, query strings, headers or user agents.
      if not re.fullmatch(r"[0-9a-fA-F:.]{2,64}", source):
        source = "unknown"
      if status in (401, 403, 429): denied[source] += 1
      path = str(row.get("RequestPath", "")).split("?", 1)[0]
      if scanner.search(path):
        sources[source] += 1
        suspicious["known-scanner-path"] += 1
    except (ValueError, TypeError, KeyError):
      invalid += 1
  return {"events": parsed, "invalidLines": invalid, "statusCounts": dict(counts),
          "suspiciousPatterns": dict(suspicious), "scannerSources": sources.most_common(20),
          "deniedSources": denied.most_common(20), "windowHours": WINDOW_HOURS,
          "maxLines": MAX_LINES, "sampleLimited": len(text.splitlines()) >= MAX_LINES,
          "attribution": "IP observado pelo proxy; não comprova identidade ou origem real."}


def collect():
  import os
  result = {"collectedAt": datetime.datetime.now(datetime.timezone.utc).isoformat(), "coverage": {},
            "host": {}, "containers": [], "access": {}, "authentication": {}}
  disk = os.statvfs("/")
  result["host"]["rootDiskUsedPercent"] = round(100 * (1 - disk.f_bavail / max(1, disk.f_blocks)), 1)
  try:
    memory = dict((key.rstrip(":"), int(value.split()[0])) for key, value in
                  (line.split(":", 1) for line in open("/proc/meminfo")))
    result["host"]["memoryAvailablePercent"] = round(100 * memory["MemAvailable"] / memory["MemTotal"], 1)
  except (OSError, ValueError, KeyError):
    result["coverage"]["memory"] = "unavailable"
  result["host"]["kernel"] = os.uname().release
  listeners = command(["ss", "-H", "-lntu"])
  result["coverage"]["listeners"] = listeners["status"]
  if listeners["status"] == "collected":
    result["host"]["listeners"] = [line.split()[4] for line in listeners["text"].splitlines()[:100] if len(line.split()) > 4]
  policy = command(["sudo", "-n", "/usr/sbin/sshd", "-T"])
  result["coverage"]["sshPolicy"] = policy["status"]
  if policy["status"] == "collected":
    allowed = {"permitrootlogin", "passwordauthentication", "kbdinteractiveauthentication", "allowtcpforwarding", "maxauthtries"}
    result["host"]["sshPolicy"] = dict(line.split(" ", 1) for line in policy["text"].splitlines() if line.split(" ", 1)[0] in allowed)
  selinux = command(["getenforce"])
  result["coverage"]["selinux"] = selinux["status"]
  if selinux["status"] == "collected": result["host"]["selinux"] = selinux["text"].strip()[:20]
  ids = command(["docker", "ps", "-q", "--no-trunc"])
  result["coverage"]["docker"] = ids["status"]
  if ids["status"] == "collected":
    # JSON de campos selecionados: nunca Config.Env, labels, Cmd ou inspect completo.
    template = '{"name":{{json .Name}},"image":{{json .Config.Image}},"user":{{json .Config.User}},"privileged":{{json .HostConfig.Privileged}},"readOnly":{{json .HostConfig.ReadonlyRootfs}},"capAdd":{{json .HostConfig.CapAdd}},"securityOpt":{{json .HostConfig.SecurityOpt}},"ports":{{json .NetworkSettings.Ports}},"log":{{json .HostConfig.LogConfig}},"mounts":{{json .Mounts}},"restartCount":{{json .RestartCount}},"state":{{json .State.Status}}}'
    all_ids = [value for value in ids["text"].splitlines() if re.fullmatch('[a-f0-9]{64}', value)]
    result["coverage"]["containerLimitReached"] = len(all_ids) > 40
    for identifier in all_ids[:40]:
      inspected = command(["docker", "inspect", "--format", template, identifier])
      if inspected["status"] != "collected":
        result["coverage"]["containerInspect"] = "partial"
        continue
      try:
        row = json.loads(inspected["text"])
        # Preserve only mount classifications, not host filenames or client data.
        row["mounts"] = [{"type": mount.get("Type"), "writable": mount.get("RW"),
                          "dockerSocket": mount.get("Destination") == "/var/run/docker.sock"}
                         for mount in row.get("mounts", [])]
        log = row.get("log", {})
        row["log"] = {"type": log.get("Type"), "maxSize": log.get("Config", {}).get("max-size"),
                      "maxFile": log.get("Config", {}).get("max-file")}
        result["containers"].append(row)
        if "traefik" in row["name"].lower():
          logs = command(["docker", "logs", "--since", str(WINDOW_HOURS)+"h", "--tail", str(MAX_LINES), identifier])
          result["coverage"]["accessLogs"] = logs["status"]
          if logs["status"] == "collected": result["access"] = access_summary(logs["text"])
      except (ValueError, TypeError, KeyError):
        result["coverage"]["containerInspect"] = "partial"
  auth = command(["sudo", "-n", "journalctl", "--no-pager", "-u", "sshd", "--since",
                  str(WINDOW_HOURS)+" hours ago", "-n", str(MAX_LINES), "-o", "cat"])
  result["coverage"]["sshLogs"] = auth["status"]
  if auth["status"] == "collected":
    lines = auth["text"].splitlines()
    result["authentication"] = {"failedAttempts": sum(bool(re.search(r"Failed password|Failed publickey|Invalid user|authentication failure", line)) for line in lines),
                                "acceptedSessions": sum("Accepted " in line for line in lines),
                                "sampleLimited": len(lines) >= MAX_LINES, "windowHours": WINDOW_HOURS}
  # Revalidate total payload before crossing the SSH transport.
  encoded = json.dumps(result, ensure_ascii=True)
  if len(encoded) > 100000: raise ValueError("collection-size")
  return encoded


if __name__ == "__main__":
  print(collect())
