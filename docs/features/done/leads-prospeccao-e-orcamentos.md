# Prospecção ativa e orçamentos vinculados a Leads

## user-history

Como responsável comercial da Olivaw,
quero cadastrar oportunidades prospectadas e elaborar propostas com esforço e condições explícitos,
para oferecer desenvolvimento e continuidade com investimento compreensível.

## acceptance-criteria

- Leads aceita cadastro manual com origem, iniciativa, referência e URL, sem exigir contato ainda desconhecido.
- Registros existentes continuam legíveis e filtráveis; prospecção ativa tem identificação própria.
- Orçamento vinculado ao lead registra análise, plataforma, plugins, programação personalizada, horas por profissional, valores, custos externos e prazo.
- Opcionais ficam fora do total; reserva, desconto e parcelas têm cálculos explícitos e arredondamento em centavos.
- Comparação avulso/continuidade informa mensalidade separada e condições negociáveis; não presume plano ou desconto aprovado.
- Edição exige leads.manage, protege contra sobrescrita concorrente e não pode ser forjada no envio anônimo.
- Apresentação HTML autônoma usa identidade visual Olivaw e escapa conteúdo recebido.
- Caso 795293 tem arquivos preparados para importação após publicação, sem cadastro remoto nesta tarefa.

## ai-prompt

Desenvolvimento concluído e validado localmente em 31/10/2026. Integração em develop e publicação pendentes pelo mantenedor. Responsável: Codex. Branch observada: feature/issue026-admin-client; sem issue específica vinculada à nova entrega. Worktree: raiz do monorepo. Não associar o identificador 795293 à issue 26.

Metadados e proposta são armazenados no snapshot existente, sem alterar schema. Contratos no projeto Client; criação e edição administrativa usam a política leads.manage e revisão concorrente existente. Reenvio público preserva a proposta interna; entradas anônimas não podem fornecer aquisição ou proposta.

Frontend inclui cadastro manual/importação JSON, página de edição e apresentação por lead e exportação HTML. Não foi criado endpoint público de proposta. O HTML inclui valores comerciais, sem credenciais. Desconto e parcelamento são simulações até validação das condições do plano.

Ver [roteiro do caso 795293](../../99freelas/795293/cadastro-apos-publicacao.md). Publicação pelo mantenedor; não gerar migrations, acessar bancos reais, fazer push/finish ou cadastrar lead antes da confirmação da publicação.

### Validações executadas

- API: `dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj -m:1 --no-restore` — 456 testes aprovados, incluindo permissões HTTP, concorrência, validação e preservação da proposta em reenvios públicos.
- Contratos: `dotnet pack ms-olivaw-api/src/Owh.Olivaw.Client/Owh.Olivaw.Client.csproj -c Release --no-restore -m:1` — pacote local gerado; sem publicação de pacote.
- Frontend: `npm run build` e `npm run lint` aprovados; `npm test` com 43 testes aprovados.
- Navegador: `npm run e2e -- e2e/admin-lead-proposal.spec.ts e2e/admin-leads.spec.ts` — 12 cenários aprovados, com API simulada, desktop e celular; inspeção visual das duas apresentações realizada.
- Build mantém avisos de orçamento de CSS em componentes existentes. Não foram alteradas dependências ou configurações de build para silenciá-los.
- C# preservado em UTF-8 com BOM e CRLF; conferir diff com `git -c core.whitespace=cr-at-eol diff --check`.

### Limites da entrega

A proposta é uma página interna autenticada com exportação HTML autônoma. Não há hospedagem pública de propostas, link de aceite, envio automático, catálogo de mensalidades aplicado ou cobrança. A situação comercial é informada manualmente; mudar para aceito não cria contrato ou cliente automaticamente. Diluição em entregas mensais é descrita nas condições da proposta, sem alterar contratos dos planos.

Nenhuma migration foi gerada. Não houve acesso a banco real, publicação, contato com lead ou cadastro remoto. [HTML preliminar do orçamento 795293](../../99freelas/795293/proposta.html) pronto para revisão; cadastro condicionado à confirmação da publicação.

## user-feedback

```text
isso, o padrao de docs já está documentado nas pastas superiores n precisa repetir, pode apenas colocar algo refente a readme e agents nessa pasta orcamentos que oriente IAs generativas a fazer os orcamentos pra gente, gerando ao fim uma tabelinha hml com o design system da olivaw.

precisamos colocar o orçamento em detalhes melhores, horas do programador, analise dos requisitos, plugins, e caso n atenda programação customizada dentro da plataforma proposta que ainda n foi demonstrada.

se houver interesse em contrato continuo com a olivaw, que oferecemos todos os serviços de manutenção, gerenciamento dos recursos, banco de dados, atualizacao de pacotes, etc etc. Podemos negociar valores de implementaçao com um bom desconto e até diluir o valor em implementacoes mensais de acordo com nossos planos da olivaw.

essa tabelinha pensando BEM, poderia ser uma página nossa.
Dentro de leads colocamos apenas os clientes que chegaram até nós, mas faz sentido ter clients que fomos atrás como esse né.

faz sentido cadastrar dentro de Leads?

o que voce acha? n codifica nada ainda
```

Autorização posterior de execução:

```text
otimo eu gostei, pode codificar e já fique pronto para cadastrar esse lead depois que eu publicar a nova versao.
```
