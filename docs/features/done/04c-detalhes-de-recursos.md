# 04c — Detalhes técnicos, renovação e backup dos recursos

## user-history

Como responsável pela operação da Olivaw,
quero registrar detalhes técnicos e datas relevantes de cada tipo de recurso,
para conhecer sua configuração declarada e planejar os cuidados necessários.

## acceptance-criteria

- Recursos compatíveis permitem tecnologia e versão; versão preenchida exige tecnologia.
- Banco lógico, namespace de mensageria/cache, subdomínio e registro DNS permitem identificação lógica, sem presumir isolamento.
- Domínio permite data de renovação e certificado permite data de validade, sem execução automática.
- Backup permite intervalo previsto em horas e retenção prevista em dias, inteiros positivos; campos desconhecidos permanecem vazios.
- API e domínio rejeitam detalhes incompatíveis com o tipo. A troca de tipo no formulário limpa detalhes incompatíveis, com aviso visível.
- Consulta e histórico preservam os novos campos; datas de calendário são transmitidas em UTC à meia-noite e apresentadas sem deslocamento por fuso.
- Mapeamento mantém propriedades tipadas no recurso e nas revisões para PostgreSQL e SQL Server.
- Publicação depende de migration solicitada separadamente e validação do modelo atualizado.

## ai-prompt

Desenvolvimento concluído e aceito pelo mantenedor nesta sessão; movido para done a pedido explícito. Integração em develop e publicação continuam pendentes; migration PostgreSQL gerada em 24/10/2026. Recorte da [visão da feature 04](../discovery/painel-olivaw/04-inventario-de-recursos.md), sobre a [base 04b](04b-inventario-de-recursos.md).

Responsável: agente Codex desta sessão. Branch existente: `feature/issue020-inventario-recursos`, issue identificada pelo branch: #20. Diretório de trabalho: `/home/makina/projects/olivaw`; execução sequencial nesta sessão, sem agentes concorrentes. Alterações preexistentes de documentação e readiness foram preservadas. Não houve criação de outra issue, commit ou integração.

### Escopo implementado

- `Technology` (80 caracteres), `TechnologyVersion` (40) e `LogicalName` (120).
- `RenewalAt` para domínio e `CertificateExpiresAt` para certificado: opcionais, datas UTC sem horário.
- `BackupIntervalHours` de 1 a 87.600 e `BackupRetentionDays` de 1 a 36.500: limites cadastrais, não recomendações operacionais.
- Tecnologia disponível para computação, instância/banco lógico, broker, instância de cache, proxy, observabilidade, registro de imagens e serviço externo.
- Identificação lógica disponível para banco, namespace de mensagens/cache, subdomínio e registro DNS.
- Contratos HTTP aditivos; campos omitidos preservam compatibilidade de entrada com clientes antigos. Atualizações continuam representando o formulário completo, seguindo o contrato anterior.
- Domínio, validators de criação/atualização, conversões, resposta por ID, snapshots, mappings e formulário Angular atualizados.

### Dependências e limites

Arquivos compartilhados: contratos de inventário, `InventoryRules`, `InventoryExtensions` e editor/API Angular de recursos. Coordenar alterações nesses arquivos antes de outro trabalho concorrente.

A base já registra histórico dos vínculos e alocações; este recorte acrescenta o histórico dos detalhes. Refinamentos adicionais por tipo, restauração verificada, medição e financeiro permanecem no planejamento e exigem escopo próprio. Não houve coleta remota, acesso a banco real, cadastro automático de clientes, geração de migration, finish, push ou publicação.

### Schema e publicação — registro da implementação inicial

**Publicação bloqueada por migration pendente.** As propriedades adicionam colunas em `inventoryresource` e `inventoryresourcechange`. O teste `PostgreSqlMigrationTest` detecta a divergência em relação ao snapshot publicado; ele foi preservado e continua falhando até a geração autorizada da migration. O startup aplica migrations e rejeita modelo pendente; não publicar esta versão sem resolver essa etapa.

Nenhuma migration ou snapshot antigo foi alterado. PostgreSQL e SQL Server têm o modelo validado em memória, sem conexão. A geração de migrations não foi contemplada para nenhum provedor; Oracle continua fora do suporte validado.

### Verificações reproduzíveis

```bash
dotnet build ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj --no-restore -m:1
dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj --no-build --no-restore -m:1
npm --prefix ms-olivaw-ng run build
npm --prefix ms-olivaw-ng run lint
npm --prefix ms-olivaw-ng run e2e -- e2e/admin-inventory.spec.ts
```

Build da API concluído. Suíte da API: 277 testes passaram e um falhou por migration PostgreSQL pendente. O build Angular concluiu com avisos de orçamento de estilos em arquivos preexistentes. Lint concluído sem erros; 10 cenários E2E do inventário passaram, incluindo desktop, celular, permissões, conflitos, vínculos e persistência de datas UTC.

### Fechamento do pacote em 24/10/2026

O bloqueio PostgreSQL por migration foi resolvido pela `Issue020Panel`, gerada mediante autorização explícita e revisada junto ao SQL. A identidade do portal recebeu migration própria `Issue020PortalIdentity`. A `Issue020` publicada foi preservada; snapshots atuais foram atualizados. Ambos os contextos estão sem diferenças pendentes e os 359 testes da API passaram. Os resultados e impedimentos anteriores acima são registros históricos desta implementação.

Build Release sem erros ou avisos, Angular build/lint e 33 testes unitários aprovados; 31 E2E do lote atual passaram. SQL Server e Oracle não receberam migrations. Nenhum SQL foi aplicado; publicação e smoke test continuam pendentes. O startup da nova imagem aplicará as migrations. Veja [comandos e publicação](../../../ms-olivaw-api/README.md#pacote-do-painel-e-portal--issue020panel).

## user-feedback

```text
vamos para 04 inventario recursos, pode inciar ai
```
