#!/usr/bin/env bash
# Cliente de deploy: autentica no Hub e acompanha o job, sem executar SSH local.
set +x
set -euo pipefail
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
export OLIVAW_REPO_ROOT="$(cd -- "$SCRIPT_DIR/../.." && pwd)"
export HUB_API_URL="${HUB_API_URL:-https://api.olivaw-hub.duckdns.org}"
export DEVOPS_API_URL="${DEVOPS_API_URL:-https://api.devops.olivaw-hub.duckdns.org}"
exec python3 - "$@" <<'PYTHON'
import argparse
import json
import os
import re
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path
from urllib.parse import urlsplit

parser = argparse.ArgumentParser(description="Implanta um serviço do Olivaw Hub pelo ms-devops-api.")
parser.add_argument("service", nargs="?", help="Nome do serviço ms-* no monorepo ou repositório independente como lisierp")
parser.add_argument("environment", nargs="?", choices=["dev", "qas", "rls", "stg", "prd"])
parser.add_argument("branch", nargs="?", help="Branch já publicada no GitHub")
parser.add_argument("--no-wait", action="store_true", help="Retorna após obter o ID do job")
parser.add_argument("--cleanup", action="store_true", help="Solicita limpeza de disco no EC2")
args = parser.parse_args()
if args.cleanup:
    if any((args.service, args.environment, args.branch)):
        parser.error("A limpeza não recebe serviço, ambiente ou branch.")
    folder = None
    job_path = "/disk-cleanups"
    credential_service = "ms-devops-api"
    print("Destino: EC2 Olivaw Hub | operação: limpeza de disco", flush=True)
else:
    if not all((args.service, args.environment, args.branch)):
        parser.error("Informe serviço, ambiente e branch.")
    job_path = "/deployments"
    if not re.fullmatch(r"[a-zA-Z0-9_-]+", args.service):
        parser.error("Informe somente o nome da pasta, como ms-novo-servico-api ou lisierp.")
    credential_service = args.service
    folder = f"olivaw-hub/{args.service}" if args.service.startswith("ms-") else args.service
    if (len(args.branch) > 200 or not re.fullmatch(r"[a-zA-Z0-9][a-zA-Z0-9._/-]*", args.branch)
            or ".." in args.branch or args.branch == "HEAD"
            or any(not part or part.startswith(".") or part.endswith(".lock") for part in args.branch.split("/"))):
        parser.error("Branch inválida.")
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

def read_hub_credentials(service):
    path = service_directory(service) / "deployment" / "hub-credentials.env"
    try:
        lines = path.read_text(encoding="utf-8").splitlines()
    except OSError:
        raise RuntimeError(f"Credenciais do Hub não encontradas para {service}: {path}.") from None
    values = {}
    for line in lines:
        line = line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, value = line.split("=", 1)
        values[key.strip()] = value.strip()
    login = values.get("HUB_LOGIN")
    password = values.get("HUB_PASSWORD")
    if not login or not password:
        raise RuntimeError(f"O arquivo de credenciais do Hub para {service} precisa definir HUB_LOGIN e HUB_PASSWORD.")
    return {"login": login, "password": password}

url = urlsplit(base)
if url.scheme != "https" or not url.netloc or url.username or url.password or url.query or url.fragment:
    sys.exit("DEVOPS_API_URL deve ser uma URL HTTPS sem credenciais, query ou fragmento.")

auth_base = os.environ["HUB_API_URL"].rstrip("/")
auth_url = urlsplit(auth_base)
if auth_url.scheme != "https" or not auth_url.netloc or auth_url.username or auth_url.password or auth_url.query or auth_url.fragment:
    sys.exit("HUB_API_URL deve ser uma URL HTTPS sem credenciais, query ou fragmento.")

credentials = {}

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
        # Sem retries automáticos: repetir POST pode criar outro job.
        with urllib.request.urlopen(call, timeout=30) as response:
            return json.load(response)
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
        raise RuntimeError(f"HTTP {error.code} em {path}: {detail}") from None
    except (urllib.error.URLError, TimeoutError, ValueError):
        raise RuntimeError(f"Falha de comunicação ou resposta inválida em {path}. Se o POST já foi enviado, confira o job antes de repetir.") from None

try:
    credentials = read_hub_credentials(credential_service)
    login = request("/identity/authentication/login", credentials, endpoint=auth_base)
    token = login.get("accessToken")
    if not token:
        raise RuntimeError("Login não retornou accessToken.")
    payload = {} if args.cleanup else {"folder": folder, "branch": args.branch, "environment": args.environment}
    job = request(job_path, payload, token)
    job_id = job.get("id")
    if not job_id:
        raise RuntimeError("Deploy não retornou o ID do job. Confira o servidor antes de reenviar.")
    print(f"Job solicitado: {job_id}", flush=True)
    print(f"Consulta: {base}{job_path}/{job_id}", flush=True)
    if args.no_wait:
        sys.exit(0)
    deadline = time.monotonic() + 1800
    previous = None
    previous_log = ""
    started = time.monotonic()
    heartbeat = started
    while time.monotonic() < deadline:
        status = request(f"{job_path}/{job_id}", token=token)
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
            sys.exit(0)
        if state == "failed":
            raise RuntimeError(f"Job {job_id} falhou. Consulte o log pelo endpoint acima.")
        if state not in {"queued", "running"}:
            raise RuntimeError(f"Estado inesperado no job {job_id}.")
        time.sleep(3)
    raise RuntimeError(f"Tempo de acompanhamento esgotado. O job {job_id} pode continuar no servidor.")
except RuntimeError as error:
    sys.exit(str(error))
PYTHON
