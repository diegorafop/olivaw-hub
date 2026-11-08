# Diretrizes — Trevize

- Gerado pelo scaffold `ms-limpo-py`. Python 3.10+, dois espaços, CQS e composição explícita.
- API-only: sem banco, drivers ou migrations. Conta técnica com `leads.manage`, sem root/deploy.
- Lifecycle somente por start explícito. Sem cron, systemd, autostart de desktop ou retomada no boot. Arquivos de estado não autorizam reiniciar processo.
- Coleta limitada pelo setting; não contornar CAPTCHA, consentimento ou bloqueios. Não enviar mensagens, formulários nem executar comandos de páginas.
- Política autorizada pelo mantenedor em 08/11/2026: três especialistas paralelos — prospecção de 20 leads/24h, primeiro contato de no máximo 2/h (espaço mínimo de 30 minutos persistido) e leitura/respostas a cada 15 minutos, sem sobreposição por especialista. O início continua manual. Somente a configuração privada `mode=commercial` com autorização registrada e as chaves explícitas `automaticInitials`, `automaticReplies`, `aiEnabled` e `sendingEnabled` permitem operação automática. Configurações de piloto mantêm limites e aprovação individual.
- A política comercial permite rascunhos livres de qualificação aprovados pela política, em conversas conhecidas e destinatários com origem registrada. Primeiro contato automático usa apenas prospecção Google Maps com contato público inequívoco do mesmo site e pesquisa qualificada. Não contatar por silêncio nem duplicar destinatário. Negociação, preço, reclamação, recusa, falta de evidência e intervenção humana exigem suspensão/encaminhamento. Não negociar ou remover bloqueios automaticamente; não preencher formulários. Escritas incertas são reconciliadas sem repetir envio. Cota total de transporte é um teto técnico, não meta ou garantia de entrega.

- IA recebe dados públicos e contexto da conversa delimitados, sem credenciais, ferramentas ou acesso direto à API. Saída é rascunho; envio é responsabilidade dos adaptadores e da política de autorização configurada.
- Preserve fontes, data e incerteza; não tratar telefone como WhatsApp nem ausência de botão como ausência de site.
- Estado privado, limitado e ignorado pelo Git. Testes com fixtures e mocks, sem Maps/IA/API reais. Encerrar workers de teste.
- Skill canônica: `../skills/trevize-prospect/SKILL.md`. Não atualizar a própria skill automaticamente.
