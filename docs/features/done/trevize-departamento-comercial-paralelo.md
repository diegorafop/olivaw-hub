# Departamento comercial paralelo e modelos dos agents

## user-history

Como responsável da Olivaw,
quero iniciar os agents manualmente com modelos definidos e três especialistas comerciais paralelos,
para coletar oportunidades, abordar novos contatos e acompanhar respostas em ritmos independentes.

## acceptance-criteria

- Trevize coleta até 20 leads a cada 24h, com primeiro ciclo elegível ao iniciar e sem sobreposição da coleta.
- Primeiro contato envia no máximo uma mensagem por 30min e duas por hora móvel. O diário preserva o ritmo após reinício, incluindo tentativas incertas; respostas não consomem essa cadência.
- Acompanhamento consulta mensagens a cada 15min, drena lotes de conversas conhecidas e responde aos rascunhos de qualificação elegíveis sob política automática explícita.
- Uma coleta bloqueada não impede os especialistas de primeiro contato e acompanhamento. Não sobrepor duas execuções do mesmo especialista; parada é cooperativa.
- Silêncio não dispara lembretes. Preservar recusas, intervenção humana, cancelamento, deduplicação e encaminhamento de negociação/incerteza. Não repetir uma escrita de transporte ambígua.
- Fila inicial aceita somente prospecção Google Maps com contato público inequívoco do mesmo site e pesquisa qualificada. Dados simulados e lead de teste não entram nessa fila.
- Baley usa `gpt-6-luna` com esforço `high`; Trevize usa `gpt-6.1-sol` com esforço `low`, tanto em qualificação quanto em respostas. Aplicar por chamada, sem mudar configuração global do Codex.
- Piloto preserva limites anteriores; modo comercial privado exige autorização registrada, OAuth e ambas as chaves de envio. Início só pelo operador, sem cron/autostart.
- Não fazer commits, push, merge ou deploy. Entregar comando para iniciar os dois agents.

## ai-prompt

Responsável: Codex. Continuação da issue #31 na branch `feature/issue031-trevize-commercial-intelligence`, worktree `/home/makina/projects/olivaw`. Implementação distribuída entre configuração/modelos do Baley, especialista de primeiro contato, especialista de acompanhamento e integração do worker Trevize. Delegação foi usada após o pedido explícito de trabalho paralelo; nenhum agente delegado iniciou processos ou acessou ambientes reais.

Estado: código implementado e validado. Os dois workers reais permanecem parados; o mantenedor iniciará usando `./olivaw-agent/olivaw-agent start`. Alterações permanecem sem commit por instrução explícita do mantenedor. Sem mudanças de API, migrations ou implantação.

`Specialists` coordena três tarefas em executor de três threads. Agendas são independentes, contadas entre inícios; execução longa não se sobrepõe. Primeiro contato consulta a fila por minuto, porém o diário de transporte impõe uma tentativa a cada 30min e até duas por hora móvel. Assim, reiniciar não libera outra abordagem. Somente leads novos, outbound Google Maps, endereço público único do mesmo site e qualificação utilizável são selecionados; pesquisa de recuperação é limitada a um candidato por rodada e tem cooldown persistente de 24h.

O acompanhamento usa lotes de até 100 mensagens e continua enquanto houver fila, com parada entre registros/rascunhos. Aprovação explícita da política comercial permite enviar textos livres de qualificação, inclusive aqueles marcados `requiresReview`; o piloto e a campanha antiga não mudam seu contrato. Apenas a última mensagem válida da thread pode receber resposta. Não retomar automaticamente conversas suspensas. Envios pendentes/incertos, limitações do histórico CRM e casos fora de qualificação exigem operador; “responder todas” não autoriza inventar informação ou contornar bloqueios.

Locks são por especialista e por decisão; interpretação IA não segura o lock global Gmail. Escritas de transporte seguem serialização e reserva CRM. Registro confirmado de envio recebe recibo local para não repetir todas as escritas históricas a cada leitura. Uma queda antes do recibo conserva a repetição idempotente no CRM. O diário não é apagado para liberar contatos.

Modo privado `commercial` foi configurado na máquina autorizada com remetente `olivawhub@gmail.com`, política registrada e transporte habilitado para o próximo início manual. O teto total é de 500 tentativas/dia UTC, incluindo respostas, um limite técnico e não uma meta ou garantia de entrega. Configurações OAuth novas continuam desabilitadas no modo controlado, com teto de cinco tentativas. Limites são locais, não distribuídos entre máquinas. Os limites de histórico da API permanecem em vigor; conversa que os alcance exige revisão, sem apagar eventos.

Baley mantém ciclos de seis horas e consulta de tickets aprovados a cada 30s, sem ampliação da autorização de correção. Modelo e esforço foram fixados conforme pedido. Os identificadores e esforços foram conferidos no catálogo local e na [documentação oficial de modelos](https://learn.chatgpt.com/docs/models); o esforço é passado por `model_reasoning_effort`, conforme [configuração oficial](https://learn.chatgpt.com/docs/config-file/config-basic#reasoning-effort).

Validação: 135 testes Trevize, 36 Baley e 3 do agrupador aprovados, sem rede nos testes automatizados. Simulações manuais isoladas confirmaram chamada aceita de Luna/high e Sol 6.1/low, sem SSH, Gmail, relatórios ou início de worker. Testes verificam coleta bloqueada sem impedir os demais especialistas, agendas independentes, deduplicação/ritmo entre reinícios, fila completa de respostas, bloqueios, parada e isolamento das configurações de modelo. Status final confirmou ambos parados. Nenhum e-mail foi enviado nesta entrega.

Comandos reproduzíveis:

```bash
PYTHONPATH=olivaw-agent/ms-trevize-py/src python3 -m unittest discover -s olivaw-agent/ms-trevize-py/tests -q
PYTHONPATH=olivaw-agent/ms-baley-py/src python3 -m unittest discover -s olivaw-agent/ms-baley-py/tests -q
python3 -m unittest discover -s olivaw-agent/tests -q
./olivaw-agent/olivaw-agent start
./olivaw-agent/olivaw-agent status
./olivaw-agent/olivaw-agent stop
```

## user-feedback

```text
n faz mai commit automatico, deixa essa parte comigo ok
```

```text
ficou OTIMO.

blz vamos fechar o pacote e colocar o trevize e baley para trabalhar.

qual o round de envio de emails do trevize?
```

```text
outra coisa, eles tem que trabalhar com um modelo especifico, no começo deixa o baley com luna high e o trevize mais esperto com sol 6.1 low
```

```text
luna 6 high para baley e sol 6.1 low para trevize
```

```text
trevize vai coletar 20 leads a cada 24 horas.

vai enviar 2 emails por hora.
a cada 15 minutos ele vai ler novas mensagens e responder todas.
```

```text
portanto ele tem 3 'sub-agents' do departamento comercial da olivaw, sao 3 especialistas trabalhando em PARALELO ok
```

```text
ajustando isso me passa o command para iniciar os agents
```
