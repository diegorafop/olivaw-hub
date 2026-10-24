# 06a — Registro e acompanhamento interno de incidentes

## user-history

Como responsável pela operação da Olivaw,
quero acompanhar uma ocorrência por recurso de origem,
para registrar impacto, responsável, próxima ação e recuperação.

## acceptance-criteria

- Um recurso tem no máximo uma ocorrência aberta, com proteção por índice único e resposta de conflito em concorrência.
- Estados Detectado, Em avaliação, Em atendimento, Em observação e Resolvido são preservados em revisões auditadas.
- Resolver exige passar por observação e registrar justificativa; reconhecer ou avaliar não resolve a ocorrência.
- Atualização com revisão antiga é rejeitada sem sobrescrever a ocorrência; o formulário conserva a edição.
- Notas e histórico são internos e protegidos por autorização root.

## ai-prompt

### Entrega implementada

IncidentFeature; `/incidents` e `/incidents/{id}/history`. O recurso é a origem canônica, evitando criar um incidente por cliente quando a dependência é compartilhada. Impacto e confirmação da recuperação são informados pelo operador.

### Evolução restante

Correlação automática de sinais, lista calculada de clientes afetados, silenciamento com expiração, manutenção, notificações e integração ao dashboard continuam no planejamento. Não há promessa de alerta externo ou atendimento permanente.

Preserve o [planejamento da feature](../discovery/painel-olivaw/06-alertas-e-incidentes.md) e os limites deste recorte. Não execute etapas futuras por estarem mencionadas no registro original.

### Execução e validação

Implementação local concluída para este recorte, com contratos, domínio, mappings, API e tela acessível pelos cards de Configuração. O planejamento restante está separado no documento de origem. Não representar este incremento como conclusão de toda a visão da feature.

Responsável: Codex desta sessão, trabalho sequencial no diretório `/home/makina/projects/olivaw`. Branch recebida: `feature/issue020-inventario-recursos`. O lote solicitado de 05 a 10 permanece sem commits ou integração; não foram inventadas issues nem atribuídos automaticamente os novos escopos à issue 020. A separação em branches por issue deve ser feita antes da integração, preservando as alterações preexistentes. Não houve agentes concorrentes.

**Schema pendente de migration: não publicar ainda.** Nenhuma migration ou snapshot publicado foi alterado. O teste de correspondência PostgreSQL deve continuar falhando enquanto o modelo não tiver migration autorizada. PostgreSQL e SQL Server são validados pelo modelo em memória; Oracle não é contemplado.

Comandos e resultados compartilhados estão no [guia das entregas 05–10](../../operacao-interna.md). Movimentação para done registra conclusão deste desenvolvimento local; não executa finish, push, integração ou implantação.

### Fechamento do pacote em 24/10/2026

O bloqueio PostgreSQL por migration foi resolvido pela `Issue020Panel`, gerada mediante autorização explícita e revisada junto ao SQL. A identidade do portal recebeu migration própria `Issue020PortalIdentity`. A `Issue020` publicada foi preservada; snapshots atuais foram atualizados. Ambos os contextos estão sem diferenças pendentes e os 359 testes da API passaram. Os resultados e impedimentos anteriores acima são registros históricos desta implementação.

Build Release sem erros ou avisos, Angular build/lint e 33 testes unitários aprovados; 31 E2E do lote atual passaram. SQL Server e Oracle não receberam migrations. Nenhum SQL foi aplicado; publicação e smoke test continuam pendentes. O startup da nova imagem aplicará as migrations. Veja [comandos e publicação](../../../ms-olivaw-api/README.md#pacote-do-painel-e-portal--issue020panel).

## user-feedback

```text
massa blz entao se terminou ja move ele para a pasta done e vamos fazer os itens 05, 06, 07, 08, 09, 10.

precisa de alguma informacao adicinal?

hj temos uma VM com aquelas condicoes de 1GB ram, pouco disoc e tals, ela abriga varios sites simples.

mesmo n sabendo os preços exatos, vamos conferir depois. mas na parte de feature de planos podemos ter uma especie de calculadora ou interface que mostra os valores calculados com uma base de valores que alimentaremos depois consultando a aws ou alimentando manualmente, enfim sugira algo para essa parte que fique pratico de usar e n seja custoso nem trabalhoso.
```
