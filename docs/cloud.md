# Configuração da VM AWS, DuckDNS e Traefik

## Referência e escopo

As configurações de `ms-limpo-api` e `ms-devops-api` seguem o appsettings de produção e os arquivos Docker do FinTalk, consultados somente para leitura. O core e a identidade são bibliotecas; configuração de banco, broker e exposição HTTP pertence aos hosts consumidores.

Foram reaproveitados os endereços internos `ms-postgres-db`, `ms-rabbit-mq` e `ms-redis-db`. O banco é separado por organização e ambiente; cada serviço possui seu schema. Domínio e prefixo de mensagens identificam serviço e ambiente. O banco `fintalk_core` não é usado pelo Olivaw Hub.

Nenhum banco, schema, usuário, fila, certificado ou recurso da AWS foi criado ou alterado nesta tarefa. Também não houve publicação de containers.

## Endereços e nomes

| Serviço | URL de produção | Banco de produção | Schema |
| --- | --- | --- | --- |
| Template | `https://ms-limpo-api.fintalk-server.duckdns.org` | `olivaw_prd` | `ms_limpo` |
| DevOps | `https://ms-devops-api.fintalk-server.duckdns.org` | `olivaw_prd` | `ms_devops` |

Os bancos usam sempre o sufixo do ambiente: `olivaw_dev`, `olivaw_qas`, `olivaw_rls`, `olivaw_stg` e `olivaw_prd`. O domínio de produção não tem sufixo; os demais usam, por exemplo, `ms-devops-api-stg.fintalk-server.duckdns.org`.

Os nomes de bancos e schemas usam somente minúsculas, números e sublinhados. Ao gerar `cloud-validacao`, o template produz `olivaw_dev` e `ms_cloud_validacao`, preservando os hífens em `ms-cloud-validacao-api` e no domínio.

`fintalk-server.duckdns.org` é a base DNS já usada pelo ambiente de referência. Não foi registrado outro domínio DuckDNS. Se trocar `PUBLIC_HOST`, atualize também `Identity__Issuer` e `Identity__Audience` para a URL correspondente.

## Governança dos dados e identidade

- Serviços internos do Olivaw compartilham `olivaw_<ambiente>`, cada um com seu schema (`ms_devops`, `ms_servico_novo`).
- Cada cliente possui seu banco, por exemplo `pastelaria_dev`, com schemas `ms_service1` e `ms_service2` para seus serviços.
- Cada serviço mantém seu `DbContext`, migrations e histórico `__efmigrationshistory` no próprio schema. Prepare o schema antes de executar migrations; `SearchPath` não o cria e não substitui permissões de acesso.
- Use usuários de banco limitados ao schema em produção. As conexões atuais com `postgres` são temporárias para testes. Evite consultas e chaves estrangeiras entre schemas de serviços; integre por APIs ou mensagens.
- A identidade pertence ao serviço principal do cliente. Serviços agregados consumirão o JWT desse emissor, sem cadastros de identidade próprios. Chave compartilhada exige isolamento por cliente/ambiente e validação de emissor, audiência e expiração; permissões e revogação também precisam de um contrato comum.

O módulo atual de identidade consulta usuários e permissões locais; aceitar tokens em um serviço agregado sem essas tabelas exigirá um modo de validação próprio. Essa implementação não faz parte desta configuração do DevOps.

Backups automatizados e restauração serão implementados posteriormente. O planejamento deve incluir retenção, cópia independente do volume da VM e testes de restauração; um restore completo do banco afeta todos os serviços da organização naquele ambiente.

## Arquivos de configuração

- `appsettings.json`: estrutura comum e integrações externas desabilitadas por padrão.
- `appsettings.<Ambiente>.json`: endereços sem senha, banco/schema, identificação JWT, Swagger e prefixo de mensagens daquele ambiente.
- `docker/docker-compose.yml`: build a partir da raiz do monorepo, porta interna e redes comuns.
- `docker/docker-compose.<dev|qas|rls|stg|prd>.yml`: ambiente, imagem, nome do container e roteamento Traefik.
- `docker/.env.example`: exemplo versionado, sem credenciais reais.
- `docker/.env` e `.env.dev`, `.env.qas`, `.env.rls`, `.env.stg`, `.env.prd`: configurações locais preparadas para os testes, ignoradas pelo Git e pelo contexto Docker. `.env` equivale à configuração de produção.

As senhas atuais de banco, RabbitMQ e Redis foram reaproveitadas do FinTalk somente nesses arquivos locais. Os hosts atuais receberam chaves JWT aleatórias por ambiente. Para futuros serviços agregados de um cliente, o emissor principal será único e a configuração de validação deverá ser compatível com seus tokens. Não reutilizamos a chave de autenticação do FinTalk. O bootstrap de root e a aplicação automática de migrations permanecem desligados.

Os arquivos reais não acompanham um clone/pull: transfira o arquivo do ambiente necessário para a VM por um meio apropriado ou recrie-o a partir do exemplo. Não é necessário copiar os cinco ambientes se usar somente produção. O gerador exclui os dotenv reais e inclui apenas os exemplos; cada novo serviço precisa receber suas próprias credenciais e chaves.

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
