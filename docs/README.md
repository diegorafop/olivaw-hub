# Documentação do Olivaw

- [Serviços e clientes — Base implementada](features/done/04a-cadastro-de-servicos.md): cadastro e vínculos com empresas validados localmente; schema coberto pela `Issue020`.

- [Inventário de recursos e ambientes — Implementado](features/done/04b-inventario-de-recursos.md): recursos compartilhados, implantações, vínculos, reservas e visão por cliente.

- [Histórico de entregas e notas de versão](features/painel-olivaw/18-historico-de-entregas.md): planejamento da linha do tempo por cliente e serviço.
- [Solicitações do cliente](features/painel-olivaw/13-solicitacoes-do-cliente.md): planejamento de pedidos de alterações, capacidade e plano.

- [Dependências e limites de atualização](dependencias.md): versões verificadas, restrições de licenciamento, auditoria e comandos de validação.
- [Acompanhamento comercial de leads](features/done/02-acompanhamento-comercial-de-leads.md): etapas, notas internas, histórico e migration PostgreSQL revisada; publicação pendente.
- [Features concluídas](features/done/README.md): entregas implementadas, com pendências de ambiente preservadas nos documentos.

- [Apresentação da Olivaw](approach/apresentacao-olivaw.md): versões resumida, média e completa, com adaptações para cada conversa.
- [Abordagem — Rodrigo / Impressão](approach/rodrigo-impressao.md): sequência de conversa, catálogo e possíveis etapas para o negócio de impressão.
- [Abordagem — Cavalcanti Terraplanagem](approach/cavalcanti-terraplanagem.md): divulgação da frota, locações e organização interna por etapas.
- [Abordagem — Andrea / Vendas](approach/andrea-vendas.md): conversa sobre gestão comercial, necessidades pontuais e indicações para a Olivaw.
- [Protocolo de descoberta do cliente](protocolo-descoberta-cliente.md): questionário, análise de negócio e evolução por MVP.
- [Protótipo navegável do briefing](prototipos/briefing-cliente.html): fluxo local com exemplo LTMJ e exportação de resumo.
- [Briefing e MVP da LTMJ](../ms-ltmj-ng/docs/briefing-e-mvp.md): dados confirmados, redes consultadas, pendências e fases.
- [Executor de deploy via SSH](deploy-executor.md): API assíncrona, script na VM, instalação e testes.
- [Configuração AWS, DuckDNS e Traefik](cloud.md): ambientes, credenciais locais, redes e comandos de validação/subida.
- [Renomeação para Olivaw](renomeacao-olivaw.md): escopo local, transição operacional e domínio pendente.
- [Git Flow e rastreabilidade](gitflow.md): features, releases, hotfixes, referências automáticas e conclusão das issues.
- [Organização e desenvolvimento do monorepo](desenvolvimento.md): pastas, projetos, soluções, referências, geração de clientes, testes e publicação independente.
- [Extração da identidade](identidade.md): decisão arquitetural, funcionalidades implementadas, verificações e limites da validação.
- [Guia de Olivaw Identity](../olivaw-identity/README.md): configuração, rotas, policies, persistência e bootstrap de root.
- [Épica — Administração e portal do cliente](features/painel-administrativo-e-portal-do-cliente.md): índice das features com escopos, dependências e critérios de aceite.
- [Primeira entrega — Dashboard administrativo Olivaw](features/done/01-dashboard-administrativo.md): estrutura da operação interna, navegação e estados iniciais, sem depender dos módulos seguintes.
- [Custos e planos da jornada comercial](features/custos-e-planos.md): cálculo pendente e informações a detalhar na etapa de parceria.
- [Site institucional](../ms-olivaw-ng/README.md): frontend derivado, domínios e pendências de contato.
- [Template Angular](../ms-limpo-ng/README.md): site público prerenderizado, administração, autenticação, tema, testes e Docker.
- [Template de APIs](../ms-limpo-api/README.md): composição do host, desenvolvimento e Docker.
- [Gerar um cliente](../ms-limpo-api/TEMPLATE.md): uso do gerador e preparação do novo projeto.
- [Core compartilhado](../ms-olivaw-core-api/README.md): bibliotecas genéricas e seus contratos.
- [Briefing](BRIEFING.md): contexto de negócio e decisões iniciais.

Comece pelo guia de desenvolvimento para entender como os componentes se relacionam. Use os READMEs dos módulos para configuração e contratos específicos.

- [Contrato do Registro.br: análise](registrobr/analise-contrato.md).
- [Compra e validação do domínio](registrobr/validacao-dominio.md).

- [Migração dos serviços para o domínio próprio](registrobr/migracao-servicos.md): DNS, Traefik, banco e scripts de deploy.
