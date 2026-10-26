# Home do Portal, chamados e despesas internas

## user-history

Como cliente, quero entender meu plano e acompanhar pedidos; como operador, quero conhecer as despesas da Olivaw por período e cliente.

## acceptance-criteria

- Preservar a prévia visual e identificar dados ilustrativos; não atribuir consumo da VM compartilhada a um cliente.
- Plano é o primeiro card; retirar transferência e aviso de consumo duplicado. Indicadores compostos devem declarar o cálculo e os dados ausentes.
- Reutilizar abertura, estados e conversas de chamados existentes, com autorização por empresa e idempotência.
- Despesas administrativas são restritas a root, com moeda, conversão explícita, data, categoria, origem e associação opcional a cliente.
- Distinguir custos brutos, créditos e desembolso líquido; despesas compartilhadas sem rateio não viram custo atribuído a clientes.
- Períodos e filtros alteram gráficos e totais; ausência de dados não vira valor fictício.

## ai-prompt

Desenvolvimento concluído e validado localmente por Codex, branch feature/issue026-admin-client, worktree /home/makina/projects/olivaw. Execução sequencial; arquivos compartilhados: app.routes, configuração, contratos, domínio e mappings. Autorizado desenvolver, sem commit, push ou deploy. O mantenedor autorizou explicitamente a migration Issue026, gerada e revisada somente para PostgreSQL. APIs existentes de chamados e autoridade separada do portal devem ser preservadas e testadas, sem criar outro fluxo de autenticação ou armazenar tokens no navegador.

Cadastro operacional pela API concluído e confirmado: Olivaw cadastrada como cliente normal, nove implantações confirmadas vinculadas à VM. Lisierp e Riesel sem associação presumida; API Cavalcante preexistente preservada sem implantação inventada. Nenhum preço, DNS ou acesso de Portal foi criado automaticamente.

Cadastro operacional pela API foi solicitado separadamente: preservar serviços existentes, cadastrar Olivaw como cliente normal, relacionar os serviços confirmados da Cavalcante/LTMJ e a infraestrutura Olivaw à VM, sem inventar implantação para containers não observados. Não publicar links no Portal automaticamente.

Estudo comercial de planos fica em discovery, sem alterar contratos vigentes nem converter estimativas em despesas realizadas. UI de Despesas será interna, sem dados de margem ou ferramentas no Portal.

### Implementação local

Prévia `/portal` sem modais, com páginas de plano, indicador, chamados, melhorias e detalhes, links e breadcrumb. A Home real existente em `/meu-negocio` reutiliza a autoridade independente do Portal e endpoints protegidos; sua navegação usa query strings compartilháveis para seções e solicitações. A mudança definitiva de endereço continua pendente, sem confundir a prévia administrativa com uma sessão de cliente.

Despesas: domínio, contratos, mappings, GET/POST root, idempotência, conversão USD/BRL explícita, créditos separados, totais e série por período, filtro por cliente sem atribuição automática de custos compartilhados. UI em `/admin/expenses`. Cadastro e consulta nesta etapa; estorno/retificação e importação de fatura são próximos recortes.

Métricas: amostra diária por cliente em `PortalUsageSample`, origem obrigatória e ingestão administrativa. Não há coletor automático novo; até alimentar a API, a Home real mostra ausência de leitura, não dados fictícios. Dias ausentes permanecem nulos e as visualizações mostram cobertura de dias observados. Armazenamento é MiB/GiB na API real; a prévia ilustra GB decimais e não define a unidade contratual.

Chamados reutilizam criação idempotente, mensagens, estados e histórico existentes. Consulta por ID mantém filtro obrigatório da empresa. Fontes internas, custos e notas privadas não são projetados na Home pública.

As tabelas novas são `expense` e `portalusagesample`, criadas pela migration PostgreSQL `20261026102348_Issue026`, autorizada pelo mantenedor. Migration, designer, snapshot e SQL entre Issue025Remediation e Issue026 revisados: apenas duas tabelas, sete índices e duas FKs RESTRICT; sem alterações destrutivas em objetos existentes. BOM UTF-8 e CRLF preservados. Modelo WriteDbContext sem mudanças pendentes. O startup aplicará a migration quando o mantenedor publicar a API. Não executado database update. Demais provedores não contemplados para migration nesta entrega. Nenhum commit, push ou deploy realizado.

### Validação

Build da API e do provedor PostgreSQL concluídos; 416 testes da API passaram, incluindo compatibilidade com a migration, idempotência, valores financeiros, lacunas de métricas e isolamento entre empresas/autoridades. Frontend: build e lint passaram, 17 E2E passaram e os cinco E2E de despesas/prévia foram repetidos após ajustes visuais. Somente avisos preexistentes de orçamento SCSS. Publicação e validação no servidor ficam com o mantenedor.

## user-feedback

```text
pode codar o backend dessas paradas ai.
uma forma de abrir o chamado e ficar nas situacoes e tals.
pode tocar o barco bora codar.
pode codar tudo ai, só n sobe nada ainda blz? nem faz commit? eu vou tomar banho n estarei no PC pode codar com autorizacao n precisa me perguntar.
```

```text
na parte portal n teremos modais, clicou em algum detail vai navegar para aquela tela detalhes e tals, gerar breadcrumb etc
tudo tem que gerar links, ser navegavel
```

```text
pode criar a migration com nome Issue026
eu já subo no servidor dai
```
