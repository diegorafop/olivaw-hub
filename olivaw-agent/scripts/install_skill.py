#!/usr/bin/env python3
"""Instala a cópia revisada de safety-docker no escopo de usuário do Codex."""
import argparse
import hashlib
from pathlib import Path
import shutil
import tempfile

SOURCE = Path(__file__).resolve().parents[1] / "skills/safety-docker"


def files(folder):
  result = {}
  for path in folder.rglob("*"):
    if path.is_symlink():
      raise ValueError("Skill não pode conter links.")
    if path.is_file():
      if path.suffix not in (".md", ".yaml", ".json"):
        raise ValueError("Arquivo não permitido na skill.")
      result[str(path.relative_to(folder))] = hashlib.sha256(path.read_bytes()).hexdigest()
  if "SKILL.md" not in result:
    raise ValueError("Skill sem manifesto.")
  return result


def install(source, destination, replace=False):
  source, destination = Path(source), Path(destination).expanduser()
  if destination.name != "safety-docker":
    raise ValueError("Destino precisa se chamar safety-docker.")
  for path in (destination, *destination.parents):
    if path.is_symlink():
      raise ValueError("Destino não pode atravessar links.")
  expected = files(source)
  if destination.exists():
    if files(destination) == expected:
      return "already-current"
    if not replace:
      raise ValueError("Já existe outra versão: revise o diff antes de usar --replace.")
  destination.parent.mkdir(parents=True, exist_ok=True)
  # Monta a cópia inteira antes de substituir a instalação solicitada.
  with tempfile.TemporaryDirectory(prefix=".safety-stage-", dir=destination.parent) as directory:
    staged = Path(directory) / "safety-docker"
    shutil.copytree(source, staged)
    if files(staged) != expected:
      raise ValueError("Cópia não confere com a fonte.")
    if destination.exists():
      backup = Path(directory) / "previous"
      destination.rename(backup)
      try:
        staged.rename(destination)
      except Exception:
        backup.rename(destination)
        raise
    else:
      staged.rename(destination)
  return "installed"


def main(argv=None):
  parser = argparse.ArgumentParser(description=__doc__)
  parser.add_argument("--destination", type=Path, default=Path.home() / ".agents/skills/safety-docker")
  parser.add_argument("--replace", action="store_true", help="Substitui versão anterior depois da revisão da fonte.")
  args = parser.parse_args(argv)
  print(install(SOURCE, args.destination, args.replace) + ": " + str(args.destination))


if __name__ == "__main__":
  main()
