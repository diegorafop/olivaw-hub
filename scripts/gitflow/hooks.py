#!/usr/bin/env python3
"""Hooks locais; nenhuma chamada de rede ou alteração do índice."""
import json
import subprocess
import sys
from pathlib import Path

from tracking import issue_for_branch, reference_commit, validate_branch

CONFIG = json.loads(Path(__file__).with_name("config.json").read_text())


def current_branch():
    return subprocess.check_output(["git", "branch", "--show-current"], text=True).strip()


def commit_message(path):
    issue = issue_for_branch(current_branch())
    if issue is None:
        value = subprocess.run(["git", "config", "--get", f"branch.{current_branch()}.olivaw-issue"], capture_output=True, text=True).stdout.strip()
        if value.isdigit():
            issue = int(value)
    if issue is None:
        return
    message = Path(path)
    original = message.read_bytes()
    updated = reference_commit(original.decode("utf-8"), issue).encode("utf-8")
    if original != updated:
        message.write_bytes(updated)


def pre_push(lines):
    for line in lines:
        local_ref, local_sha, remote_ref, remote_sha = line.split()
        if not local_sha.strip("0"):
            continue
        if remote_ref.startswith("refs/tags/"):
            continue
        if not remote_ref.startswith("refs/heads/"):
            continue
        branch = remote_ref.removeprefix("refs/heads/")
        validate_branch(branch, CONFIG["production"], CONFIG["development"])


def main():
    if sys.argv[1] == "commit-msg":
        commit_message(sys.argv[2])
    elif sys.argv[1] == "pre-push":
        pre_push(sys.stdin)
    else:
        raise ValueError("Hook desconhecido.")


if __name__ == "__main__":
    try:
        main()
    except (ValueError, OSError, subprocess.CalledProcessError) as error:
        print(f"Olivaw Git Flow: {error}", file=sys.stderr)
        sys.exit(1)
