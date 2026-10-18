# ms-devops-api

Serviço interno de implantação e manutenção do Olivaw Hub. Usa projetos `Owh.Devops`, banco próprio e executor SSH restrito na VM. Sua autenticação vem exclusivamente dos JWTs emitidos pela [ms-olivaw-hub-api](../ms-olivaw-hub-api/README.md).

A API não possui cadastro de usuários, tabelas de identidade, bootstrap de root ou rotas de login. Ela valida assinatura, algoritmo, emissor, audiência e expiração localmente, sem consultar o Hub. As permissões vêm do token e valem até expirar.

## Endpoints

- `POST /deployments`: cria um job com `folder`, `branch`, `environment`.
- `GET /deployments/{id}`: consulta um job da pasta autorizada.
- `POST /disk-cleanups`: agenda a limpeza da VM; exclusivo de root do Hub.
- `GET /disk-cleanups/{id}`: consulta a limpeza; exclusivo de root.
- `GET /ready` e `GET /version`: informações operacionais públicas.

Uma conta delegada precisa de `devops.deploy` e de uma autorização por pasta, por exemplo `devops.deploy:olivaw-hub/ms-ltmj-ng`. Isso vale tanto para criar como para consultar deployments. A allowlist da VM é uma segunda restrição independente. Uma conta técnica de cliente não recebe root.

## Banco, infraestrutura e execução

Os bancos próprios são `devops_dev` até `devops_prd`, com schema `ms_devops`. A migration inicial atual é vazia: não existem entidades persistidas do serviço nesta versão. O banco é reservado às futuras funcionalidades próprias; os jobs atuais ficam no estado privado do executor na VM. O startup aplica as migrations automaticamente.

```bash
dotnet build ms-devops-api/Devops.sln -c Release -m:1
dotnet test ms-devops-api/tests/Owh.Devops.Tests/Owh.Devops.Tests.csproj -c Release --no-build
bash scripts/deployment/deploy.sh ms-ltmj-ng prd develop
bash scripts/deployment/cleanup.sh
```

Os scripts autenticam no Hub e chamam esta API. Não executam SSH localmente. A limpeza remove imagens sem containers associados, cache de build, temporários vencidos e journal arquivado, com lock compartilhado com deploys. Não remove volumes ou redes Docker nem trunca logs ativos.

Veja [instalação e manutenção do executor](../docs/deploy-executor.md) e [a ordem da nova publicação](../docs/identity-architecture.md). Os dotenv versionados do Hub e da DevOps compartilham o secret interno por ambiente; as APIs de clientes usam secrets próprios. Credenciais, tokens e hashes não devem ser impressos.
