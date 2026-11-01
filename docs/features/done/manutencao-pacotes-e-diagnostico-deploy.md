# Manutenção de pacotes e diagnóstico de falhas de deploy

## user-history

Como operador da Olivaw, quero atualizar dependências compatíveis e identificar falhas do Git no deploy, para publicar as aplicações sem perder arquivos ou expor credenciais em logs.

## acceptance-criteria

- Corrigir o bloqueio de checkout identificado na VM preservando os arquivos existentes e os containers.
- Manter backup verificável fora do checkout e conferir a troca de branch em dry-run, sem executar deploy.
- Executor registra diagnósticos fixos e úteis para erros Git, sem publicar stderr bruto, URLs com credenciais ou conteúdo de arquivos.
- Captura de erro limitada em memória, drenando o pipe para não bloquear o processo; timeout continua encerrando e recolhendo o grupo de processos.
- Atualizar referências npm dos quatro frontends e NuGet de todos os projetos para versões estáveis compatíveis; atualizar lockfiles e template.
- Preservar exatamente MassTransit 8.5.12 e MediatR 12.5.0, com conferência das dependências transitivas relacionadas.
- Manter TypeScript 6.0 e tipos do Node 24, compatíveis com Angular 22.2 e os containers Node 24.
- Validar builds, testes, lint e auditorias; documentar restrições ou avisos remanescentes. Sem commit, push ou deploy pelo agente.

## ai-prompt

Desenvolvimento e validação concluídos pelo Codex em 01/11/2026, branch `feature/issue027-agent-prospector`, worktree principal. Escopo ampliado pelo mantenedor durante a investigação do deploy; não abrir outra branch ou alterar histórico como efeito colateral. Sem mudança de schema e sem migrations.

### Causa e correção na VM

Job `9467a4db-9e2b-4990-8efb-5020a4075866` falhou no Checkout, antes de Compose. A VM estava em develop; cinco arquivos não rastreados de `ms-infrastructure-app/scripts` e `tests` impediam a troca. As duas pastas pertenciam a root; os arquivos eram de ec2-user. Comparação byte a byte confirmou que os cinco já eram idênticos ao branch remoto solicitado.

Sob o lock do executor, foi criado backup privado em `/home/ec2-user/.local/state/olivaw/deploy-backups/20261101T142203Z`, com manifesto SHA-256. Foram corrigidos apenas os donos das pastas scripts/tests; as cópias preservadas foram retiradas do checkout. Git status ficou limpo e `git read-tree --dry-run -m -u HEAD origin/feature/issue027-agent-prospector` retornou 0. Branch develop preservado. Nenhum volume, certificado, container ou serviço alterado. Não foi possível atribuir a criação das pastas root a uma pessoa ou processo com as evidências disponíveis.

Não adicionar limpeza destrutiva automática ao deploy. Arquivos divergentes devem continuar bloqueando atualização para revisão, não serem descartados. O executor compartilhado usa um checkout por repositório e serializa os jobs; alternar branches exige checkout limpo e diretórios graváveis pelo usuário de execução.

### Diagnóstico do executor

`scripts/deployment/deploy.py` captura somente stderr dos passos Git com teto de 32 KiB em memória; excesso é drenado e descartado. Saída pública usa mensagens fixas por categoria: permissões, arquivos não rastreados, alterações locais, autenticação, rede, divergência, branch ausente, disco e lock. Compose continua sem captura bruta. Preservado timeout e kill/reap do grupo.

Mudança do executor é local. A cópia root em `/opt/olivaw/deploy.py` não foi substituída: publicar os fontes e seguir [instalação do executor](../../deploy-executor.md) para ativar o diagnóstico novo. Deploy da API sozinho não atualiza essa cópia instalada.

### Dependências

- Angular core/compiler 22.2.4; CLI/Build/SSR/Material/CDK/schematics 22.2.5.
- tslib 2.9.1, ESLint 10.14.0, PostCSS 8.5.32 e typescript-eslint 8.72.1.
- TypeScript 6.0.3 e tipos Node 24.19.2 preservados por compatibilidade; não instalar latest 7/26 indiscriminadamente.
- Pacotes Microsoft ASP.NET/EF/JSON/configuração/criptografia atualizados de 10.0.13 para 10.0.14; XunitXml.TestLogger 8.1.0 e MicroElements.Swashbuckle.FluentValidation 7.2.3.
- MassTransit 8.5.12, MediatR 12.5.0 e respectivas transitivas preservados. Outros pacotes que já estavam na última versão estável permaneceram iguais.
- Removido override de Vite: Angular Build 22.2.5 já exige 8.3.5.
- Override restrito `minimatch@^10.2.0` para 11.0.1 corrige GHSA-g3hv-75wg-m249 da cadeia de lint. Revisar/remover quando consumidores incorporarem a correção. Não usar downgrade/force sugerido pelo audit.

Fontes: [compatibilidade Angular](https://angular.dev/reference/versions), [política .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core), registros oficiais npm/NuGet e [advisory minimatch](https://github.com/advisories/GHSA-g3hv-75wg-m249).

### Validação

- Build Release da solução: 0 erros e 0 warnings.
- Suíte .NET completa: 958 testes em sete projetos, aprovados. Árvore resolvida confirma MassTransit 8.5.12 e MediatR 12.5.0, incluindo transportes/contratos relacionados sem mudança frente ao baseline.
- Quatro frontends: build e lint aprovados; 76 testes unitários (46 Olivaw, 18 LTMJ, 6 Cavalcante, 6 template).
- E2E: Olivaw executou 100 casos; dois testes antigos falharam por navegação anterior de Leads e seletor de status ambíguo. Ajustados somente os testes; ambos os arquivos foram repetidos com nove casos aprovados. Os outros 98 passaram na execução completa. LTMJ 8, Cavalcante 5 e template 3 aprovados.
- Testes do executor: 29 aprovados, incluindo stderr volumoso, sanitização e timeout.
- Instalação npm concluiu com zero vulnerabilidades reportadas nos quatro projetos. Auditoria NuGet, incluindo transitivas, sem vulnerabilidades reportadas. Isso retrata a consulta atual, não uma garantia permanente.
- Warnings de orçamento SCSS já existentes nos frontends permanecem; não houve refatoração visual para escondê-los. npm informa scripts nativos sem aprovação global; builds/testes usaram os binários opcionais disponíveis, sem liberar scripts indiscriminadamente.

Nenhum teste usa banco de cliente ou publicação remota. As aplicações usam seus próprios lockfiles; E2E foram executados com APIs simuladas e portas isoladas para não reaproveitar o frontend localhost de outro projeto.

Comandos reproduzíveis: `dotnet restore Olivaw.sln -m:1`, `dotnet build Olivaw.sln -c Release --no-restore -m:1`, `dotnet test Olivaw.sln -c Release --no-build -m:1`, `dotnet list Olivaw.sln package --include-transitive --no-restore --format json`, `dotnet list Olivaw.sln package --vulnerable --include-transitive --no-restore --format json`; em cada frontend, `npm ci`, `npm run build`, `npm run lint`, `npm test`, `npm run e2e` e `npm audit`. Não reutilizar um servidor de outro projeto na porta de E2E; as validações usaram configurações temporárias com portas próprias. Nenhum desses comandos autoriza deploy.

## user-feedback

```text
ta falahndo muito nosso deploy, consegue ivestigar porque?

makina@xps:~/projects/olivaw$ bash scripts/deployment/deploy.sh ms-olivaw-api prd feature/issue027-agent-prospector
Destino: olivaw/ms-olivaw-api | ambiente: prd | branch: feature/issue027-agent-prospector
Job solicitado: 9467a4db-9e2b-4990-8efb-5020a4075866
Consulta: https://api.devops.olivaw.com.br/deployments/9467a4db-9e2b-4990-8efb-5020a4075866
Deployment started.
Fetch: started.
Fetch: exit code 0.
Checkout: started.
Checkout: exit code 1.
Deployment failed; subsequent steps were skipped.
Status: failed
Job 9467a4db-9e2b-4990-8efb-5020a4075866 falhou. Consulte o log pelo endpoint acima.

ai já aproveita para atualizar todos os nossos pacotes npm e dotnet do nosso projeto, obviamene n o mass transit e mediaor que sao pagos acima de outras major versions

otimo, perfeito
```
