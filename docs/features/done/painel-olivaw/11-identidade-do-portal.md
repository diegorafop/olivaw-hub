# 11 — Identidade e vínculos do portal do cliente

## user-history

Como responsável pela operação da Olivaw,
quero contar com identidade e vínculos do portal do cliente,
para criar a fronteira de acesso do portal de relacionamento antes de apresentar dados de clientes..

## acceptance-criteria

- Cliente não acessa outra empresa por URL, filtro, corpo, exportação ou assinatura de atualização.
- Revogação e troca de vínculo produzem comportamento testado; ausência de permissão é negada no servidor.
- JWT de um aplicativo de cliente não concede acesso ao portal nem à operação interna.
- Convites não expõem dados de terceiros e possuem validade e destinatário definidos.

## ai-prompt

### Entrega e validação local

Concluída em 24/10/2026 por Codex, na branch `feature/issue020-inventario-recursos`, worktree atual.

Autoridade própria no mesmo host, reutilizando Olivaw Identity com chave, emissor, audiência, esquema JWT, tabelas e histórico de migrations independentes. Convites de 48 horas com hash, destinatário e uso único; vínculos múltiplos, papéis Consulta/Solicitações/Gestão, revogação, recuperação administrativa e auditoria. Sem root do portal ou acesso derivado de contas técnicas. A API verifica o vínculo atual em cada operação. Testes HTTP comprovam separação dos tokens internos/portal, negação entre empresas e revogação imediata; testes de convite e preservação exata de senha também passaram.

### Banco e publicação

Migrations PostgreSQL `Issue020Panel` e `Issue020PortalIdentity` geradas e SQL revisado, sem alterar a `Issue020` publicada. Modelos de ambos os contextos sem diferenças pendentes. Suíte da API: 359 testes aprovados; Angular: 33; E2E do lote administrativo: 29; portal: 2. Build Release da API e build/lint Angular verificados. SQL Server e Oracle não receberam migrations.

Não houve conexão a banco real, execução de SQL, integração Git Flow, push ou publicação. O startup da nova imagem aplicará as duas migrations automaticamente. O acesso externo do portal exige `PortalIdentity:Issuer`, `Audience`, `SigningKey` e `TokenMinutes` independentes da autoridade interna; sem configuração válida, o login retorna indisponibilidade. Conferência no ambiente permanece pendente.

### Referências

Veja [operação interna](../../../operacao-interna.md) e [comandos da API](../../../../ms-olivaw-api/README.md). A numeração organiza funcionalidades, sem criar issues adicionais. O feedback abaixo preserva o planejamento original e não redefine o estado atual.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
# 11 — Identidade e vínculos do portal do cliente

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../../discovery/painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Criar a fronteira de acesso do portal de relacionamento antes de apresentar dados de clientes.

## Dependências

[Clientes](../03-cadastro-de-clientes.md). Decisão arquitetural explícita sobre autoridade externa; a preferência é autoridade própria do portal reutilizando Olivaw Identity, separada da administração interna.

## Escopo da entrega

- Definir emissor, audiência, segredos e responsabilidade do host do portal sem reutilizar identidades dos aplicativos dos clientes.
- Convite/acesso, vínculos usuário–empresa, papéis mínimos e revogação, com fluxo a definir.
- Validar no servidor acesso a cliente, projeto e sistema em consultas, escritas, arquivos e canais de atualização.
- Troca de empresa somente entre vínculos autorizados. Não confiar em um clientId enviado pelo navegador.
- Preservar validação local de JWT nos consumidores; contas técnicas de deployment não se tornam usuários do portal.

## Fora desta entrega

- Dashboard do cliente, autoatendimento, login universal de todos os aplicativos e cópia de secrets.
- Gestão das contas internas dos sistemas de clientes, como a administração da LTMJ.

## Critérios de aceite

- Cliente não acessa outra empresa por URL, filtro, corpo, exportação ou assinatura de atualização.
- Revogação e troca de vínculo produzem comportamento testado; ausência de permissão é negada no servidor.
- JWT de um aplicativo de cliente não concede acesso ao portal nem à operação interna.
- Convites não expõem dados de terceiros e possuem validade e destinatário definidos.

## Decisões antes de implementar

- Autoridade/host e contrato de API, convite, recuperação de acesso, papéis e revogação de sessões.
- Vínculos múltiplos, responsável por conceder acesso e trilha de auditoria.

## Perfis iniciais propostos em 22/10/2026

- **Root:** acesso global da operação Olivaw, reservado ao mantenedor e à Elisiane conforme informado. Não depende de permissões atribuídas a access profiles; autenticação e auditoria continuam obrigatórias.
- **Administrador Olivaw:** perfil interno com permissões explícitas para administrar clientes, serviços, recursos e acompanhamento. Escopo global da operação; ações remotas e comerciais seguem as autorizações próprias de cada fluxo.
- **Gestor da Empresa:** perfil externo vinculado explicitamente a empresas. Consulta seus dados, serviços, recursos, disponibilidade e métricas quando disponíveis; pode criar e acompanhar solicitações. Não administra infraestrutura nem altera diretamente plano, capacidade ou dados de outras empresas.

Os dois nomes de access profiles são uma proposta, ainda sem criação ou seed. Perfil define ações; vínculo define quais empresas podem ser acessadas. Não modelar isolamento apenas como uma hierarquia em que um perfil herda dados de outro. Usuário externo sem vínculo válido não recebe acesso a empresa alguma. Tráfego e disponibilidade dependem de fontes reais das features 05 e 07.

A possibilidade de login do cliente na plataforma foi solicitada; emissor, audiência e host continuam como decisões arquiteturais pendentes. Isso não autoriza reutilizar tokens dos aplicativos dos clientes.
```
