#!/bin/sh
set -eu
api_url=${API_URL:-http://localhost:5137}
# Restringe caracteres para escrever JSON sem interpolação insegura.
case "$api_url" in
  http://?*|https://?*) ;;
  *) echo 'API_URL deve ser uma URL absoluta HTTP(S).' >&2; exit 1 ;;
esac
case "$api_url" in
  *[!a-zA-Z0-9:/._%~-]*) echo 'API_URL contém caracteres não permitidos.' >&2; exit 1 ;;
esac
umask 077
mkdir -p /tmp/olivaw
printf '{"apiUrl":"%s"}\n' "$api_url" > /tmp/olivaw/config.json
