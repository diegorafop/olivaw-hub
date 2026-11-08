#!/usr/bin/env bash
# Executar na VM como ec2-user. Não cria banco vazio nem remove dados.
set -euo pipefail
umask 077
mode=${1:---check}
if [[ "$mode" != --check && "$mode" != --apply ]]; then
  echo 'Uso: bash scripts/deployment/migrate-olivaw-database.sh [--check|--apply]' >&2
  exit 2
fi
psql_admin() {
  docker exec -i -u postgres ms-postgres-db psql -X -v ON_ERROR_STOP=1 -At -d postgres "$@"
}
old_exists=$(psql_admin -c "SELECT count(*) FROM pg_database WHERE datname='olivaw_hub_prd'")
new_exists=$(psql_admin -c "SELECT count(*) FROM pg_database WHERE datname='olivaw_prd'")
if [[ "$old_exists" == 1 && "$new_exists" == 1 ]]; then
  echo 'Ambos os bancos existem. Interrompido: conferir os dados antes de qualquer migração.' >&2
  exit 1
fi
if [[ "$old_exists" == 0 && "$new_exists" == 0 ]]; then
  echo 'Nenhum dos bancos administrativos foi encontrado. Interrompido.' >&2
  exit 1
fi
database=olivaw_hub_prd
if [[ "$new_exists" == 1 ]]; then database=olivaw_prd; fi
schema_state=$(docker exec -u postgres ms-postgres-db psql -X -v ON_ERROR_STOP=1 -At -d "$database" -c "SELECT nspname FROM pg_namespace WHERE nspname IN ('ms_olivaw_hub','ms_olivaw') ORDER BY nspname")
if [[ "$schema_state" != ms_olivaw_hub && "$schema_state" != ms_olivaw ]]; then
  echo 'Schema ausente ou ambíguo. Interrompido.' >&2
  exit 1
fi
printf 'Banco encontrado: %s; schema: %s\n' "$database" "$schema_state"
if [[ "$mode" == --check ]]; then
  echo 'Somente leitura. Use --apply para parar as APIs administrativas, fazer backup e renomear.'
  exit 0
fi
state_directory=$(python3 -c 'import json; print(json.load(open("/etc/olivaw/deployment.json"))["stateDirectory"])')
exec 9>"$state_directory/deployment.lock"
flock -n 9 || { echo 'Há outra implantação em andamento.' >&2; exit 1; }
backup_directory="$HOME/olivaw-backups/$(date -u +%Y%m%dT%H%M%SZ)"
mkdir -p "$backup_directory"
# Parar somente os consumidores do banco administrativo; PostgreSQL e clientes continuam ativos.
for container in ms-olivaw-hub-api-prd ms-olivaw-api-prd ms-devops-api-prd; do
  if docker container inspect "$container" >/dev/null 2>&1; then
    docker stop "$container" >/dev/null
  fi
done
# Rejeitar sessões inesperadas em vez de encerrá-las silenciosamente.
connections=$(psql_admin -c "SELECT count(*) FROM pg_stat_activity WHERE datname='$database'")
if [[ "$connections" != 0 ]]; then
  echo 'Ainda há conexões no banco. APIs permanecem paradas; conferir as sessões antes de continuar.' >&2
  exit 1
fi
docker exec -u postgres ms-postgres-db pg_dump -Fc -d "$database" > "$backup_directory/$database.dump"
test -s "$backup_directory/$database.dump"
docker exec -i -u postgres ms-postgres-db pg_restore --list < "$backup_directory/$database.dump" > "$backup_directory/restore-list.txt"
if [[ "$old_exists" == 1 ]]; then
  psql_admin -c 'ALTER DATABASE olivaw_hub_prd RENAME TO olivaw_prd;' >/dev/null
fi
if [[ "$schema_state" == ms_olivaw_hub ]]; then
  docker exec -i -u postgres ms-postgres-db psql -X -v ON_ERROR_STOP=1 -d olivaw_prd -c 'ALTER SCHEMA ms_olivaw_hub RENAME TO ms_olivaw;' >/dev/null
fi
# Ajustar somente as chaves de permissão de implantação ligadas às pastas renomeadas.
docker exec -i -u postgres ms-postgres-db psql -X -v ON_ERROR_STOP=1 -d olivaw_prd <<'SQL'
BEGIN;
UPDATE ms_olivaw.authorization
SET "key" = replace(replace("key", 'olivaw-hub/', 'olivaw/'), 'ms-olivaw-hub-', 'ms-olivaw-')
WHERE "key" LIKE 'devops.deploy:olivaw-hub/%';
COMMIT;
SQL
echo "Backup privado: $backup_directory/$database.dump"
echo 'Banco olivaw_prd e schema ms_olivaw preparados. As APIs administrativas continuam paradas para implantação coordenada.'
echo 'Não restaurar o dump sobre o banco existente sem um plano de recuperação.'
