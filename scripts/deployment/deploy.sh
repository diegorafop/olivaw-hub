#!/usr/bin/env bash
# Cliente de deploy: autentica no Olivaw e acompanha o job, sem executar SSH local.
set +x
set -euo pipefail
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
export OLIVAW_REPO_ROOT="$(cd -- "$SCRIPT_DIR/../.." && pwd)"
export OLIVAW_API_URL="${OLIVAW_API_URL:-https://api.olivaw.com.br}"
export DEVOPS_API_URL="${DEVOPS_API_URL:-https://api.devops.olivaw.com.br}"
exec python3 - "$@" <<'PYTHON'
import argparse
import http.client
import json
import os
import re
import sys
import time
import urllib.error
import urllib.request
import uuid
from pathlib import Path
from urllib.parse import urlsplit

parser = argparse.ArgumentParser(description="Implanta um serviço do Olivaw pelo ms-devops-api.")
parser.add_argument("service", nargs="?", help="Nome do serviço ms-* no monorepo ou repositório independente como lisierp")
parser.add_argument("environment", nargs="?", choices=["dev", "qas", "rls", "stg", "prd"])
parser.add_argument("branch", nargs="?", help="Branch já publicada no GitHub")
parser.add_argument("--no-wait", action="store_true", help="Retorna após obter o ID do job")
parser.add_argument("--cleanup", action="store_true", help="Solicita limpeza de disco no EC2")
parser.add_argument("--job", help="Retoma somente o acompanhamento de um job existente, sem solicitar outro")
args = parser.parse_args()
if args.job:
    try:
        args.job = str(uuid.UUID(args.job))
    except ValueError:
        parser.error("Informe um UUID válido em --job.")
    if args.no_wait or args.environment or args.branch:
        parser.error("--job não recebe ambiente, branch ou --no-wait.")
if args.cleanup:
    if any((args.service, args.environment, args.branch)):
        parser.error("A limpeza não recebe serviço, ambiente ou branch.")
    folder = None
    job_path = "/disk-cleanups"
    credential_service = "ms-devops-api"
    print("Destino: EC2 Olivaw | operação: limpeza de disco", flush=True)
else:
    if not args.service or (not args.job and not all((args.environment, args.branch))):
        parser.error("Informe serviço, ambiente e branch.")
    job_path = "/deployments"
    if not re.fullmatch(r"[a-zA-Z0-9_-]+", args.service):
        parser.error("Informe somente o nome da pasta, como ms-novo-servico-api ou lisierp.")
    credential_service = args.service
    folder = f"olivaw/{args.service}" if args.service.startswith("ms-") else args.service
    if not args.job and (len(args.branch) > 200 or not re.fullmatch(r"[a-zA-Z0-9][a-zA-Z0-9._/-]*", args.branch)
            or ".." in args.branch or args.branch == "HEAD"
            or any(not part or part.startswith(".") or part.endswith(".lock") for part in args.branch.split("/"))):
        parser.error("Branch inválida.")
    if args.job:
        print(f"Retomando acompanhamento: {args.job} | serviço: {args.service}", flush=True)
    else:
        print(f"Destino: {folder} | ambiente: {args.environment} | branch: {args.branch}", flush=True)
base = os.environ["DEVOPS_API_URL"].rstrip("/")
repo_root = Path(os.environ["OLIVAW_REPO_ROOT"]).resolve()
external_services_file = Path(os.environ.get(
    "OLIVAW_EXTERNAL_SERVICES_FILE",
    repo_root / "scripts" / "deployment" / "external-services.json",
))

def service_directory(service):
    local = repo_root / service
    if local.is_dir():
        return local
    try:
        configured = json.loads(external_services_file.read_text(encoding="utf-8"))
    except (OSError, ValueError):
        configured = {}
    if not isinstance(configured, dict):
        configured = {}
    if service == "lisierp" and os.environ.get("OLIVAW_LISIERP_SERVICE_DIR"):
        configured[service] = os.environ["OLIVAW_LISIERP_SERVICE_DIR"]
    relative_path = configured.get(service) if isinstance(configured, dict) else None
    if not isinstance(relative_path, str) or not relative_path:
        raise RuntimeError(f"Serviço {service} não foi encontrado no monorepo nem em {external_services_file}.")
    directory = Path(relative_path)
    if not directory.is_absolute():
        directory = repo_root / directory
    directory = directory.resolve()
    if not directory.is_dir():
        raise RuntimeError(f"Diretório local do serviço {service} não existe: {directory}.")
    return directory

def read_olivaw_credentials(service):
    path = service_directory(service) / "deployment" / "olivaw-credentials.env"
    try:
        lines = path.read_text(encoding="utf-8").splitlines()
    except OSError:
        raise RuntimeError(f"Credenciais do Olivaw não encontradas para {service}: {path}.") from None
    values = {}
    for line in lines:
        line = line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, value = line.split("=", 1)
        values[key.strip()] = value.strip()
    login = values.get("OLIVAW_LOGIN")
    password = values.get("OLIVAW_PASSWORD")
    if not login or not password:
        raise RuntimeError(f"O arquivo de credenciais do Olivaw para {service} precisa definir OLIVAW_LOGIN e OLIVAW_PASSWORD.")
    return {"login": login, "password": password}

url = urlsplit(base)
if url.scheme != "https" or not url.netloc or url.username or url.password or url.query or url.fragment:
    sys.exit("DEVOPS_API_URL deve ser uma URL HTTPS sem credenciais, query ou fragmento.")

auth_base = os.environ["OLIVAW_API_URL"].rstrip("/")
auth_url = urlsplit(auth_base)
if auth_url.scheme != "https" or not auth_url.netloc or auth_url.username or auth_url.password or auth_url.query or auth_url.fragment:
    sys.exit("OLIVAW_API_URL deve ser uma URL HTTPS sem credenciais, query ou fragmento.")

credentials = {}

class ApiFailure(RuntimeError):
    def __init__(self, status_code, path, detail):
        self.status_code = status_code
        super().__init__(f"HTTP {status_code} em {path}: {detail}")

def request(path, payload=None, token=None, endpoint=None):
    headers = {"Accept": "application/json"}
    data = None
    if payload is not None:
        headers["Content-Type"] = "application/json"
        data = json.dumps(payload).encode()
    if token:
        headers["Authorization"] = f"Bearer {token}"
    call = urllib.request.Request((endpoint or base) + path, data=data, headers=headers)
    try:
        # Esta função não repete chamadas. Somente o acompanhamento repete GETs seguros.
        with urllib.request.urlopen(call, timeout=30) as response:
            raw = response.read(2097153)
            if len(raw) > 2097152:
                raise ValueError("Resposta excede o limite do cliente.")
            return json.loads(raw)
    except urllib.error.HTTPError as error:
        messages = []
        try:
            problem = json.loads(error.read(65536))
            if isinstance(problem, dict):
                errors = problem.get("errors")
                if isinstance(errors, dict):
                    for field, values in errors.items():
                        if isinstance(values, str):
                            values = [values]
                        if isinstance(values, list):
                            messages.extend(f"{field}: {value}" for value in values if isinstance(value, str))
                if not messages:
                    for key in ("detail", "title"):
                        if isinstance(problem.get(key), str):
                            messages.append(problem[key])
        except (ValueError, OSError):
            pass
        detail = " | ".join(messages) or "A API não retornou detalhes JSON."
        for secret in (token, (payload or {}).get("password"), credentials.get("password")):
            if secret:
                detail = detail.replace(secret, "[oculto]")
        detail = "".join(character for character in detail if character.isprintable())[:2000]
        raise ApiFailure(error.code, path, detail) from None
    except (urllib.error.URLError, TimeoutError, ValueError, OSError, http.client.HTTPException):
        raise RuntimeError(f"Falha de comunicação ou resposta inválida em {path}. Se o POST já foi enviado, confira o job antes de repetir.") from None

def authenticate():
    login = request("/identity/authentication/login", credentials, endpoint=auth_base)
    if not isinstance(login, dict):
        raise RuntimeError("Login não retornou uma resposta válida.")
    token = login.get("accessToken")
    if not token:
        raise RuntimeError("Login não retornou accessToken.")
    return token

def interrupted(job_id, reason):
    print(f"Não foi possível concluir o acompanhamento do job {job_id}: {reason}", flush=True)
    print(f"O job pode continuar no executor. Acompanhe pelo ID: {base}{job_path}/{job_id}", flush=True)
    command = "--cleanup" if args.cleanup else args.service
    print(f"Retome sem criar outro job: bash scripts/deployment/deploy.sh {command} --job {job_id}", flush=True)
    print("Não envie outro deploy para este mesmo trabalho.", flush=True)
    return 2

def watch_job(job_id, token):
    deadline = time.monotonic() + 1800
    previous = None
    previous_log = ""
    started = time.monotonic()
    heartbeat = started
    failures = 0
    refreshed = False
    transient = {408, 425, 429, 500, 502, 503, 504}
    while time.monotonic() < deadline:
        try:
            status = request(f"{job_path}/{job_id}", token=token)
            if (not isinstance(status, dict) or status.get("status") not in {"queued", "running", "succeeded", "failed"}
                    or status.get("id", job_id) != job_id):
                raise RuntimeError("Resposta de acompanhamento inválida.")
        except ApiFailure as error:
            if error.status_code == 401 and not refreshed:
                try:
                    token = authenticate()
                except RuntimeError:
                    return interrupted(job_id, "não foi possível renovar a autenticação")
                refreshed = True
                print("Autenticação renovada; consultando o mesmo job, sem reenviar a implantação.", flush=True)
                continue
            if error.status_code == 403:
                print("Confira a permissão devops.deploy:<pasta> e se o executor da EC2 retorna a pasta do job.", flush=True)
            if error.status_code not in transient:
                return interrupted(job_id, f"HTTP {error.status_code}")
            failures += 1
        except RuntimeError:
            failures += 1
        else:
            failures = 0
            refreshed = False
        if failures:
            if failures >= 6:
                return interrupted(job_id, "seis falhas consecutivas ao consultar o status")
            delay = min(15, 3 * 2 ** (failures - 1), max(0, deadline - time.monotonic()))
            print(f"Falha temporária ao consultar o job {job_id} ({failures}/6). Nova consulta em {int(delay)}s; nenhuma implantação será reenviada.", flush=True)
            time.sleep(delay)
            continue
        state = status.get("status")
        log = status.get("log", "")
        if isinstance(log, str) and log != previous_log:
            addition = log[len(previous_log):] if log.startswith(previous_log) else log
            for line in addition.strip().splitlines():
                print(line, flush=True)
            previous_log = log
        if time.monotonic() - heartbeat >= 15:
            print(f"Acompanhando job {job_id}: {int(time.monotonic() - started)}s decorridos.", flush=True)
            heartbeat = time.monotonic()
        if state != previous:
            print(f"Status: {state}", flush=True)
            previous = state
        if state == "succeeded":
            print("Job concluído pelo executor.")
            return 0
        if state == "failed":
            raise RuntimeError(f"Job {job_id} falhou. Consulte o log pelo endpoint acima.")
        if state not in {"queued", "running"}:
            raise RuntimeError(f"Estado inesperado no job {job_id}.")
        time.sleep(3)
    return interrupted(job_id, "tempo de acompanhamento esgotado")

try:
    credentials = read_olivaw_credentials(credential_service)
    token = authenticate()
    if args.job:
        job_id = args.job
    else:
        payload = {} if args.cleanup else {"folder": folder, "branch": args.branch, "environment": args.environment}
        job = request(job_path, payload, token)
        if not isinstance(job, dict) or not job.get("id"):
            raise RuntimeError("Deploy não retornou o ID do job. Confira o servidor antes de reenviar.")
        job_id = job["id"]
        print(f"Job solicitado: {job_id}", flush=True)
    print(f"Consulta: {base}{job_path}/{job_id}", flush=True)
    if args.no_wait:
        sys.exit(0)
    sys.exit(watch_job(job_id, token))
except RuntimeError as error:
    sys.exit(str(error))
PYTHON
