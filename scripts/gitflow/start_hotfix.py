#!/usr/bin/env python3
"""Inicia hotfix versionada com Git Flow e registra sua issue nos hooks locais."""
import subprocess
import sys

from tracking import VERSION_HOTFIX

if len(sys.argv) != 3:
    raise SystemExit("Uso: python3 scripts/gitflow/start_hotfix.py NUMERO_ISSUE VERSAO")
number, version = sys.argv[1:]
if not number.isdigit() or int(number) < 1:
    raise SystemExit("Informe uma issue positiva.")
branch = "hotfix/" + version
if not VERSION_HOTFIX.fullmatch(branch):
    raise SystemExit("Use versão estável, por exemplo 1.0.1.")
subprocess.run(["git", "flow", "hotfix", "start", version], check=True)
subprocess.run(["git", "config", "--local", f"branch.{branch}.olivaw-issue", str(int(number))], check=True)
print(f"{branch} associada à issue #{int(number)}; faça os commits e use git flow hotfix finish {version}.")
