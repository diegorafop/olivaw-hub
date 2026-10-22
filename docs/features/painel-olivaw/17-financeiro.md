# 17 — Financeiro da Olivaw

## Estado e objetivo

Planejada; sem implementação, integração financeira ou definição tributária nesta etapa. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). Registrada em 21/10/2026 a pedido do mantenedor.

Conhecer quanto custa manter a Olivaw, quanto cada cliente contribui para a operação e quanto dinheiro está disponível. Reunir gastos em nuvem, impostos, cálculo de custos, contas a pagar e receber, fluxo de caixa e resultado. O cadastro de clientes continua sendo a entrega atual; financeiro terá implementação própria.

## Dependências e limites

- [Cadastro de clientes](../done/03-cadastro-de-clientes.md) para vincular receitas, custos e cobranças à empresa correta.
- [Inventário](04-inventario-de-recursos.md) e [consumo](07-consumo-e-capacidade.md) para atribuição técnica de custos, quando houver medição confiável.
- [Planos e condições](08-planos-e-condicoes.md) e [definição de custos e planos](../custos-e-planos.md) para valores e condições aprovados.
- [Identidade do portal](11-identidade-do-portal.md) para futuras consultas do cliente. Cadastro de empresa não cria `UserProfile`, login ou convite automaticamente.
- Regras tributárias, regime, alíquotas, base de cálculo e obrigações serão definidos com o contador. Nenhuma taxa será presumida pelo sistema.

Separar orçamento, estimativa, obrigação, pagamento e custo medido. Consumo técnico não gera cobrança variável ou alteração contratual automaticamente.

## Entregas menores

### 17.1 — Custos da operação

- Registrar gastos com nuvem, servidores, armazenamento, tráfego, backups, domínios, certificados pagos, licenças, ferramentas, APIs e agentes de IA.
- Distinguir custos fixos e variáveis, recorrentes e pontuais, diretos e compartilhados, com fornecedor, competência, vencimento, moeda e centro de custo.
- Começar com registros manuais rastreáveis. Importação de faturas e integração com provedores serão avaliadas separadamente.
- Conservar valor e moeda originais; registrar data, fonte e taxa quando houver conversão para reais.
- Atribuir custos diretos ao cliente/projeto e aplicar rateios versionados aos compartilhados. Mostrar parcela não atribuída e evitar contar a mesma despesa duas vezes.
- Comparar estimativa com gasto efetivamente confirmado. Uma fatura da AWS não é uma medição individual de cada cliente.

Reutilizar a [modelagem de recursos e consumidores da feature 04](04-inventario-de-recursos.md#preparação-para-projetar-custos): um recurso pode atender vários clientes e ser alcançado por vários serviços, sem duplicar sua despesa. Cada item de custo tem origem, moeda, período e vigência. VM, disco, tráfego e IP são itens separados apenas quando o contrato/fatura assim os identificar; componentes já incluídos em um pacote não são somados novamente.

Banco lógico, namespace, subdomínio ou container podem servir à atribuição sem constituir uma cobrança adicional. Renovação anual de domínio pode ter referência mensal para projeção, preservando o desembolso real no fluxo de caixa. Rateio por capacidade reservada, medição, participação fixa ou outro critério depende de política aprovada e versionada; não assumir divisão igual ou percentuais pela simples existência do vínculo. Atribuições preservam histórico e deixam a parcela de plataforma/ociosidade/não atribuída visível.

### 17.2 — Contas a pagar e receber

- Registrar fornecedores, despesas operacionais, mensalidades, criação de projetos e outras receitas aprovadas.
- Controlar emissão/registro, vencimento, recebimento ou pagamento, descontos, juros aprovados, pagamentos parciais, cancelamentos e estornos.
- Vincular cobranças a cliente, contrato/plano e período, preservando as condições históricas.
- Tratar recorrência sem gerar duplicatas ao reprocessar um período. Alterações de plano não reescrevem cobranças anteriores.
- Registrar quem confirmou uma baixa, quando e com qual referência. Cobrança emitida não significa dinheiro recebido.
- Integração bancária, gateway, conciliação automática e emissão fiscal exigem escopo e autorização próprios; não são pré-requisitos do registro manual inicial.

### 17.3 — Fluxo de caixa e resultado

- Mostrar saldo inicial, entradas, saídas e saldo final por conta e período, separando realizado e previsto.
- Tratar transferências entre contas sem duplicar receitas ou despesas.
- Destacar compromissos próximos, valores em atraso e recorrências futuras, com fonte e atualização dos dados.
- Separar visão de caixa, baseada na liquidação, da visão de resultado por competência.
- Avaliar receita recorrente, despesas, margem de contribuição e resultado por cliente/projeto quando o rateio permitir; mostrar explicitamente lacunas e custos ainda não atribuídos.
- Apresentar tabelas de lançamentos e gráficos úteis de evolução, previsto × realizado e composição de custos. Ausência de dados não deve aparecer como saldo zero confirmado.

### 17.4 — Impostos, custos e precificação

- Registrar provisões, obrigações, vencimentos e pagamentos de impostos segundo regras aprovadas pelo contador, com vigência e histórico.
- Simular preço de implantação e mensalidade considerando infraestrutura, trabalho, suporte, melhorias incluídas, ferramentas, IA, taxas, impostos definidos e margem desejada.
- Distinguir custo de implantação de custo recorrente e separar capacidade contratada de consumo efetivo.
- Comparar custo e receita por plano e por cliente, preservando condições já contratadas.
- Tratar simulações como estimativas internas. Não publicar preços, modificar contratos, aumentar mensalidades ou emitir cobranças a partir de uma simulação.
- Avaliar crescimento e migração de plano com a equipe e o cliente, antes de qualquer alteração contratual.

## Consulta financeira no portal do cliente

Entrega futura vinculada a este módulo e ao [portal de plano e evolução](14-plano-e-evolucao-no-portal.md):

- Consultar somente as próprias cobranças, vencimentos, situação e histórico de pagamentos, inclusive pagamentos parciais e estornos quando aplicáveis.
- Mostrar descrição, período e condições contratadas em linguagem simples; distinguir pendente, vencido, parcialmente pago, pago e cancelado conforme os dados confirmados.
- Exibir comprovantes ou documentos apenas quando existirem fonte, armazenamento e autorização definidos. Uploads continuam fora da etapa atual.
- Não expor fornecedores, impostos internos, custo de infraestrutura, rateios, salários, dados bancários da operação ou margens da Olivaw.
- O servidor deve resolver o vínculo usuário–empresa e autorizar cada consulta e documento. Nunca confiar somente em um identificador de cliente enviado pelo navegador.
- Consultar pagamentos não autoriza processar pagamentos, alterar dados bancários, renegociar ou mudar plano automaticamente.

## Interface e permissões

A área interna terá entrada Financeiro quando a primeira entrega estiver utilizável, com resumo do período e áreas de Custos, Contas, Fluxo de caixa e Precificação. Seguir a identidade clara da Olivaw, com informação progressiva e dados rastreáveis, evitando um dashboard cheio de métricas sem origem.

Separar permissões de consulta financeira, manutenção de lançamentos, confirmação de pagamentos, acesso a custos/margens e simulação de preços. Auditoria deve registrar operações relevantes, correções e cancelamentos. Não apagar histórico de lançamentos liquidados; definir correções e estornos.

## Critérios de aceite

- Valores têm moeda, período, origem e estado; estimativas e confirmações são distinguíveis.
- Custos diretos, compartilhados e não atribuídos fecham com o total considerado, sem duplicação.
- Pagamentos parciais, cancelamentos e estornos repercutem corretamente no saldo; transferências não inflam o resultado.
- Contas previstas e realizadas não se somam duas vezes ao fluxo de caixa.
- Mudanças de regras preservam condições e apurações históricas, com rastreabilidade.
- O portal protege o isolamento entre empresas e não expõe custos ou margens internos.
- Nenhum preço, cobrança, imposto ou desconto é inventado; integrações são verificadas com dados de teste, sem movimentar dinheiro real.

## Decisões antes de implementar

- Contas financeiras, saldo inicial, categorias, centros de custo e responsáveis por conferir lançamentos.
- Primeiras fontes de dados e estratégia de conciliação, recorrência e prevenção de duplicidades.
- Critérios e vigência de rateio, moedas, arredondamento e tratamento de diferenças cambiais.
- Regime e regras tributárias confirmados pelo contador; distinção entre visão gerencial e escrituração contábil.
- Condições de cobrança, cancelamento, pagamento parcial, estorno e encerramento de contrato.
- Provedor de pagamentos/documentos, se necessário, sem tornar essa decisão obrigatória para o primeiro controle manual.
- Retenção, auditoria, exportação e permissões de cada operação.

Detalhar cada entrega em arquivo próprio antes de sua implementação. Este planejamento não autoriza migrations, acesso a contas bancárias, movimentações, cobranças, integrações de produção ou publicação.
