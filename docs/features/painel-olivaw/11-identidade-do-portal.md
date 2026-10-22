# 11 — Identidade e vínculos do portal do cliente

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Criar a fronteira de acesso do portal de relacionamento antes de apresentar dados de clientes.

## Dependências

[Clientes](../done/03-cadastro-de-clientes.md). Decisão arquitetural explícita sobre autoridade externa; a preferência é autoridade própria do portal reutilizando Olivaw Identity, separada da administração interna.

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
