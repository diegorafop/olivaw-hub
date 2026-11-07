# Trevize — Enriquecimento de e-mail, Gmail API e conversa inicial

## user-history

Como operador da Olivaw, quero que o Trevize encontre e registre e-mails comerciais dos Leads coletados e envie uma abordagem individual pela conta Olivaw, acompanhando respostas e conduzindo a conversa inicial, para qualificar oportunidades e encaminhar contatos à equipe sem perder histórico ou repetir mensagens.

## acceptance-criteria

- Preservar coleta de até 20 novos Leads por ciclo, limite configurável e intervalo atual de 24 horas após o término anterior, sem sobreposição. Não prometer completar 20 se fontes, tempo ou duplicidade impedirem.
- Preservar start explícito e parada cooperativa: nenhum autostart no boot, ao abrir Gmail, ao instalar extensão ou ao abrir o painel. Reinício do PC não retoma envios sem novo start.
- Pesquisar e-mail comercial efetivamente publicado no site oficial, páginas de contato/sobre/rodapé e dados públicos do Maps quando disponíveis. Não inferir endereço, fabricar padrões ou tratar telefone como e-mail.
- Registrar e-mail no campo principal do Lead, com URL, data e contexto da fonte. A qualificação não pode ser o único local onde o contato fica salvo.
- Priorizar contato comercial ligado à identidade da empresa. Múltiplos endereços sem preferência clara vão para revisão; não enviar para todos. E-mail de agência, fornecedor, política de privacidade ou encarregado de dados não vira destinatário comercial por simples ocorrência no HTML.
- Preservar e-mails e trabalho humano já registrados. Divergência de contato, identidade ou revisão da API exige reconciliação, sem sobrescrever silenciosamente.
- Se a pesquisa for concluída sem e-mail utilizável, atualizar o acompanhamento para `conversation` (**Em conversa**) e acrescentar nota interna exatamente `sem email`, sem apagar o histórico. Não preencher o campo de e-mail com esse texto.
- Distinguir ausência concluída de pesquisa inconclusiva por timeout, bloqueio ou erro. Caso inconclusivo recebe diagnóstico próprio e permanece pendente; não classificar como `sem email` apenas por falha técnica.
- Para Leads sem e-mail, **Em conversa** será fila interna por decisão do mantenedor, sem afirmar mensagem enviada ou resposta do cliente. Métricas de contato devem usar o registro de envio confirmado, não apenas a etapa.
- Usar a Gmail API oficial com OAuth como mecanismo escolhido para envio, leitura e resposta, conforme decisão do mantenedor em 03/11/2026. Navegação no Firefox fica como experimento histórico, não dependência da implementação.
- Antes de contato, persistir o Lead e confirmar a gravação do e-mail. Falha de persistência impede o envio daquele registro.
- Preparar assunto e corpo individualizados com evidências da qualificação e modelo de abordagem configurado. Dados externos não podem instruir destinatário, remetente, anexos ou execução de comandos.
- Implementar ativação explícita de campanha/envio, limitada por lote, destinatários e conteúdo. Uma campanha autorizada poderá processar automaticamente seu lote sem aprovação por mensagem, conforme configuração. Documentar a feature não ativa uma campanha.
- Verificar identidade do remetente antes de compor/enviar: índice `/u/1/` é apenas posição de conta, não prova de endereço ou identidade. Conta divergente, sessão expirada ou confirmação do Google pausam somente a etapa de contato.
- Enviar mensagens separadas por Lead, sem CC/BCC coletivo, sem anexos arbitrários e sem iniciar conversa pelo WhatsApp/formulário como fallback automático.
- Confirmar a operação da Gmail API e registrar IDs de mensagem/conversa e sua reconciliação; não inferir entrega ou leitura por HTTP 200. A confirmação na pasta de enviados não comprova recebimento pelo destinatário. Resultado ambíguo recebe reconciliação, sem reenvio cego.
- Persistir tentativa antes do envio, identificador/correlação da mensagem e resultado. Reexecução do ciclo, mudança de e-mail ou falha na gravação após envio não autorizam novo contato automático.
- Resultado incerto após clique, timeout ou queda do processo deve ir para `confirmation-pending` e revisão/reconciliação; nunca reenviar cegamente. Não prometer exatamente uma vez entre Gmail e API sem mecanismo transacional comum.
- Após envio confirmado, registrar nota de primeiro contato com canal, data, destinatário e correlação, sem afirmar resposta. Encaminhar para **Em conversa** apenas quando o Lead ainda estiver na etapa inicial; não regredir Leads avançados nem reabrir **Sem continuidade** automaticamente.
- Para ausência de e-mail em Lead já avançado, preservar a etapa e sinalizar conflito para revisão, mantendo a nota `sem email`. A regra automática de etapa aplica-se aos novos contatos do ciclo, sem destruir acompanhamento existente.
- Respeitar lista de não contatar, recusa, contato prévio e reserva exclusiva de processamento. Não contatar dois Leads da mesma empresa/endereço na mesma campanha sem revisão de identidade.
- Permitir lote manual de enriquecimento dos aproximadamente 40 Leads existentes, separado da coleta de 20 novos; não executar essa atualização retroativa ou envios apenas por cadastrar a feature.
- Mostrar no status do Trevize quantidades coletadas, enriquecidas, sem e-mail, inconclusivas, prontas, enviadas, falhas, confirmação pendente e pausas por sessão, sem expor credenciais.
- Consultar respostas às conversas vinculadas aos Leads, com OAuth de leitura adequado, paginação, checkpoint e deduplicação por ID. Não considerar apenas mensagens não lidas: a leitura manual no Gmail não pode fazer o agente perder uma resposta.
- Correlacionar respostas por conta, `threadId` e cabeçalhos, preservando o contexto do Lead. Mesmo assunto ou endereço igual isoladamente não comprova vínculo. Mensagens sem vínculo seguro vão para revisão.
- Permitir respostas automáticas de qualificação conforme roteiro comercial autorizado, com limites por conversa e janela de tempo. Não responder a si próprio, mensagens automáticas, notificações de entrega ou recusas com nova oferta.
- Respeitar recusa de contato e suspender novos envios; pedidos de orçamento, negociação, contrato, reclamação ou tema fora do roteiro são encaminhados ao operador com resumo, sem inventar preço, prazo, garantia ou compromissos.
- Detectar intervenção humana e pausar a automação da conversa para não responder em paralelo; retomada só por decisão explícita do operador.
- Preservar a conversa na resposta usando `threadId`, `In-Reply-To`, `References` e assunto compatível, em vez de abrir novo contato a cada mensagem.
- Testar extração, atualização, concorrência, retomada, leitura e envio com fixtures/mocks. Prova real inicial deve usar destinatário de teste controlado pelo operador. Validação prevista para 04/11/2026, sem agendamento automático ou envio nesta entrega documental.

## ai-prompt

### Estado, escopo e rastreabilidade

Discovery criada em 03/11/2026. Responsável pela documentação: Codex. Branch observada: `feature/issue030-dashboard-agents`; worktree `/home/makina/projects/olivaw`. Não há issue própria desta melhoria. Arquivos compartilhados nesta entrega: este documento e referência em `docs/README.md`. Nenhuma implementação, alteração de configuração, start de agente, envio ou cadastro remoto autorizado pela simples elaboração da feature.

O pedido amplia futuramente o limite atual de coleta sem contato. As restrições vigentes no AGENTS e na skill do Trevize não foram alteradas nesta documentação. Ao implementar a melhoria, atualizar de forma explícita e delimitada esses contratos e a documentação operacional, preservando as restrições contra formulários e contatos fora da campanha autorizada. A IA de qualificação deve continuar sem credenciais ou ferramentas de envio; o adaptador executa operações previamente delimitadas.

### Base real consultada

- [Coleta agendada implementada](../done/trevize-coleta-agendada-de-leads.md).
- [Discovery original](trevize-prospeccao-e-qualificacao-de-leads.md).
- [Projeto Trevize](../../../olivaw-agent/ms-trevize-py/README.md), [instruções](../../../olivaw-agent/ms-trevize-py/AGENTS.md) e [skill canônica](../../../olivaw-agent/skills/trevize-prospect/SKILL.md).
- [Adaptador de Leads](../../../olivaw-agent/ms-trevize-py/src/ms_trevize/data/lead_api.py) cria atualmente o cadastro com `email` vazio e só entrega qualificação; é uma lacuna real.
- [Leitura passiva do site](../../../olivaw-agent/ms-trevize-py/src/ms_trevize/data/website_review.py) e [identidade do candidato](../../../olivaw-agent/ms-trevize-py/src/ms_trevize/domain/candidate.py) devem ser estendidos sem perder limites e normalização.
- [Contrato de acompanhamento](../../../ms-olivaw-api/src/Owh.Olivaw.Client/Features/LeadFeature/Commands/UpdateLeadFollowUp.cs) tem revisão, status e nota. `PUT /leads/{id}/follow-up` usa `followUpRevision`; não confundir com revisão do snapshot.
- [Controller administrativo](../../../ms-olivaw-api/src/Owh.Olivaw.Api/Features/LeadFeature/LeadReadController.cs) usa `leads.manage` para cadastro, consulta, qualificação e acompanhamento.
- [Atualização pública de Lead](../../../ms-olivaw-api/src/Owh.Olivaw.Api/Features/LeadFeature/LeadController.cs) exige `X-Lead-Key`; não é endpoint administrativo de contato por simples token técnico. Definir atualização administrativa mínima e autenticada para e-mail/fonte e registro de abordagem; não obter write key pública, reaproveitar root ou reconstruir snapshots para contornar contrato ausente.

### Fluxo funcional proposto

1. Start manual, pré-verificação da API e coleta limitada atual.
2. Identificar Lead/candidato e consultar dados existentes, não contatar descartados ou já abordados.
3. Ler fontes públicas delimitadas; priorizar páginas da mesma empresa e contato comercial. Aplicar limite de páginas por domínio, redirects, tamanho e tempo; preservar proteção contra destinos privados e conteúdo malicioso.
4. Validar formato, fonte e vínculo com a empresa. Formato válido não comprova que a caixa existe; não usar SMTP de sondagem. Registrar inconclusivo se a evidência não permitir concluir.
5. Criar/atualizar contato pela API e confirmar. Não recriar Lead ou substituir qualificação humana.
6. Sem e-mail após pesquisa concluída: acrescentar `sem email` e mover novo contato para **Em conversa**, de forma idempotente. Interface/histórico deixam claro que não houve envio.
7. Com e-mail: verificar exclusões, campanha habilitada, remetente, destinatário e reserva de envio; preparar mensagem e registrar intenção.
8. Usar o adaptador Gmail escolhido, confirmar resultado e persistir o histórico pela API. Se houver incerteza, pausar aquela intenção para reconciliação.
9. Fechar lote com resumo; a falta de sessão Gmail não converte empresas em `sem email` nem perde os contatos enriquecidos.

### Navegação no Gmail: preferência inicial e alternativas

O mantenedor solicitou testar navegação na sessão Firefox já autenticada. A preferência inicial foi navegar na interface. **Decisão posterior de 03/11/2026: o mantenedor escolheu Gmail API oficial, com validação prevista para amanhã, 04/11/2026.** Esta decisão substitui a preferência por navegação. Definir adaptador Gmail API em Python; não implementar extensão ou controle de desktop como requisito deste recorte. Nenhum teste OAuth ou envio foi executado nesta atualização.

Opções comparadas antes da decisão pela API (preservadas como contexto, não implementação paralela):

- **Extensão autorizada no Firefox:** content script limitado ao Gmail pode ler elementos e operar a interface já autenticada, com comunicação restrita ao Trevize por ponte local/extensão. Requer desenvolvimento e instalação autorizada, controle de origem, autenticação da ponte e confirmação do contexto/remetente. Não garante estabilidade diante de mudanças do Gmail ou aceitação do Google. Evitar acesso a toda a caixa quando bastarem compositor, remetente e confirmação da mensagem específica.
- **Controle de desktop/acessibilidade:** interação por elementos acessíveis ou tela/teclado em sessão gráfica autorizada. Depende de permissão do sistema, foco e desktop disponível; não pressupor que captura de tela dê controle de mouse/teclado. Perda de foco deve impedir envio, não tentar clicar por coordenada em janela desconhecida.
- **Sessão de navegador dedicada:** operador autentica manualmente uma sessão separada. Só é alternativa após demonstrar leitura e interação; abrir outro Firefox ou executar Playwright não concede acesso à sessão existente. Não copiar cookies/perfil/senhas nem abrir simultaneamente o mesmo perfil em instâncias concorrentes. Um browser automatizado continua sujeito a bloqueios do Google.
- **Gmail API com OAuth — escolhida:** mecanismo principal para integração persistente, aceito pelo mantenedor nesta conversa. Permite autorização da conta, escopos mínimos e envio de mensagem/rascunho. Não exige compartilhar senha com o agente. Não tratar API como implementada ou habilitada; definir antes quem guarda tokens, permissões de leitura necessárias e como revogar. Autorização para enviar não exige automaticamente leitura irrestrita da caixa.

Não contornar CAPTCHA, MFA ou bloqueios de login com modo stealth, exportação de sessão ou alteração de segurança do Google. Pausar para intervenção quando o mecanismo autorizado não estiver disponível. O fato de o Google eventualmente bloquear navegador automatizado não comprova que toda integração seja impossível.

### Prova exploratória realizada nesta conversa

Em 03/11/2026, a pedido explícito do mantenedor:

- Firefox aceitou abertura do Gmail e depois da URL `https://mail.google.com/mail/u/1/#inbox` informada pelo operador.
- Captura direta via GNOME Shell Screenshot foi negada por permissão do desktop.
- Solicitação pelo portal de screenshot, com confirmação visual do operador, gerou uma captura temporária em `/tmp`. A imagem permitiu ler remetentes e assuntos da caixa de entrada, com 10 mensagens listadas e 4 não lidas naquele momento.
- Não foi verificado o endereço exato do remetente; índice da conta e avatar não substituem essa verificação antes de enviar.
- Não foram testados cliques, composição, rascunhos ou envio. Nenhuma mensagem foi enviada, aberta individualmente, alterada ou excluída pelo agente.
- A confirmação manual da captura prova acesso visual naquele instante, não execução autônoma nem controle da interface. Não anexar a captura da caixa privada à documentação/Git, nem transcrever mensagens pessoais.
- O mantenedor ofereceu usuário/senha e outra instância Firefox; registrar como alternativa oferecida, sem solicitar, receber ou persistir senha nesta etapa. Preferir login pelo próprio operador ou autorização OAuth.

### Decisão Gmail API e atendimento inicial

O exemplo Python indicado pelo mantenedor é ponto de partida, não integração pronta. Preparar projeto Google Cloud, habilitar Gmail API e configurar cliente OAuth apropriado a aplicativo local, tela de consentimento e conta de teste quando aplicável. O operador autoriza a conta no navegador; não compartilhar senha. Guardar credenciais/refresh token apenas em armazenamento local privado com acesso restrito, fora do Git e dos prompts; prever expiração, revogação e reautorização. Não supor que abrir Gmail no Firefox autorize chamadas de API.

Para envio e leitura, avaliar `gmail.send` + `gmail.readonly`. O segundo é escopo restrito e permite ler conteúdo da caixa, não apenas os Leads; a seleção das conversas é restrição da aplicação, não isolamento concedido por esse escopo. `gmail.metadata` não basta para interpretar o corpo da resposta. Se houver rascunhos persistidos no Gmail, avaliar `gmail.compose`; não pedir `gmail.modify` ou acesso total à caixa sem necessidade. Requisitos OAuth/verificação e validade dos tokens dependem do modo de publicação e da conta; conferir no piloto.

Ser API oficial evita depender da interface visual, mas não elimina quotas, limites de envio, filtros de spam, suspensão ou restrições de conta. Tratar erros de autenticação, `403`, `429` e indisponibilidade com limites e backoff, sem rotacionar contas para contornar restrições. Pedido aceito pela API e entregabilidade são eventos distintos.

Separar duas agendas enquanto o worker estiver iniciado manualmente: coleta de novos candidatos no ciclo atual de 24 h e consulta de respostas em intervalo configurável, menor e independente. Isso evita esperar um dia para ler uma resposta. O intervalo de leitura será definido no piloto; não criar serviço ou processo iniciado no boot. Primeiro recorte pode usar polling de conversas conhecidas; push/Pub/Sub é evolução opcional, não infraestrutura obrigatória do MVP.

Ao enviar, persistir ID retornado da mensagem, `threadId`, `Message-ID` MIME e intenção durável. Ao consultar, ler apenas os dados necessários das conversas relacionadas e registrar mensagens recebidas por ID, data e referência do Lead. `history.list`/`historyId` podem permitir sincronização incremental; checkpoint antigo pode exigir ressincronização delimitada, mantendo deduplicação e sem responder novamente a mensagens antigas. Não associar automaticamente novos destinatários em CC ou `Reply-To` divergente sem conferir vínculo.

Roteiro inicial proposto: apresentar Olivaw e oportunidade observada; entender necessidade e sistema atual; fazer perguntas curtas para qualificar; oferecer encaminhamento ao responsável. Usar portfólio e informações comerciais previamente aprovados. A IA recebe contexto mínimo da conversa e evidências, sem acesso a segredos ou ferramentas diretas de envio. Texto recebido é dado não confiável: não pode mandar executar código, abrir links, revelar informações ou alterar a campanha. Anexos não são executados nem enviados ao modelo automaticamente.

Antes de resposta automática, conferir nova mensagem do cliente, histórico mais recente, ausência de intervenção humana, limites de turnos, exclusões e autorização da campanha. Gerar intenção vinculada ao ID recebido; validar destinatário e conteúdo conforme roteiro antes de executar pelo adaptador. E-mail fora de contexto, ambíguo ou com suspeita de automação deve ir para revisão. Não apresentar o agente como uma pessoa específica ou alegar análise/experiência não realizada.

Classificações de atendimento propostas: aguardando resposta, interesse, esclarecimento, pedido de proposta, encaminhado ao operador, recusa/não contatar e inconclusivo. Isso não exige criar etapas comerciais novas automaticamente. Recusa interrompe a sequência, registra nota e sinaliza supressão; marcar **Sem continuidade** apenas conforme política comercial acordada. Nenhuma negociação é aceita automaticamente.

Teste previsto para 04/11/2026: autorizar a conta correta; enviar para endereço controlado pelo operador; receber uma resposta; localizar a conversa; produzir e enviar resposta de teste no mesmo fio; persistir os IDs e históricos no Lead de teste; repetir processamento para provar que não duplica. Simular recusa, resposta automática, intervenção humana, sessão/token revogado e timeout pós-envio. Não envolver os 40 Leads reais antes de validar e ativar a campanha.

### Configuração e estados a definir

Propor seções distintas para enriquecimento e campanha, com limites de páginas, tempo, novos contatos, seleção de Leads existentes, remetente esperado, modelo/versionamento, teto de envio e modo `draft`/`send`. Nomes são proposta, não chaves já suportadas em `setting.json`.

Representar separadamente localização do e-mail (`found`, `not-found`, `inconclusive`, `conflict`) e execução da abordagem (`pending`, `reserved`, `prepared`, `sent`, `failed`, `confirmation-pending`, `suppressed`). A etapa comercial existente permanece independente desses estados técnicos.

A chave da intenção deve ser estável por Lead e campanha. Hash do conteúdo ajuda a auditar, mas uma reescrita não pode autorizar repetição. Registrar endereço original e normalizado, identidade da empresa, fonte, data, remetente, versão do modelo, assunto/corpo aprovado, timestamps e correlação. Não guardar cookies, senha, token OAuth ou HTML completo do Gmail no snapshot, logs ou prompt.

Persistência durável via API Olivaw. Se exigir extensão de schema, seguir contratos e testes locais; migrations somente mediante pedido explícito. Preferir operações mínimas idempotentes com concorrência, preservando histórico humano. Não prometer endpoint novo como existente antes da implementação.

### Testes e condições para avanço

Cobrir fixtures de mailto/texto visível, e-mail de terceiro no rodapé, empresa homônima, múltiplos contatos, fonte indisponível e ausência real. Testar atualização sem perda de campos, conflito 409, recusa, contato anterior, suspensão, nota única `sem email`, não regressão de etapa e identidades duplicadas.

No Gmail, testar primeiro com interface simulada: conta divergente, compositor errado, destinatário modificado, sessão expirada, MFA, foco perdido, falha de rede, sucesso seguido de falha na API e retomada após crash. Nenhum teste automatizado de rotina deve acessar a conta real ou enviar para Leads reais.

Dependências para sair de discovery: provar OAuth e Gmail API escolhidos, confirmar remetente real, fechar escopos, contratos de contato/auditoria, sincronização das respostas, limites de campanha, roteiro de atendimento e transferência humana. Depois realizar teste controlado com endereço do operador e confirmação específica de envio. A migração dos cerca de 40 Leads existentes requer seleção explícita do lote; não iniciar automaticamente nesta documentação.

### Referências oficiais consultadas em 03/11/2026

- [Google — login em navegador compatível](https://support.google.com/accounts/answer/7675428?hl=pt-BR): logins controlados por automação podem ser bloqueados.
- [Playwright — browsers](https://playwright.dev/python/docs/browsers#firefox): não presumir suporte ao Firefox pessoal já aberto a partir do runtime atual do Trevize.
- [Mozilla — content scripts](https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/Content_scripts): leitura/modificação da página condicionada às permissões de origem.
- [Gmail API — envio e exemplo Python](https://developers.google.com/workspace/gmail/api/guides/sending#python) e [escopos](https://developers.google.com/workspace/gmail/api/auth/scopes): mecanismo OAuth escolhido, com leitura e envio separados.
- [Conversas e respostas](https://developers.google.com/workspace/gmail/api/guides/threads), [sincronização](https://developers.google.com/workspace/gmail/api/guides/sync) e [erros/limites](https://developers.google.com/workspace/gmail/api/guides/handle-errors): persistência do fio, atualização incremental e quotas.

### Validação desta entrega documental

Conferir exatamente os quatro blocos obrigatórios, links relativos, coerência com o código consultado e distinção entre prova visual e automação funcional. Não alterar `features/index.md`, reservado às entregas concluídas. Sem build, migrations, publicação, start de agente ou envio por consequência desta documentação.

## user-feedback

```text
n tem implementado, eu acho que o gmail ate bloqueia se aberto em janela de automaca, o agent teria que navegar mesmo.
o que voce acha é possivel modificar nosso trevize para além de coletar os 20 leads por round, pesquisar pelo email de contato no site desses caras ou no proprio google maps. E salvar a info do email no nosso lead claro, ta faltando essa info.

e entao abrir o gmail e enviar email para os leads que encontrou o email, para o que n encontrou ele marca etapa para 'Em conversa' e preenche uma nota interna com 'sem email'

se SIM, por favor escreva uma nova feature de melhoria pra gente deixando claro esse funcionamento e melhor explicado

faz um teste ai voce abrindo meu firefox, vamos ver se tu consegues

tem a minha conta selecionada, mas na verdade a conta é esse link aqui: https://mail.google.com/mail/u/1/#inbox
pode tirar o print veja se consegue identificar os emails

eu precisei dar ok na print, mas tudo bem por hora.

quais outras opções voce teria para ler essa página e tentar enviar emails pelo olivaw?

eu posso te passar a senha e user da conta olivaw. se quiser abrir outro firefox instance no seu sandbox tbm n tem problema

se isso ficar mais facil de voce interagir
```


```text
cara gostei da ideia da API do gmail acho melhor, ate porque é oficial ele n bloquearia.

https://developers.google.com/workspace/gmail/api/guides/sending#python

aqui tem ate o codigo em python.
deixe anotado na feature amanha a gente valida ok.

tu conseguiria ler msgs tbm via API? porque ai o trevise faria a tratativa inicial toda, responderia o cliente caso ele responda a primeira mensagem e tals.
```

## Atualização de implementação — issue031

O fluxo foi implementado em recortes: descoberta de canais, enriquecimento retroativo, Gmail OAuth, intenções idempotentes, campanhas aprovadas, perguntas limitadas por IA, supressão, encaminhamento, cancelamento/revisão e painel de comunicação. Ver [estado da implementação](../in-progress/trevize-gmail-acompanhamento.md) e [roteiro operacional](../../../olivaw-agent/ms-trevize-py/GMAIL.md).

A configuração entregue mantém campanha, IA de atendimento e envio desabilitados. Não houve OAuth real, envio, deploy ou IA paga. Não inclui extração adicional de e-mails do Maps nem execução de formulários. O texto acima preserva a discovery histórica; não ativa automação por si só.
