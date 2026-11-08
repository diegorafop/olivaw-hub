# Configuração — Hub de cadastros e ferramentas

## user-history

Como operador da Olivaw,
quero encontrar os cadastros em uma página de cards,
para navegar sem acumular menus e submenus na barra lateral.

## acceptance-criteria

- Barra lateral principal contém Visão geral, Leads e Configuração para root.
- Configuração apresenta cards para Clientes, Serviços, Recursos, Saúde, Incidentes, Consumo, Planos, Cuidados e Atendimento.
- Configuração permanece ativa ao abrir uma dessas telas, sem duplicar links no menu principal.
- Cards funcionam em desktop, celular e teclado; telas principais oferecem retorno ao hub.
- Breadcrumb acompanha Configuração, módulo e ficha/criação, com ancestrais clicáveis e página atual identificada; parâmetros de filtro não mudam a hierarquia.
- Usuário sem root não vê o acesso e não alcança as rotas restritas.

## ai-prompt

Desenvolvimento local concluído a pedido do mantenedor. Hub em `/admin/configuration`, usando componentes standalone e links existentes, sem alteração das rotas anteriores dos cadastros. O hub não consulta banco nem exige schema próprio.

As regras foram registradas no [AGENTS.md do frontend](../../../ms-olivaw-ng/AGENTS.md). Não acrescente novos links laterais para cada cadastro; acrescente cards ao hub. As rotas e APIs continuam autorizadas individualmente.

Testes E2E cobrem navegação compacta, nove cards, telas em desktop/celular, bloqueio sem root e breadcrumb clicável até formulário de criação. Resultados e comandos no [guia das entregas](../../operacao-interna.md). Integração e publicação não foram executadas.

## user-feedback

```text
essa parte de cadastraos basicos como recursos, incidentes e outros voce vai criar um menu unico chamado 'configuracao' e esse menu será uma paǵina de cards cada card é uma parada daquelas. Vamos trabalhar com ess modelo para n ter muitos menus e submenus

quando clicar em um desses cards de navegação o breadcromb tem que acompanhar e permitir voltar com clique
```
