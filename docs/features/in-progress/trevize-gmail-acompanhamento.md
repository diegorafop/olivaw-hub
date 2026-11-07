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

Sem migration, campanha agendada, resposta autônoma por IA ou submissão de formulários. Não muda a skill canônica. Limites locais pressupõem um operador/diretório, não uma cota global distribuída. O diário de transporte precisa ser preservado ao trocar de máquina; a ausência de uma mensagem em Enviados não autoriza reenvio. Revisão e aprovação são exigidas para cada resposta. Esta entrega não encerra uma eventual discovery de atendimento autônomo.
