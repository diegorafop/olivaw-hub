# Limpeza de caches sem comprometer recuperação

A limpeza é uma manutenção específica, não uma resposta automática a qualquer alerta de disco. Exigir aprovação do operador sobre alvo, categoria de cache, idade mínima, limite de espaço, prévia de candidatos e impacto esperado em novos builds. Aplicar o gate de saúde de [remediation.md](remediation.md) antes e depois. Se qualquer container estiver parado ou sem saúde confirmada, interromper a limpeza e registrar Requer intervenção.

## Preservação obrigatória

- Nunca remover containers parados, inclusive históricos e rollback. Nunca iniciá-los para liberar a limpeza.
- Preservar volumes, bancos, filas, redes, ACME, certificados, chaves, backups e imagens necessárias à recuperação. Não executar `down -v`, `container prune`, `volume prune`, `network prune` ou `system prune`.
- O `docker system prune` inclui containers parados e redes, portanto não atende à decisão do mantenedor mesmo sem `--volumes`. A opção `-a` amplia a remoção de imagens; não usá-la para “organizar” a VM.
- Não apagar `/var/lib/docker`, arquivos abertos de logs, caches de aplicações ou diretórios arbitrários. Rotação de logs é um procedimento independente de apagar dados.

## Planejar uma limpeza restrita

Investigar somente leitura com ferramentas disponíveis, como `docker system df`, e separar cache de build de imagens/volumes. Se o build estiver ativo ou não for possível provar o escopo do cache, adiar. Para um plano apenas de cache de build, conferir a versão local e considerar `docker builder prune` com idade e espaço reservado explícitos; esse comando não deve ser ampliado para uma limpeza genérica. A prévia e o comando precisam ser compatíveis com o builder utilizado; não inventar suporte a dry-run.

No executor atual do Baley, limpeza de cache ainda não é procedimento catalogado: encaminhar para intervenção em vez de executar comandos gerados pela IA. A skill pode orientar a investigação e preparar um plano manual revisável. Habilitar uma limpeza automática futura exige implementar e testar um procedimento específico, os gates e o contrato de aprovação; a aprovação genérica de um achado não libera prune.

Fontes oficiais: [escopo de system prune](https://docs.docker.com/reference/cli/docker/system/prune/) e [builder prune](https://docs.docker.com/reference/cli/docker/builder/prune/).
