# Trevize — Gmail e acompanhamento comercial

## Objetivo

Preparar as etapas posteriores à descoberta de contatos: validação pequena, enriquecimento das fichas existentes, OAuth Gmail, teste controlado e abordagem comercial com texto e destinatário aprovados. O mantenedor realizará o deploy.

## Entrega local

- `leads collect-preview`: até cinco candidatos, sem IA paga, CRM ou mensagens; evidências somente na saída.
- `leads inspect` e `leads enrich --apply`: leitura/mescla de canais por revisão, preservando textos humanos e acompanhamento. `--limit 40` permite preparar a conferência do lote existente; `--id` seleciona fichas específicas.
- `gmail configure/authorize/profile`: cliente oficial por OAuth com PKCE, escopos send/readonly, conta conferida e estado privado.
- `leads prepare/prepare-reply --approve`: destinatário conhecido, texto imutável, intenção canônica e aprovação individual. Não envia.
- `leads send --confirm`: reserva idempotente no CRM antes de enviar; timeout não gera reenvio. Até cinco leads aprovados e cinco tentativas por dia UTC por diretório.
- `leads sync/reconcile/outbox`: histórico por lead, respostas reconhecidas apenas após persistência, recuperação de falhas e bloqueio após intervenção humana.
- `PUT /leads/{id}/communications`: `leads.manage`, revisões pública e de acompanhamento, metadados no snapshot e notas em `LeadChange`, na mesma transação. Repetição idempotente e conteúdo divergente bloqueado.
- Envio/resposta move somente Novo contato para Em conversa. Etapas posteriores são preservadas. Corpo de e-mail no CRM limitado a trecho de 1.500 caracteres; anexos não são coletados.

## Verificação

Testes isolados exercitam coleta e enriquecimento sem escrita, preservação de dados humanos, aprovação, bloqueios, reservas concorrentes, queda antes/depois do envio, reconciliação, falha de CRM após Gmail, leitura independente de UNREAD e intervenção humana. Testes da API cobrem contrato, handler, autenticação e controller. Comandos e roteiro de aceite no [GMAIL.md](../../../olivaw-agent/ms-trevize-py/GMAIL.md).

Validação executada: 71 testes Python e 485 testes da API aprovados; build da API sem warnings e `dotnet pack` do cliente concluído. Smoke local dos comandos `leads --help`, `leads outbox` e `gmail status` confirmou que não iniciam o worker; Gmail informa `setup-required`. Links relativos e diff conferidos.

Não foi efetuado deploy, configurado OAuth real, executada coleta/enriquecimento remoto ou enviado e-mail real. Para o aceite, publicar primeiro a API compatível; o operador fornece um cliente OAuth Desktop e autoriza a conta no navegador, depois executa o roteiro com endereços próprios antes de escolher leads.

## Limites

Sem migration ou submissão de formulários. Campanhas e atendimento pelo roteiro limitado foram codificados, mas estão desabilitados por padrão; ver a ampliação abaixo. Não muda a skill canônica. Limites locais pressupõem um operador/diretório, não uma cota global distribuída. O diário de transporte precisa ser preservado ao trocar de máquina; a ausência de uma mensagem em Enviados não autoriza reenvio. Revisão e aprovação são exigidas para cada resposta. Esta entrega não encerra uma eventual discovery de atendimento autônomo.

## Ampliação da issue031

No teste controlado, o envio inicial foi aceito pelo Gmail e a resposta do destinatário foi lida. O provedor reescreveu o `Message-ID`; o agente agora confirma pelo ID durável retornado pelo envio, com destinatário, remetente, assunto e conteúdo conferidos, sem repetir a mensagem. A classificação foi inicialmente interrompida por um aviso de inicialização do Codex; a execução agora suprime apenas esse aviso de recurso experimental, preservando as restrições de ferramentas. O diagnóstico posterior escolheu encaminhamento ao operador por pergunta sobre finalidade/origem do contato.

Preparada retomada auditada `resumed` e `leads resume --approve --note ...`, com preservação do histórico, revisão local versionada, recusa não removível e bloqueio de envios incertos. A API v1.1.2 precisa ser publicada para essa retomada; ela não foi executada automaticamente no teste. Mensagem inicial única, sem segunda resposta automática, sem campanha de leads reais.

Validação desta ampliação: 90 testes Python e 506 testes da API aprovados. Os relatos abaixo descrevem verificações anteriores ao OAuth e ao teste controlado; o estado atual é conta autorizada, envio automático desativado e uma mensagem inicial enviada ao endereço próprio do mantenedor.

Implementados campanhas locais aprovadas, agenda de consulta independente da coleta no worker manual, classificador de IA sem ferramentas e com perguntas fixas, limites por conversa/janela, supressão de recusas e transferência ao operador. O classificador não pode inventar preços, prazos ou texto livre para envio. Cancelamento e revisão são permitidos apenas antes de reservar o envio. O endpoint de descoberta promove e-mail principal vazio e inequívoco, preserva revisão humana e registra `sem email` uma única vez após pesquisa concluída, sem regredir etapas.

O painel Trevize combina indicadores de captação e de comunicação persistida, com limites de amostra explícitos. Estados de worker/falhas locais permanecem no status local e não são apresentados como disponibilidade remota. Esta ampliação exige alterações coordenadas em agente, API e interface; o tamanho é justificado pelo fluxo transacional de preparação, cancelamento, supressão e apresentação, validado em conjunto.

Envio bloqueado no setting e no transporte Gmail. Nenhuma campanha real foi criada, ativada ou enviada. Sem OAuth real, IA paga, deploy, banco remoto ou migrations. O mantenedor realizará push e publicação após o Git Flow local.

## Validação integrada da finalização local

82 testes Python, 497 testes .NET, 14 testes unitários dos dashboards e 15 E2E de Agents/fichas passaram. O build Angular passou com os avisos de orçamento de CSS já existentes. A integração conserva os gráficos, filtros, paginação e documentos da issue030 e acrescenta os indicadores de comunicação da issue031. Sem envio real, ativação de campanha, push, deploy ou migrations.
