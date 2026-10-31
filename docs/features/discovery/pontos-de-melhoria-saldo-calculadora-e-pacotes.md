# Saldo, calculadora e pacotes de pontos de melhoria

## user-history

Como operador, quero estimar e acompanhar os pontos de melhoria por cliente e ciclo, para administrar a evolução contratada sem comprometer a rentabilidade.

## acceptance-criteria

- Projetar saldo por assinatura e ciclo mensal, sem acumulação: Bronze 1, Prata 4, Ouro 12 e Diamante 36 na configuração base.
- Preservar versão do plano e aprovação da estimativa, impedindo consumo duplicado e concorrência entre solicitações.
- Definir reserva, consumo após aceite, cancelamento e devolução sem cobrar correções de defeitos.
- Estudar estimativa interna baseada em complexidade, esforço e testes, com explicação simples ao cliente.
- Estudar pacotes adicionais com preço e validade explícitos, antes de implementar venda ou cobrança.

## ai-prompt

Discovery: não implementado. A apresentação comercial está em [planos públicos](../done/planos-publicos-oferta-e-pontos-de-melhoria.md). Não confundir a franquia apresentada com saldo operacional existente.

Antes de desenvolver, definir ciclo, regras de aceite e disputas, trilha de auditoria, autorização por cliente e idempotência. Usar APIs protegidas; não permitir que cliente altere diretamente o próprio saldo. Definir preços e estimativas com dados reais de trabalho, incluindo análise, testes e comunicação. Não anunciar pontos comerciais como pontos de função formais.

Pacotes e calculadora são evolução futura, sem implementação de IA nesta etapa.

## user-feedback

```text
ai coloque alguns pontos disponiveis pré contratrados por cada plano.
e no futuro teriamos uma calculadora de pontos (eu to pensando em pontos de funcao) mas sei que é complexo de explicar isso para leigos, mas internamente para n perder dinheiro mediriamos dessa forma sacou?

e no futuro permitiriamos o cliente comprar pacotes de pontos para ter suas novas funcionalidades.

isso, pontos n acumulam o prato talvez uns 4 pontos só.
no ouro uns 12 ta otimo, no diamante 36
```
