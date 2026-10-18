# Identidade do Hub e dos clientes

A `ms-olivaw-hub-api` é a autoridade administrativa do Olivaw Hub em `https://api.olivaw-hub.duckdns.org`. Mantém contas humanas da operação, contas técnicas de clientes, perfis, permissões e vínculos. O frontend institucional aponta para essa API.

A `ms-devops-api` tem banco próprio, sem usuários, senhas, bootstrap ou endpoints de login. Ela valida assinatura, emissor, audiência e expiração dos JWTs do Hub localmente. Os serviços internos compartilham o secret interno por ambiente e usam os claims assinados até expirar. Não consultam o Hub por requisição. Alterações de permissões passam a valer ao emitir um novo token; logout na autoridade não revoga um token já emitido nos consumidores.

Cada cliente, como LTMJ, mantém banco, usuários, emissor, audiência e secret próprios. O login da equipe LTMJ ocorre na API da LTMJ. Uma conta técnica `ltmj` no Hub é outra identidade: ela recebe JWT interno para executar somente as operações concedidas pelo Hub, sem compartilhar os usuários finais do cliente.

## Contratos e persistência

Commands e queries usam inglês e CRUD: `Create`, `Read`, `Update`, `Delete`. Autenticação usa `AuthenticateUser`, `LogoutUser` e `ChangePassword`. JSON, rotas e entidades de identidade usam inglês. Projetos, assemblies e namespaces usam o prefixo `Owh`.

As tabelas são `authorization`, `accessprofile`, `accessprofileauthorization`, `userprofile` e `userprofileaccessprofile`. PKs e FKs internas são `int`; `ExternalId` é o GUID público. A entidade base usa `CreatedAt`, `UpdatedAt`, `DeletedAt` e `ActorUserId`. Tabelas e colunas usam lowercase sem underscores. As autoridades têm essas tabelas; consumidores não.

`UserProfile.IsServiceAccount` distingue contas técnicas de humanas. Root pertence à operação e nasce apenas pelo bootstrap. Uma conta delegada de deploy precisa de `devops.deploy` e da autorização por pasta, por exemplo `devops.deploy:olivaw-hub/ms-ltmj-api` e `devops.deploy:olivaw-hub/ms-ltmj-ng`. A mesma restrição protege a consulta dos logs dos jobs. A allowlist do executor é adicional à autorização do JWT.

## Reinicialização e primeira publicação

Esta entrega recria as migrations para bases vazias. O mantenedor apagará e preparará os bancos antes de publicar. O código desta entrega não apaga bancos remotos. Os serviços internos compartilham um banco por ambiente (`olivaw_hub_prd` em produção), cada um no seu schema (`ms_olivaw_hub` e `ms_devops`). Clientes como LTMJ mantêm bancos próprios, como `ltmj_prd`. Os startups aplicam migrations automaticamente, sem flags. Somente autoridades executam bootstrap de root. Os usuários comuns e seus vínculos serão definidos depois da publicação.

Na primeira publicação, com a branch já disponível na VM e os bancos vazios criados, execute os comandos na raiz do checkout:

```bash
docker compose --project-name ms-olivaw-hub-api-prd \
  --env-file ms-olivaw-hub-api/docker/.env.prd \
  -f ms-olivaw-hub-api/docker/docker-compose.yml \
  -f ms-olivaw-hub-api/docker/docker-compose.prd.yml up -d --build

docker compose --project-name ms-devops-api-prd \
  --env-file ms-devops-api/docker/.env.prd \
  -f ms-devops-api/docker/docker-compose.yml \
  -f ms-devops-api/docker/docker-compose.prd.yml \
  -f ms-devops-api/docker/docker-compose.ssh.yml up -d --build
```

Depois que o Hub e a DevOps estiverem ativos, o cliente genérico autentica no Hub e publica os demais serviços:

```bash
bash scripts/deployment/deploy.sh ms-ltmj-api prd <branch-publicada>
bash scripts/deployment/deploy.sh ms-ltmj-ng prd <branch-publicada>
bash scripts/deployment/deploy.sh ms-olivaw-hub-ng prd <branch-publicada>
bash scripts/deployment/cleanup.sh
```

Inclua `olivaw-hub/ms-olivaw-hub-api` na allowlist de `/etc/olivaw/deployment.json` e atualize o executor conforme [instalação e limpeza](deploy-executor.md). Preserve as entradas dos outros serviços. DNS/Traefik precisam encaminhar `api.olivaw-hub.duckdns.org` para o novo Hub. Os containers usam nomes com o sufixo do ambiente.
