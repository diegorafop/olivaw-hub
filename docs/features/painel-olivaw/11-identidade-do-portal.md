# 11 — Identidade e vínculos do portal do cliente

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Criar a fronteira de acesso do portal de relacionamento antes de apresentar dados de clientes.

## Dependências

[Clientes](03-cadastro-de-clientes.md). Decisão arquitetural explícita sobre autoridade externa; a preferência é autoridade própria do portal reutilizando Olivaw Identity, separada da administração interna.

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
