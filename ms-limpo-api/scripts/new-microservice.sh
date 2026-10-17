#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 || $# -gt 2 || ! $1 =~ ^[a-z][a-z0-9]*(-[a-z0-9]+)*$ ]]; then
    echo "Uso: $0 nome-do-servico [diretorio-de-saida]" >&2
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
dotnet new mps-microservice \
    --name "$service_name" \
    --serviceSlug "$service_slug" \
    --serviceToken "$service_token" \
    --output "$output_dir"

echo "Microservice criado em: $output_dir"
