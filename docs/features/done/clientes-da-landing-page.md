# Clientes na landing page da Olivaw

## user-history

Como visitante da Olivaw, quero conhecer os clientes apresentados no site, para consultar trabalhos reais e visitar seus sites.

## acceptance-criteria

- Apresentar LTMJ e Cavalcante no mesmo bloco de clientes.
- Cavalcante usa o símbolo já existente no projeto e link para https://cavalcante.olivaw.com.br/.
- Links indicam abertura em nova aba e usam `noopener noreferrer`.
- Layout funciona em desktop e celular, com navegação por teclado e nomes acessíveis.

## ai-prompt

Desenvolvimento concluído localmente em 30/10/2026. Responsável: Codex, branch feature/issue026-admin-client, workspace atual. Sem commit, push ou deploy.

`LandingClients` mantém a LTMJ e acrescenta Cavalcante Terraplanagem. Grade com duas colunas no desktop e uma no celular. Símbolo copiado do SVG existente em ms-cavalcante-ng, sem alteração no projeto do cliente; cópia pública em ms-olivaw-ng/public/assets/clients/cavalcante-tractor.svg. Não há dependência de asset remoto ou nova imagem gerada.

Build do frontend passou; quatro testes E2E de clientes/contato passaram, incluindo os dois tamanhos, destinos dos links e carregamento do símbolo. Conferência visual local do bloco desktop realizada. Dados comerciais e publicação do cliente permanecem na [documentação da Cavalcante](../../clients/cavalcante/README.md). Esta feature documenta apenas a apresentação na landing do hub.

## user-feedback

adicione a cavalcante ao nosso box de clientes na landing page
