# 08a — Base de custos e estudo dos planos

## user-history

Como responsável pela operação da Olivaw,
quero comparar cenários de limites e funcionalidades de planos,
para definir propostas sustentáveis antes de aprovar a oferta comercial.

## acceptance-criteria

- Custos em BRL são versionados com fonte e vigência; preço desconhecido fica vazio.
- O estudo registra nome, funcionalidades, armazenamento em GB decimal e memória opcional em MiB; não gera contrato.
- Uma única versão de cada origem entra no estudo. A parcela de cada custo é explícita, sem rateio igual automático ou medição presumida.
- O cálculo separa custo atribuído, saldo das origens, suporte, reserva e margem sobre o preço; margem deve ser menor que 100%.
- Qualquer preço pendente impede sugerir uma mensalidade final. Salvar mantém as referências e o resultado daquela versão.
- Cenários podem ser usados como base de novos estudos, preservando o anterior e permitindo comparar planos.
- Limites são hipóteses e não comprovam capacidade livre, isolamento ou oferta vigente; 2 GB é exemplo informado pelo mantenedor.

## ai-prompt

### Entrega implementada

PlanFeature; `/plans/costs`, `/plans/estimate` e `/plans/studies`. Fórmula: `(custo atribuído + horas × custo/hora) × (1 + reserva) ÷ (1 − margem)`. Tributos e outras despesas devem ser incluídos explicitamente. Valores iniciais zero são hipóteses revisáveis, sem preços AWS presumidos. Duas casas para moeda/percentuais, três para limites. Consultas de custos e linhas usam lotes limitados, sem uma consulta por origem.

### Evolução restante

Integração AWS, comparação de capacidade real, catálogo comercial aprovado e franquias continuam planejados. A API também prepara condições aprovadas por cliente em `/plans/conditions`, com referência do aceite e vigência, sem vinculá-las automaticamente a estudos. A interface de condições na ficha do cliente ainda não foi entregue.

Preserve o [planejamento da feature](../discovery/painel-olivaw/08-planos-e-condicoes.md) e os limites deste recorte. Não execute etapas futuras por estarem mencionadas no registro original.

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

a calculadora é um estudo talvez para gente chegar em um custo por plano e features por plano, ex bronze ate 2gb armazenamento e tals
```
