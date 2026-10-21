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
