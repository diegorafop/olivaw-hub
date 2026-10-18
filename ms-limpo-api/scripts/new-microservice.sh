#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 || $# -gt 3 || ! $1 =~ ^[a-z][a-z0-9]*(-[a-z0-9]+)*$ ]]; then
    echo "Uso: $0 nome-do-servico [diretorio-de-saida] [dono-do-banco]" >&2
    exit 2
fi

database_owner=${3:-olivaw}
if [[ ! $database_owner =~ ^[a-z][a-z0-9]*(_[a-z0-9]+)*$ ]]; then
    echo "O dono do banco deve usar minusculas, numeros e sublinhados, por exemplo pastelaria_joao." >&2
    exit 2
fi

service_slug=$1
script_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
repo_dir=$(cd -- "$script_dir/.." && pwd)
service_name=$(printf '%s' "$service_slug" | awk -F- '{ for (part = 1; part <= NF; part++) printf "%s%s", toupper(substr($part, 1, 1)), substr($part, 2) }')
service_token=$(printf '%s' "$service_slug" | tr '[:lower:]-' '[:upper:]_')
output_dir=${2:-"$repo_dir/../ms-$service_slug-api"}

if [[ -e $output_dir ]]; then
    echo "O diretorio de saida ja existe: $output_dir" >&2
    exit 1
fi

monorepo_dir=$(cd -- "$repo_dir/.." && pwd)
output_parent=$(cd -- "$(dirname -- "$output_dir")" && pwd)
if [[ "$output_parent" != "$monorepo_dir" ]]; then
    echo "Gere o serviço diretamente na raiz do monorepo para preservar os ProjectReference." >&2
    exit 1
fi

dotnet new install "$repo_dir" --force
dotnet new owh-microservice \
    --name "$service_name" \
    --serviceSlug "$service_slug" \
    --serviceToken "$service_token" \
    --databaseOwnerInput "$database_owner" \
    --output "$output_dir"

python3 "$script_dir/configure-generated-service.py" "$repo_dir" "$output_dir" "$service_slug" "$database_owner"

echo "Microservice criado em: $output_dir"
