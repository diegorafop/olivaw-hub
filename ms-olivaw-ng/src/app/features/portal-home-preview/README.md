# Prévia da Home do Portal

Protótipo local em `/portal`, acessível somente pela sessão administrativa root existente. A autenticação definitiva do cliente ainda não faz parte desta prévia. Nenhuma API de métricas ou solicitações é chamada; os dados são ilustrativos e a simulação de chamado dura apenas até recarregar a página.

Os filtros Dia, Semana, Mês e Ano alteram os dois gráficos. O primeiro card apresenta o plano; o segundo mostra a média ilustrativa das franquias de espaço e melhorias, com cálculo e valores individuais visíveis. Não há card de transferência. Armazenamento não reinicia mensalmente. Acessos representam visualizações de páginas, não pessoas únicas. Valores, datas, empresa e limites Bronze são exemplos, não uma oferta comercial. Nesta prévia, 1 GB = 1.000 MB; a unidade contratual precisa ser definida na implementação.

O seletor de cenário permite comparar uso normal, proximidade do limite e ausência de leituras. Sem dados não significa consumo zero. O percentual demonstrativo é a média simples de dois percentuais: espaço e melhorias utilizadas. Não representa desempenho ou disponibilidade e não deve esconder um recurso próximo do limite. A implementação real utiliza somente métricas contratadas e disponíveis; atualmente identifica armazenamento separadamente. Chamados e melhorias são acompanhamento, sem SLA presumido. A franquia é uma hipótese comercial, não um contrato vigente.

Referências consultadas em 25/10/2026:

- [Hostinger — Uso de recursos](https://www.hostinger.com/support/2436138-how-to-check-resource-usage-in-hostinger/): consumo, limites e históricos; considerar a atribuição por site em ambientes compartilhados.
- [SiteGround — Dashboard](https://www.siteground.com/kb/what-can-i-do-in-site-tools-dashboard): armazenamento e estatísticas de tráfego.
- [Vercel — Acompanhamento de uso](https://vercel.com/docs/pricing/manage-and-optimize-usage): métricas e acompanhamento de limites.

A implementação autorizada está documentada em `docs/features/done/portal-home-chamados-despesas.md`. As páginas usam links e breadcrumb, sem modais; a simulação de chamado navega para o detalhe. O requisito de isolamento JWT está registrado separadamente em `docs/features/discovery/seguranca-jwt-portal-e-devops.md`.

Validação: `npm run build`, `npm run lint` e `npm run e2e -- e2e/portal-home-preview.spec.ts`, executados na pasta `ms-olivaw-ng`. O E2E usa identidade simulada, verifica bloqueios de acesso, filtros, cenários, responsividade e ausência de chamadas de negócio.
