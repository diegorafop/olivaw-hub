"""Auditoria SSH limitada, relatório evolutivo e entrega autenticada pela API."""
from datetime import datetime, timezone
import hashlib
import http.client
import json
import os
from pathlib import Path
import re
import stat
import subprocess
import time
from urllib.parse import urlsplit
from uuid import uuid4

from ms_baley.data.local_runtime import atomic_json, now
from ms_baley.data.process_io import bounded_run
from ms_baley.data.ai_review import AIReview


def private_file(path):
  path = Path(path).expanduser()
  if path.is_symlink() or not path.is_file() or path.stat().st_uid != os.getuid():
    raise ValueError("Arquivo privado inválido.")
  if stat.S_IMODE(path.stat().st_mode) & 0o077:
    raise ValueError("Arquivo privado exige permissão 0600 ou mais restrita.")
  return path


def findings_for(evidence, expected_ports=(22, 80, 443)):
  findings = []
  def add(rule, title, severity, detail, recommendation):
    findings.append({"ruleId": rule, "title": title, "severity": severity, "confidence": "confirmed",
                     "evidence": detail, "recommendation": recommendation})
  host = evidence.get("host", {})
  if host.get("rootDiskUsedPercent", 0) >= 85:
    add("host.disk-pressure", "Disco próximo do limite", "high", str(host["rootDiskUsedPercent"])+"% ocupado", "Revisar crescimento e retenção sem apagar volumes.")
  if host.get("memoryAvailablePercent", 100) < 10:
    add("host.memory-pressure", "Pouca memória disponível", "medium", str(host["memoryAvailablePercent"])+"% disponível", "Investigar consumo e capacidade da VM.")
  if host.get("sshPolicy", {}).get("passwordauthentication") == "yes":
    add("ssh.password", "SSH permite autenticação por senha", "high", "Política efetiva passwordauthentication=yes", "Revisar acesso por chaves antes de restringir senhas.")
  if host.get("sshPolicy", {}).get("permitrootlogin") == "yes":
    add("ssh.root", "SSH permite login root", "high", "Política efetiva permitrootlogin=yes", "Restringir acesso administrativo após validar recuperação.")
  if host.get("selinux") in ("Permissive", "Disabled"):
    add("host.selinux", "SELinux sem aplicação de políticas", "low", host["selinux"], "Avaliar políticas compatíveis com os serviços.")
  for listener in host.get("listeners", []):
    if not isinstance(listener, dict): continue  # Compatibility with older stored reports.
    if listener.get("address") not in ("0.0.0.0", "::", "*"): continue
    port = listener.get("port")
    if listener.get("protocol") == "tcp" and port in expected_ports: continue
    sensitive = port in (2375, 2376, 5432, 3306, 6379, 5672, 15672, 27017, 9200, 11211, 111, 161)
    add("host.sensitive-listener" if sensitive else "host.unexpected-listener", "Serviço sensível escutando nas interfaces do host" if sensitive else "Porta não prevista escutando nas interfaces do host",
        "high" if sensitive else "medium", str(listener.get("protocol"))+"/"+str(port),
        "Verificar necessidade e regras de firewall/Security Group. Listener não comprova acesso pela internet.")
  firewall = host.get("firewallIPv4", {})
  if firewall.get("inputPolicy") == "ACCEPT" and firewall.get("inputRuleCount") == 0:
    add("host.firewall-input", "Cadeia INPUT IPv4 sem regras de entrada", "medium", "INPUT ACCEPT, sem regras nesta cadeia.",
        "Revisar proteção do host e Security Group; coordenar regras preservando SSH, Docker e sites. Não comprova exposição na AWS.")
  if host.get("imdsTokenlessStatus") == 200:
    add("host.imdsv1", "Metadados EC2 aceitam requisição sem token", "high", "GET de índice de metadados sem token retornou 200, sem ler credenciais.",
        "Avaliar exigir IMDSv2 após conferir dependências da instância.")
  for name, metadata in host.get("permissions", {}).items():
    mode = metadata.get("mode", 0)
    if name == "dockerSocket" and mode & 0o002:
      add("host.socket-permissions", "Socket Docker permite escrita a outros usuários", "critical", "Permissão de escrita global no socket.", "Restringir acesso ao daemon a identidades administrativas autorizadas.")
    if name in ("sshDirectory", "authorizedKeys") and mode & 0o022:
      add("host.ssh-permissions", "Permissões de escrita amplas no acesso SSH", "high", name+" modo "+oct(mode), "Revisar proprietário e remover escrita de grupo/outros sem invalidar o acesso legítimo.")
  for control, value in host.get("kernelControls", {}).items():
    if value == 0:
      add("host.kernel-links", "Proteção de links do kernel desabilitada", "medium", control+"=0", "Avaliar habilitar a proteção de links após validar compatibilidade.")
  for container in evidence.get("containers", []):
    name = container.get("name", "container")
    if container.get("privileged"):
      add("docker.privileged", "Container privilegiado", "critical", name, "Remover privilégios desnecessários após validar requisitos.")
    if "SYS_ADMIN" in (container.get("capAdd") or []):
      add("docker.capabilities", "Capability administrativa adicional", "high", name, "Revisar necessidade de SYS_ADMIN.")
    if any(mount.get("dockerSocket") for mount in container.get("mounts", [])):
      add("docker.socket", "Container acessa o socket Docker", "high", name, "Avaliar proxy de socket com operações mínimas; mount somente leitura não limita a API.")
    log = container.get("log", {})
    if log.get("type") == "json-file" and not log.get("maxSize"):
      add("docker.logs", "Log Docker sem limite explícito de tamanho", "medium", name, "Configurar rotação de tamanho e quantidade preservando a aplicação.")
    if container.get("restartCount", 0) >= 5:
      add("docker.restarts", "Container com reinícios recorrentes", "medium", name, "Correlacionar falhas de aplicação, memória e health checks.")
    for port, bindings in (container.get("ports") or {}).items():
      if any(binding.get("HostIp") in ("", "0.0.0.0", "::") for binding in (bindings or [])) and port.split('/')[0] in ("2375", "2376", "5432", "6379", "5672", "15672"):
        add("docker.published-service", "Serviço sensível publicado no host", "high", name+" "+port, "Verificar Security Group e necessidade de exposição; publicação não prova alcance externo.")
  access = evidence.get("access", {})
  if access.get("suspiciousPatterns"):
    add("access.scanning", "Acessos a caminhos comuns de varredura", "medium", str(sum(access["suspiciousPatterns"].values()))+" eventos na amostra", "Correlacionar IP e status; respostas negadas não comprovam invasão.")
  if sum(count for status, count in access.get("statusCounts", {}).items() if status.startswith("5")) >= 10:
    add("access.server-errors", "Respostas 5xx recorrentes", "medium", "Ao menos 10 erros na amostra", "Correlacionar proxy, aplicação e recursos da VM.")
  if evidence.get("authentication", {}).get("failedAttempts", 0) >= 10:
    add("ssh.failed-logins", "Tentativas de autenticação SSH recorrentes", "medium", str(evidence["authentication"]["failedAttempts"])+" eventos", "Revisar origens e controles de acesso sem atribuir autoria pelo IP.")
  return findings


class ReportHTTPError(ValueError):
  def __init__(self, status):
    super().__init__("Report API request failed")
    self.status = status


class ReportClient:
  def __init__(self, settings, project):
    self.settings, self.project = settings, project

  def request(self, path, payload, token=None, method="POST"):
    endpoint = urlsplit(self.settings["baseUrl"])
    if endpoint.scheme != "https" or endpoint.hostname != "api.olivaw.com.br" or endpoint.port not in (None, 443) or endpoint.path not in ("", "/") or endpoint.query or endpoint.fragment or endpoint.username:
      raise ValueError("API de destino não autorizada.")
    connection = http.client.HTTPSConnection(endpoint.hostname, timeout=20)
    headers = {"Content-Type": "application/json", "Accept": "application/json"}
    if token: headers["Authorization"] = "Bearer " + token
    try:
      connection.request(method, path, None if payload is None else json.dumps(payload, ensure_ascii=False).encode(), headers)
      response = connection.getresponse()
      data = response.read(131073)
      if len(data) > 131072: raise ValueError("Resposta excede limite.")
      if response.status not in (200, 201):
        raise ReportHTTPError(response.status)
      return json.loads(data)
    finally:
      connection.close()

  def send(self, report):
    path = private_file(self.project / self.settings["credentialsPath"])
    credentials = json.loads(path.read_text())[self.settings["credentialsSection"]]
    # Autenticar em cada lote; JWT permanece apenas em memória.
    auth = self.request("/identity/authentication/login", {"login": credentials["login"], "password": credentials["password"]})
    token = auth.get("accessToken") or auth.get("token")
    if not isinstance(token, str) or not token or len(token) > 32768:
      raise ValueError("Login sem token compatível.")
    response = self.request("/security-reports", {"rawData": json.dumps(report, ensure_ascii=False, separators=(",", ":"))}, token)
    if response.get("runId") != report["runId"] or not response.get("externalId"):
      raise ValueError("Confirmação de entrega incompatível.")
    return response


class AuditService:
  def __init__(self, runtime, runner=None, client=None):
    self.runtime = runtime
    self.settings = runtime.settings
    self.runner = runner or bounded_run
    self.client = client or ReportClient(self.settings.get("reportApi", {}), runtime.path.parent)

  def collect(self):
    ssh = self.settings["ssh"]
    if not ssh.get("enabled") or ssh.get("strictHostKeyChecking") is not True:
      raise ValueError("SSH exige habilitação e validação estrita de host.")
    if not re.fullmatch(r"[a-zA-Z0-9.-]{1,253}", ssh["host"]) or ssh["host"].startswith('-') or not re.fullmatch(r"[a-z_][a-z0-9_-]{0,31}", ssh["user"]):
      raise ValueError("Alvo SSH inválido.")
    key = private_file(self.runtime.path.parent / ssh["privateKeyPath"])
    known = Path(ssh["knownHostsPath"]).expanduser()
    if not known.is_file(): raise ValueError("known_hosts obrigatório.")
    window = self.settings["audit"]["logWindowHours"]
    source = Path(__file__).with_name("remote_probe.py").read_text().replace("WINDOW_HOURS = 6", "WINDOW_HOURS = " + str(window))
    source = Path(__file__).with_name("process_io.py").read_text() + "\n" + source.replace("from ms_baley.data.process_io import bounded_run", "")
    args = ["ssh", "-F", "/dev/null", "-T", "-o", "BatchMode=yes", "-o", "IdentitiesOnly=yes",
            "-o", "StrictHostKeyChecking=yes", "-o", "UserKnownHostsFile="+str(known),
            "-o", "ConnectTimeout=10", "-o", "ConnectionAttempts=1", "-o", "ServerAliveInterval=15",
            "-o", "ServerAliveCountMax=2", "-i", str(key), "-p", str(ssh["port"]),
            ssh["user"]+"@"+ssh["host"], "python3", "-s", "-"]
    result = self.runner(args, input=source, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL,
                         text=True, timeout=min(self.settings["execution"]["timeoutSeconds"], 180))
    if result.returncode or len(result.stdout.encode()) > 120000:
      raise ValueError("Coleta SSH falhou ou excedeu limite.")
    evidence = json.loads(result.stdout)
    if not isinstance(evidence, dict): raise ValueError("Coleta incompatível.")
    return evidence

  def audit(self):
    report = {"schemaVersion": "1.0", "runId": str(uuid4()), "agentId": "baley",
              "hostId": self.settings["audit"]["hostId"], "startedAt": now()}
    skill = (self.runtime.path.parent / self.settings["skill"]["path"]).resolve()
    report["skill"] = {"name": "safety-docker", "sha256": hashlib.sha256(skill.read_bytes()).hexdigest()}
    try:
      evidence = self.collect()
      findings = findings_for(evidence, self.settings["audit"].get("expectedPublicTcpPorts", (22, 80, 443)))
      coverage = evidence.get("coverage", {})
      coverage["accessSampleLimitReached"] = evidence.get("access", {}).get("sampleLimited", False)
      coverage["sshSampleLimitReached"] = evidence.get("authentication", {}).get("sampleLimited", False)
      limitations = [key+": "+str(value) for key, value in coverage.items() if value not in ("collected", False)]
      if not evidence.get("access", {}).get("events"):
        limitations.append("Sem eventos de access log observados; não comprova ausência de tráfego.")
      limitations.extend(["Amostra limitada de logs; intervalos podem se sobrepor.",
                          "Não inclui exploração, varredura de CVEs, revisão de código ou Security Groups AWS.",
                          "Listeners mostram interfaces locais; alcance externo, regras nftables nativas e firewall IPv6 não foram comprovados."])
      if not evidence.get("access", {}).get("events"):
        coverage["accessLogObservation"] = "partial"
      report.update(executionStatus="partial" if any(value is True or value in ("unavailable", "limited", "partial") for value in coverage.values()) else "completed",
                    result={"summary": {"findingCount": len(findings), "containerCount": len(evidence.get("containers", []))},
                            "coverage": coverage, "findings": findings, "limitations": limitations, "evidence": evidence})
    except (OSError, ValueError, KeyError, TypeError, subprocess.TimeoutExpired):
      report.update(executionStatus="failed", result={"summary": {}, "coverage": {}, "findings": [],
                    "limitations": ["Coleta SSH indisponível; nenhuma conclusão sobre a segurança do alvo."]})
    try:
      report["ai"] = AIReview(self.settings["ai"]).analyze(report, skill.read_text())
    except (OSError, ValueError, KeyError, TypeError, IndexError, subprocess.TimeoutExpired):
      report["ai"] = {"status": "failed", "provider": self.settings["ai"].get("provider")}
      report["result"]["limitations"].append("Interpretação IA indisponível; achados determinísticos preservados.")
      if report["executionStatus"] == "completed": report["executionStatus"] = "partial"
    report["completedAt"] = now()
    return report

  def prune(self):
    self.runtime.prepare()
    directory = self.runtime.state / "outbox"
    if directory.is_symlink(): raise ValueError("Fila não pode ser link simbólico.")
    directory.mkdir(mode=0o700, exist_ok=True)
    directory.chmod(0o700)
    files = sorted(directory.glob("*.json"), key=lambda path: path.stat().st_mtime)
    total = sum(path.stat().st_size for path in files)
    dropped = 0
    for path in files:
      if time.time() - path.stat().st_mtime > self.settings["localState"]["retentionDays"] * 86400 or total > self.settings["localState"]["maxBytes"]:
        total -= path.stat().st_size
        path.unlink()
        dropped += 1
    return dropped

  def cycle(self):
    self.runtime.prepare()
    dropped = self.prune()
    report = self.audit()
    encoded = json.dumps(report, ensure_ascii=False).encode()
    if len(encoded) > 220000: raise ValueError("Relatório excede limite.")
    path = self.runtime.state / "outbox" / (report["runId"]+".json")
    atomic_json(path, report)
    dropped += self.prune()
    delivered = 0
    delivery = "pending"
    if self.settings["reportApi"]["enabled"]:
      for pending in sorted(path.parent.glob("*.json"), key=lambda item: item.stat().st_mtime)[:4]:
        if pending.is_symlink(): raise ValueError("Fila não pode conter links.")
        try:
          self.client.send(json.loads(pending.read_text()))
          pending.unlink()
          delivered += 1
          if pending == path: delivery = "delivered"
        except (OSError, ValueError, KeyError, TypeError, http.client.HTTPException):
          break
    return {"runId": report["runId"], "executionStatus": report["executionStatus"],
            "completedAt": report["completedAt"], "delivery": delivery, "deliveredCount": delivered,
            "pendingCount": len(list(path.parent.glob("*.json"))), "droppedCount": dropped}
