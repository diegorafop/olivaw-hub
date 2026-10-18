# Documentação do Olivaw

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
- [Site institucional](../ms-olivaw-ng/README.md): frontend derivado, domínios e pendências de contato.
- [Template Angular](../ms-limpo-ng/README.md): site público prerenderizado, administração, autenticação, tema, testes e Docker.
- [Template de APIs](../ms-limpo-api/README.md): composição do host, desenvolvimento e Docker.
- [Gerar um cliente](../ms-limpo-api/TEMPLATE.md): uso do gerador e preparação do novo projeto.
- [Core compartilhado](../ms-olivaw-core-api/README.md): bibliotecas genéricas e seus contratos.
- [Briefing](../BRIEFING.md): contexto de negócio e decisões iniciais.

Comece pelo guia de desenvolvimento para entender como os componentes se relacionam. Use os READMEs dos módulos para configuração e contratos específicos.

- [Contrato do Registro.br: análise](registrobr/analise-contrato.md).
- [Compra e validação do domínio](registrobr/validacao-dominio.md).
