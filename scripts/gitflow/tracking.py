#!/usr/bin/env python3
"""Convenções de rastreabilidade compartilhadas pelos hooks e pela automação."""
import re

ISSUE_BRANCH = re.compile(r"^(feature|bugfix|hotfix)/issue0*([1-9][0-9]*)-([a-z0-9]+(?:-[a-z0-9]+)*)$")
RELEASE_BRANCH = re.compile(r"^release/[0-9]+\.[0-9]+\.[0-9]+(?:-[a-z0-9]+(?:[.-][a-z0-9]+)*)?$")
VERSION_HOTFIX = re.compile(r"^hotfix/[0-9]+\.[0-9]+\.[0-9]+$")


def issue_for_branch(branch):
    match = ISSUE_BRANCH.fullmatch(branch)
    if match:
        return int(match.group(2))
    return None


def validate_branch(branch, production, development):
    if branch in (production, development):
        return
    if issue_for_branch(branch) is not None:
        return
    if VERSION_HOTFIX.fullmatch(branch):
        return
    if RELEASE_BRANCH.fullmatch(branch):
        return
    raise ValueError("Use feature/issue001-descricao, bugfix/issue001-descricao, hotfix/issue001-descricao ou release/1.0.0.")


def reference_commit(message, issue):
    before, separator, after = message.partition("# ------------------------ >8 ------------------------")
    content = "\n".join(line for line in before.splitlines() if not line.startswith("#"))
    if not content.strip():
        return message
    if re.search(rf"(?<![\w/])#{issue}\b", content):
        return message
    result = before.rstrip() + f"\n\nRefs #{issue}\n"
    if separator:
        result += "\n" + separator + after
    return result


def referenced_issues(messages):
    issues = set()
    for message in messages:
        for number in re.findall(r"(?im)^Refs\s*:?\s*#([1-9][0-9]*)\b", message):
            issues.add(int(number))
        for number in re.findall(r"(?:feature|bugfix|hotfix)/issue0*([1-9][0-9]*)-", message):
            issues.add(int(number))
    return sorted(issues)

