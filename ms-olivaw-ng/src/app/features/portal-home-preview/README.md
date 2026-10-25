# Prévia da Home do Portal

Protótipo local em `/portal`, acessível somente pela sessão administrativa root existente. A autenticação definitiva do cliente ainda não faz parte desta prévia. Nenhuma API de métricas ou solicitações é chamada; os dados são ilustrativos e a simulação de chamado dura apenas até recarregar a página.

Os filtros Dia, Semana, Mês e Ano alteram os dois gráficos. Os cards superiores mostram consumo atual e transferência de outubro, independentemente do período do gráfico. Armazenamento não reinicia mensalmente. Acessos representam visualizações de páginas, não pessoas únicas. Valores, datas, empresa e limites Bronze são exemplos, não uma oferta comercial. Nesta prévia, 1 GB = 1.000 MB; a unidade contratual precisa ser definida na implementação.

O seletor de cenário permite comparar uso normal, proximidade do limite e ausência de leituras. Sem dados não significa consumo zero. O percentual destacado identifica o recurso mais próximo do limite, sem fazer média entre recursos diferentes. Chamados e melhorias são acompanhamento, sem franquia ou SLA sugerido.

Referências consultadas em 25/10/2026:

- [Hostinger — Uso de recursos](https://www.hostinger.com/support/2436138-how-to-check-resource-usage-in-hostinger/): consumo, limites e históricos; considerar a atribuição por site em ambientes compartilhados.
- [SiteGround — Dashboard](https://www.siteground.com/kb/what-can-i-do-in-site-tools-dashboard): armazenamento e estatísticas de tráfego.
- [Vercel — Acompanhamento de uso](https://vercel.com/docs/pricing/manage-and-optimize-usage): métricas e acompanhamento de limites.

A definição das atividades e features da Home aguarda a revisão visual do mantenedor. O requisito de isolamento JWT está registrado separadamente em `docs/features/discovery/seguranca-jwt-portal-e-devops.md`.

Validação: `npm run build`, `npm run lint` e `npm run e2e -- e2e/portal-home-preview.spec.ts`, executados na pasta `ms-olivaw-ng`. O E2E usa identidade simulada, verifica bloqueios de acesso, filtros, cenários, responsividade e ausência de chamadas de negócio.
