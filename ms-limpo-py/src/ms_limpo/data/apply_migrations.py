"""Comando exclusivo da esteira; nunca importado pela inicialização do agente."""

import argparse
import hashlib
import os
from pathlib import Path
import re
import sys


MIGRATIONS = Path(__file__).with_name("migrations")


def load_migrations(directory):
    migrations = []
    versions = set()
    for path in sorted(directory.glob("*.sql")):
        if not re.fullmatch(r"[0-9]{4}_[a-z0-9_]+\.sql", path.name):
            raise ValueError(f"Nome de migração inválido: {path.name}")
        version = int(path.name[:4])
        if version in versions:
            raise ValueError(f"Versão duplicada: {version}")
        versions.add(version)
        content = path.read_bytes()
        if not content.strip():
            raise ValueError(f"Migração vazia: {path.name}")
        migrations.append((path.name, hashlib.sha256(content).hexdigest(), content.decode("utf-8")))
    return migrations


def validate_history(migrations, applied):
    available = {name: checksum for name, checksum, _ in migrations}
    for name, checksum in applied.items():
        if available.get(name) != checksum:
            raise ValueError(f"Migração aplicada foi removida ou alterada: {name}")
    applied_names = sorted(applied)
    if applied_names != [name for name, _, _ in migrations[:len(applied_names)]]:
        raise ValueError("Histórico incompatível: migrações precisam ser aplicadas em ordem.")


def apply_migrations(dsn, schema, directory=MIGRATIONS):
    if not re.fullmatch(r"[a-z][a-z0-9_]{0,62}", schema):
        raise ValueError("Schema inválido; use letras minúsculas, números e underscore.")
    migrations = load_migrations(directory)
    import psycopg
    from psycopg import sql

    table = sql.Identifier(schema, "schema_migration")
    # Fechar a conexão libera o lock de sessão inclusive após erro.
    with psycopg.connect(dsn, autocommit=True, connect_timeout=15) as connection:
        connection.execute("SET lock_timeout = '30s'")
        connection.execute("SET statement_timeout = '5min'")
        exists = connection.execute(
            "SELECT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = %s)", (schema,)
        ).fetchone()[0]
        if not exists:
            raise ValueError("Schema não provisionado. Execute o provisionamento antes da migração.")
        connection.execute("SELECT pg_advisory_lock(hashtextextended(%s, 0))",
                           (f"ms-limpo:migrations:{schema}",))
        connection.execute("SELECT set_config('search_path', %s, false)", (f'"{schema}"',))
        connection.execute(sql.SQL("""CREATE TABLE IF NOT EXISTS {} (
            name text PRIMARY KEY,
            checksum text NOT NULL,
            applied_at timestamptz NOT NULL DEFAULT now()
        )""").format(table))
        applied = dict(connection.execute(sql.SQL("SELECT name, checksum FROM {} ORDER BY name").format(table)))
        validate_history(migrations, applied)
        count = 0
        for name, checksum, content in migrations:
            if name in applied:
                continue
            with connection.transaction():
                connection.execute(content, prepare=False)
                connection.execute(sql.SQL("INSERT INTO {} (name, checksum) VALUES (%s, %s)").format(table),
                                   (name, checksum))
            count += 1
        return count


def main(argv=None):
    parser = argparse.ArgumentParser(description="Aplica migrações PostgreSQL previamente provisionado.")
    parser.add_argument("--schema", required=True)
    arguments = parser.parse_args(argv)
    dsn = os.environ.get("AGENT_MIGRATION_DSN")
    if not dsn:
        parser.error("Defina AGENT_MIGRATION_DSN com a credencial de migração.")
    try:
        count = apply_migrations(dsn, arguments.schema)
    except Exception:
        # Mensagens de drivers podem incluir infraestrutura ou dados sensíveis.
        print("Falha ao aplicar migrações. Verifique dependências, acesso e histórico do schema.", file=sys.stderr)
        return 1
    print(f"Migrações aplicadas: {count}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
