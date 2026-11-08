# Diretrizes — Orçamentos e portfólios

Siga as [diretrizes documentais](../AGENTS.md) e o [padrão de histórias](../features/AGENTS.md), sem reproduzi-los nesta pasta. Preserve os portfólios existentes.

## Elaborar orçamento

- Trabalhe em `<identificador>/` e use o [roteiro do README](README.md). Identificador de anúncio não é issue.
- Preserve o anúncio integral. Separe fatos, hipóteses e perguntas; nunca invente plataforma, contatos, experiência ou aprovação.
- Analise requisitos, plugins mantidos, licenças e lacunas. Se plugins não atenderem, estime programação personalizada dentro da plataforma apresentada. Sem demonstração da plataforma, mantenha cenários condicionais.
- Detalhe horas por atividade e profissional: análise, programação, configuração, testes, homologação e orientação. Informe taxa assumida, reserva, inclusões, exclusões, prazo e custos externos; diferencie estimativa interna de preço aprovado.
- Consulte fontes atuais para recomendar ferramentas e registre links e data. Não trate mensalidades, licenças ou disponibilidade como garantidas.
- Prepare abordagem comercial da Olivaw, sem enviar mensagens ou cadastrar remotamente sem autorização. Prospecção ativa pertence a Leads; cliente exige contratação conforme o fluxo comercial.
- Ofereça implementação avulsa e continuidade opcional. Manutenção, recursos, banco de dados, backups e atualização de pacotes têm limites de plano. Desconto e distribuição da implementação em entregas mensais exigem valor total, prazo, mensalidade separada, permanência e cancelamento definidos; não invente benefícios ou preços dos planos.

## Resultado esperado

- Gere `cadastro-lead.json` e `orcamento-proposta.json` conforme os contratos atuais de Leads, com UUID estável, sem credenciais nem contatos fictícios.
- Use a página `/admin/leads/<id>/proposal` para editar e salvar a proposta. Ao final, produza uma tabela HTML compartilhável pelo botão **Baixar proposta HTML**, com a identidade visual Olivaw e comparativo avulso/continuidade.
- HTML precisa escapar textos recebidos e mostrar escopo, horas, investimento, opcionais, custos externos, prazo e condições. Dados internos de credenciais e custos/margem não entram no documento comercial.
- Documente como importar depois da publicação e como conferir duplicidade. Preparar arquivos não significa cadastrar lead, enviar proposta, contratar ou implantar.
