# 04b — Inventário de recursos e ambientes

## user-history

Como operador da Olivaw,
quero inventariar recursos, ambientes e implantações,
para conhecer seus vínculos, reservas e histórico.

## acceptance-criteria

- Escopo, dependências e limites do registro original são respeitados.
- Entregas têm validações e pendências documentadas, sem apresentar planejamento como funcionalidade disponível.

## ai-prompt

Concluída no desenvolvimento; preserve pendências de publicação e ambiente.

Trabalhe somente no recorte autorizado desta feature. Confira os AGENTS.md aplicáveis, a implementação atual e o registro completo em `user-feedback`. Preserve os limites e as evidências históricas; registre testes e pendências. Migrations, finish, push e publicação dependem de autorização explícita.

### Estado

Implementado e validado localmente na branch `feature/issue020-inventario-recursos`. Complementa o [cadastro de serviços](04a-cadastro-de-servicos.md) e conclui a base de inventário prevista na [feature 04](../discovery/painel-olivaw/04-inventario-de-recursos.md). A evolução por tipo permanece na 04c, e medição/custos nas features 07/17.

O schema permanece coberto pela única migration PostgreSQL `Issue020`, gerada na correção de startup. Esta conclusão não gerou outra migration, não aplicou SQL nem publicou a nova interface/API. O mantenedor informou a recuperação da API anterior na AWS; os fluxos novos descritos aqui ainda precisam de publicação e smoke test no ambiente.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
# 04b — Inventário de recursos e ambientes

## Estado

Implementado e validado localmente na branch `feature/issue020-inventario-recursos`. Complementa o [cadastro de serviços](04a-cadastro-de-servicos.md) e conclui a base de inventário prevista na [feature 04](../discovery/painel-olivaw/04-inventario-de-recursos.md). A evolução por tipo permanece na 04c, e medição/custos nas features 07/17.

O schema permanece coberto pela única migration PostgreSQL `Issue020`, gerada na correção de startup. Esta conclusão não gerou outra migration, não aplicou SQL nem publicou a nova interface/API. O mantenedor informou a recuperação da API anterior na AWS; os fluxos novos descritos aqui ainda precisam de publicação e smoke test no ambiente.

## Organização

- Serviço representa a aplicação lógica, como uma API ou frontend.
- Implantação representa o serviço em um ambiente: desenvolvimento, qualidade, release, homologação ou produção. Há no máximo uma implantação vigente por serviço/ambiente; um período encerrado conserva identidade/histórico e não é reativado.
- Recurso representa um ativo físico ou lógico: VM, volume, instância PostgreSQL, banco lógico, broker/namespace de mensagens, instância/namespace de cache, domínio, subdomínio, IP, DNS, proxy, certificado, backup, observabilidade, registro de imagens, serviço externo ou outro.
- A implantação usa recursos com finalidade e reservas declaradas. Recursos se relacionam entre si e podem beneficiar clientes diretamente, independentemente de existir uma aplicação.

Nenhum recurso da LTMJ ou da plataforma é pré-cadastrado. O operador informa somente o que conhece, sem descoberta remota, provisionamento ou verificação de URLs.

## Cadastro e identidade

Recursos têm identificador técnico imutável, nome, tipo, finalidade, provedor, conta/contrato, região, identificador do provedor, proprietário, pagador, responsável operacional, escopo, situação, fonte e referência técnica. A confirmação distingue informado, conferido e desativado; conferido exige fonte. Monitoramento permite não configurado ou planejado, sem exibir saúde inventada.

A identidade do provedor é única por provedor/conta/região/tipo/identificador e fica preservada depois de atribuída. Um ativo recriado requer outro recurso. Identificador desconhecido fica vazio, sem inventar valores; IP ou URL não substituem a identidade estável. Referências aceitam HTTP(S), sem credenciais, query ou fragmento, e nunca são acessadas pelo cadastro.

Capacidades são colunas tipadas: vCPU, memória em GiB e armazenamento em GiB. Reservas usam as mesmas unidades nos usos por implantação. Ausência aparece como desconhecida, distinta de zero. A ficha soma somente reservas diretamente declaradas para aquele recurso; não soma novamente reservas herdadas pela cadeia e não converte valores em consumo ou custo.

## Vínculos e histórico

- Vínculo direto com cliente, relação técnica e uso por implantação têm início/fim, finalidade, revisão e autores de abertura/encerramento obtidos da identidade autenticada.
- Encerrar preserva o período; retomar cria outro vínculo. Não há exclusão ou reescrita silenciosa de períodos antigos.
- Relações disponíveis: faz parte de, executa em, armazena em, exposto por e depende de. Tipos incompatíveis e autorreferência são recusados; ciclos de composição também. Dependências gerais podem formar ciclos e a consulta termina com deduplicação.
- Encerrar implantação encerra seus usos atuais na mesma unidade de persistência. Encerrar serviço encerra suas implantações e respectivos usos. Recursos compartilhados continuam cadastrados.
- Desativar recurso exige encerrar seus vínculos ativos. Alterar tipo exige encerrar relações técnicas; identidade do provedor atribuída continua imutável.
- Revisões otimistas detectam alterações concorrentes. Uma revisão compartilhada da topologia protege composição, encerramentos e mudanças dos beneficiários contra decisões simultâneas incompatíveis. Conflitos retornam HTTP 409; a interface preserva o formulário e oferece recarregamento explícito.
- Histórico do cadastro e de implantações mostra até 20 snapshots recentes. Os períodos de vínculos, inclusive encerrados, têm paginação própria e permanecem preservados.

## Consulta e interface

`/admin/resources` oferece busca, paginação e filtros por tipo, provedor, cliente, ambiente, situação e compartilhamento. O formulário diferencia capacidade, responsabilidades, identidade e fonte. A ficha mostra beneficiários distintos, reservas, vínculos diretos, relações técnicas, usos por ambiente e histórico.

A ficha do serviço oferece ambientes/implantações. A ficha da implantação permite escolher recursos existentes, declarar reservas e encerrar o período com confirmação. A ficha do cliente reúne recursos diretos e dependências alcançadas pelos seus serviços, com origens navegáveis e deduplicação por recurso. Dois caminhos para o mesmo disco continuam sendo um disco.

Compartilhado significa mais de um cliente identificado nos vínculos atuais. Um cliente identificado não comprova exclusividade física, isolamento ou percentual de custo. As origens mostram o vínculo direto, serviço/ambiente ou recurso anterior na cadeia; é possível navegar pelos recursos de origem para acompanhar o caminho.

Acesso é exclusivo do root no frontend e no servidor, conforme a base administrativa atual. Operadores de leads e contas de empresa não recebem acesso ao inventário; o portal com isolamento por cliente continua em seu planejamento próprio.

## Rotas e persistência

- `GET/POST /resources`, `GET/PUT /resources/{externalId}`.
- `GET/POST /deployments`, `GET/PUT /deployments/{externalId}`.
- `POST /resource-customers`, `PUT /resource-customers/{externalId}` para encerrar o período.
- `POST /resource-dependencies`, `PUT /resource-dependencies/{externalId}` para encerrar o período.
- `POST /deployment-resources`, `PUT /deployment-resources/{externalId}` para encerrar o período.
- `GET /inventory-links`: um recurso ou implantação como âncora, tipo de vínculo, inclusão de encerrados e paginação.

Payloads públicos ficam no projeto Client. PK/FK internas continuam `int`, e a interface usa somente GUIDs públicos. Escritas e consultas simples usam `IRepository<T>`; os repositórios específicos acrescentam paginação/projeções, cálculo dos beneficiários e coordenação da topologia ausentes no contrato genérico. Não há consultas a identidades externas durante autorização.

Coleções HTTP têm paginação de até 100 itens, com padrão 20. Clientes e origens na ficha são prévias de até 100 com total explícito. A análise atual é limitada a 5.000 recursos, 20.000 relações por conjunto e 100.000 pares de alcance. Ao exceder o limite, retorna erro explícito, sem apresentar totais parciais. Limites de expansão também restringem a fila de travessia. Ampliar esse porte exige evoluir a estratégia de consulta.

## Validação

- Build Release da API: sem erros nem warnings.
- 239 testes da API: domínio, CQS, validators, controllers, autorização HTTP, conflitos, históricos e tradução SQL/modelo PostgreSQL e SQL Server.
- Frontend: build, lint e 33 testes unitários, incluindo filtros vazios e contratos HTTP.
- 31 testes Playwright dos fluxos administrativos: 9 de inventário, além de serviços, clientes, leads e dashboard. Desktop/mobile, períodos, reservas, conflitos, origens compartilhadas, proteção de saída e acesso root. Screenshots locais revisadas; não há rolagem horizontal nos cenários verificados.
- Factory de design time e `has-pending-model-changes`: modelo PostgreSQL sem diferenças em relação à `Issue020`. `dotnet pack` valida os contratos públicos sem publicação.

Os comandos reproduzíveis ficam nos READMEs da [API](../../../ms-olivaw-api/README.md) e do [frontend](../../../ms-olivaw-ng/README.md). Testes usam HTTP/repositórios simulados e tradução SQL sem banco real. Build do frontend mantém quatro avisos preexistentes de orçamento de estilos (três páginas públicas e lista de leads); não há aviso novo de inventário. Não foram geradas migrations SQL Server/Oracle nem testado Oracle. Não houve deploy ou teste em banco/infraestrutura remota.

## Próximas evoluções

A 04c mantém propriedades específicas por tipo, referências detalhadas de backup/renovação e refinamentos de alocação. Coleta e saúde dependem das fontes das features 05–07. Despesas, preços, rateio e reconciliação de custos ficam na feature 17. O inventário não escolhe percentuais, altera plano, gera cobrança nem comprova política de backup executada.
```
