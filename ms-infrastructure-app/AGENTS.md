# Diretrizes — Infraestrutura compartilhada

## Escopo

- Este módulo contém Compose e procedimentos de infraestrutura da Olivaw; não é API e não recebe scaffold .NET.
- Preserve os nomes de serviços e redes usados pelas aplicações: `ms-postgres-db`, `ms-rabbit-mq`, `ms-traefik-pr`, `postgres-net`, `rabbit-net` e `traefik-net`. Redis é opcional e usa `redis-net`.
- Antes de alterar recursos existentes, confira projeto Compose, nomes reais dos volumes e origem do ACME. Nunca substitua um volume de produção por um volume novo vazio.
- Não execute `down -v`, prune, restauração, atualização de imagens, alteração de firewall/SSH ou recriação remota como parte de validação.

## Segredos e segurança

- Nenhuma chave SSH privada, ACME, dump, token ou saída bruta de auditoria entra no Git. A chave administrativa existe somente na máquina do operador com 0600.
- Preserve a chave restrita do executor e certificados TLS; não apague arquivos apenas por terem extensão `.pem`.
- Use o nome real `.env`, conforme preferência explícita do mantenedor; não criar `.env.example`. Credenciais ainda não configuradas ficam vazias. Preserve a política de dotenv versionados do repositório privado; nunca imprima seus valores.
- Bancos/cache/broker não publicam portas no host. Acesso externo passa pelo Traefik; dashboard administrativo não deve ser exposto.
- Access log omite cabeçalhos por padrão; permita somente User-Agent. Não registre Authorization, Cookie, X-Lead-Key ou corpos.
- Documente controles confirmados, riscos, limitações e ações pendentes sem afirmar que a auditoria garante ausência de comprometimento.

## Verificação

- `docker compose --env-file ms-infrastructure-app/.env -f ms-infrastructure-app/docker-compose.yml config --quiet` valida sem iniciar containers.
- Execute `python3 ms-infrastructure-app/scripts/check-private-keys.py` e `python3 -m unittest discover -s ms-infrastructure-app/tests -v`.
- Redis fica em profile; não habilite outro serviço permanente na VM de 1 GB sem avaliar capacidade.
- Documentação em português, links relativos e feature de segurança em discovery até a revisão das correções pendentes.
