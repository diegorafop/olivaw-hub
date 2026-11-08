# 04a — Cadastro de serviços e vínculo com clientes

## user-history

Como operador da Olivaw,
quero cadastrar serviços e seus vínculos com clientes,
para identificar quais aplicações atendem cada empresa.

## acceptance-criteria

- Cadastro de aplicação lógica, com nome, identificador técnico único e imutável, tipo, descrição, responsável interno textual opcional, escopo e situação.
- Tipos iniciais: frontend, API, worker, tarefa agendada e outro. Banco, servidor e domínio continuam como recursos da próxima entrega.
- Situações Planejado, Em implantação, Ativo e Encerrado, com transições livres. A situação é cadastral e não comprova saúde.
- Escopo Interno Olivaw, sem empresa fictícia, ou Atendimento a clientes. Até 100 clientes por serviço; um cliente pode ter vários serviços.
- Vínculos usando PK/FK internas e GUIDs públicos, sem copiar empresas ou duplicar o serviço por cliente/ambiente.
- Encerrar serviço, tornar interno ou retirar cliente encerra os vínculos afetados e conserva período, autor da abertura e autor do encerramento. Reativar pode criar novo período, preservando os anteriores.
- Revisão para concorrência; conflito retorna HTTP 409. Identificador duplicado é protegido também por índice único.
- Lista paginada com busca por nome/identificador e filtros por tipo, situação e cliente. Filtro de cliente é acessível a partir de sua ficha.
- Menu Serviços, formulário de cadastro/edição e seção Serviços na ficha do cliente, com ações para vincular existente ou preparar novo cadastro. Nenhuma preparação persiste antes de salvar.
- Histórico do cadastro com snapshot tipado dos dados alterados, autor e data. Ficha mostra vínculos ativos e os 20 encerramentos e 20 alterações mais recentes; histórico completo permanece no banco.

## ai-prompt

Concluída no desenvolvimento; preserve pendências de publicação e ambiente.

Trabalhe somente no recorte autorizado desta feature. Confira os AGENTS.md aplicáveis, a implementação atual e o registro completo em `user-feedback`. Preserve os limites e as evidências históricas; registre testes e pendências. Migrations, finish, push e publicação dependem de autorização explícita.

### Estado e escopo

Primeiro recorte da [feature 04](../discovery/painel-olivaw/04-inventario-de-recursos.md), implementado e validado localmente na branch `feature/issue018-resources-flow` em 22/10/2026. A base é comum às opções de escopo apresentadas ao mantenedor: somente serviços ou serviços com a base de recursos/ambientes.

**Migration PostgreSQL gerada na correção da issue 020.** A `Issue020` reúne o modelo de serviços e a base de inventário já mapeada, corrigindo a falha de startup por modelo pendente. Comandos e validações estão no [README da API](../../../ms-olivaw-api/README.md#correção-de-startup--issue020). Nenhum banco foi alterado durante a validação local; após publicar a imagem atualizada, o startup aplica automaticamente a migration. A interface e os casos de uso da [04b](04b-inventario-de-recursos.md) foram concluídos e validados localmente.

### Entrega implementada

- Cadastro de aplicação lógica, com nome, identificador técnico único e imutável, tipo, descrição, responsável interno textual opcional, escopo e situação.
- Tipos iniciais: frontend, API, worker, tarefa agendada e outro. Banco, servidor e domínio continuam como recursos da próxima entrega.
- Situações Planejado, Em implantação, Ativo e Encerrado, com transições livres. A situação é cadastral e não comprova saúde.
- Escopo Interno Olivaw, sem empresa fictícia, ou Atendimento a clientes. Até 100 clientes por serviço; um cliente pode ter vários serviços.
- Vínculos usando PK/FK internas e GUIDs públicos, sem copiar empresas ou duplicar o serviço por cliente/ambiente.
- Encerrar serviço, tornar interno ou retirar cliente encerra os vínculos afetados e conserva período, autor da abertura e autor do encerramento. Reativar pode criar novo período, preservando os anteriores.
- Revisão para concorrência; conflito retorna HTTP 409. Identificador duplicado é protegido também por índice único.
- Lista paginada com busca por nome/identificador e filtros por tipo, situação e cliente. Filtro de cliente é acessível a partir de sua ficha.
- Menu Serviços, formulário de cadastro/edição e seção Serviços na ficha do cliente, com ações para vincular existente ou preparar novo cadastro. Nenhuma preparação persiste antes de salvar.
- Histórico do cadastro com snapshot tipado dos dados alterados, autor e data. Ficha mostra vínculos ativos e os 20 encerramentos e 20 alterações mais recentes; histórico completo permanece no banco.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
# 04a — Cadastro de serviços e vínculo com clientes

## Estado e escopo

Primeiro recorte da [feature 04](../discovery/painel-olivaw/04-inventario-de-recursos.md), implementado e validado localmente na branch `feature/issue018-resources-flow` em 22/10/2026. A base é comum às opções de escopo apresentadas ao mantenedor: somente serviços ou serviços com a base de recursos/ambientes.

**Migration PostgreSQL gerada na correção da issue 020.** A `Issue020` reúne o modelo de serviços e a base de inventário já mapeada, corrigindo a falha de startup por modelo pendente. Comandos e validações estão no [README da API](../../../ms-olivaw-api/README.md#correção-de-startup--issue020). Nenhum banco foi alterado durante a validação local; após publicar a imagem atualizada, o startup aplica automaticamente a migration. A interface e os casos de uso da [04b](04b-inventario-de-recursos.md) foram concluídos e validados localmente.

## Entrega implementada

- Cadastro de aplicação lógica, com nome, identificador técnico único e imutável, tipo, descrição, responsável interno textual opcional, escopo e situação.
- Tipos iniciais: frontend, API, worker, tarefa agendada e outro. Banco, servidor e domínio continuam como recursos da próxima entrega.
- Situações Planejado, Em implantação, Ativo e Encerrado, com transições livres. A situação é cadastral e não comprova saúde.
- Escopo Interno Olivaw, sem empresa fictícia, ou Atendimento a clientes. Até 100 clientes por serviço; um cliente pode ter vários serviços.
- Vínculos usando PK/FK internas e GUIDs públicos, sem copiar empresas ou duplicar o serviço por cliente/ambiente.
- Encerrar serviço, tornar interno ou retirar cliente encerra os vínculos afetados e conserva período, autor da abertura e autor do encerramento. Reativar pode criar novo período, preservando os anteriores.
- Revisão para concorrência; conflito retorna HTTP 409. Identificador duplicado é protegido também por índice único.
- Lista paginada com busca por nome/identificador e filtros por tipo, situação e cliente. Filtro de cliente é acessível a partir de sua ficha.
- Menu Serviços, formulário de cadastro/edição e seção Serviços na ficha do cliente, com ações para vincular existente ou preparar novo cadastro. Nenhuma preparação persiste antes de salvar.
- Histórico do cadastro com snapshot tipado dos dados alterados, autor e data. Ficha mostra vínculos ativos e os 20 encerramentos e 20 alterações mais recentes; histórico completo permanece no banco.

## Regras adotadas nesta base

Identificador técnico aceita até 80 caracteres com letras minúsculas, números e hífens, como `ms-exemplo-api`. Nome aceita até 120, descrição até 1.000 e responsável até 120 caracteres. Servidor valida vínculos duplicados e clientes inexistentes/removidos. Serviço de clientes não encerrado exige ao menos um vínculo; interno ou encerrado recebe lista vazia.

Acesso inicial exclusivo do root interno, preservando a política de Clientes. Não cria access profiles, identidades externas, usuários técnicos ou vínculos de portal. Ativação/encerramento não dispara deploy, provisionamento, suspensão, cobrança ou mudança de plano.

Alterações de dados e vínculos usam a mesma unidade transacional do contexto EF. A abertura/encerramento dos vínculos não é apagada em substituições; o cadastro mantém identidade técnica estável. Leituras simples usam `IRepository<T>`; `IRepositoryManagedService` existe somente para paginação, contagem e projeção no banco.

## Validação local

- 158 testes da API: domínio, commands/validators, queries, controller, HTTP em memória e modelo/tradução SQL PostgreSQL e SQL Server, sem conexão a banco real.
- 14 testes de navegador: serviços e regressões de clientes/dashboard, incluindo desktop/celular, compartilhamento, encerramento de vínculos, serviço interno e concorrência.
- 28 testes unitários existentes do frontend, lint e builds; avisos preexistentes de estilos das páginas públicas permanecem registrados no README.
- Capturas de desktop/celular conferidas. Banco, nuvem, dados de clientes e infraestrutura não foram acessados; não houve seed da LTMJ.

Comandos e contratos no [README da API](../../../ms-olivaw-api/README.md#serviços-e-vínculos-com-clientes) e no [README do frontend](../../../ms-olivaw-ng/README.md#serviços-administrativos).

## Continuação da feature 04

Implantações por ambiente, recursos físicos/lógicos compartilhados, relações e caminhos de dependência estão implementados na [04b](04b-inventario-de-recursos.md). O detalhamento por tipo permanece no [planejamento da 04c](../discovery/painel-olivaw/04-inventario-de-recursos.md). Medição e financeiro continuam nas features 07 e 17. A conclusão deste primeiro recorte não conclui toda a issue 018 nem toda a feature 04.
```
