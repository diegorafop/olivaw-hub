# 03 — Cadastro e ficha de clientes

## user-history

Como operador da Olivaw,
quero cadastrar empresas e seus contatos,
para manter uma base confiável dos clientes atendidos.

## acceptance-criteria

- Operador autorizado cria, consulta e atualiza uma empresa e seus contatos; consultas e escritas verificam permissão no servidor.
- Falhas de validação e possíveis duplicidades são compreensíveis; filtros e paginação não perdem o contexto.
- LTMJ pode ser cadastrada com informações confirmadas, sem inventar plano ou recursos.
- Alterar a situação cadastral não suspende sistemas nem muda cobrança.

## ai-prompt

Concluída no desenvolvimento; preserve pendências de publicação e ambiente.

Trabalhe somente no recorte autorizado desta feature. Confira os AGENTS.md aplicáveis, a implementação atual e o registro completo em `user-feedback`. Preserve os limites e as evidências históricas; registre testes e pendências. Migrations, finish, push e publicação dependem de autorização explícita.

### Estado e objetivo

Interface, contratos, domínio e API implementados localmente para avaliação. Migration PostgreSQL `Issue013Customers` gerada e revisada; publicação e teste real no ambiente aguardam o mantenedor. Parte da [épica de administração e portal](../discovery/painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Representar as empresas atendidas pela Olivaw e seus responsáveis, estabelecendo a base de vínculo para os demais módulos.

### Dependências

[Dashboard](01-dashboard-administrativo.md). A [gestão de leads](02-leads-administrativos.md) só é dependência para a conversão explícita de um lead; o cadastro manual pode ser entregue antes dela.

### Escopo da entrega

- Cadastro progressivo: nome comercial, dados cadastrais necessários, contatos, responsável interno e observações.
- Definir situação em implantação, ativo, pausado ou encerrado, com transições explícitas.
- Listagem paginada, busca, filtros, ficha e trilha mínima das alterações.
- Quando leads estiverem disponíveis, permitir conversão explícita com tratamento de duplicidade e preservação da origem.
- Adicionar Clientes à navegação e conectar contagens confirmadas ao dashboard.
- Cliente é uma empresa, com múltiplos contatos e projetos futuros; não é usuário de login nem conta técnica.

### Fora desta entrega

- Criação automática de contas do portal, servidores, contratos, cobrança ou sistemas.
- Obrigatoriedade de preencher toda informação fiscal antes de registrar um cliente inicial.

### Decisões antes de implementar

Decisões do primeiro recorte:

- Nome comercial e pelo menos um contato com nome e e-mail ou WhatsApp válidos. Até oito contatos por empresa; quando ambos os canais forem informados, ambos precisam ser válidos. Razão social, CNPJ com 14 dígitos, responsável Olivaw e observações são opcionais.
- Nome comercial não é único: empresas diferentes podem ter o mesmo nome. CNPJ informado e origem de lead têm índices únicos. Conversão repetida de lead informa o cliente já vinculado; não gera nova empresa automaticamente.
- Cliente nasce em implantação. Pode avançar para ativo, pausado ou encerrado; ativo pode pausar ou encerrar; pausado pode retomar implantação ou atividade, ou encerrar. Encerrado reabre somente em implantação. A interface pede confirmação ao mudar a situação.
- Não há exclusão física de empresa nem criação automática de usuários, `UserProfile`, contas técnicas, credenciais ou convites. Por decisão do mantenedor, clientes ficam sem login nesta etapa; vínculos com pessoas autorizadas serão definidos na feature 11.
- Controle inicial exclusivo de root, também no servidor. Revisão concorrente evita sobrescrever alterações; a ficha mostra as vinte movimentações mais recentes, com operador, momento e situação anterior/nova. O histórico mínimo não é um diff completo dos valores anteriores.
- Conversão é uma preparação explícita do formulário; busca contexto e contatos do lead, exige revisão e só persiste ao cadastrar. Nenhum lead, LTMJ ou outro cliente é cadastrado como efeito dos testes.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
# 03 — Cadastro e ficha de clientes

## Estado e objetivo

Interface, contratos, domínio e API implementados localmente para avaliação. Migration PostgreSQL `Issue013Customers` gerada e revisada; publicação e teste real no ambiente aguardam o mantenedor. Parte da [épica de administração e portal](../discovery/painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Representar as empresas atendidas pela Olivaw e seus responsáveis, estabelecendo a base de vínculo para os demais módulos.

## Dependências

[Dashboard](01-dashboard-administrativo.md). A [gestão de leads](02-leads-administrativos.md) só é dependência para a conversão explícita de um lead; o cadastro manual pode ser entregue antes dela.

## Escopo da entrega

- Cadastro progressivo: nome comercial, dados cadastrais necessários, contatos, responsável interno e observações.
- Definir situação em implantação, ativo, pausado ou encerrado, com transições explícitas.
- Listagem paginada, busca, filtros, ficha e trilha mínima das alterações.
- Quando leads estiverem disponíveis, permitir conversão explícita com tratamento de duplicidade e preservação da origem.
- Adicionar Clientes à navegação e conectar contagens confirmadas ao dashboard.
- Cliente é uma empresa, com múltiplos contatos e projetos futuros; não é usuário de login nem conta técnica.

## Fora desta entrega

- Criação automática de contas do portal, servidores, contratos, cobrança ou sistemas.
- Obrigatoriedade de preencher toda informação fiscal antes de registrar um cliente inicial.

## Critérios de aceite

- Operador autorizado cria, consulta e atualiza uma empresa e seus contatos; consultas e escritas verificam permissão no servidor.
- Falhas de validação e possíveis duplicidades são compreensíveis; filtros e paginação não perdem o contexto.
- LTMJ pode ser cadastrada com informações confirmadas, sem inventar plano ou recursos.
- Alterar a situação cadastral não suspende sistemas nem muda cobrança.

## Decisões antes de implementar

Decisões do primeiro recorte:

- Nome comercial e pelo menos um contato com nome e e-mail ou WhatsApp válidos. Até oito contatos por empresa; quando ambos os canais forem informados, ambos precisam ser válidos. Razão social, CNPJ com 14 dígitos, responsável Olivaw e observações são opcionais.
- Nome comercial não é único: empresas diferentes podem ter o mesmo nome. CNPJ informado e origem de lead têm índices únicos. Conversão repetida de lead informa o cliente já vinculado; não gera nova empresa automaticamente.
- Cliente nasce em implantação. Pode avançar para ativo, pausado ou encerrado; ativo pode pausar ou encerrar; pausado pode retomar implantação ou atividade, ou encerrar. Encerrado reabre somente em implantação. A interface pede confirmação ao mudar a situação.
- Não há exclusão física de empresa nem criação automática de usuários, `UserProfile`, contas técnicas, credenciais ou convites. Por decisão do mantenedor, clientes ficam sem login nesta etapa; vínculos com pessoas autorizadas serão definidos na feature 11.
- Controle inicial exclusivo de root, também no servidor. Revisão concorrente evita sobrescrever alterações; a ficha mostra as vinte movimentações mais recentes, com operador, momento e situação anterior/nova. O histórico mínimo não é um diff completo dos valores anteriores.
- Conversão é uma preparação explícita do formulário; busca contexto e contatos do lead, exige revisão e só persiste ao cadastrar. Nenhum lead, LTMJ ou outro cliente é cadastrado como efeito dos testes.

## Implementação e verificação

Rotas `/admin/customers`, `/admin/customers/new` e `/admin/customers/:id`; endpoints e requisitos de publicação no [README da API](../../../ms-olivaw-api/README.md#cadastro-administrativo-de-clientes). O dashboard consulta contagens reais por situação, com estado de erro se a fonte não estiver disponível. Não confundir situação cadastral com saúde técnica.

Criação, atualização e consulta por ID injetam `IRepository<Customer>` diretamente. `IRepositoryCustomer : IRepository<Customer>` e `RepositoryCustomer : Repository<Customer>` existem somente para paginação com contagem e agregação por situação no banco, ausentes no contrato genérico; usam LINQ/EF, sem SQL bruto. Contatos e histórico usam entidades relacionais com chaves internas e vínculos explícitos.

Testes HTTP em memória verificam autorização, validação, duplicidade de origem, revisão concorrente e auditoria. Testes do modelo verificam PostgreSQL e SQL Server sem conexão. Playwright cobre cadastro e contatos em desktop/celular, filtros, recuperação de erro e preparação de conversão. Não houve teste contra banco real. Após autorização explícita do mantenedor, foi gerada somente a migration PostgreSQL `Issue013Customers`, revisando SQL, designer e snapshot e confirmando ausência de mudanças pendentes no modelo. Não foi aplicada em banco; publicação e teste na AWS ficam com o mantenedor. SQL Server e Oracle não receberam migrations.
```
