# Planos públicos, oferta de criação e pontos de melhoria

## user-history

Como cliente potencial, quero entender os planos e as condições de evolução, para contratar uma primeira entrega adequada ao meu negócio.

## acceptance-criteria

- Catálogo público em `/planos`, com detalhes navegáveis por plano, breadcrumb e interesse preservado em `/projeto?plano=bronze`.
- Bronze por R$ 49 mensais; demais valores sob proposta. Prata com 5 GB totais e duas contas administrativas do site do cliente.
- Franquias mensais: Bronze 1, Prata 4, Ouro 12 e Diamante 36 pontos, sem acumulação.
- Estimativa informada e aprovada antes de executar; consumo confirmado após entrega e aceite. Defeitos do escopo entregue não consomem pontos.
- Bronze atende melhorias simples. Exemplos não são preços universais: uma foto pronta em bloco existente pode custar 1 ponto; uma seção reaproveitada de evento pode custar 3, após análise.
- Criação e implantação incluídas em novas assinaturas com prazo superior a 12 meses: Bronze Essencial; Prata Essencial ou Intermediário. Escopo e enquadramento precisam constar na proposta aprovada.
- Explicar orçamento proporcional ao escopo e esforço, etapas, testes, aprovação e entrega por MVP.
- Não consultar APIs administrativas na página pública nem divulgar composição interna de custos.

## ai-prompt

Implementação local concluída na issue 026, branch `feature/issue026-admin-client`, pelo Codex, no worktree principal. Publicação, commit e push ficam com o mantenedor.

Os conteúdos comerciais compartilhados ficam em `ms-olivaw-ng/src/app/features/public/plans/plan-offerings.ts`. São usados no catálogo público, na jornada de projeto e nas condições sugeridas de novas versões administrativas. Versões publicadas e contratos existentes não são alterados automaticamente. Não há mudança de modelo nem migration nesta entrega.

5 GB representam 5.000.000.000 bytes, aproximadamente 4,657 GiB no dimensionamento técnico. As duas contas pertencem à administração do site contratado, sem conceder acesso ao administrativo Olivaw. Vídeos não significam streaming ou tráfego ilimitados.

Pontos são créditos comerciais de melhoria, não uma medição formal de pontos de função. A franquia considera o ciclo mensal e não acumula. Criação inicial incluída na oferta não consome pontos; trabalho fora do escopo ou acima da franquia depende de orçamento e aprovação. Diamante tem 36 pontos na configuração base, com condições específicas no contrato personalizado.

A entrega apresenta regras e sugestões contratuais: ainda não implementa saldo operacional, reserva ou débito automático de pontos. Calculadora e pacotes adicionais permanecem em [discovery](../discovery/pontos-de-melhoria-saldo-calculadora-e-pacotes.md).

A oferta não elimina os custos internos de criação: registrar aquisição do cliente e amortização no estudo de rentabilidade. Medir análise, execução, testes, comunicação e entrega antes de estabelecer equivalência interna de esforço por ponto. Bronze é uma estratégia inicial de aquisição e indicações; não pressupor lucro ou ausência garantida de prejuízo.

Validação: build e lint Angular; testes E2E do catálogo público desktop/mobile, navegação, interesse, jornada de projeto e despesas. Rotas públicas pré-renderizadas com metadados e canonical próprios. Avisos de orçamento SCSS preexistentes permanecem. Sem deploy ou acesso a banco real.

## user-feedback

```text
isso, ai talvez 1 ponto para o bronze

o bronze é o bem basico, o cara pode pedir coisas simples mesmp

claro nesse começo n vamos ganhar dinheiro de verdade com o bronze apenas expandir a carteira de clientes e que eles nos indiquem a outros clientes

isso, pontos n acumulam o prato talvez uns 4 pontos só.
no ouro uns 12 ta otimo, no diamante 36
```
