# Segurança do servidor, rotas públicas e infraestrutura reproduzível

## user-history

Como responsável pela Olivaw,
quero proteger o acesso administrativo, conhecer as superfícies públicas e versionar a infraestrutura,
para reduzir riscos e conseguir reconstruir os serviços em outra VM sem perder dados.

## acceptance-criteria

- Chave SSH administrativa fora do índice Git, ignorada e preservada localmente em 0600; cópias administrativas identificadas no servidor são removidas sem apagar chaves de host, certificados HTTPS ou a identidade restrita do executor.
- Riscos do histórico Git e rotação da chave são tratados separadamente; retirada do índice não é apresentada como remoção histórica.
- Auditoria distingue evidências do host, análise do código, hipóteses, limitações e correções efetivamente aplicadas; nenhum segredo aparece no relatório.
- Todas as rotas anônimas das APIs do monorepo são inventariadas, incluindo middleware, com revisão de exposição, autorização, abuso e custo operacional.
- `ms-infrastructure-app` contém Compose, `.env` real, instruções de agentes e procedimentos para preservar redes, volumes e ACME em migração; Redis é opcional.
- Banco, broker e cache não publicam portas no host; access logs omitem credenciais e têm retenção limitada.
- Validação é local e não inicia containers nem modifica dados. Alterações de SSH, firewall, versões e produção exigem plano de implantação com verificação e recuperação.

## ai-prompt

### Estado, responsável e escopo

Em discovery por solicitação do mantenedor. Auditoria inicial e contenção da cópia privada concluídas em 24/10/2026; hardening, rotação e validação de recuperação ainda pendentes. Responsável: Codex, execução sequencial no worktree `/home/makina/projects/olivaw`, branch recebida `feature/issue023-tests-and-adjusts`. Nenhuma issue adicional foi criada ou presumida. Arquivos compartilhados: `AGENTS.md`, `.gitignore`, índice documental e novo módulo de infraestrutura.

Autorização explícita para SSH, auditoria e limpeza de cópias privadas; a leitura remota usou verificação estrita do host, sem desabilitar a identificação SSH. Senhas atuais de banco/API em configuração de containers ficam fora da remediação imediata, por decisão do mantenedor; não foram reproduzidas. Não houve alteração de senha, firewall, SSHD, bancos, Security Groups ou implantação.

### Alterações já realizadas

- `git rm --cached docs/secrets/fintalk-key.pem` removeu a chave do índice, preservando a cópia local em 0600. Regras de ignore para chaves SSH e atualização da exceção anterior no `AGENTS.md`. Isso ainda precisa de commit/publicação para alterar a ponta remota do repositório.
- Varredura local de cabeçalhos de chave privada encontrou somente esse arquivo. Scanner preventivo no novo módulo verifica conteúdo de arquivos rastreados sem imprimir segredos, inclusive em extensões diferentes de `.pem`.
- No servidor, foi localizada `/home/ec2-user/fintalk/ms-fintalk-dat/fintalk-key.pem`, modo 0644. O SHA-256 do conteúdo foi comparado com o arquivo local sem registrar o conteúdo; a cópia era idêntica e foi apagada. A outra localização sugerida `/home/ec2-user/ms-fintalk-dat/fintalk-key.pem` já estava ausente. A cópia local permaneceu íntegra e disponível.
- `/etc/olivaw/ssh/deployment_key`, modo 0400, proprietário UID 1654 e montada somente no DevOps, foi preservada. É identidade operacional distinta da chave administrativa. Chaves de host SSH e ACME também foram preservados.
- Criado [módulo de infraestrutura](../../../ms-infrastructure-app/README.md) com Compose, `.env`, AGENTS, preparação explícita de VM nova e scanner de chaves. Sem `example` no nome, conforme nova preferência. Na preparação inicial, credenciais ficaram vazias. Após autorização de migração, o .env local/remoto foi preenchido com os valores preservados dos containers existentes, sem imprimi-los; não houve troca de senha.

### Evidências da VM

Leitura por SSH em 24/10/2026. Amazon Linux 2023, kernel `6.18.48-109.150.amzn2023.x86_64`, Docker Engine `25.0.16`.

- Host escuta TCP 22/80/443 em IPv4 e IPv6; apenas 80/443 são publicados por containers. PostgreSQL 5432, RabbitMQ 5672/15672 e APIs 8080 não estão publicados no host. Isso não prova a política externa dos Security Groups, que não foi consultada.
- SSH: senha e autenticação interativa desabilitadas, chave pública habilitada; `MaxAuthTries=6`, encaminhamento TCP e X11 permitidos. `PermitRootLogin=without-password`; a entrada de root em authorized_keys tem comando/restrições. Não se conclui que root tenha acesso administrativo irrestrito. Arquivos authorized_keys de root e ec2-user estão em 0600; ec2-user tem duas entradas, uma restrita.
- SELinux `Permissive`; firewalld inativo e política INPUT observada `ACCEPT`. É ausência de defesa local adicional, sem inferir que a AWS permite todo tráfego. Não ativar firewall/SELinux às cegas em produção: pode cortar SSH, Docker ou mounts.
- A requisição sem token ao metadata de instance-id retornou 401, compatível com exigência de IMDSv2. Não foram lidas credenciais IAM nem user-data; opções da instância e hop limit ainda precisam de revisão no plano AWS.
- Traefik `v3.0`, sem dashboard inseguro nos argumentos e com `exposedbydefault=false`. Access log desabilitado. Socket Docker montado `:ro`, mas isso não impede operações pela API do socket. Nenhum container observado está em modo privileged; APIs usam usuário `app`, frontends usam 101 e Lisierp usa node.
- Todos os containers observados usam `json-file` sem opções de rotação; não foi encontrado daemon.json com retenção global. Aplicações continuam sem essa limitação após apenas versionar o novo Compose.
- Redes `postgres-net`, `rabbit-net` e `traefik-net` são compartilhadas e não estão marcadas internal. Uma aplicação comprometida nessas redes pode alcançar serviços internos. Marcar rede internal indiscriminadamente pode quebrar egress e roteamento; revisar segmentação por dependência.
- Volumes confirmados: `ms-fintalk-dat_postgres-data`, `ms-fintalk-dat_rabbitmq-data`, `ms-fintalk-dat_rabbitmq-logs`. ACME atual em `/home/ec2-user/ms-fintalk-dat/acme.json`, 0600. Não alterar nomes ou criar volume vazio ao migrar.
- Recursos: aproximadamente 913 MiB de RAM, 238 MiB disponíveis na leitura e 766 MiB de swap em uso; disco raiz de 30 GB com 35% ocupado. Não instalar scanners pesados ou Redis permanente por padrão.
- Consulta a advisories de segurança do DNF usando somente cache não listou atualizações. Metadados tinham cerca de dez horas; não é prova de sistema/imagens atualizados. Imagens não foram analisadas por scanner de CVEs.
- Contagem amostral dos logs SSH das últimas 24 horas encontrou uma ocorrência de usuário inválido. O limite de saída não permite atestar ausência de intrusão nem fazer atribuição de autoria.

### Riscos e prioridades

| Prioridade | Achado | Situação e ação |
| --- | --- | --- |
| Alta | Chave administrativa já existiu no Git e na VM com 0644 | Cópia de trabalho remota removida; referências históricas confirmadas nos clones `olivaw` e `fintalk`. A chave ainda pode ser recuperada do histórico e continua válida. Rotacionar: gerar outra somente na máquina local, adicionar chave pública, verificar nova sessão, revogar a antiga e atualizar acessos. Avaliar reescrita coordenada dos dois históricos depois; não fazer force-push automaticamente. |
| Alta | Traefik acessa socket Docker | Confinar acesso por proxy com endpoints mínimos ou outra descoberta, validar compatibilidade. `:ro` sozinho não limita comandos do daemon. Sem mudança remota nesta etapa. |
| Média | Falta de access logs e rotação | Configuração corrigida no Compose novo; requer recriação planejada do Traefik. O passado sem access log não identifica IP de scanner. Demais aplicações precisam receber retenção própria. |
| Média | Readiness anônimo mostra detalhes e consulta dependência | Resposta contém host do banco e estado interno; restringir detalhes à operação, manter probe público mínimo e aplicar timeout/limite. Compatibilidade do contrato precisa ser revisada antes de alterar as APIs. |
| Média | Rate limiting usa IP da conexão sem forwarded headers configurados no código | Atrás do Traefik, usuários podem compartilhar a mesma partição e causar bloqueio coletivo. Conferir ambiente efetivo e adicionar confiança somente no proxy conhecido; nunca confiar em X-Forwarded-For arbitrário. |
| Média | CORS global amplo | Core permite qualquer origem, cabeçalho e método, sem AllowCredentials. Não concede token ou permissão, mas permite escritas públicas de qualquer site. Definir allowlist por aplicação/ambiente; não tratar CORS como controle contra bots. |
| Média | Leituras públicas de portfólio potencialmente custosas | Listagem LTMJ materializa todos os eventos antes de paginar; fotos públicas não têm limite próprio de requisições. Paginar no banco, controlar custo/armazenamento e manter apenas eventos visíveis. |
| Média | Limites de corpo e isolamento incompletos | Leads limitam 64 KiB; login/convites não definem limite específico de corpo. Avaliar limite global conservador e exceção para upload autenticado, timeout e limites de conexão. Evitar afetar upload de fotos autorizado de até 102 MiB. |
| Média | Host depende muito da defesa AWS | Revisar Security Groups IPv4/IPv6 e SSH por origem, modo SELinux, root, forwarding e política do firewall; executar mudanças com sessão de recuperação disponível. |
| Média | Versões e backup sem prova de recuperação | Traefik 3.0, RabbitMQ 3 e imagens por tags mutáveis exigem revisão de suporte/advisories e pinagem testada. Não houve restauração de backup nem comprovação de RPO/RTO. |
| Baixa | Sessão administrativa acessível a JavaScript | Olivaw usa sessionStorage por API para sessão interna; portal mantém token em memória. Rever CSP/XSS e bibliotecas; não migrar para cookie sem desenhar CSRF/SameSite e compatibilidade. |

A remoção do arquivo é contenção parcial. Não há evidência suficiente para afirmar que a chave não foi copiada antes; a orientação de rotação segue a [documentação do GitHub](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/removing-sensitive-data-from-a-repository). Alterar somente permissões não revoga cópias existentes.

### Inventário das rotas anônimas e middleware

Revisão estática das APIs Olivaw, DevOps, LTMJ, template Limpo, identidade compartilhada e Core. Deploy ativo pode usar commit/configuração diferentes. Não foram enviados logins, convites, leads, uploads, comandos de deploy ou leituras de banco em produção para testar a auditoria.

| Superfície | Acesso e controles no código | Pendência |
| --- | --- | --- |
| `POST /identity/authentication/login` nas autoridades | AllowAnonymous intencional, política identity-login de dez/minuto por IP, senha hasheada, falha genérica e bloqueio após cinco falhas por 15 minutos | Confirmar IP efetivo atrás do proxy e limitar corpo. DevOps consumidor não registra os controllers da autoridade. |
| `PUT /leads/{externalId}` Olivaw | AllowAnonymous intencional, chave X-Lead-Key com hash e comparação constante, revisão/idempotência, rate limit de vinte/minuto, corpo 64 KiB; resposta sem ficha interna | Pode criar novo lead público, por desenho. Monitorar spam, retenção, consentimento e abuso; leitura/histórico/follow-up exigem leads.manage. |
| `POST /portal/sessions` | AllowAnonymous intencional, JWT próprio e identity-login | Limite por IP compartilha política com convites; login desabilitado sem autoridade válida. |
| `POST /portal/invitations/read` | Token aleatório de 256 bits, hash no banco, expiração e rate limiting; não é busca por e-mail | Possuir link permite consultar destinatário/empresa. Evitar vazamento de links e limitar corpo. |
| `POST /portal/invitations/accept` | Token válido, destinatário, validade, uso único; conta ativa exige autenticação explícita do portal; conta nova exige senha e stamp compatível | Preservar tratamento de concorrência e transações; auditar fluxos de recuperação e revogação em novos testes. |
| `GET /ready` nas quatro APIs | AllowAnonymous, retorna detalhes, consulta conectividade com banco | Divulgação de infraestrutura e custo por chamada, achado médio. Não confundir com acesso público ao banco. |
| `/health` do Core | UseHealthChecks é inserido em UseRoutingCore antes de UseSecurityCore nos hosts examinados; pode responder sem autenticação e sem executar o fallback | Manter corpo mínimo e controlar taxa/custo; validação dinâmica do pipeline ainda pendente, sem chamada em produção. |
| `GET /portfolio/events` LTMJ | Controller força PublicOnly=true, propriedade interna não vem do cliente; exige visibilidade | Paginação ocorre em memória; limitar custo de consultas. |
| `GET /portfolio/events/{eventId}/photos/{photoId}` LTMJ | Leitura pública somente do evento visível, salvo permissão portfolio.manage; nosniff/no-store | Rate limit/cache/limite de tráfego; validar armazenamento e exclusão lógica em revisão própria. |
| `/swagger` e `/openapi/v1.json` | Habilitados em Development ou por swagger:enabled; middleware pode responder antes da autorização | Confirmar flag runtime e bloquear em produção quando desnecessário. Não assumir proteção pelo fallback. |
| `/metrics` | Condicional a services:loki:configured; map usa autorização fallback | Confirmar configuração e endpoint em teste integrado antes de expor. |
| Demais controllers | Controllers são mapeados com RequireAuthorization; identidade configura DefaultPolicy e FallbackPolicy. Gestão exige políticas identity.manage, olivaw.root, leads.manage, portfolio.manage, devops.deploy ou devops.root; portal exige portal.member e vínculo atual | Manter testes negativos de token ausente, autoridade errada e empresa trocada. `/version` não tem AllowAnonymous. Rotas inexistentes retornarem 401 é efeito do fallback, não prova de invasão. |

Frontend: a busca inicial não encontrou uso de bypassSecurityTrust ou innerHTML nos arquivos examinados; não equivale a auditoria completa de XSS/dependências. Guards não substituem controle no servidor. Config JSON do frontend deve conter apenas URLs públicas. O upload de fotos está autenticado; os limites grandes podem pressionar a VM mesmo com autorização.

### Validação e limites

- Compose base e profile Redis validados com `config --quiet`, usando valores sintéticos apenas no ambiente do comando; nenhum container iniciado.
- Scanner sem chaves privadas no índice após retirada; quatro testes aprovados cobrem detecção por conteúdo/fronteira de leitura, preservação local, recusa de VM nova com containers existentes e proibição de criar volumes/ACME no modo de conferência.
- SSH confirmou leitura privilegiada, inventário de portas/mounts/redes/permissões e exclusão da cópia exata identificada. Varredura abrangeu `/home`, `/root`, `/etc/olivaw`, `/opt`, `/tmp`, `/var/tmp`, e listagem adicional de candidatos em `/etc/ssh` e `/srv`. Diretórios Git, caches, node_modules e artefatos foram excluídos da busca de conteúdo; não foi varrido byte a byte todo o disco, imagens, volumes ou backups. Históricos Git foram checados separadamente por referências.
- Suítes locais reexecutadas e aprovadas: identidade 102, Olivaw 359, DevOps 93 e LTMJ 51, total 605 testes. Nenhuma alteração de código de API foi aplicada nesta auditoria. Não executar teste de carga nem exploração em produção.
- Security Groups/NACLs/IAM, DNS externo, política EBS/backups, imagens/dependências transitivas, outros repositórios da VM, histórico completo de autenticação e investigação forense permanecem fora da comprovação desta leitura. Uma análise inicial ampla não garante ausência de comprometimento.
- O novo Compose não foi instalado. Não houve reinício de proxy, banco, broker ou aplicação. Não houve reescrita histórica, commit, push ou fechamento remoto de issue.

### Retenção de logs

O novo Compose usa `json-file`, `max-size=5m` e `max-file=3`: aproximadamente 15 MB por container do módulo, removendo arquivos antigos conforme gira. É limite de tamanho, não prazo por dias; a janela histórica depende da taxa de requisições. Não há arquivamento externo nem garantia de reter um número mínimo de dias. A configuração só vale após recriar cada container e não atinge automaticamente as aplicações existentes. Na migração autorizada de 24/10/2026, os três containers de infraestrutura foram recriados e a retenção já está ativa neles. O RabbitMQ também possui volume próprio de logs: a rotação do stdout Docker não limita esse volume. Incluir esse volume, journals do host e demais aplicações na política de retenção antes de declarar o disco protegido integralmente.

### Migração autorizada e verificação em 24/10/2026

Após a auditoria inicial, o mantenedor autorizou explicitamente parar a infraestrutura antiga, subir o novo Compose e testar acesso das APIs. Essa autorização foi executada, sem alterar as pendências de segurança restantes. Os estados anteriores acima descrevem a leitura inicial; a configuração de logs dos três containers de infraestrutura foi corrigida nesta migração.

- Mesmo volume PostgreSQL, mesmos dois volumes RabbitMQ, mesmas redes, credenciais e image IDs. Nenhum volume removido, criado ou renomeado. ACME no mesmo bind e em 0600. `--pull never --no-build`; sem upgrade de imagens.
- Hostname RabbitMQ preservado como `ce49564db068`, mantendo `rabbit@ce49564db068`. Preservar apenas o volume sem essa identidade pode fazer RabbitMQ abrir outro diretório Mnesia.
- Backup lógico dos bancos, configuração e ACME em diretório root-only `/var/backups/olivaw-infra/20261024T153252Z`; cópia do volume RabbitMQ realizada depois de parar o broker. Arquivos privados 0600, fora do Git.
- Nova stack ativa no projeto `ms-infrastructure-app`. Containers anteriores parados e renomeados com sufixo rollback; não iniciar dois PostgreSQL/RabbitMQ simultaneamente sobre os mesmos volumes. Limpeza somente após aceitação, sem `-v`.
- PostgreSQL conservou identificador do cluster e os mesmos bancos/OIDs da leitura prévia. Não foi comparado o conteúdo de todas as tabelas; esta conferência prova reutilização do cluster, não uma auditoria integral de integridade de dados.
- Olivaw, DevOps e LTMJ: readiness interno e HTTPS com TLS validado responderam HTTP 200 e status pass do PostgreSQL. `/version` sem autenticação continuou 401. Containers das três APIs não foram substituídos nem reiniciados.
- Cada container de API alcança a porta AMQP 5672 por DNS `ms-rabbit-mq`. RabbitMQ está plenamente iniciado e seu endpoint administrativo interno autenticado respondeu 200 com a mesma identidade do nó. Sem publicação de mensagens de negócio nem processamento artificial de filas. Não havia conexões/consumidores ativos na leitura; alcance TCP e acesso administrativo não comprovam um fluxo completo de mensagens de aplicação. Credenciais das APIs Olivaw/DevOps autenticaram no broker com HTTP 200; LTMJ está com MassTransit desabilitado. Contagem dos logs recentes não encontrou os padrões de falha de conexão inspecionados.
- Traefik produziu seis entradas durante a conferência; campos de acesso presentes, sem Authorization, Cookie ou X-Lead-Key. Todos os três novos containers têm json-file com max-size=5m/max-file=3 e zero reinícios na primeira verificação. Redis não foi iniciado.
- A migração não reescreveu histórico, não rotacionou a chave administrativa e não corrigiu automaticamente os demais riscos desta feature. Recuperação integral em VM nova e hardening continuam pendentes.

O recorte instalado está documentado separadamente em [infraestrutura compartilhada concluída](../done/infraestrutura-compartilhada.md).

### Próxima execução

Primeiro rotacionar a chave e planejar a remoção coordenada dos históricos. Access logs/rotação da infraestrutura já foram aplicados; ampliar retenção para aplicações, volume RabbitMQ e host. Depois corrigir readiness público, forwarded headers confiáveis e paginação/custo público, com testes. Revisar socket Docker, segmentação, AWS e versões antes de declarar hardening concluído. Ao terminar cada recorte, documentar evidência e mover apenas a entrega concluída para done; este plano permanece em discovery.

Referências primárias: [segurança do daemon Docker](https://docs.docker.com/engine/security/), [access logs Traefik](https://doc.traefik.io/traefik/v3.2/reference/install-configuration/observability/logs-and-accesslogs/), [proxies confiáveis no ASP.NET Core](https://learn.microsoft.com/en-us/dotnet/core/compatibility/aspnet-core/8.0/forwarded-headers-unknown-proxies) e [opções IMDS da AWS](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/configuring-instance-metadata-options.html).

## user-feedback

```text
aqui está o nosso compose .

eu quero que voce faça o seguinte, verifique onde temos a chave ssh salva aqui no projeto e remova ela do versionamento.
vai ficar apenas na nossa maquina aqui de casa.

ai voce entra no nosso servidor usando o command "makina@xps:~/projects/olivaw$ ssh -i docs/secrets/fintalk-key.pem ec2-user@98.95.250.122"

lá dentro voce tbm vai investigar se tem a chave salva em algm lugar do disco.
e ai voce vai fazer uma analise de segurança no servidor e ver se temos algo perigoso lá pra gente arrumar blz?

por hora senha de banco de dados ou API que estiver dentro dos containers da nada. mas deixa anotada a analise de segurança em uma nova feature em discovery blz.

ai voce vai fazer o seguinte tbm aqui no nosso projeto vamos criar um ms-infrastructure-app, dentro dele vamos versionar o arquivo de compose necessário para subir o bd, rabbit, redis etc. PORQUE se a gente precisar trocar de VM temos tudo dentro do projeto.

coloque arquivos agents md dentro dele tbm blz.

pode iniciar ai nossa feature de segurança

algumas correcoes voce mesmo faz como limpar as chaves pem e tals para que ninguem roube elas

verifique tbm nossas rotas anonimas faça a analise completa de segurança

para de criar esses .env com 'example' no nome, porque nomear de example? deixa o nome real mesmo. OU example é padrao?

esses logs n podem entupir nosso servidor blz, tem um tempo que mantém eles lá?

vi que voce criou volumes ou variaveis de volme tbm tem que preservar os mesmos blz ou perdemos dados.

após confirmar isso faz o seguinte, mata os containers de infra e sobe o nosso novo compose blz?
ai testa se as apis tão acessando coretamente
```
