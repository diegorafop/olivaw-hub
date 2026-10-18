# Diretrizes — Provedor PostgreSql

Estas regras complementam o [guia do template](../../AGENTS.md). Este projeto pertence à API consumidora e mantém os artefatos específicos de PostgreSql; as regras gerais de persistência estão em [Dados](../Owh.Limpo.Dados/AGENTS.md).

## Responsabilidade

- Mantenha aqui somente código e migrations deste provedor. Não misture artefatos de outro banco nem mova migrations para core ou identidade.
- O contexto e os mapeamentos compartilhados são compostos em `Owh.Limpo.Dados`. Entidades de identidade são importadas da biblioteca, sem cópias locais.
- Clientes gerados mantêm histórico de migrations próprio. Alterar uma biblioteca não aplica mudanças automaticamente aos bancos dos clientes.
- Não transporte migrations ou seeds do sistema anterior. O usuário root é criado pelo bootstrap configurável depois da preparação do schema.

## Mudanças e validação

Gere ou altere arquivos de migrations mediante solicitação do mantenedor. A aplicação das migrations existentes é automática em todo startup e não exige nova habilitação. Revise alterações destrutivas, constraints, dados existentes e o provedor de destino antes da execução. Compile o projeto e o host consumidor quando modificar integração ou dependências.

Testes SQLite não comprovam compatibilidade específica com PostgreSql. Não conecte a bancos reais para validar documentação ou como efeito colateral de uma feature. Registre os testes e limitações observados.
