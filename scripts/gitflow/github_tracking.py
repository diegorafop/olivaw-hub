#!/usr/bin/env python3
"""Registra branches nas issues e fecha entregas integradas em master."""
import json
import os
import sys
from pathlib import Path
from urllib.error import HTTPError
from urllib.parse import quote
from urllib.request import Request, urlopen

from tracking import RELEASE_BRANCH, VERSION_HOTFIX, issue_for_branch, referenced_issues, validate_branch


class GitHub:
    def __init__(self, repository, token):
        self.root = f"https://api.github.com/repos/{repository}"
        self.token = token

    def request(self, method, path, payload=None):
        data = None if payload is None else json.dumps(payload).encode()
        request = Request(self.root + path, data=data, method=method, headers={
            "Authorization": "Bearer " + self.token,
            "Accept": "application/vnd.github+json",
            "Content-Type": "application/json",
            "X-GitHub-Api-Version": "2022-11-28",
        })
        with urlopen(request, timeout=30) as response:
            return json.load(response)

    def commits(self, base, head):
        page = 1
        messages = []
        while True:
            comparison = self.request("GET", f"/compare/{quote(base, safe='')}...{quote(head, safe='')}?per_page=100&page={page}")
            commits = comparison["commits"]
            messages.extend(item["commit"]["message"] for item in commits)
            if len(commits) < 100:
                return messages
            page += 1

    def comment_once(self, number, marker, text):
        page = 1
        while True:
            comments = self.request("GET", f"/issues/{number}/comments?per_page=100&page={page}")
            if any(marker in (item.get("body") or "") for item in comments):
                return
            if len(comments) < 100:
                break
            page += 1
        self.request("POST", f"/issues/{number}/comments", {"body": marker + "\n" + text})


def synchronize(api, repository, branch, event, config):
    if event.get("deleted"):
        return
    validate_branch(branch, config["production"], config["development"])
    if branch == config["development"]:
        return
    if branch == config["production"]:
        before = event.get("before", "")
        if not before.strip("0"):
            print("Primeiro push de master: nenhuma issue histórica será fechada.")
            return
        if event.get("forced"):
            raise ValueError("Push forçado em master: fechamento automático suspenso para revisão.")
        issues = referenced_issues(api.commits(before, event["after"]))
        delivery = f"https://github.com/{repository}/commit/{event['after']}"
        for number in issues:
            item = api.request("GET", f"/issues/{number}")
            if "pull_request" in item:
                raise ValueError(f"#{number} é um PR, não uma issue.")
            if item["state"] == "closed":
                continue
            marker = f"<!-- olivaw:delivery:{event['after']} -->"
            api.comment_once(number, marker, f"Código integrado em `master` pelo fluxo de release/hotfix: [commit da entrega]({delivery}). Isso registra a integração do código, não uma implantação na AWS.")
            api.request("PATCH", f"/issues/{number}", {"state": "closed", "state_reason": "completed"})
            print(f"Issue #{number} concluída pela integração em master.")
        return
    issue = issue_for_branch(branch)
    if issue is not None:
        issues = [issue]
    elif VERSION_HOTFIX.fullmatch(branch) or RELEASE_BRANCH.fullmatch(branch):
        issues = referenced_issues(api.commits(config["production"], branch))
        if VERSION_HOTFIX.fullmatch(branch) and not issues:
            raise ValueError("Hotfix sem issue: configure a associação com start_hotfix.py e confira Refs #N nos commits.")
    else:
        issues = []
    for number in issues:
        item = api.request("GET", f"/issues/{number}")
        if "pull_request" in item:
            raise ValueError(f"#{number} é um PR, não uma issue.")
        marker = f"<!-- olivaw:branch:{branch} -->"
        url = f"https://github.com/{repository}/tree/{quote(branch, safe='')}"
        api.comment_once(number, marker, f"Trabalho rastreado no branch [{branch}]({url}). A issue permanece aberta até a integração do código em `master`.")
        print(f"Branch {branch} referenciado na issue #{number}.")


def main():
    event = json.loads(Path(os.environ["GITHUB_EVENT_PATH"]).read_text())
    config = json.loads(Path(__file__).with_name("config.json").read_text())
    api = GitHub(os.environ["GITHUB_REPOSITORY"], os.environ["GH_TOKEN"])
    synchronize(api, os.environ["GITHUB_REPOSITORY"], os.environ["GITHUB_REF_NAME"], event, config)


if __name__ == "__main__":
    try:
        main()
    except HTTPError as error:
        print(f"GitHub retornou HTTP {error.code}. Confira as permissões de Actions e a existência das issues. Nenhum merge ou deploy foi executado.", file=sys.stderr)
        sys.exit(1)
    except (ValueError, OSError, KeyError) as error:
        print(f"Rastreabilidade: {error}", file=sys.stderr)
        sys.exit(1)
