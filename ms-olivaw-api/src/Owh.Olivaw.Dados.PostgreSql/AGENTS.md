# Diretrizes — Provedor PostgreSql

Estas regras complementam o [guia do template](../../AGENTS.md). Este projeto pertence à API consumidora e mantém os artefatos específicos de PostgreSql; as regras gerais de persistência estão em [Dados](../Owh.Olivaw.Dados/AGENTS.md).

## Responsabilidade

- Mantenha aqui somente código e migrations deste provedor. Não misture artefatos de outro banco nem mova migrations para core ou identidade.
- O contexto e os mapeamentos compartilhados são compostos em `Owh.Olivaw.Dados`. Entidades de identidade são importadas da biblioteca, sem cópias locais.
- Clientes gerados mantêm histórico de migrations próprio. Alterar uma biblioteca não aplica mudanças automaticamente aos bancos dos clientes.
- Não transporte migrations ou seeds do sistema anterior. O usuário root é criado pelo bootstrap configurável depois da preparação do schema.

## Mudanças e validação

Gere ou altere arquivos de migrations mediante solicitação do mantenedor. A aplicação das migrations existentes é automática em todo startup e não exige nova habilitação. Revise alterações destrutivas, constraints, dados existentes e o provedor de destino antes da execução. Compile o projeto e o host consumidor quando modificar integração ou dependências.

Testes SQLite não comprovam compatibilidade específica com PostgreSql. Não conecte a bancos reais para validar documentação ou como efeito colateral de uma feature. Registre os testes e limitações observados.

## Padronização de migrations

- Gere migrations somente mediante pedido explícito; `new feature` não inclui essa etapa.
- Confira a branch atual. Use a issue em PascalCase como nome: `feature/issue013-logo-whatsapp` corresponde a `Issue013`. Se já existir uma migration com esse nome, preserve o histórico e acrescente um sufixo descritivo, como `Issue013LeadIndex`. Sem issue identificável, confirme o nome com o mantenedor.
- Gere somente para os provedores solicitados, no projeto do provedor da API consumidora. O Core não contém migrations. Não copie históricos entre serviços.
- Use a factory de design time quando disponível, sem iniciar o host nem acessar bancos reais. Compile com `-m:1`; use `--no-build` no EF somente após compilar o modelo atual. `--no-restore` pressupõe dependências restauradas.
- Revise migration, designer, snapshot e SQL entre a migration anterior e a nova. Preserve BOM UTF-8 e CRLF dos arquivos C#. Não altere migrations antigas já publicadas.
- Gerar e revisar não autoriza `database update`, conexão a bancos reais ou redeploy. O startup das APIs aplica as migrations pendentes; registre essa consequência antes da publicação.
- Documente comandos reproduzíveis no README da API e consulte os [comandos do Core](../../../ms-olivaw-core-api/README.md#migrations-dos-microservices-consumidores). Informe as validações executadas e os provedores não contemplados.
