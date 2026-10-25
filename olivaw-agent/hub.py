"""Agrupador local: descobre projetos padronizados e delega operações sem shell."""
import argparse
import json
import os
from pathlib import Path
import re
import subprocess
import sys

ROOT = Path(__file__).resolve().parent


def discover(root):
  agents, rejected = [], []
  for directory in sorted(root.glob("ms-*-py")):
    try:
      if directory.is_symlink() or not directory.is_dir():
        raise ValueError("Projeto deve ser diretório regular.")
      slug = directory.name[3:-3]
      if not re.fullmatch(r"[a-z][a-z0-9]*(?:-[a-z0-9]+)*", slug):
        raise ValueError("Nome fora do padrão.")
      settings = directory / "setting.json"
      metadata = directory / "pyproject.toml"
      if settings.is_symlink() or metadata.is_symlink() or settings.stat().st_size > 65536:
        raise ValueError("Configuração inválida.")
      config = json.loads(settings.read_text())
      module = "ms_" + slug.replace("-", "_")
      if not isinstance(config.get("agent"), dict) or config["agent"].get("id") != slug or config["agent"].get("module") != module:
        raise ValueError("Identidade não corresponde à pasta.")
      source = directory / "src" / module
      if (directory / "src").is_symlink() or source.is_symlink() or (source / "__main__.py").is_symlink() or not (source / "__main__.py").is_file() or not source.resolve().is_relative_to(directory.resolve()):
        raise ValueError("Pacote de agente ausente ou fora do projeto.")
      project = re.search(r"(?ms)^\[project\]\s*\n(.*?)(?=^\[|\Z)", metadata.read_text())
      if not project or not re.search(r"(?m)^name\s*=\s*['\"]" + re.escape(directory.name) + r"['\"]\s*$", project[1]):
        raise ValueError("Distribuição não corresponde à pasta.")
      agents.append({"name": slug, "module": module, "directory": directory, "settings": settings})
    except (ValueError, OSError, KeyError, TypeError):
      rejected.append({"agent": directory.name, "error": "invalid-project"})
  return agents, rejected


def operate(root, operation, only=None):
  agents, errors = discover(root)
  if only:
    agents = [agent for agent in agents if agent["name"] == only]
    errors = [error for error in errors if error["agent"] == "ms-" + only + "-py"]
  results = []
  for agent in agents:
    directory = agent["directory"]
    executable = directory / ".venv" / ("Scripts/python.exe" if os.name == "nt" else "bin/python")
    if not executable.is_file():
      executable = Path(sys.executable)
    allowed = {"PATH", "HOME", "LANG", "LC_ALL", "LC_CTYPE", "TZ", "PYTHONIOENCODING",
               "SSL_CERT_FILE", "SSL_CERT_DIR", "HTTP_PROXY", "HTTPS_PROXY", "NO_PROXY",
               "http_proxy", "https_proxy", "no_proxy"}
    environment = {key: value for key, value in os.environ.items() if key in allowed}
    environment["PYTHONPATH"] = str(directory / "src")
    try:
      result = subprocess.run([str(executable), "-m", agent["module"], operation,
                               "--settings", str(agent["settings"])], cwd=directory,
                              env=environment, capture_output=True, text=True,
                              timeout=300 if operation == "audit" else 180 if operation == "update" else 15)
      output = result.stdout if result.returncode == 0 else result.stderr or result.stdout
      if len(output.encode()) > 1048576:
        raise ValueError("Saída excedeu limite.")
      results.append({"agent": agent["name"], "exitCode": result.returncode, "result": json.loads(output)})
    except (ValueError, OSError, subprocess.TimeoutExpired):
      results.append({"agent": agent["name"], "exitCode": 1, "error": "agent-operation-failed"})
  return {"operation": operation, "agents": results, "rejectedProjects": errors,
          "success": bool(results) and not errors and all(r["exitCode"] == 0 for r in results)}



def review_text(report):
  lines = []
  for agent in report["agents"]:
    result = agent.get("result", {})
    lines.append("#agent " + agent["agent"])
    for section in ("source", "discovery", "learn"):
      lines.append("\n#" + section)
      for item in result.get(section, []):
        if section == "learn":
          lines.append(str(item["number"]) + " - " + item["action"])
          lines.extend("    " + evidence["url"] for evidence in item.get("evidence", []))
        else:
          lines.append(item["url"] + " [" + (item.get("status", "não verificada; revisar") if section == "source" else "não verificada; revisar") + "]")
      if not result.get(section):
        lines.append("Nenhum item retornado nesta consulta.")
    lines.append("\nResultado: " + result.get("status", "failed") + "; nenhuma alteração aplicada.")
    for search in result.get("research", {}).get("searches", []):
      if search["status"] != "ok":
        lines.append("Busca indisponível: " + search["query"] + " [" + search.get("error", "failed") + "]")
  for error in report["rejectedProjects"]:
    lines.append("Projeto recusado: " + error["agent"])
  return "\n".join(lines)


def main(argv=None):
  parser = argparse.ArgumentParser(description="Opera todos os projetos de agentes do agrupador Olivaw.")
  parser.add_argument("operation", choices=("start", "stop", "status", "update", "audit"))
  parser.add_argument("--agent", help="Opcional: nome curto de um agente, como baley.")
  parser.add_argument("--format", choices=("json", "text"), default="json")
  args = parser.parse_args(argv)
  result = operate(ROOT, args.operation, args.agent)
  print(review_text(result) if args.format == "text" and args.operation == "update" else json.dumps(result, ensure_ascii=False, indent=2))
  return 0 if result["success"] else 1
