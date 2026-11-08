# Roteiro de investigação

## Host e acesso

- Identifique SO/kernel, recursos de memória/swap/disco, tempo de coleta e limites do ambiente. Não instalar ferramentas como efeito da leitura.
- Leia políticas efetivas SSH: senha/keyboard-interactive, root, forwarding, tentativas e authorized_keys. Não despejar chave pública/privada; reporte contagem, permissões e restrições pertinentes.
- Procure candidatos a chaves por cabeçalho/metadados em diretórios autorizados, com limites e exclusões declarados. Não afirmar varredura integral quando imagens, backups, histórico ou diretórios foram excluídos.
- Diferencie arquivo de trabalho removido de chave revogada e de histórico apagado. Uma chave antiga recuperável e ainda válida demanda rotação coordenada, sem force-push automático.
- Confira listeners IPv4/IPv6, firewall e SELinux. Ausência de firewall local não prova Security Group aberto. Não ler user-data ou credenciais IAM para verificar IMDS; status da requisição sem token pode fornecer evidência limitada.

## Docker e dados

- Selecione campos de inspect: usuário, privileged/capabilities, security_opt, read-only, ports, redes, mounts, projeto Compose, image ID e retenção. Nunca imprimir Config.Env, labels de autenticação ou argumentos secretos.
- Compare portas publicadas com listeners do host; EXPOSE não é publicação. Analise acesso lateral em redes compartilhadas e dependências antes de sugerir segmentação.
- Socket Docker montado `:ro` ainda oferece acesso à API do daemon; avalie proxy de permissões mínimas. Não executar operações de daemon para provar a possibilidade de escalada.
- Confira volumes persistentes, origem ACME, identidade RabbitMQ e backup/restauração. Verifique se backups existem sem ler conteúdo de clientes nem executar restauração em produção.
- Logs stdout e logs em volume têm retenções independentes. `max-size`/`max-file` limitam espaço, não dias; conferir configuração efetiva de cada container, journal e backups. Não apagar logs durante investigação sem autorização.

## Proxy e aplicação

- Traefik: descoberta opt-in, dashboard, HTTPS, certificado, endpoints publicados, confiança em headers e acesso ao socket. Revisar flags/versionamento e limites efetivos, sem trocar imagens durante auditoria.
- Access log: preferir IP, UTC, host, método, caminho, status e roteamento; omitir Authorization, Cookie, Referer e X-Lead-Key. Tratar URLs como potencial fonte de segredo. Não atribuir autoria só por IP/User-Agent.
- Inventarie AllowAnonymous, fallback/default authorization, políticas dos controllers e middleware que responde antes da autorização. Inclua health/readiness, login/convite, escrita pública, fotos/arquivos, Swagger/OpenAPI e métricas.
- Confira isolamento por empresa/identidade no servidor; guards frontend/CORS não substituem autorização. Avalie tamanho de corpo, custo da consulta, paginação no banco, reenvio e limites de taxa.
- Atrás de proxy, validar origem real da partição do rate limiter. Nunca habilitar confiança irrestrita em X-Forwarded-For como atalho.
- Revisão de código e leitura real do endpoint são evidências diferentes. Evitar POST de negócio, login em massa, upload, disparo de deploy ou consulta de dados de clientes apenas para investigar.

## Evidências e prioridade

Registre comando/fonte sem valores secretos, horário, escopo, status e conclusão limitada. Priorize credenciais válidas expostas, possibilidade de controlar host e superfície pública antes de ajustes cosméticos. Não reduzir severidade por haver senha em arquivo permitido; registre a decisão do mantenedor e o risco remanescente sem trocar senhas fora da tarefa.
