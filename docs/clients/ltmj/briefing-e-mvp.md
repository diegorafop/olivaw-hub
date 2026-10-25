# LTMJ Eventos — briefing, proposta e MVP

## Estado e origem

Landing e backoffice do primeiro cliente, criados a partir de `ms-limpo-ng`; backend derivado de `ms-limpo-api`. A implantação da issue 009 não foi executada nesta alteração. O [protocolo Olivaw](../../protocolo-descoberta-cliente.md) é reutilizável em outros negócios; este documento aplica o processo à LTMJ.

Briefing fornecido pelo responsável via WhatsApp, em 17/10/2026. O texto abaixo é uma síntese; não replica nomes pessoais ou a conversa privada completa.

## Confirmado pelo cliente

- Nome: LTMJ Eventos.
- Essência: transformar momentos especiais em experiências inesquecíveis, com sabor, elegância e excelência nos detalhes.
- Atuação descrita: eventos personalizados, casamentos, aniversários e outras festas; bar, almoço, jantar, churrasco, garçons e cozinheiros.
- Perguntas recorrentes: valores, cardápio e possibilidade de acrescentar/retirar itens.
- Há material de diferentes tipos; foram recebidas 12 imagens para o primeiro churrasco de confraternização.
- Ação principal desejada: solicitar orçamento.
- Preto e dourado são a direção inicial; a resposta recebida sobre dourado estava abreviada e a paleta ainda precisa de aprovação visual.

## Observado nas redes públicas

Em 17/10/2026, consultamos:

- [Facebook oficial informado](https://www.facebook.com/L.T.M.JEVENTOS/): a descrição pública apresenta gastronomia, bartender, churrasco, buffet, estrutura/equipe e Curitiba e região.
- [Instagram informado por URL](https://www.instagram.com/eventosltmj/): descrição pública de bartender, garçons, churrasco, gastronomia e eventos sociais/corporativos em Curitiba e região.

O identificador escrito inicialmente no briefing foi `@eventoltmj`; a URL fornecida e o perfil consultado usam `@eventosltmj`. O protótipo segue a URL confirmada na solicitação. Confirmar com o cliente antes da publicação.

O Facebook forneceu uma imagem pública de perfil, armazenada em `public/assets/ltmj/perfil-facebook.jpg`, com registro em `origem.json`. É o logotipo real preto/dourado. Não foi possível obter álbuns/fotos de eventos sem autenticação nas páginas públicas consultadas. Não foram usadas fotos de outras empresas, perfis privados, imagens geradas como portfólio ou depoimentos inventados.

## Hipótese de valor

Uma vitrine própria deve explicar os serviços e transformar a conversa de orçamento em um pedido inicial organizado. O ganho esperado é reduzir perguntas repetidas e facilitar o primeiro contato; isso é hipótese a validar, não promessa de vendas.

## MVP proposto

- Página pública responsiva: marca real, serviços, ocasiões, personalização, processo e dúvidas frequentes.
- CTA “Solicitar orçamento” conduz a um assistente curto com ocasião, serviços, convidados aproximados, data opcional, cidade e observações.
- O resumo é revisável e copiável; WhatsApp só é ativado quando o número comercial for confirmado. Até lá há acesso ao Instagram e Facebook oficiais, sem simular envio.
- Mural próprio com filtros de ocasião e galeria, carregado pela API com eventos visíveis; o churrasco inicial pode ser importado pelo painel.
- Landing prerenderizada e mural carregado no navegador; API separada; backoffice mobile em `/gestao` com cadastro, upload, edição, visibilidade e exclusão confirmada.
- Metadados básicos, canonical e sitemap com domínio técnico proposto, ainda sujeito à aprovação.

## Entregas seguintes

| Fase | Entrega proposta                                         | Condição para começar                                                                   |
| ---- | -------------------------------------------------------- | --------------------------------------------------------------------------------------- |
| 1    | Publicar a landing e direcionar orçamentos ao canal real | Aprovar identidade, serviços, cidade/região, número e material público.                 |
| 2    | Administrar eventos e portfólio                          | Definir campos, categorias, quem publica, armazenamento e limites de imagens.           |
| 3    | Receber comentários/depoimentos moderados                | Definir identificação, moderação, anti-spam e publicação; nada aparece automaticamente. |
| 4    | Organizar propostas/agenda e indicadores                 | Validar rotina comercial, responsável e informações necessárias.                        |

Não haverá migração de banco, upload público, comentários abertos, pagamento, agenda com disponibilidade real ou campanha paga neste protótipo. Novas funcionalidades do backend seguirão features CQS e contratos em inglês, com nomes como `Event`, `GalleryImage`, `QuoteRequest` e `Review`, sem gerar migrations sem pedido.

## Pendências de aprovação

- WhatsApp comercial com DDD; e-mail, horário de atendimento e responsável.
- Região de atendimento e condições de deslocamento.
- Cardápios reais, opções/remanejamento e limites de personalização.
- Fotos/vídeos selecionados, autorização de publicação e legendas verdadeiras.
- Nome jurídico, domínio final e frase de apresentação final.
- Investimento, prazo e manutenção, ainda sem valores acordados.
- Não inferir preços, capacidades, anos de experiência, disponibilidade ou avaliações.

## Critérios de aceite do protótipo

- Cliente reconhece a marca e a descrição dos serviços.
- Um visitante identifica como pedir orçamento em poucos segundos.
- Consegue montar e copiar um resumo, sem preço automático ou falsa confirmação de envio.
- Mobile navega sem transbordamento; formulários têm labels e validação; movimento reduzido é respeitado.
- Links sociais usam os perfis informados. Fotos e informações não confirmadas são distinguidas de fatos.

## Projetos e ambientes

`ms-ltmj-ng` e `ms-ltmj-api` têm builds, imagens e implantações independentes, com `dev/qas/rls/stg/prd`. Domínios técnicos propostos: `ltmj.olivaw.com.br` e `api.ltmj.olivaw.com.br`. Não foram registrados ou publicados. Banco proposto por ambiente: `ltmj_<ambiente>`; schema `ms_ltmj`. Não foi criado banco, migration, usuário ou fila.

## Mural de eventos

Componente em `src/app/features/public/events/`. O catálogo estático guarda o churrasco inicial apenas para importação explícita. O mural usa a API como fonte de verdade e não exibe modelos ou conteúdo fixo se ela falhar. Consulte [gestão de portfólio](gestao-portfolio.md).

Cada evento admite título, ocasião, descrição, serviços efetivamente realizados e fotos com texto alternativo/legenda. Data e local são opcionais; não exibir nomes de contratantes sem aprovação. O diálogo permite navegar entre fotos e seguir para orçamento. Não há afirmação de quantidade de clientes, marcas atendidas ou depoimentos sem confirmação. Cadastro e upload foram implementados na issue 009; comentários e moderação continuam como evolução. Não há persistência de portfólio no navegador.
