# Migração dos serviços para olivaw.com.br

## Destino e roteamento

A instância existente passa a usar o Elastic IP `98.95.250.122`. Os registros A da raiz, `www`, `lisierp`, `ltmj`, `api`, `api.ltmj` e `api.devops` apontam para esse endereço. Os servidores DNS permanecem no Registro.br; não há wildcard.

O Traefik existente usa as portas 80/443, os entrypoints `web` e `websecure`, a rede `traefik-net` e o resolver `myresolver`, com desafio ACME TLS-ALPN na porta 443. As regras dos projetos passam a usar os novos hosts; não é necessário reinstalar o proxy. `www.olivaw.com.br` compartilha o serviço institucional. Cada hostname recebe certificado próprio.

| Serviço | Endereço | Persistência |
| --- | --- | --- |
| Site institucional | `https://olivaw.com.br` e `https://www.olivaw.com.br` | Sem volume de dados |
| Autoridade administrativa | `https://api.olivaw.com.br` | `olivaw_prd`, schema `ms_olivaw` |
| DevOps | `https://api.devops.olivaw.com.br` | `olivaw_prd`, schema `ms_devops` |
| LTMJ | `https://ltmj.olivaw.com.br` | API separada |
| API LTMJ | `https://api.ltmj.olivaw.com.br` | `ltmj_prd`, schema `ms_ltmj`; volume `ms-ltmj-api-prd_portfolio-data` |
| Lisierp | `https://lisierp.olivaw.com.br` | Volume `lisierp_prd_data`; API no próprio domínio em `/api` |

## Renomear o banco existente

Comandos para executar na VM, no checkout atualizado. O script padrão somente confere a existência dos bancos e schemas:

```bash
cd /home/ec2-user/olivaw
bash scripts/deployment/migrate-olivaw-database.sh --check
bash scripts/deployment/migrate-olivaw-database.sh --apply
```

`--apply` obtém o lock do executor, para somente as APIs administrativas, exige ausência de conexões restantes, grava um dump privado em `~/olivaw-backups/<data>/`, verifica sua listagem e renomeia os recursos existentes. O script recusa coexistência de bancos ou schemas com os dois nomes, preserva o histórico de migrations e ajusta somente permissões de deployment das pastas renomeadas. As APIs ficam paradas até a atualização coordenada. O dump contém dados e deve permanecer fora do Git e de logs públicos.

As operações SQL centrais são as seguintes; não execute estas linhas isoladamente com aplicações conectadas ou sem backup:

```sql
-- Conectado ao banco postgres, fora de uma transação:
ALTER DATABASE olivaw_hub_prd RENAME TO olivaw_prd;

-- Em seguida, conectado ao banco olivaw_prd:
ALTER SCHEMA ms_olivaw_hub RENAME TO ms_olivaw;
```

Não recriar o banco, não remover volumes e não renomear `ltmj_prd`, `fintalk_core` ou o banco legado `devops_prd`. A revisão não identificou necessidade de mudar esses recursos.

## Checkout e executor

O checkout da VM passa de `/home/ec2-user/olivaw-hub` para `/home/ec2-user/olivaw`. O executor instalado em `/opt/olivaw/deploy.py` e sua configuração em `/etc/olivaw/deployment.json` precisam acompanhar a mudança. A allowlist passa a conter `olivaw/ms-olivaw-api`, `olivaw/ms-olivaw-ng`, `olivaw/ms-devops-api`, `olivaw/ms-ltmj-api`, `olivaw/ms-ltmj-ng` e o repositório independente `lisierp`. Preservar os demais parâmetros instalados, as chaves SSH, os jobs e os volumes.

Os scripts Bash/Python do monorepo e Bash/PowerShell/Node do Lisierp usam `OLIVAW_API_URL=https://api.olivaw.com.br` e `DEVOPS_API_URL=https://api.devops.olivaw.com.br`. O arquivo é `deployment/olivaw-credentials.env`, com `OLIVAW_LOGIN` e `OLIVAW_PASSWORD`; senhas não são exibidas nem alteradas pela troca de nomes.

Antes de futuros deploys por Git, publicar as alterações dos dois repositórios na branch escolhida. Uma transferência direta nesta migração não significa commit, push ou finish. Usar a branch que contém a renomeação; branches anteriores continuam contendo os nomes antigos.

## Validação

Validar Compose com `config --quiet` para não exibir credenciais. Conferir readiness das APIs, settings públicos dos frontends, persistência dos volumes, regras de host e certificados para cada endereço. DNS autoritativo precisa retornar o IP novo antes da emissão ACME. Um teste de roteamento com resolução forçada não comprova propagação pública ou validade HTTPS.

## Recuperação

Preservar imagens anteriores e backup das configurações antes da troca. Para voltar ao nome antigo do banco, parar as APIs novas e renomear banco/schema de volta, com verificação de sessões e dados escritos após a migração. Não restaurar o dump sobre dados novos automaticamente. Só depois reinstalar as configurações, imagens e caminhos antigos. Rollback dos hosts DuckDNS depende também de seus apontamentos, que o mantenedor decidiu descontinuar.

## Resultado observado — 18/10/2026

- Checkout remoto atualizado para a base da branch `feature/issue011-setup-br-domain`, com configurações transferidas diretamente enquanto o push do mantenedor fica pendente. Lisierp permanece em `main`. Sincronização final por Git depende da publicação das alterações locais nos dois repositórios.
- Backup de configurações e imagens anteriores preservados na VM; dump privado do banco administrativo criado e conferido antes da renomeação.
- Banco administrativo renomeado para `olivaw_prd` e schema para `ms_olivaw`; duas chaves de permissão de deployment atualizadas. `ltmj_prd` permaneceu preservado.
- Os seis containers migrados responderam HTTP 200 nos endpoints internos de saúde/readiness, sem reinícios no momento da conferência. Os frontends retornaram as novas URLs de API.
- Volumes `ms-ltmj-api-prd_portfolio-data` e `lisierp_prd_data` preservados.
- Executor e allowlist instalados na VM com os caminhos novos. Containers antigos da autoridade e do site permaneceram parados para recuperação.
- DNS autoritativo ainda sem registros A na última consulta. Acesso público, emissão dos certificados e validação HTTPS permanecem pendentes dessa publicação.
- Validação local: 35 combinações Compose válidas, 7 testes do gerador e 27 testes do cliente/executor de deploy passaram; o teste do cliente Node do Lisierp também passou. APIs publicadas e frontends compilados localmente; imagens de runtime construídas na VM.

A verificação posterior por IP forçado e Host/SNI confirmou HTTP 200 no Traefik para os sete endereços. Essa verificação desconsiderou a confiança do certificado apenas para diagnóstico, sem enviar credenciais; não comprova HTTPS público válido. A resolução pública ainda não retornava os registros A.

Por exceção explícita do mantenedor, os arquivos existentes em `docs/secrets` permanecem no Git privado até a migração para Key Vault. Eles continuam excluídos do contexto Docker, assim como os arquivos de credenciais de deployment dos frontends.
