# Cavalcante — gestão de conteúdo e acompanhamento comercial

## user-history

Como responsável pelo atendimento,
quero atualizar conteúdo e acompanhar pedidos,
para manter o site correto e organizar retornos.

## acceptance-criteria

- Área privada usa identidade independente da Cavalcante e autorização na API.
- Permissões diferenciam conteúdo público e dados comerciais; mudanças relevantes têm auditoria.
- Validar estados comerciais, contatos e critérios de busca com a equipe antes de modelar.
- Agenda, reservas, contratos e financeiro só entram após descoberta específica.
- Nenhum schema ou migration é gerado sem pedido explícito.

## ai-prompt

Hipótese de evolução, sem autorização de implementação nesta etapa. Login/admin existentes são apenas infraestrutura herdada. Não assumir que a empresa usa planilhas ou precisa de ERP. Depende de entrevistas, política de dados e priorização. Manter IRepository<T> para leituras comuns e respeitar a arquitetura Olivaw.

## user-feedback

```text
vamos começar um projeto novo da empresa cavalcante terraplanagem. O nome padrao da olivaw com api (backend) e ng (frontend).

vamos chamalo de cavalcante que é nosso novo cliente e estaremos ensinando o Nicolas nosso novo estagiario.

vamos começar com um estudo do site que eles ja tem, https://locacoesdemaquinas.com.br/
queremos que voce investigue todo o conteudo do site e utilize de todos os conceitos da literatura sobre usabilidade de sistemas e interface.
tbm queremos que o site fique menos poluido e mais limpo para navegar e tbm queremos que seja masi facilmente atrativo e informacoes faceis e tbm mais simples/facil de encontrar na internet. Começaremos com o projeto, voce vai usar os fontes 'limpos' e criar ms-cavalcante-api e ms-cavalcante-ng .

primeiro cria as cascas/projteos limpos e alimente a nossa pasta docs com as features (funcionalidades que voce pretende impelmentar e nos sugerir)
faça um estudo de cores sobre o negocio qeu tipo de cores sao mais atrativas para uma empresa de terraplanagem e aluguel de maquinas. e  tbm faça um 'emoji' tipo um logo de tratorzinho maneiro para a gente usar de 'avatar' no nosso site blz. TBM sera nosso favicon.


por n codifique nada com API, apenas crie os projetos limpos e faça um prototipo bem maneiro da landing page
```

```text
eu quero que o link de acesso seja cavalcante.olivaw.com.br, ja ajuste a publicação para que o link seja esse
```
