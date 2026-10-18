# Configuração da VM AWS, DuckDNS e Traefik

## Referência e escopo

As configurações de `ms-limpo-api` e `ms-devops-api` seguem o appsettings de produção e os arquivos Docker do FinTalk, consultados somente para leitura. O core e a identidade são bibliotecas; configuração de banco, broker e exposição HTTP pertence aos hosts consumidores.

Foram reaproveitados os endereços internos `ms-postgres-db`, `ms-rabbit-mq` e `ms-redis-db`. Cada cliente mantém bancos isolados dos serviços internos. Olivaw e DevOps compartilham um banco por ambiente e mantêm schemas exclusivos. Domínio e prefixo de mensagens identificam serviço e ambiente. O banco `fintalk_core` não é usado pelo Olivaw.

Nenhum banco, schema, usuário, fila, certificado ou recurso da AWS foi criado ou alterado nesta tarefa. Também não houve publicação de containers.

## Endereços e nomes

| Serviço | URL de produção | Banco de produção | Schema |
| --- | --- | --- | --- |
| Template | `https://api.limpo.olivaw-hub.duckdns.org` | `olivaw_prd` | `ms_limpo` |
| Olivaw API | `https://api.olivaw-hub.duckdns.org` | `olivaw_prd` | `ms_olivaw` |
| DevOps | `https://api.devops.olivaw-hub.duckdns.org` | `olivaw_prd` | `ms_devops` |
| LTMJ API | `https://api.ltmj.olivaw-hub.duckdns.org` | `ltmj_prd` | `ms_ltmj` |

Os bancos usam o sufixo do ambiente: `_dev`, `_qas`, `_rls`, `_stg` e `_prd`. O domínio de produção não tem sufixo; os demais usam, por exemplo, `api.devops-stg.olivaw-hub.duckdns.org`.

Os routers Traefik usam hosts explícitos e solicitam certificados por hostname; não presumem um único certificado wildcard para todos os níveis de subdomínio.

Os nomes de bancos e schemas usam somente minúsculas, números e sublinhados. Ao gerar `cloud-validacao`, o template produz `olivaw_dev` e `ms_cloud_validacao`, preservando os hífens em `ms-cloud-validacao-api` e no domínio.

`olivaw-hub.duckdns.org` é a base aprovada para o Olivaw. O cadastro dessa base e o apontamento para a VM devem ser feitos no DuckDNS; a configuração local não registra o domínio. APIs usam `api.<serviço>.olivaw-hub.duckdns.org`; futuros frontends usam `<serviço>.olivaw-hub.duckdns.org`. Emissor e audiência JWT são identificadores explícitos por autoridade/ambiente e devem coincidir nos consumidores; o host HTTP é configurado separadamente.

## Governança dos dados e identidade

- Olivaw e DevOps compartilham `olivaw_<ambiente>`, cada serviço no próprio schema (`ms_olivaw` e `ms_devops`). Somente o Olivaw mantém identidade administrativa interna.
- Cada cliente possui seu banco, por exemplo `pastelaria_dev`, com schemas `ms_service1` e `ms_service2` para seus serviços.
- Cada serviço mantém seu `DbContext`, migrations e histórico `__efmigrationshistory` no próprio schema. Prepare o schema antes de executar migrations; `SearchPath` não o cria e não substitui permissões de acesso.
- As conexões usam o usuário `postgres` nesta etapa; posteriormente, crie um usuário por serviço com acesso limitado ao próprio schema. Evite consultas e chaves estrangeiras entre schemas de serviços; integre por APIs ou mensagens.
- A identidade pertence ao serviço principal do cliente. Serviços agregados consumirão o JWT desse emissor, sem cadastros de identidade próprios. Chave compartilhada exige isolamento por cliente/ambiente e validação de emissor, audiência e expiração; permissões e revogação também precisam de um contrato comum.

`AddOlivawIdentityConsumer` valida JWT localmente com secret, emissor e audiência do Olivaw. Os claims assinados definem permissões até o token expirar; não há consulta ao Olivaw por requisição. Contas técnicas de clientes no Olivaw são independentes dos usuários locais dos clientes.

Backups automatizados e restauração serão implementados posteriormente. O planejamento deve incluir retenção, cópia independente do volume da VM e testes de restauração; um restore completo do banco afeta todos os serviços da organização naquele ambiente.

## Arquivos de configuração

- `appsettings.json`: estrutura comum e integrações externas desabilitadas por padrão.
- `appsettings.<Ambiente>.json`: endereços sem senha, banco/schema, identificação JWT, Swagger e prefixo de mensagens daquele ambiente.
- `docker/docker-compose.yml`: build a partir da raiz do monorepo, porta interna e redes comuns.
- `docker/docker-compose.<dev|qas|rls|stg|prd>.yml`: ambiente, imagem, nome do container e roteamento Traefik.
- `docker/.env.example`: exemplo versionado, sem credenciais reais.
- `docker/.env` e `.env.dev`, `.env.qas`, `.env.rls`, `.env.stg`, `.env.prd`: configurações dos ambientes versionadas no repositório privado e excluídas do contexto Docker. `.env` é o arquivo padrão local; os comandos de publicação sempre indicam `.env.<ambiente>` explicitamente.

As senhas atuais de banco, RabbitMQ e Redis foram reaproveitadas do FinTalk somente nesses arquivos locais. Os hosts atuais receberam chaves JWT aleatórias por ambiente. Para futuros serviços agregados de um cliente, o emissor principal será único e a configuração de validação deverá ser compatível com seus tokens. Não reutilizamos a chave de autenticação do FinTalk. Todos aplicam suas migrations no startup. Olivaw e autoridades de clientes executam bootstrap de root automaticamente; DevOps não tem bootstrap.

Os dotenv dos ambientes de todos os projetos acompanham o Git privado por decisão do mantenedor. Novos clones recebem a configuração sem transferência manual ou scripts de preparação. Key Vault será adotado futuramente. O gerador de API cria dotenv iniciais versionados com secrets JWT e senhas root independentes por serviço e ambiente.

## Traefik e redes Docker

O Compose de infraestrutura do FinTalk define `web` em 80 e `websecure` em 443, redireciona HTTP para HTTPS e usa o resolver ACME `myresolver`. Os novos routers usam esse mesmo entrypoint/resolver. As aplicações recebem HTTP na porta **8080** pela rede Docker; não precisam de PFX nem de portas públicas próprias.

Redes externas esperadas:

| Rede | Acesso |
| --- | --- |
| `traefik-net` | Traefik e APIs |
| `postgres-net` | PostgreSQL e APIs |
| `rabbit-net` | RabbitMQ e APIs |

Os Compose de nuvem usam `expose: 8080`, sem `ports`, permitindo executar Limpo, DevOps e outros serviços na mesma VM sem disputar uma porta do host. Labels definem explicitamente a rede, o router e o backend HTTP/8080 de cada serviço e ambiente.

Se a VM estiver diferente dos arquivos de referência, confira os nomes das redes, entrypoints, resolver e as portas disponíveis antes da subida. Alterar somente `TRAEFIK_ENTRYPOINT` para `web` não converte os routers configurados com TLS em HTTP; uma VM exclusivamente HTTP exige ajuste explícito nas labels TLS. Não foi modificada a infraestrutura do FinTalk.

## RabbitMQ e Redis

RabbitMQ usa o broker e virtual host existentes no ambiente, com prefixos como `ms-devops-api-prd` e `ms-limpo-api-stg`, evitando misturar topologias entre serviços/ambientes. As credenciais locais mantêm o usuário de teste disponível no FinTalk. Não foram criados usuários ou virtual hosts novos.

A connection string de Redis foi preservada, mas **não existe integração Redis ativa no código atual**, inclusive no FinTalk analisado. Os projetos continuam usando cache em memória; `Cache:Redis:Configured=false` registra esse estado. O Compose de infraestrutura de referência também não contém `ms-redis-db`. Portanto, endereço/senha preparados não comprovam que há Redis acessível. Quando implementarmos seu consumo, será necessário confirmar o container, a rede compartilhada e o prefixo de chaves antes de habilitá-lo.

## Conferir e subir posteriormente

Todos os comandos partem da raiz do monorepo. Para validar DevOps em produção sem criar containers:

```bash
docker compose --env-file ms-devops-api/docker/.env.prd \
  -f ms-devops-api/docker/docker-compose.yml \
  -f ms-devops-api/docker/docker-compose.prd.yml config --quiet
```

Após preparar os bancos/schemas, as migrations do serviço e confirmar o ambiente da VM:

```bash
docker compose --env-file ms-devops-api/docker/.env.prd \
  -f ms-devops-api/docker/docker-compose.yml \
  -f ms-devops-api/docker/docker-compose.prd.yml up -d --build
```

Para o template, substitua `ms-devops-api` por `ms-limpo-api`. Para outro ambiente, troque o arquivo de variáveis e o override pelo mesmo sufixo; não combine `.env.qas` com o override `.prd`, pois as variáveis contêm nomes e credenciais daquele ambiente.

`OLIVAW_ENV_FILE` aponta para o dotenv que o container recebe. O parâmetro `--env-file` fornece também os valores usados na interpolação do Compose. Mantenha ambos coerentes. Para inspecionar a configuração sem revelar segredos em logs, use `config --quiet`; a saída completa de `config` contém valores sensíveis.

## Acesso local em Docker

O override local desabilita a descoberta Traefik e publica somente em loopback: Limpo em `127.0.0.1:8081` e DevOps em `127.0.0.1:8082`.

```bash
docker compose --env-file ms-devops-api/docker/.env.dev \
  -f ms-devops-api/docker/docker-compose.yml \
  -f ms-devops-api/docker/docker-compose.override.yml up --build
```

Esse modo ainda pressupõe as redes e o PostgreSQL indicados no dotenv. O RabbitMQ fica desabilitado no override local. Fora de Docker, os appsettings Development usam localhost; credenciais e chave JWT devem ser fornecidas pelo ambiente ou user secrets, pois `dotnet run` não carrega os dotenv do Compose automaticamente.

## Validação e limites

A validação local resolve as combinações dos dois serviços nos cinco ambientes e os dois overrides locais. Confere hosts, banco/schema, issuer, prefixos, redes e ausência de publicação de portas nos ambientes de nuvem. A geração de um serviço com hífen confirma a normalização PostgreSQL e a exclusão de credenciais.

Configuração validada não comprova disponibilidade da VM, bancos preparados, permissões, regras do Security Group ou emissão de certificado. Antes da primeira implantação, confirme a resolução DNS, o acesso público ao Traefik nas portas usadas e a existência dos bancos/schemas previstos. O bootstrap de root só pode ser executado depois de preparar o schema; consulte [Olivaw Identity](../olivaw-identity/README.md).

Referências: [roteamento Docker no Traefik](https://doc.traefik.io/traefik/reference/routing-configuration/other-providers/docker/) e [configuração de proxy no ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0). Headers encaminhados por proxies devem ser tratados com uma configuração explícita de proxies confiáveis quando incorporarmos essa integração; não foi habilitada confiança irrestrita em headers recebidos.

## Frontends Angular

`ms-limpo-ng` e `ms-olivaw-ng` seguem o mesmo padrão de cinco ambientes das APIs: Compose base sem portas públicas, override local em loopback e `docker-compose.<dev|qas|rls|stg|prd>.yml` para Traefik. Os dotenv públicos preenchidos são versionados e acompanham o clone, sem script de preparação. O Schematics personaliza os seis arquivos conhecidos (local e cinco de nuvem); os arquivos de ambiente acompanham o repositório privado até a futura migração para Key Vault. Cada ambiente tem projeto Compose, imagem, container e routers próprios.

O site institucional de produção usa `olivaw-hub.duckdns.org`; a API futura usa `api.olivaw-hub.duckdns.org`. Os ambientes adicionais usam `<ambiente>.olivaw-hub.duckdns.org` e `api.<ambiente>.olivaw-hub.duckdns.org`. Para frontends de cliente com subdomínio, o gerador usa `<cliente>-<ambiente>.olivaw-hub.duckdns.org`, alinhado aos hosts das APIs. DNS e certificados desses hosts precisam ser conferidos na VM.

No container, `API_URL` do dotenv gera os settings públicos em runtime. O Nginx entrega arquivos do frontend e não faz proxy para a API. Não há configurações de banco, JWT secreto ou RabbitMQ no frontend. O contexto de build é somente a pasta do frontend. Consulte os comandos em [Docker do site](../ms-olivaw-ng/docker/README.md) e [Docker do template](../ms-limpo-ng/docker/README.md).

A estrutura de arquivos passa a ser compatível com o comando do executor DevOps; o serviço ainda precisa ser autorizado na allowlist da VM. Não houve implantação nem alteração remota nesta preparação.
