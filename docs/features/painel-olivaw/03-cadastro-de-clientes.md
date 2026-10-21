# 03 — Cadastro e ficha de clientes

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

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

- Campos mínimos, unicidade, transições, permissões e tratamento de dados de contato.
- Necessidade de razão social/documento nesta primeira versão e regra de desativação sem perder histórico.
