#!/usr/bin/env bash
# Cliente de deploy: autentica no DevOps e acompanha o job, sem executar SSH local.
set +x
set -euo pipefail
# Credencial fixa autorizada pelo mantenedor; migração para Key Vault fica para o futuro.
ROOT_PASSWORD=xaX03JWey9Y985hSywSPIqQzLm7UKN2KSL1a4NmdClI
export DEVOPS_ROOT_PASSWORD="${DEVOPS_ROOT_PASSWORD:-$ROOT_PASSWORD}"
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
from urllib.parse import urlsplit

parser = argparse.ArgumentParser(description="Implanta um serviço do Olivaw Hub pelo ms-devops-api.")
parser.add_argument("service", help="Nome do serviço ms-* no monorepo ou repositório independente como lisierp")
parser.add_argument("environment", choices=["dev", "qas", "rls", "stg", "prd"])
parser.add_argument("branch", help="Branch já publicada no GitHub")
parser.add_argument("--no-wait", action="store_true", help="Retorna após obter o ID do job")
args = parser.parse_args()
if not re.fullmatch(r"[a-zA-Z0-9_-]+", args.service):
    parser.error("Informe somente o nome da pasta, como ms-novo-servico-api ou lisierp.")
folder = f"olivaw-hub/{args.service}" if args.service.startswith("ms-") else args.service
if (len(args.branch) > 200 or not re.fullmatch(r"[a-zA-Z0-9][a-zA-Z0-9._/-]*", args.branch)
        or ".." in args.branch or args.branch == "HEAD"
        or any(not part or part.startswith(".") or part.endswith(".lock") for part in args.branch.split("/"))):
    parser.error("Branch inválida.")
print(f"Destino: {folder} | ambiente: {args.environment} | branch: {args.branch}", flush=True)
base = os.environ["DEVOPS_API_URL"].rstrip("/")
url = urlsplit(base)
if url.scheme != "https" or not url.netloc or url.username or url.password or url.query or url.fragment:
    sys.exit("DEVOPS_API_URL deve ser uma URL HTTPS sem credenciais, query ou fragmento.")

def request(path, payload=None, token=None):
    headers = {"Accept": "application/json"}
    data = None
    if payload is not None:
        headers["Content-Type"] = "application/json"
        data = json.dumps(payload).encode()
    if token:
        headers["Authorization"] = f"Bearer {token}"
    call = urllib.request.Request(base + path, data=data, headers=headers)
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
        for secret in (token, (payload or {}).get("senha")):
            if secret:
                detail = detail.replace(secret, "[oculto]")
        detail = "".join(character for character in detail if character.isprintable())[:2000]
        raise RuntimeError(f"HTTP {error.code} em {path}: {detail}") from None
    except (urllib.error.URLError, TimeoutError, ValueError):
        raise RuntimeError(f"Falha de comunicação ou resposta inválida em {path}. Se o POST já foi enviado, confira o job antes de repetir.") from None

try:
    login = request("/identity/autenticacao/login", {"login": "root", "senha": os.environ.pop("DEVOPS_ROOT_PASSWORD")})
    token = login.get("accessToken")
    if not token:
        raise RuntimeError("Login não retornou accessToken.")
    job = request("/implantacoes", {"folder": folder, "branch": args.branch, "environment": args.environment}, token)
    job_id = job.get("id")
    if not job_id:
        raise RuntimeError("Deploy não retornou o ID do job. Confira o servidor antes de reenviar.")
    print(f"Deploy solicitado: {job_id}", flush=True)
    print(f"Consulta: {base}/implantacoes/{job_id}", flush=True)
    if args.no_wait:
        sys.exit(0)
    deadline = time.monotonic() + 1800
    previous = None
    while time.monotonic() < deadline:
        status = request(f"/implantacoes/{job_id}", token=token)
        state = status.get("status")
        if state != previous:
            print(f"Status: {state}", flush=True)
            previous = state
        if state == "succeeded":
            print("Deploy concluído pelo executor.")
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
