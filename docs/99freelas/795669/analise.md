# Análise técnica — Club do Real

## Leitura do anexo

O PDF foi lido integralmente. Especifica cinco calculadoras (economia; juros/parcelamentos; comparação de compras; despesas domésticas; custos/precificação), catálogo de 30 oportunidades, desafio com progresso automático e administração. Pede duas propostas, entrega integral do código, infraestrutura/repositório do contratante e garantia de 90 dias. Tecnologia React/Next, Node, PostgreSQL e Supabase é interesse, não imposição. ASP.NET Core/Angular deve ser justificado e aprovado, não apresentado como escolha já aceita pelo cliente.

## Arquitetura proposta

Aplicação web responsiva em Angular, API ASP.NET Core, PostgreSQL, serviço com identidade e dados próprios, aproveitando padrões Olivaw. Não prometer que o template já tem cadastro público completo, diagnóstico ou Kiwify. Reaproveitar componentes apenas com licença e direito de entrega identificados. Não criar dependência de pacotes privados inacessíveis ao cliente; o projeto entregue deve compilar e operar nas contas dele. Identificar bibliotecas preexistentes e licenças; entregar código necessário ao build sem exigir transferência indevida da propriedade intelectual de terceiros.

Painel por permissões, dados pessoais isolados por usuário, histórico de respostas e regras versionadas. Valores monetários com precisão decimal, arredondamento consistente e períodos definidos. Cálculos no servidor para resultado persistido e testes contra exemplos aprovados. “Escalável” significa projeto com caminho de evolução e teste de carga acordado, não capacidade ilimitada ou microserviços obrigatórios.

## Escopo A e B

A inclui identidade e recuperação, entradas manuais de renda/despesa, saldo e comprometimento, histórico básico, simulador de economia mensal/anual, detector simples por regras, guia editável, um desafio de 30 dias, progresso, administração básica e acesso comercial por um produto Kiwify. É recorte proposto, precisa de aceite. Não inclui as outras quatro calculadoras, catálogo de renda, metas completas ou indicadores avançados.

B inclui A e todos os módulos funcionais do PDF no recorte detalhado em estimativa.md. Administração de ferramentas corresponde a conteúdo e parâmetros previamente definidos; não é construtor genérico de fórmulas. B inclui indicadores básicos de uso, sem plataforma de analytics externa ou rastreamento irrestrito. Estrutura preparada para novos planos não inclui lançar múltiplos planos/assinaturas agora. Confirmar primeiro modelo: pagamento único com duração de acesso definida ou uma assinatura inicial. Variações futuras exigem escopo próprio.

## Fórmulas, conteúdo e qualidade

Quem fornece textos do guia, 30 oportunidades, desafios, fórmulas e critérios de desperdício? A estimativa presume conteúdos fornecidos/aprovados pelo cliente. Não inclui produção editorial completa nem consultoria financeira individual. “Despesa desnecessária” depende de critérios e decisão do usuário; não classificar automaticamente gasto como desperdício sem explicar a regra.

Economia potencial não é economia realizada. Usuário deve registrar ou confirmar a realização e a base de comparação; sem Open Finance não há confirmação bancária automática. Separar juros simples/compostos, taxa nominal/efetiva, periodicidade, entrada e total de parcelas; não assumir que simulador equivale a CET de contrato real. Fórmulas de precificação devem especificar custos fixos/variáveis, margem versus markup e tributos considerados. Aprová-las com o responsável editorial antes de fechar os testes. Nenhuma fórmula financeira foi implementada ou validada nesta análise.

## Kiwify e direitos de acesso

Consumir eventos documentados com validação de autenticidade e idempotência; não confiar em redirecionamento do checkout como prova de pagamento. Manter vínculo por compra/produto e identificador de transação, sem conceder privilégios por e-mail livre. Tratar duplicidade, reenvio e ordem de eventos; reembolsos/contestações não podem ser desfeitos por aprovação antiga. Reconciliação e intervenção administrativa auditável.

Confirmar política de cancelamento e acesso até o fim do período; evitar bloquear indiscriminadamente assinatura cancelada ainda válida. Conta administrativa da Kiwify e segredos ficam no servidor, não no Angular. Usar mocks/fixtures e eventos fictícios para validação. A documentação informa ausência de sandbox; testes financeiros reais precisam de autorização específica e não fazem parte desta análise. Proposta não inclui saques ou reembolsos disparados pela aplicação.

## Propriedade, entrega e operação

Repositório, domínio, AWS e banco em contas do contratante desde o início, acesso delegado à Olivaw. Instruções de build/deploy, inventário de dependências/licenças, procedimentos de backup/restauração, acesso ao banco e transferência segura de credenciais. Sem senha em documento ou chat. Código-fonte integral do projeto e uso permitido das dependências devem ser contratualmente claros. Administração de infraestrutura por mensalidade é opcional e não muda a propriedade do cliente.

Garantia de 90 dias para erros de desenvolvimento conforme contrato; distinguir da operação contínua e da evolução. Sem mensalidade obrigatória para corrigir defeitos cobertos. Confidencialidade, dados de testes e logs mínimos; dados financeiros privados não aparecem em relatórios de outros usuários ou em analytics públicos.

## Alternativas e componentes

Reaproveitamento preferido: templates e componentes Olivaw para identidade/CRUD e infraestrutura; domínio de diagnóstico e calculadoras será programado. CMS/membership genéricos podem reduzir área de conteúdo, mas não comprovam cobertura de cálculos, progresso e Kiwify; não selecionados. Sem necessidade de IA, plugin de IA ou pacote financeiro pago presumidos. Licenças dos componentes realmente escolhidos serão verificadas no levantamento; não propor produto externo sem aderência demonstrada.

## Perguntas para fechar a proposta

- Qual o produto inicial, duração de acesso e modelo comercial Kiwify? Já existe conta/produto?
- Há identidade visual, telas e briefing técnico adicional? O PDF é funcional, não detalha todos os modelos e fórmulas.
- Quem fornece os conteúdos, fórmulas e exemplos aceitos? Como se define economia realizada?
- Quais campos/categorias do diagnóstico e regras para o detector? Dados sempre manuais?
- Quais visões e filtros nos indicadores e relatórios administrativos?
- O desafio reinicia, libera uma tarefa por dia ou fica todo disponível? Qual fuso/data de início?
- Quantos usuários simultâneos e qual volume inicial? Precisa de anexos, exportação ou vídeos?
- Aceita .NET/Angular? Contas de repositório/cloud e domínio estão prontas?
- Como ocorrerão os aceites por etapa e quais regras da garantia e suporte?

## Validações previstas

Isolamento entre usuários, recuperação de senha, bloqueio por direito de acesso e administração por perfil. Webhooks inválidos/duplicados/atrasados, reembolso e cancelamento, reativação e falha de envio de e-mail. Fórmulas com zeros, negativos, taxas/períodos, arredondamento e entrada incompleta. Persistência de progresso em dois dispositivos, conteúdo inativado, edição de desafio e histórico. Responsividade/acessibilidade, reconciliação de dados, build independente e restauração de backup em homologação. Nenhum teste executado contra conta Kiwify ou dados reais.

## Fontes consultadas em 03/11/2026

- [Anúncio](https://www.99freelas.com.br/project/desenvolvimento-full-stack-de-plataforma-digital-do-zero-795669): descrição pública conferida.
- [PDF recebido](anexos/briefing-club-do-real.pdf) e [texto extraído](anexos/briefing-extraido.txt): requisitos adicionais e propriedade.
- [Kiwify — informações gerais](https://docs.kiwify.com.br/api-reference/general): API, autenticação e ausência de sandbox.
- [Kiwify — receber e validar webhooks](https://docs.kiwify.com.br/api-reference/webhooks/receiving): autenticidade, duplicidade e reenvio.
- [Kiwify — eventos de assinatura](https://docs.kiwify.com.br/api-reference/webhooks/subscription-events): diferenciar cancelamento de direitos ainda válidos.
- [AWS Pricing Calculator](https://calculator.aws/): ferramenta para dimensionamento posterior; não foi produzida cotação AWS nesta análise.
