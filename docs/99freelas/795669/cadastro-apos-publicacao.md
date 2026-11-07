# Registro operacional — 795669

Autorizado cadastro interno em 03/11/2026. UUID estável `17c3851e-ae39-5857-bdfd-d265de91fda1`. Referência `795669`, URL e título devem ser conferidos para evitar duplicidade. Criar apenas uma oportunidade para as alternativas A/B.

O cadastro contém anúncio e transcrição integral do PDF em requisitos; arquivo PDF preservado localmente, não anexado remotamente por API nesta operação. Contatos não fornecidos permanecem vazios. Não atribuir canal pessoal encontrado no PDF ao cadastro como contato confirmado.

Rascunho principal: `orcamento-proposta.json` (MVP/A). Completa em `orcamento-completo.json`, ambas descritas comercialmente nos arquivos. Não somar os dois totais nem substituir a alternativa A por B sem revisão.

`POST /leads` e `PUT /leads/<id>/proposal` com revisão atual, leitura posterior para confirmar. Painel: `/admin/leads/17c3851e-ae39-5857-bdfd-d265de91fda1/proposal`. HTMLs inicialmente gerados pelo gerador do frontend; exportação pelo botão do painel pode ser feita após revisão.

Cópia do PDF autorizada pela entrega do arquivo para análise. SHA-256 original/cópia: `7c996aa033307e13ac052f7dbb7b0b05074c1b9ef4c2b27f35f32222e4e58ddc`. Extraído com `pdftotext -layout`, sem modificar o original em Downloads.

Cadastro não significa proposta enviada ou contratação. Nenhum acesso a conta Kiwify, pagamento ou infraestrutura do anunciante.

## Cadastro confirmado em 03/11/2026

- Consulta por referência sem UUID divergente identificado.
- Criação do Lead retornou HTTP 201; orçamento MVP/A salvo com HTTP 200.
- Leitura posterior confirmou igualdade do rascunho, revisão 2, status `draft`.
- Requisitos do Lead incluem o texto do anúncio e a transcrição integral do PDF. PDF binário permanece na pasta local; não foi anexado remotamente.
- Ambas as alternativas constam das condições da proposta principal; os detalhes e HTMLs A/B ficam na pasta. A alternativa B não gerou outro Lead nem sobrescreveu o MVP.
- HTMLs gerados localmente pelo gerador do frontend; não baixados pelo botão do painel nesta tarefa.
- Proposta comercial com 2.573 caracteres, abaixo do limite de 3.000.
- Integridade da cópia PDF, presença da transcrição no cadastro, cálculos, JSONs e links conferidos.
- Nenhuma proposta enviada ao anunciante, contratação, implantação ou teste financeiro realizado.
