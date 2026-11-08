# Análise técnica — 795444

## Parecer

Boa aderência à experiência declarada do mantenedor em ERPs, workflows, arquitetura e DevOps. Não há evidência de trabalho prévio específico em concreto pré-fabricado; não afirmar esse diferencial. O anúncio descreve gestão de projetos e sua relação com produção, sem especificar um ERP/MES industrial completo. Ter sido criado com Claude não comprova arquitetura ruim nem justifica reescrita por si só.

Acesso somente após contrato é condição explícita do cliente. Oferecer diagnóstico independente com NDA, relatório de riscos e plano. Não exigir código ou dados sigilosos antes da contratação. Nenhum sistema foi inspecionado nesta análise.

## Diagnóstico e arquitetura

Levantar tecnologias, dependências/licenças, organização de código, deploy, banco, backups, restauração, identidade, permissões, logs e trilhas de auditoria. Mapear contrato → fases → executivo → fábrica, responsáveis, transições, revisão/retrabalho e dependências. Inspecionar em ambiente isolado e acordar operações permitidas; não iniciar a aplicação contra banco de produção.

Preferência do mantenedor: ASP.NET Core e Angular sobre a base Olivaw, serviço com identidade/banco próprios, autorização no backend e segredos separados. Primeiro comparar preservação/refatoração e substituição gradual. Não é necessário criar microserviço por módulo. Migração requer inventário, ensaio, reconciliação de contagens/custos/horas, janela acordada e retorno à versão anterior. O template reduz infraestrutura repetida, mas não contém o domínio do cliente pronto.

## IA e regras objetivas

- Atrasos: comparar prazo/status, calendários e dependências. Alertas configuráveis e rastreáveis; não depender de LLM para decidir se a data venceu.
- Gargalos: medir fila, tempo parado, carga e retrabalho; IA pode explicar achados, sem tratar hipótese como causa comprovada.
- Horas e custos: cálculos determinísticos e taxas históricas por colaborador/fase; mudança de taxa não pode alterar silenciosamente o passado. Separar horas previstas, apontadas, aprovadas e retrabalho.
- Resumos: gerar sobre indicadores consultados com data de corte e vínculo às obras/fases; revisão antes de mensagens ou alterações gerenciais.
- Perguntas naturais: ferramentas com contratos e consultas permitidas, filtros no servidor, limites de linhas/tempo e auditoria; nunca entregar credencial de banco ou executar SQL livre produzido pelo modelo. Permissões e sigilo de custos continuam válidos para respostas da IA.
- Previsões: inventariar obras concluídas, porte/tipo, fase, duração efetiva, horas e alterações de escopo. Começar por baseline de comparáveis com intervalo e hipóteses; validar em casos separados, evitando usar informação futura. Sem histórico suficiente, informar impossibilidade de previsão confiável e propor coleta estruturada. Não prometer precisão, treinamento de modelo próprio ou prazo certo pela simples inclusão de IA.

A fase de IA considera um provedor (OpenAI ou Claude), selecionado após comparar custo, dados e qualidade. Não é preciso contratar ambos. O fato de o sistema ter sido desenvolvido com Claude não implica que utilize sua API em produção.

## Segurança e confidencialidade proposta

NDA e cláusulas de finalidade, pessoas autorizadas e acesso mínimo antes da inspeção. Credenciais temporárias protegidas, dados sintéticos ou anonimizados nos testes quando possível, trilha de acesso e devolução/exclusão ao encerrar conforme contrato. Proibir uso de código, obras e documentos em demonstrações públicas ou portfólio sem autorização. Restringir custo por colaborador e informação comercial por perfil.

Sigilo contratual com Olivaw não significa que o processamento de IA seja local ou sem retenção. Provedor, campos enviados, região quando aplicável, retenção e subprocessadores precisam de autorização. APIs comerciais e planos pessoais têm condições distintas; não prometer retenção zero por padrão. Permitir desligar IA sem interromper cadastro, prazos e apontamentos. Conteúdo de documentos não pode autorizar ações ou ampliar acesso por instruções embutidas.

## Alternativas e licenças

OpenProject foi consultado como referência de gestão de tempo/custos. Serve para comparar o quanto da necessidade é gestão padronizada, mas não comprova aderência ao fluxo da fábrica, migração ou licenças de todos os recursos. Não selecionado: a proposta prefere estrutura Olivaw e reaproveitamento do sistema atual quando útil. Plugins/componentes do protótipo devem ter manutenção e licença inventariadas no diagnóstico; não trocar a arquitetura por ferramenta externa sem avaliação e aceite.

## Dúvidas essenciais

- Qual stack, hospedagem, banco, propriedade do código e processo de backup atuais?
- Quantos usuários simultâneos, obras, fases, anexos e registros históricos? Sistema já opera diariamente?
- Produção significa acompanhar entrega à fábrica ou gerir peças, lotes, máquinas, estoque e capacidade fabril?
- Quem aponta/aprova horas, qual custo/hora vigente por período e quais papéis podem ver valores?
- Quais regras de cronograma, aprovação, revisão e comunicação com calculistas externos?
- Qual histórico confiável existe para previsão e como será avaliada a utilidade das estimativas?
- Qual dado pode sair para API de IA e quais exigências de confidencialidade/retencão?
- Primeira entrega prioritária, orçamento disponível, prazo e disponibilidade do cliente para homologar?

## Resultados e critérios previstos

Diagnóstico deve entregar mapa atual, riscos priorizados, decisão sobre reaproveitamento e plano com critérios e custos revistos. Núcleo deve suportar fluxo de uma obra com responsáveis, permissões e dependências. Horas/custos precisam reconciliar exemplos aprovados; reunião semanal deve consultar indicadores consistentes. IA deve responder casos de teste com evidência, recusar acesso indevido e reconhecer dados insuficientes. Exercitar restauração e importação piloto em homologação. Nada disso já foi testado no sistema do cliente.

## Fontes consultadas em 03/11/2026

- [Anúncio público](https://www.99freelas.com.br/project/avaliar-e-refazer-sistema-de-gestao-de-projetos-e-producao-com-ia-795444): descrição e condição de acesso após contrato.
- [OpenAI — ferramentas/function calling](https://developers.openai.com/api/docs/guides/function-calling): integração com funções da aplicação; a aplicação deve validar acesso e execução.
- [OpenAI — controles de dados](https://developers.openai.com/api/docs/guides/your-data): políticas e elegibilidade de controles conforme recurso/configuração.
- [Claude — retenção de API](https://platform.claude.com/docs/en/manage-claude/api-and-data-retention): retenção e exceções por recurso/modelo; não presumir retenção zero.
- [OpenProject — tempo e custos](https://www.openproject.org/docs/user-guide/time-and-costs/): referência comparativa, sem seleção ou aquisição de ferramenta.
