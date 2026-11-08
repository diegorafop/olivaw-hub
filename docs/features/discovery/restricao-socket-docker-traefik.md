# Traefik — Restrição do acesso ao socket Docker

## user-history

Como operador do hub Olivaw,
quero limitar o acesso do Traefik à API Docker,
para reduzir o impacto de um comprometimento do proxy preservando os sites e o deploy.

## acceptance-criteria

- Traefik descobre as rotas por um proxy dedicado de acesso mínimo, sem montar diretamente o socket.
- O proxy fica em rede privada exclusiva, sem portas publicadas no host; bloquear verbos de escrita e permitir somente endpoints comprovadamente necessários ao provider.
- Descoberta inicial, eventos, recriação de um serviço e atualização de rotas são testados com a versão efetiva do Traefik e Docker antes da implantação.
- Executor de deploy mantém acesso separado; não enviar suas operações de escrita ao proxy de leitura do Traefik.
- Volumes, redes das aplicações, nomes dos serviços, identidade RabbitMQ e ACME permanecem preservados.
- RAM e CPU adicionais são avaliadas para a VM pequena; o proxy ainda tem acesso ao daemon e não elimina todo o risco.

## ai-prompt

Atualização local em 30/10/2026: Compose opcional `ms-infrastructure-app/docker-compose.socket-proxy.yml` e `socket-proxy.cfg` preparados com HAProxy 3.2.25, rede interna exclusiva sem portas publicadas, GET limitado a versão, ping, lista/inspect de containers e eventos; HEAD somente ping. Chamadas derivadas do código do provider Traefik v3.0.0. Traefik sem montagem do socket; deploy mantém canal independente. Limites 64 MiB, 0,10 vCPU, 32 processos/conexões, filesystem somente leitura, sem capabilities, logs limitados. Volumes, redes externas, RabbitMQ e ACME preservados. O socket continua acessível ao processo do proxy e inspect revela metadados: risco residual documentado. Ainda falta teste de runtime isolado, memória e disponibilidade/digest da imagem; não marcar como pronto para produção. O Compose padrão permanece preservado. Nenhum container reiniciado e nenhum deploy executado.


Proposta em discovery, sem autorização de alteração do servidor nesta entrega. O alerta Baley de acesso de `/ms-traefik-pr` ao socket decorre do provider Docker do Traefik, responsável pela descoberta dos sites. Não é somente o executor de deploy. A montagem do socket `:ro` não transforma a API do daemon em somente leitura.

A [documentação oficial Traefik v3.0](https://doc.traefik.io/traefik/v3.0/providers/docker/#docker-api-access) recomenda autorização/filtragem e rede privada. Conferir versão efetiva e selecionar implementação mantida, com imagem estável fixada. Não escolher nem atualizar imagem automaticamente só por existir no feed do Baley.

Antes da implementação, levantar chamadas do provider e definir allowlist de endpoints e métodos, inclusive eventos. Não usar liberação genérica de todos os GET: algumas operações de leitura expõem metadados sensíveis. A allowlist também não protege contra comprometimento do próprio proxy/socket. O serviço que mantém o socket continuará sendo registrado como ponto sensível pela auditoria; não ocultar o risco só porque mudou de container.

Preparar Compose revisável, configuração de proxy, validação isolada e rollback. Medir custo na VM, preservar volumes existentes e coordenar uma janela para recriar apenas os componentes necessários. Não aplicar firewall, trocar imagens, reiniciar serviços ou executar Compose em produção por consequência desta análise. Ver [infraestrutura compartilhada](../../../ms-infrastructure-app/README.md).

## user-feedback

```text
o baley n deve achar orobleas somente em containers, mas no servidor em si, tbm deve olhar outras falhas de segurança ,porta aberta etc etc etc.

esse aqui: Prioridade alta: revisar a necessidade de acesso de /ms-traefik-pr ao socket Docker e planejar restrição às operações mínimas necessárias, considerando proxy de socket. A montagem somente leitura não restringe as operações da API Docker.

é do nosso deploy né? o que poderiamos melhorar?
```
