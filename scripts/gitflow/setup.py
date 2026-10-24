#!/usr/bin/env python3
"""Ativação local idempotente sem criar branches nem trocar a branch atual."""
import json
import subprocess
from pathlib import Path

root = Path(__file__).resolve().parents[2]
config = json.loads(Path(__file__).with_name("config.json").read_text())


def git(*args, check=True):
    return subprocess.run(["git", "-C", str(root), *args], check=check, text=True, capture_output=True)


try:
    subprocess.run(["git", "flow", "version"], check=True, capture_output=True)
    for key, expected in [("gitflow.branch.master", config["production"]), ("gitflow.branch.develop", config["development"])]:
        actual = git("config", "--get", key, check=False).stdout.strip()
        if actual != expected:
            raise ValueError(f"Configure Git Flow: {key} deve ser {expected}. Execute git flow init e preserve suas branches existentes.")
    existing = git("config", "--get", "core.hooksPath", check=False).stdout.strip()
    if existing not in ("", ".githooks"):
        raise ValueError(f"core.hooksPath já usa {existing}; integre os hooks existentes antes de substituir a configuração.")
    for name in ["commit-msg", "pre-push"]:
        path = root / ".githooks" / name
        path.chmod(path.stat().st_mode | 0o111)
    git("config", "--local", "core.hooksPath", ".githooks")
    print("Hooks ativados. Commits recebem a issue do branch; pushes usam as convenções do projeto.")
except (ValueError, OSError, subprocess.CalledProcessError) as error:
    message = error.stderr.strip() if isinstance(error, subprocess.CalledProcessError) and error.stderr else str(error)
    raise SystemExit(f"Não foi possível ativar o fluxo local: {message}")
