# Features concluídas

Entregas implementadas e verificadas localmente. Esta pasta registra conclusão de desenvolvimento; publicação e validações específicas de ambiente permanecem descritas em cada documento.

| Feature                                                              | Entrega                                                                | Observação                                                                                                                        |
| -------------------------------------------------------------------- | ---------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| [01 — Dashboard administrativo](01-dashboard-administrativo.md)      | Estrutura da operação, navegação e acesso interno                      | Gráficos e monitoramento dependem das fontes previstas nas próximas features                                                      |
| [02 — Consulta administrativa de leads](02-leads-administrativos.md) | Lista, ficha, filtros, API protegida e resumo                          | Acompanhamento implementado na [feature 02b](02-acompanhamento-comercial-de-leads.md) |
| [02b — Acompanhamento comercial](02-acompanhamento-comercial-de-leads.md) | Etapas livres, notas internas, histórico e filtros | Migration PostgreSQL única Issue017 revisada; publicação e teste no ambiente pendentes |
| [03 — Cadastro de clientes](03-cadastro-de-clientes.md)              | Empresas, contatos, origem, situação, histórico e migration PostgreSQL | Publicação e teste na AWS ficam com o mantenedor; sem login automático                                                            |

O planejamento restante está na [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md) e em [painel-olivaw](../painel-olivaw/).

- [04a — Serviços e vínculos com clientes](04a-cadastro-de-servicos.md): base administrativa de serviços; schema coberto pela `Issue020`.
- [04b — Inventário de recursos e ambientes](04b-inventario-de-recursos.md): cadastro único de recursos, implantações, relações, reservas e visão por cliente; desenvolvimento/testes concluídos, publicação dos novos fluxos pendente.
