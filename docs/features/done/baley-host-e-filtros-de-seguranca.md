# Baley — Auditoria do host e leitura visual dos relatórios

## user-history

Como operador do hub Olivaw,
quero consultar riscos por gravidade e auditar também o servidor,
para priorizar correções sem confundir exposição, tráfego suspeito e comprometimento confirmado.

## acceptance-criteria

- A lista mostra tags de resultado Danger, Warning, Safe ou indisponível, com contagens por gravidade e indicação independente de coleta parcial.
- Safe é restrito a relatório compatível, concluído e sem achados; falha de coleta nunca vira Safe.
- Os detalhes têm filtros com contagens, estado selecionado acessível, recuperação de Todos e preservação de gravidades desconhecidas.
- Leitura da IA, evidências, cobertura e dados técnicos são expansíveis; recomendação e prioridade ficam visíveis nos cards.
- A auditoria classifica listeners inesperados, serviços sensíveis, permissões de SSH/socket, firewall INPUT IPv4, proteções de links e metadados EC2 sem token.
- Listener nas interfaces do host não é apresentado como prova de alcance pela internet. Security Groups e firewall IPv6 não são afirmados como verificados.
- A auditoria é somente leitura e não altera firewall, containers ou o proxy de produção.

## ai-prompt

Entrega implementada na issue #25, branch `feature/issue025-baley-agent`, responsável Codex, worktree do monorepo. Evolução da [base Baley e relatórios](agentes-python-baley-e-relatorios.md). Sem alteração da API ou schema relacional; campos novos seguem em RawData.

Na UI administrativa, as contagens agrupam critical/high em Danger, medium em Warning e preservam níveis baixo, informativo e desconhecido. Filtro de gravidade atua localmente sobre o relatório inteiro e mantém as contagens globais; filtros sem achados ficam desabilitados. Safe é resultado agregado, não um achado inventado a partir da ausência de dados. Guards e autorização da API permanecem preservados.

O host é coletado por SSH verificado: listeners TCP/UDP IPv4/IPv6 estruturados e limitados, política/quantidade de regras da cadeia INPUT IPv4, metadados de permissões, controles `fs.protected_hardlinks`/`fs.protected_symlinks` e status de uma requisição sem token ao índice IMDS. Não ler IAM credentials, user-data ou corpos de metadados. Portas TCP previstas inicialmente 22/80/443 são configuráveis no setting; a lista não muda regras da máquina. Não tratar loopback como binding público. Sensibilidade do serviço e confirmação de acesso externo são informações diferentes.

A [restrição do socket Traefik](../discovery/restricao-socket-docker-traefik.md) continua em discovery. O socket é usado pelo provider de descoberta das rotas; o executor de deploy precisa de acesso independente. Não implantar proxy nem esconder o risco do componente que ainda precisar montar o socket.

Validações: 24 testes Python do Baley aprovados, quatro testes de classificação UI e cinco E2E, incluindo filtros, contagens, expansão de texto, autorização, breadcrumb, erro e celular. Build e lint Angular aprovados; somente warnings SCSS preexistentes fora dos arquivos alterados. Coleta real ampliada leu 11 containers e 353 eventos; novas leituras do host responderam, sem remediação. Nenhum teste conecta banco real. JSONs e credenciais locais continuam ignorados e privados.

O supervisor local foi reiniciado para carregar as regras novas, mantendo seis horas entre ciclos. O status anterior confirmou três entregas à API, fila zerada; nova execução com os controles ampliados confirmou entrega de um relatório, com fila zerada. Publicação da UI atualizada continua com o mantenedor; o agente não implantou frontend ou infraestrutura. Mover para done indica validação local, não publicação dessa revisão visual.

## user-feedback

```text
estou vendo a UI por si só tem muito texto a telha de detalhes do baley. podemos pensar em uma especie de tags para os cards da lista de verificacoes "Danger" "Warning" "Safe" etc. a lista em si poderia fazer mais informacoes como 1 elevado, 2 warning, etc.

da uma melhorada na UI.

na tela detalhes os cards ficaram bons mas da uma pensada com filtros acima pelo nivel de falha de segurança e tals.

o baley n deve achar orobleas somente em containers, mas no servidor em si, tbm deve olhar outras falhas de segurança ,porta aberta etc etc etc.

esse aqui: Prioridade alta: revisar a necessidade de acesso de /ms-traefik-pr ao socket Docker e planejar restrição às operações mínimas necessárias, considerando proxy de socket. A montagem somente leitura não restringe as operações da API Docker.

é do nosso deploy né? o que poderiamos melhorar?
```
