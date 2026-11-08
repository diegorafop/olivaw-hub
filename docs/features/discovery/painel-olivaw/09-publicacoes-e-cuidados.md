# 09 — Histórico de publicações, backups e cuidados

## user-history

Como responsável pela operação da Olivaw,
quero contar com histórico de publicações, backups e cuidados,
para concentrar evidências de entrega e proteção dos sistemas sem transformar o painel em um terminal remoto..

## acceptance-criteria

- Operador vê somente jobs e sistemas permitidos; frontend não recebe credenciais DevOps.
- Falha ou ausência de evidência aparece claramente; “backup concluído” não é “restauração testada”.
- Logs não expõem chaves, dados de clientes ou detalhes para perfis não autorizados.

## ai-prompt

Primeiro recorte implementado: [09a — Evidências de publicações, backups e restauração](../../done/09a-evidencias-de-cuidados.md). O restante desta visão permanece no planejamento, conforme as exclusões e pendências do recorte concluído.


Em descoberta; decisões e dependências ainda precisam ser resolvidas antes de mover para ready.

Trabalhe somente no recorte autorizado desta feature. Confira os AGENTS.md aplicáveis, a implementação atual e o registro completo em `user-feedback`. Preserve os limites e as evidências históricas; registre testes e pendências. Migrations, finish, push e publicação dependem de autorização explícita.

### Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Concentrar evidências de entrega e proteção dos sistemas sem transformar o painel em um terminal remoto.

### Dependências

[Inventário](04-inventario-de-recursos.md). Reaproveitar a API DevOps existente com suas permissões; fontes de backups precisam ser definidas separadamente.

### Escopo da entrega

- Consultar jobs autorizados, versão, ambiente, resultado de publicação e vínculo com o sistema.
- Distinguir job bem-sucedido de saúde funcional verificada após publicar; relacionar à [saúde](05-coleta-e-saude.md) quando disponível.
- Registrar última execução de backup, resultado, retenção informada e teste de restauração como evidências diferentes.
- Acompanhar vencimentos de domínio/certificado quando houver fonte confirmada.
- Histórico técnico útil na ficha e últimos acontecimentos do dashboard, com links protegidos e logs sem segredos.

### Fora desta entrega

- Botões de deploy, limpeza, rollback ou restauração. Ações executáveis exigem feature e autorização próprias.
- Criar rotinas remotas de backup ou certificar recuperação apenas porque existe um arquivo.

### Decisões antes de implementar

- Fontes existentes, retenção, permissões, correlação de jobs e definição de evidência de backup/restauração.
- Qual integração somente de leitura é prioritária, sem exigir todos os cuidados no primeiro incremento.

## user-feedback

Registro original preservado; somente caminhos de links foram ajustados à reorganização. Não representa uma nova solicitação de execução.

```markdown
# 09 — Histórico de publicações, backups e cuidados

## Estado e objetivo

Planejada; não implementada por este documento. Parte da [épica de administração e portal](../painel-administrativo-e-portal-do-cliente.md). A numeração organiza o planejamento, sem criar issues ou autorizar publicação, migrations e operações remotas.

Concentrar evidências de entrega e proteção dos sistemas sem transformar o painel em um terminal remoto.

## Dependências

[Inventário](04-inventario-de-recursos.md). Reaproveitar a API DevOps existente com suas permissões; fontes de backups precisam ser definidas separadamente.

## Escopo da entrega

- Consultar jobs autorizados, versão, ambiente, resultado de publicação e vínculo com o sistema.
- Distinguir job bem-sucedido de saúde funcional verificada após publicar; relacionar à [saúde](05-coleta-e-saude.md) quando disponível.
- Registrar última execução de backup, resultado, retenção informada e teste de restauração como evidências diferentes.
- Acompanhar vencimentos de domínio/certificado quando houver fonte confirmada.
- Histórico técnico útil na ficha e últimos acontecimentos do dashboard, com links protegidos e logs sem segredos.

## Fora desta entrega

- Botões de deploy, limpeza, rollback ou restauração. Ações executáveis exigem feature e autorização próprias.
- Criar rotinas remotas de backup ou certificar recuperação apenas porque existe um arquivo.

## Critérios de aceite

- Operador vê somente jobs e sistemas permitidos; frontend não recebe credenciais DevOps.
- Falha ou ausência de evidência aparece claramente; “backup concluído” não é “restauração testada”.
- Logs não expõem chaves, dados de clientes ou detalhes para perfis não autorizados.

## Decisões antes de implementar

- Fontes existentes, retenção, permissões, correlação de jobs e definição de evidência de backup/restauração.
- Qual integração somente de leitura é prioritária, sem exigir todos os cuidados no primeiro incremento.
```
