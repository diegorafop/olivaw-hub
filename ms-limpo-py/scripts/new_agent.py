"""Gera um agente independente usando somente os arquivos declarados do template."""

import argparse
import json
from pathlib import Path
import re


TEMPLATE = Path(__file__).resolve().parents[1]
# Lista fechada: novos arquivos do template exigem inclusão consciente aqui.
PACKAGE_FILES = (
    "__init__.py", "__main__.py", "bootstrap.py", "cli.py", "configuration.py",
    "core/__init__.py", "core/execution_repository.py",
    "domain/__init__.py", "domain/execution.py",
    "data/__init__.py", "data/apply_migrations.py",
    "data/memory_execution_repository.py", "data/migrations/__init__.py",
    "data/migrations/README.md", "features/__init__.py",
    "features/execution/__init__.py", "features/execution/commands/__init__.py",
    "features/execution/commands/create_execution.py",
    "features/execution/queries/__init__.py",
    "features/execution/queries/get_execution.py",
)
ROOT_FILES = (".editorconfig", "setting.json", ".gitignore", "AGENTS.md", "README.md",
              "pyproject.toml", "tests/test_template.py",
              ".vscode/settings.json", ".vscode/extensions.json")


def generate(name, output, template=TEMPLATE):
    # Limite também mantém agent_<nome> dentro dos 63 bytes do PostgreSQL.
    if not re.fullmatch(r"[a-z][a-z0-9]*(?:-[a-z0-9]+)*", name) or len(name) > 57:
        raise ValueError("Nome inválido: use até 57 letras minúsculas/números separados por hífen, iniciando por letra.")
    if name == "limpo" or name.startswith("ms-") or name.endswith("-py"):
        raise ValueError("Informe somente o nome curto, sem ms- ou -py; limpo é reservado.")
    output = Path(output).expanduser().resolve(strict=True)
    if not output.is_dir():
        raise ValueError("--output precisa ser um diretório existente.")
    project = f"ms-{name}-py"
    command = name
    destination = output / project
    # Recusa inclusive diretórios vazios e links quebrados; mkdir é exclusivo.
    if destination.exists() or destination.is_symlink():
        raise ValueError(f"Destino já existe: {destination}")
    template = Path(template).resolve(strict=True)
    if output == template or template in output.parents:
        raise ValueError("O destino deve ficar fora do template.")
    package = "ms_" + name.replace("-", "_")
    files = [(path, path) for path in ROOT_FILES]
    files += [(f"src/ms_limpo/{path}", f"src/{package}/{path}") for path in PACKAGE_FILES]
    rendered = []
    for source, target in files:
        path = template / source
        if not path.resolve().is_relative_to(template):
            raise ValueError(f"Arquivo do template fora da raiz: {source}")
        # Não permite links nem em componentes intermediários.
        if any((template / parent).is_symlink() for parent in (Path(source), *Path(source).parents)):
            raise ValueError(f"Link no template: {source}")
        content = path.read_text(encoding="utf-8")
        if source.endswith(".py"):
            content = re.sub(r"(?m)^(from|import) ms_limpo(?=[.\s])", rf"\1 {package}", content)
            if source.endswith("data/apply_migrations.py"):
                content = content.replace('f"ms-limpo:migrations:', f'f"{command}:migrations:')
        elif source == "setting.json":
            settings = json.loads(content)
            settings["agent"] = {"id": name, "module": package}
            content = json.dumps(settings, ensure_ascii=False, indent=2) + "\n"
        elif source in ("README.md", "AGENTS.md", "pyproject.toml"):
            content = content.replace("ms-limpo-py", project)
            content = content.replace("ms-limpo", command)
            content = content.replace("ms_limpo", package)
            content = content.replace("agent_giskard", "agent_" + name.replace("-", "_"))
            if source == "README.md":
                start = content.index("## Criar um agente")
                end = content.index("## Organização", start)
                content = content[:start] + content[end:]
                start = content.index("## Agentes derivados")
                end = content.index("## Validação", start)
                content = content[:start] + content[end:]
                content = content.replace("Template mínimo", "Esqueleto gerado do ms-limpo-py: template mínimo", 1)
            elif source == "AGENTS.md":
                start = content.index("## Criar novos agentes")
                end = content.index("## Arquitetura", start)
                content = content[:start] + content[end:]
            elif source == "pyproject.toml":
                content = content.replace("Template Python para agentes locais com CQS", f"Agente local {name} com CQS")
        rendered.append((target, content))
    # Toda validação e leitura ocorre antes da primeira escrita.
    destination.mkdir()
    for target, content in rendered:
        path = destination / target
        path.parent.mkdir(parents=True, exist_ok=True)
        with path.open("x", encoding="utf-8", newline="\n") as file:
            file.write(content)
    return destination


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("name", help="Somente o nome curto: giskard gera ms-giskard-py; daneel-olivaw gera ms-daneel-olivaw-py")
    parser.add_argument("--output", type=Path, default=Path(".."), help="Diretório pai existente (padrão: ..)")
    args = parser.parse_args(argv)
    try:
        destination = generate(args.name, args.output)
    except (ValueError, OSError) as error:
        parser.error(str(error))
    print(f"Projeto criado: {destination}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
