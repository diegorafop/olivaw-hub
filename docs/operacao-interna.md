# Operação interna — Painel e portal

## Acesso

Entre como root e abra **Configuração** na navegação principal. O hub reúne os cadastros e ferramentas em cards. Leads e Visão geral continuam na navegação principal; os módulos mantêm autorização própria no servidor.

O breadcrumb acompanha os níveis da rota, com os ancestrais clicáveis e o item atual identificado. Em uma ficha de recurso, o caminho é Visão geral → Configuração → Recursos → Ficha do recurso.

As [entregas concluídas](features/index.md) registram os incrementos efetivamente implementados. A visão ampla das features 05 a 10 continua no planejamento; automações e integrações ainda não implementadas não foram marcadas como prontas.

## Estudos dos planos

1. Cadastre uma origem de custo mensal e sua fonte. Deixe o preço vazio quando desconhecido. Registre outra versão quando o preço mudar.
2. Selecione uma única versão de cada origem. Informe explicitamente a parcela usada no cenário; o sistema não presume divisão igual nem consumo medido.
3. Dê um nome ao estudo, descreva as funcionalidades e preencha os limites comerciais desejados. Bronze com 2 GB é um exemplo a avaliar, sem oferta ou quota aprovada.
4. Informe suporte, custo por hora, reserva e margem sobre o preço. Valores iniciais zero devem ser revisados. Custos anuais precisam ser convertidos para equivalente mensal, com a fonte preservada; a interface usa BRL.
5. Calcule e salve o cenário. Use um cenário como base para comparar outra combinação, preservando a versão anterior e seus preços.

O cálculo é `(custo atribuído + horas × custo/hora) × (1 + reserva) ÷ (1 − margem)`. Margem sobre preço difere de acrescentar o percentual ao custo. Tributos e despesas ausentes precisam ser incluídos explicitamente. Custo desconhecido não vira zero; uma mensalidade final não é sugerida enquanto houver preço pendente.

Armazenamento do estudo usa GB decimal; leituras do inventário usam GiB binário. Memória opcional do estudo usa MiB. Um limite por plano não comprova capacidade para todos os clientes. A VM com 1 GB de RAM e pouco disco foi informada pelo mantenedor; não houve cadastro automático, medição de espaço livre, cálculo de preços AWS ou acesso à infraestrutura real.

O estudo não cria oferta, não altera condições contratadas nem efetua cobrança. A API de condições por cliente exige referência explícita do aceite e preserva versões por vigência; sua interface específica permanece pendente.

## Saúde, consumo e cuidados

- Saúde registra verificações pontuais, com fonte, horário e validade. Sem amostras recentes, o estado permanece sem confirmação ou desatualizado. Um HTTP 200 não comprova todos os fluxos de negócio.
- HTTP pontual depende da referência HTTPS do recurso e de uma URL exata em `Monitoring:AllowedUrls`. Sem lista configurada, nenhuma coleta é permitida. Não há agendamento periódico ativo.
- O transporte HTTP bloqueia redirecionamentos, cookies, proxy e endereços privados/metadata; conecta somente a IPv4 público resolvido naquele momento, com TLS validado para o destino original. Timeout total de 5 segundos, conexão de 3 segundos e limite de cabeçalhos de 16 KiB.
- Consumo preserva valor pontual em GiB, instante, origem e qualidade manual/medida/estimada. Selecionar “medida na fonte” registra a informação recebida, sem executar coleta automática. Amostras não são somadas como consumo acumulado nem rateadas por cliente.
- Cuidados registra separadamente publicação, backup e teste de restauração. Evidência manual não equivale a consulta de jobs DevOps, monitoramento funcional ou garantia de restauração.
- Chaves de amostra tornam reenvios idempotentes. Consultas são limitadas e paginadas; retenção automática ainda exige política própria.

## Incidentes e atendimento

Incidentes têm recurso de origem único enquanto abertos, gravidade, impacto, responsável, próxima ação e revisões. Resolver exige observação e justificativa. O fluxo não envia notificações nem executa recuperação. Correlação automática, manutenção e silenciamento com expiração continuam no planejamento.

Atendimento registra pedido por cliente com solicitante, tipo, responsável, próxima ação e revisões. Encerrar ou cancelar exige justificativa. Reabertura é uma mudança auditada. Notas e histórico de alterações continuam internos. O portal permite pedidos vinculados a serviço e mensagens públicas; respostas privadas são identificadas separadamente no atendimento. Tarefas compartilhadas permanecem no planejamento.

Atualizações concorrentes retornam conflito e preservam o texto no formulário. Nenhum pedido aprova prazo, escopo, franquia ou investimento automaticamente.

## Schema e integração

**Base PostgreSQL liberada para publicação do código validado.** Em 24/10/2026 foram autorizadas, geradas e revisadas `Issue020Panel` (18 tabelas e detalhes de inventário) e `Issue020PortalIdentity` (cinco tabelas da autoridade do portal). Os dois contextos estão sem diferenças pendentes. Migrations antigas foram preservadas; o avanço é aditivo. O startup aplicará as duas migrations antes de atender requisições. Não houve execução de SQL ou validação contra banco real.

PostgreSQL e SQL Server têm mappings e consultas verificados em memória, sem conexão a banco. Oracle não foi contemplado. Migrations geradas somente para PostgreSQL. Não houve execução de SQL, finish, push ou implantação. SQL Server e Oracle não receberam migrations.

O trabalho solicitado em lote permanece sem commits na branch recebida `feature/issue020-inventario-recursos`. Antes de integrar, separar os escopos em branches por issue, sem associar automaticamente todas as features à issue 020. As alterações preexistentes de readiness e documentação foram preservadas.

## Validação reproduzível

```bash
dotnet build ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj --no-restore -m:1
dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj --no-build --no-restore -m:1
npm --prefix ms-olivaw-ng run build
npm --prefix ms-olivaw-ng run lint
npm --prefix ms-olivaw-ng run test
npm --prefix ms-olivaw-ng run e2e -- e2e/admin-configuration.spec.ts e2e/admin-inventory.spec.ts e2e/admin-customers.spec.ts e2e/admin-services.spec.ts e2e/portal.spec.ts
```

Build Release da API: zero erros e avisos. Suíte da API: 359 testes aprovados, incluindo correspondência dos modelos de negócio e portal com suas migrations. Build Angular e lint concluídos; avisos preexistentes de orçamento de estilos permanecem. Testes unitários Angular: 33 aprovados. E2E do fechamento: 31 aprovados (29 administrativos e dois do portal, desktop/celular). Os testes não utilizam bancos reais, URLs reais de clientes ou credenciais de produção. Imagem Docker e smoke test do ambiente não foram executados neste fechamento.

## Portal Meu negócio

- Em Configuração → Acessos do portal, selecione a empresa, crie o convite e entregue o link ao destinatário. Convites expiram em 48 horas; o token é exibido uma vez e não fica armazenado em texto aberto. Cadastro de cliente sozinho não cria login nem vínculo.
- Papéis Consulta, Solicitações e Gestão permitem, respectivamente, consulta, abertura de pedidos e acesso às condições financeiras publicáveis. Todo acesso é limitado aos vínculos ativos no servidor; o papel Gestão não concede acesso à operação interna.
- Publique explicitamente os serviços e rótulos que devem aparecer no portal. URLs, topologia e notas internas do inventário não são publicadas automaticamente.
- A prévia administrativa é somente consulta. Ela não gera token de cliente nem habilita envio de pedidos ou respostas.
- Pedidos e mensagens originais são preservados. Reenvio usa chave própria para evitar duplicação; a equipe escolhe se sua resposta é privada ou pública. Reabrir pedido não aprova nova execução.
- Entregas começam em rascunho e só aparecem ao cliente após publicação explícita, data passada e referência de confirmação. Notas podem ser corrigidas com histórico ou retiradas com motivo. Publicação de nota não executa deploy.
- Meu plano mostra somente condições vigentes cadastradas com referência de aceite. Propostas, aceite formal no portal, financeiro e IA ainda não foram concluídos. Pedir mais espaço ou outro plano não altera o contratado.

O login externo requer a configuração independente de `PortalIdentity` descrita no [README da API](../ms-olivaw-api/README.md#pacote-do-painel-e-portal--issue020panel). Sem configuração válida, retorna 503. As tabelas são preparadas no startup mesmo com o login desabilitado. O portal reutiliza o host existente, sem outro processo permanente na VM.
