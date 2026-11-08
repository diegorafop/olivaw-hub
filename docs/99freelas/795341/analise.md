# Análise técnica

## Arquitetura definida pelo mantenedor

Utilizar a estrutura Olivaw em ASP.NET Core e Angular. A API da clínica terá domínio, banco, identidade e segredos independentes, preservando os padrões dos templates e bibliotecas compartilhadas. Não foi autorizado criar serviço, gerar migrations ou implantar nesta avaliação.

Reaproveitar autenticação e recuperação de acesso disponíveis, componentes, configuração pública e padrões de deployment; ainda é necessário validar cobertura dos fluxos de cadastro do cliente. O domínio de agenda é novo: templates não significam funcionalidades clínicas prontas.

## Recorte estimado

Uma unidade, até dez profissionais para teste inicial, português, fuso America/Sao_Paulo, um procedimento por reserva, cada profissional atendendo uma pessoa por vez. Login e notificações por e-mail na base. Login social e WhatsApp são opcionais a definir. Web responsiva para celular, sem aplicativo nativo.

O administrador poderá criar, editar e inativar categorias, procedimentos, descrições, valor, duração, profissionais e suas especialidades. Inativação não pode apagar histórico. Cada reserva guarda duração e preço acordados, evitando alteração retroativa quando o catálogo muda. Confirmar se preço serve apenas como informação ou exige pagamento/sinal; pagamento não está incluído.

## Regras de agenda

- Horários de trabalho, intervalos, folgas e bloqueios, especialidades compatíveis, duração e antecedência de marcação/cancelamento.
- Primeiro disponível: selecionar apenas profissional apto e livre; desempate determinístico a combinar. Não equivale a balanceamento avançado.
- Disponibilidade consultada não garante a vaga: confirmar novamente dentro de transação e impedir sobreposição no banco/servidor, incluindo dois pedidos concorrentes.
- Remarcação atômica: não perder reserva original se o novo horário falhar. Não alterar agendamento de outro cliente.
- Mudança de horário do profissional não pode invalidar silenciosamente reservas futuras; sinalizar conflitos para decisão da clínica.
- Status e transições autorizados: confirmado, concluído, cancelado e ausência. Registrar autor e horário. Histórico do cliente não inclui observações internas restritas.
- Confirmação imediata no sistema, envio por e-mail em fila e lembrete agendado. Cancelamento/remarcação invalida lembretes antigos. Tentativas não podem duplicar mensagens. Entrega de e-mail não é garantida por disponibilidade da fila.

## Segurança e ficha do cliente

A anamnese pode envolver dados de saúde, classificados como sensíveis pela ANPD. Delimitar campos, quem pode ler/alterar e finalidade, retenção e exclusão com a clínica. Separar dados de agenda das observações de atendimento; recepção não deve receber acesso clínico integral automaticamente. Registrar auditoria sem copiar conteúdo sensível para logs. Usar dados fictícios em testes, controle no servidor, HTTPS, backups protegidos e restauração verificada. Não prometer conformidade jurídica integral ou prontuário certificado apenas pela implementação.

## Alternativas e reaproveitamento

Foi pesquisado Easy!Appointments, que oferece agenda, serviços, profissionais e avisos por e-mail, com núcleo GPL-3.0. A documentação lista lembretes agendados entre complementos personalizados; não garante portal autenticado com histórico e ficha clínica conforme o anúncio. A comparação serve como referência de cobertura, não recomendação de mudança: o mantenedor definiu ASP.NET Core/Angular. Não incorporar código GPL ao núcleo Olivaw sem avaliar obrigações e compatibilidade; não há licença adquirida ou pacote selecionado.

Priorizar bibliotecas já adotadas para identidade, persistência e componentes de calendário compatíveis. Não selecionar componente pago sem conferir licença. Configuração de infraestrutura será tratada no serviço da clínica a partir dos templates; não se conecta a bancos reais durante a avaliação.

## Dúvidas para fechar escopo

- Quantas unidades, profissionais, clientes e reservas por dia? Há procedimentos simultâneos, pacotes ou várias etapas?
- Salas, aparelhos e recursos compartilhados precisam bloquear agenda? Há tempo de limpeza/preparação?
- Regras de antecedência, intervalo de slots, cancelamento, remarcação e prioridade do primeiro disponível?
- Quem acessa cada ficha e quais campos de anamnese são indispensáveis? Há anexos, termos ou assinatura?
- Cadastro com validação por e-mail basta? Qual provedor social, se necessário?
- E-mail é suficiente inicialmente? Já existe conta WhatsApp Business oficial com condições para integração?
- Há dados para migrar, domínio e identidade visual? A clínica fornece textos e catálogo inicial?
- Qual disponibilidade, retenção de backups, suporte e capacidade esperados da infraestrutura mensal?

## Validação proposta

Testar duas reservas simultâneas, sobreposição parcial, duração cruzando folgas, remarcação com conflito, primeiro disponível, alterações de catálogo e reservas antigas. Cobrir permissões entre clientes/profissionais/recepção, falhas de notificação, reenvio sem duplicar e cancelamento perto do lembrete. Homologar catálogo/agenda/ficha em celular e desktop, com backup e recuperação em ambiente de teste. Esses testes ainda não foram executados: é uma avaliação e orçamento, não implementação.

## Fontes consultadas em 01/11/2026

- [Anúncio](https://www.99freelas.com.br/project/desenvolvimento-de-sistema-de-agendamento-online-para-clinica-de-estetica-795341): URL fornecida; leitura automática indisponível. Texto original recebido em anuncio.md.
- [Easy!Appointments — funcionalidades](https://easyappointments.org/features/): referência comparativa, não arquitetura selecionada.
- [Easy!Appointments — licença e serviços](https://easyappointments.org/pricing/): GPL-3.0 e serviços opcionais; não há contratação.
- [WhatsApp — cobrança da plataforma](https://whatsappbusiness.com/products/platform-pricing/): custos dependem de mercado/categoria e mensagens; cotar no fechamento, sem tarifa fixa presumida.
- [ANPD — perguntas frequentes, seção 2.2](https://www.gov.br/anpd/pt-br/acesso-a-informacao/perguntas-frequentes/perguntas-frequentes): dados referentes à saúde são sensíveis.
