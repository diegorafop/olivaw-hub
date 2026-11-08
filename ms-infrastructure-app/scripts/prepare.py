#!/usr/bin/env python3
"""Confere recursos existentes; criação de infraestrutura vazia exige --new-host."""
import argparse
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--new-host', action='store_true', help='Cria redes/volumes vazios somente em VM nova, antes de restaurar dados.')
    parser.add_argument('--redis', action='store_true', help='Inclui os recursos opcionais do Redis.')
    args = parser.parse_args()
    command = ['docker', 'compose', '--env-file', str(ROOT / '.env'), '-f', str(ROOT / 'docker-compose.yml')]
    if args.redis:
        command += ['--profile', 'redis']
    result = subprocess.run(command + ['config', '--format', 'json'], capture_output=True, text=True)
    if result.returncode:
        raise SystemExit('Compose inválido: preencha o .env e execute docker compose config --quiet para detalhes.')
    config = json.loads(result.stdout)
    # Nunca imprimir o JSON: ele contém a configuração interpolada.
    existing = subprocess.run(['docker', 'ps', '-a', '--format', '{{.Names}}'], check=True, capture_output=True, text=True).stdout.splitlines()
    if args.new_host and any(name in existing for name in ['ms-postgres-db', 'ms-rabbit-mq', 'ms-traefik-pr', 'ms-redis-db']):
        raise SystemExit('Recusado --new-host: existem containers da infraestrutura. Use conferência e migração dos recursos atuais.')
    failures = []
    for kind, group in [('network', 'networks'), ('volume', 'volumes')]:
        for key, resource in config.get(group, {}).items():
            if not args.redis and key.startswith('redis'):
                continue
            name = resource['name']
            found = subprocess.run(['docker', kind, 'inspect', name], capture_output=True).returncode == 0
            if not found and args.new_host:
                subprocess.run(['docker', kind, 'create', name], check=True, stdout=subprocess.DEVNULL)
                print('Criado ' + kind + ': ' + name)
            elif not found:
                failures.append(kind + ': ' + name)
    mounts = config['services']['ms-traefik-pr']['volumes']
    acme = Path(next(mount['source'] for mount in mounts if mount['target'] == '/acme.json'))
    if not acme.exists() and args.new_host:
        acme.parent.mkdir(mode=0o700, parents=True, exist_ok=True)
        acme.touch(mode=0o600, exist_ok=False)
        print('ACME vazio criado para VM nova; restaurar o existente quando houver.')
    elif not acme.is_file() or acme.is_symlink() or acme.stat().st_mode & 0o077:
        failures.append('ACME precisa ser arquivo regular 0600; conferir TRAEFIK_ACME_FILE.')
    if failures:
        raise SystemExit('Recursos não preparados; nada foi iniciado:\n- ' + '\n- '.join(failures))
    print('Recursos preparados. Nenhum container foi iniciado e nenhum backup foi restaurado.')


if __name__ == '__main__':
    main()
