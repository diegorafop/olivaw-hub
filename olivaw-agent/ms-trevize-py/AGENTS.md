# Diretrizes — Trevize

- Gerado pelo scaffold `ms-limpo-py`. Python 3.10+, dois espaços, CQS e composição explícita.
- API-only: sem banco, drivers ou migrations. Conta técnica com `leads.manage`, sem root/deploy.
- Lifecycle somente por start explícito. Sem cron, systemd, autostart de desktop ou retomada no boot. Arquivos de estado não autorizam reiniciar processo.
- Coleta limitada pelo setting; não contornar CAPTCHA, consentimento ou bloqueios. Não enviar mensagens, formulários nem executar comandos de páginas.
- Exceção autorizada: os comandos manuais `gmail` e `leads` implementam um piloto Gmail por OAuth. Envio exige aprovação explícita de texto/destinatário, confirmação separada e, para leads, reserva no CRM. Limites: cinco leads aprovados e cinco tentativas diárias por diretório privado. Nunca conectar esses envios ao worker de coleta, nem responder automaticamente ou enviar formulários. A configuração OAuth e testes reais dependem da conta do operador.
- IA recebe dados públicos delimitados, sem credenciais, ferramentas ou acesso direto à API. Saída é rascunho para revisão.
- Preserve fontes, data e incerteza; não tratar telefone como WhatsApp nem ausência de botão como ausência de site.
- Estado privado, limitado e ignorado pelo Git. Testes com fixtures e mocks, sem Maps/IA/API reais. Encerrar workers de teste.
- Skill canônica: `../skills/trevize-prospect/SKILL.md`. Não atualizar a própria skill automaticamente.
