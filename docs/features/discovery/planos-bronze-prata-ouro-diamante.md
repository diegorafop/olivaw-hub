# Planos Olivaw — Bronze, Prata, Ouro e Diamante

## user-history

Como responsável pela Olivaw,
quero separar planos por resultado, esforço e custo,
para vender serviços claros e sustentáveis sem prometer capacidade ou trabalho ilimitados.

## acceptance-criteria

- Bronze é estático e usa endereço da Olivaw; Prata permite domínio próprio, API, PostgreSQL e conteúdo gerenciável; Ouro contempla gestão personalizada e múltiplos usuários.
- Diamante é contrato personalizado, sem preço ou franquias universais.
- Distinguir implantação inicial, mensalidade, correções e pequenas melhorias; mudanças grandes recebem orçamento aprovado antes da execução.
- Propostas não alteram contratos, mensalidades, DNS ou franquias dos clientes atuais sem aprovação.
- Cálculo inclui infraestrutura, ferramenta de IA, trabalho humano, reserva operacional e provisão comercial; créditos AWS não são redução permanente de custo.
- A capacidade da VM depende de medição e teste de carga, não de dividir RAM ou disco pelo número de clientes.

## ai-prompt

Discovery comercial, não tabela de preços aprovada. Responsável: Codex, branch feature/issue026-admin-client, worktree atual, sem commit, push ou deploy. Referências consultadas em 25/10/2026. Valores em reais abaixo são sugestões; câmbio, tributos, horas e capacidade são hipóteses editáveis, não cotações ou orientação tributária.

### Proposta de catálogo

| Item | Bronze — Presença | Prata/Silver — Conteúdo | Ouro — Gestão |
| --- | --- | --- | --- |
| Mensalidade sugerida | R$ 299 | R$ 699 | R$ 1.299 |
| Implantação sugerida | R$ 1.990 | R$ 4.990 | A partir de R$ 14.900, após escopo |
| Objetivo | Apresentar a empresa e receber contatos externos | Cliente atualiza conteúdo do próprio site | Processos de gestão construídos para o negócio |
| Site | Até 5 páginas estáticas no escopo inicial | Até 10 páginas e 2 tipos de conteúdo | Site e módulos de gestão definidos na proposta |
| Endereço | Subdomínio Olivaw, ex.: cliente.olivaw.com.br | Domínio próprio permitido | Domínio próprio permitido |
| API do negócio | Não | 1 API para o conteúdo acordado | API e regras de negócio acordadas |
| PostgreSQL | Não há banco exclusivo para o site estático | 1 banco lógico com isolamento e credenciais próprias | 1 banco lógico; isolamento e capacidade conforme carga |
| Conteúdo | Alterações pela equipe dentro da franquia | Editor para publicar textos, imagens e conteúdo contratado | Gestão de entidades e fluxos previstos no escopo |
| Usuários do sistema | Sem login de gestão de conteúdo | Até 2 contas editoras | Até 10 contas nominativas, com perfis de acesso |
| Espaço inicial total | 2 GiB de arquivos | 3 GiB somando arquivos e banco | 5 GiB somando arquivos e banco |
| Pequenas melhorias por mês | 2 de até 30 min cada | 3 de até 1 h cada | 6 de até 1 h cada |
| Trabalho mensal previsto no custo | 1 h para melhorias | 3 h melhorias + 1 h operação | 6 h melhorias + 2 h operação |
| Correções do escopo entregue | Sem cobrança adicional, sem consumir franquia | Igual | Igual |
| Atendimento | Chamados no Portal em horário comercial | Igual, com acompanhamento do conteúdo | Igual, com triagem dos fluxos de gestão |

**Diamante — Sob medida:** preço, implantação, capacidade, usuários, integrações, segurança, infraestrutura, franquias, prazos e níveis de atendimento definidos em contrato próprio. Referência de entrada para análise comercial: a partir de R$ 2.490/mês, sem transformar esse valor em oferta universal. Projetos que não cabem no Ouro migram para avaliação de Diamante, não para promessa de horas ilimitadas.

Subdomínio Bronze pertence à zona da Olivaw. O cliente mantém seus conteúdos e a possibilidade de exportação conforme contrato; não confundir propriedade do endereço com propriedade do conteúdo. No Prata e superiores, recomendar domínio registrado em nome do cliente, com configuração técnica pela Olivaw. Registro/renovação, e-mail e serviços de terceiros não estão embutidos nestes preços. A decisão do mantenedor permite domínio próprio a partir do Silver; este estudo interpreta Bronze como subdomínio Olivaw. Nada foi alterado no DNS dos clientes existentes.

O login do Portal para chamados e acompanhamento é comum aos planos e não conta como usuário editor/gestor do sistema do cliente. Quantidade de contas não é garantia de usuários simultâneos; concorrência e carga precisam de teste.

### O que é uma melhoria

Pedido pequeno e delimitado: ajustar uma seção, um campo ou uma regra existente dentro do tempo do plano, incluindo implementação, teste, publicação e comunicação. A equipe estima antes de aceitar; um novo módulo, integração, migração, redesign ou novo fluxo de negócio é projeto/orçamento, mesmo que o cliente o descreva como um único pedido. O cliente aprova o escopo antes de consumir a franquia. Sugestão: franquia mensal não acumula nem vira crédito financeiro. Aguardar aprovação comercial dessa regra.

Correção é reparar defeito reproduzível no comportamento contratado, inclusive regressões e falhas causadas pela implementação da Olivaw. Não reduz a franquia, nem gera cobrança extra por quantidade de chamados. Mudança de regra, conteúdo incorreto enviado pelo cliente, serviço externo descontinuado ou novo requisito exige classificação transparente e aprovação, sem reclassificar bugs para faturar. A reserva abaixo ajuda a cobrir correções, mas precisa ser confrontada com horas reais. Não há promessa de atendimento 24/7 ou prazo de resolução fixo nesta proposta.

### Conta-base solicitada: US$ 10 de infra + US$ 100 de IA

Hipóteses de trabalho: câmbio **R$ 5,50/US$**, cinco clientes pagantes, ferramenta de IA **US$ 100/mês**, trabalho humano **R$ 80/h**, reserva operacional **15%** e provisão comercial/tributária ilustrativa **6% da receita**. Não são câmbio do dia, alíquota determinada ou tarifa por usuário do Codex. O custo da ferramenta é overhead compartilhado da operação; não é multiplicado por site. Assumimos dólar para os 100/mês; se forem R$ 100, substituir esse valor na fórmula.

- Infra: US$ 10 × 5,50 = **R$ 55/mês**.
- Ferramenta: US$ 100 × 5,50 = **R$ 550/mês**.
- Base fixa: **R$ 605/mês**, ou **R$ 121 por cliente**, com cinco pagantes.
- Custo-base por plano = 121 + horas × 80.
- Custo com reserva = custo-base × 1,15.
- Receita após provisão ilustrativa = mensalidade × 0,94.
- Resultado estimado = receita após provisão − custo com reserva. Não inclui todos os custos de uma empresa e não representa lucro contábil.

| Plano | Fixo rateado | Trabalho | Custo com reserva | Mensalidade | Resultado estimado após provisão |
| --- | --- | --- | --- | --- | --- |
| Bronze | R$ 121 | R$ 80 | R$ 231,15 | R$ 299 | R$ 49,91 |
| Prata | R$ 121 | R$ 320 | R$ 507,15 | R$ 699 | R$ 149,91 |
| Ouro | R$ 121 | R$ 640 | R$ 875,15 | R$ 1.299 | R$ 345,91 |

A implantação é cobrada separadamente para financiar o desenvolvimento inicial. Hipóteses de esforço: 12 h Bronze, 30 h Prata e 100 h Ouro. Precisam de orçamento por projeto; o Ouro não cobre qualquer sistema de gestão por R$ 14.900. Horas de IA não substituem revisão humana, testes e responsabilidade de operação.

Com apenas dois clientes pagantes, o fixo rateado vira **R$ 302,50 por cliente**: Bronze a R$ 299 não cobre esse cenário acrescido de trabalho e reserva. A Olivaw como cliente interno não é um terceiro pagante e não deve diluir artificialmente a conta. Manter capital para a fase inicial, rever preço ou reduzir overhead. O rateio de Despesas real não será feito automaticamente com essa hipótese; ela pertence ao estudo.

### Infra real e limite da hipótese

A consulta da VM confirmou 30 GiB de disco, cerca de 15 GiB usados e 16 GiB disponíveis, 913 MiB de RAM visível e swap em uso. Cinco sites estáticos podem ser um cenário a testar; cinco APIs com banco e sistemas personalizados não são capacidade demonstrada. Cinco franquias Bronze de 2 GiB somam 10 GiB, antes de backups, logs, imagens e margem de crescimento. Reservar capacidade efetiva e acompanhar ocupação antes de vender toda a franquia.

US$ 10 é uma hipótese otimista. Referência ilustrativa sem créditos, 730 h, Linux us-east-1: t3.micro a US$ 0,0104/h ≈ **US$ 7,59**; IPv4 público a US$ 0,005/h ≈ **US$ 3,65**; se o volume de 30 GiB for gp3 a US$ 0,08/GiB-mês, mais **US$ 2,40**. Total ilustrativo **US$ 13,64**, sem backups, tráfego adicional, excedente de créditos de CPU, impostos ou outros serviços. Tipo e cobrança do EBS precisam ser confirmados na fatura. Não é custo medido de toda a conta.

Com infra US$ 15 e IA US$ 100: fixo R$ 632,50/mês, R$ 126,50 por pagante em cinco clientes; aumentam R$ 6,33 por plano depois da reserva, mantendo as outras premissas. APIs/sistemas podem exigir uma VM maior ou separação; não fixar seu custo no orçamento da máquina atual.

### Referências e comparação

- [AWS T3](https://aws.amazon.com/ec2/instance-types/t3/): especificações e referência de preço Linux/us-east-1.
- [AWS IPv4](https://aws.amazon.com/vpc/pricing/): cobrança de endereço público.
- [AWS EBS](https://aws.amazon.com/ebs/pricing/): armazenamento provisionado e cobrança por tipo/região.
- [Hostinger](https://www.hostinger.com/br/hospedagem-de-sites): hospedagem é infraestrutura; preços promocionais/renovação não equivalem a desenvolvimento personalizado e manutenção incluídos. Não comparar mensalidades sem equalizar o escopo.
- A referência de IA de 100/mês foi informada pelo mantenedor e adotada como premissa; créditos extras, API de modelos e outras licenças precisam de registro separado quando existirem.

### Próximas decisões

Aprovar preços, moeda da ferramenta, cotação, esforço, margem, regras de correção e franquias. Validar capacidade por perfil e definir backups e restauração antes de prometer esses serviços no contrato. Só então cadastrar estudos/condições comerciais pela API. Não atribuir automaticamente Bronze/Prata/Ouro aos clientes atuais.

## user-feedback

```text
podemos fazer um calculo onde temos gasto de 10 US mensal e temos carga para digamos assim uns 5 sites certo? 2GB de espaço pra cada um de fotos que vão subir e dados no BD que sera pouco, acho que no plano bronze n teria direito a API por exemplo, ou seja somente dados estaticos.
plano prata API e BD e funcionalidades como cadastro de conteudo para o proprio app, para o plano ouro, sistemas de gestao personalizados, multi usuarios, permitir que mais pessoas se loguem e tals.
faça 3 planos bem claros e distintos e tenha uma media de custo com INFRA + custo codex (100 mes) .
no plano Diamante será um plano mais customizado, deixe claro que aqui tem contratos customizados.
tbm coloque um numero de pedidos de melhorias, correcoes sao feitas sem cobrança adicional. Features sao limitadas quantidades por plano.
plano bronze o dominio fica na olivaw, plano silver ja pode dominio proprio
```
