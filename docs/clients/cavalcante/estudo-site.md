# Estudo do site atual

## Método e limites

Fonte primária: [site informado pelo mantenedor](https://locacoesdemaquinas.com.br/). Leitura HTML por HTTP, em 24/10/2026, com conferência visual das sete fotos selecionadas. A versão sem `www` retornou HTTP 403 neste ambiente; a versão [com www](https://www.locacoesdemaquinas.com.br/) permitiu ler as páginas. Isso é um achado de acesso neste ambiente, não prova de indisponibilidade para todos os visitantes ou para o Google.

A auditoria cobre a página inicial, as 12 páginas institucionais do menu, `robots.txt` e os sete sitemaps indicados pelo sitemap principal. O inventário ampliado das publicações está em [inventario-conteudo.json](inventario-conteudo.json). Arquivos temporários de pesquisa ficaram em `/tmp/cavalcante-study`, fora do repositório. Não foram enviados formulários, mensagens ou pedidos reais.

É uma avaliação especializada, sem entrevistas ou teste com clientes. As prioridades são hipóteses de melhoria, ainda sem métricas de conversão. Não equivale a certificação WCAG, auditoria de segurança ou análise exaustiva de toda a literatura de interação humano-computador.

## Inventário institucional

Todos os caminhos abaixo usam o domínio do site atual.

| Caminho | Conteúdo encontrado | Destino proposto |
| --- | --- | --- |
| `/` | Carrossel de seis chamadas, empresa, frota, valores, serviços, materiais, avaliações e contato | Landing com hierarquia e objetivo de orçamento |
| `/empresa/` | Apresentação, missão, visão, valores e diferenciais | Resumo institucional e futura página própria |
| `/retroescavadeira/` | Vídeo, apresentação, fotos e chamada de locação | Catálogo com entrada para retroescavadeira |
| `/servicos/` | Vitrine geral de serviços e materiais | Índice de serviços |
| `/servicos/terraplenagem/` | Nivelamento, aterros, cortes e remoções | Página específica de terraplenagem |
| `/servicos/perfuracao-de-solo/` | Perfuração com equipamento acoplado e aplicações | Página de perfuração, com limites a confirmar |
| `/servicos/demolicao/` | Demolição residencial, comercial, industrial, mecânica e manual | Página de demolição após validar escopo atual |
| `/maquinas-para-locacao/` | Resumo das categorias e fotos | Catálogo por aplicação |
| `/maquinas-para-locacao/locacao-de-bob-cat/` | Equipamentos compactos, texto comercial e galeria | Minicarregadeira, com Bobcat como termo de busca |
| `/maquinas-para-locacao/locacao-de-mini-escavadeira/` | Escavação em áreas compactas e galeria | Mini escavadeira |
| `/maquinas-para-locacao/locacao-de-caminhao-cacamba/` | Basculante para apoio e transporte | Caminhão caçamba |
| `/maquinas-para-locacao/locacao-de-vassoura-coletora-bob-cat/` | Acessório de limpeza, aplicações e fotos | Vassoura coletora como equipamento/acessório |
| `/contato/` | Formulário, captcha, WhatsApp, telefone e endereço | Contato direto e futura solicitação estruturada |

## Conteúdo fora do menu

O sitemap apresenta **356 publicações**, **350 tags**, **327 categorias**, **5 arquivos de data**, **1 arquivo de autor**, **12 páginas** e **2 entradas gerais**. Essas são contagens de URLs declaradas, não contagens de conteúdos únicos nem de páginas indexadas pelo Google. Há exemplos como `hello-world`, variações de “preço terraplanagem Curitiba”, “melhor preço”, bairros e cidades, além de temas como fundações, estacas, destoca e materiais.

A quantidade de URLs e as variações de termos motivam revisão editorial. Não se deve migrar automaticamente capacidade técnica, profundidades de perfuração, cobertura regional ou toda promessa comercial dessas publicações para o novo site. As 356 publicações responderam HTTP 200 e tiveram o corpo de artigo extraído. As páginas trazem tanto descrições comerciais quanto sequências de palavras-chave e localidades; a hipótese de redundância editorial requer curadoria, sem confundir variação de título com conteúdo útil distinto. O resultado da leitura, tamanho e hashes de integridade estão preservados no inventário. Arquivos de categoria, tag, autor e data foram inventariados pelos sitemaps; suas paginações não foram percorridas individualmente, pois são agregações das publicações.

`robots.txt` permite o conteúdo público e anuncia `/sitemap.xml` e `/sitemap.html`. O sitemap XML principal responde e referencia sete índices. `/sitemap_index.xml` exibiu uma página de conteúdo não encontrado; esse caminho não é o sitemap anunciado. Não recomendar “criar sitemap porque não existe”: o trabalho necessário é melhorar sua seleção e consistência.

## Dados e conteúdo a preservar

- Nome divulgado: Cavalcante Terraplenagem e Locação. O projeto usa a grafia **Cavalcante** indicada pelo mantenedor. O documento antigo de abordagem usa “Cavalcanti”; não é fonte para contatos ou mudança de marca.
- WhatsApp principal: `(41) 99654-4458`; telefone adicional: `(41) 99802-7675`.
- Endereço divulgado: Rua Boa Esperança, 571, Alto Boqueirão, Curitiba/PR.
- Serviços: terraplenagem, escavações, cortes, aterros, limpeza, perfuração, demolição; pavimentação aparece no conteúdo e pede validação de oferta.
- Materiais divulgados: terra para aterro, terra preta, brita 1, pedrisco, pó de pedra e saibro. Outros materiais aparecem em publicações antigas; confirmar catálogo comercial.
- Fotografias da frota e trabalhos: usar seleção com legenda, procedência e autorização. A publicação de fotos no site não comprova propriedade atual dos equipamentos.
- O site apresenta avaliações incorporadas e referência a seis avaliações. Não copiar nomes, notas ou depoimentos para o protótipo como evidência atual.
- Há dois endereços de e-mail distintos no HTML protegido do site, um corporativo e outro Gmail. Confirmar o canal oficial; o protótipo prioriza telefone e WhatsApp para evitar direcionamento ambíguo.

## Diagnóstico e prioridades

| Evidência observada | Consequência provável | Proposta | Prioridade |
| --- | --- | --- | --- |
| Título e textos alternativos com longas sequências de termos repetidos | Leitura ruim, sobretudo em leitores de tela e resultados de busca | Títulos descritivos e alt específico por imagem | Alta |
| Seis chamadas em carrossel | Parte da mensagem depende de mudança de slide | Uma mensagem inicial fixa e duas ações claras | Alta |
| Empresa, frota e serviços reaparecem em vários blocos | Mais esforço para localizar o conteúdo desejado | Agrupar por tarefa e remover redundância | Alta |
| “Saiba mais”, “contato” e chamadas genéricas repetidas | Destino do clique pouco claro | “Consultar mini escavadeira”, “Pedir orçamento” | Alta |
| Retroescavadeira isolada no mesmo nível de serviços e locações | Mistura entre serviço e tipo de equipamento | Catálogo com cinco entradas iniciais | Média |
| Contato em imagens, dois telefones e e-mails divergentes | Dificuldade para escolher o canal | WhatsApp principal; demais canais no rodapé | Alta |
| Formulário pede diversos dados e captcha | Fricção potencial, ainda sem medição | Começar por contato contextual; formulário só em etapa própria | Média |
| Centenas de URLs por termos e localidades | Risco de repetição editorial e navegação sem foco | Inventariar, consolidar por intenção e mapear redirecionamentos | Alta |
| Promessas de menor preço e melhores diárias sem comprovação | Expectativa comercial difícil de sustentar | Linguagem objetiva; cotação humana | Alta |

São impactos inferidos, não falhas medidas com usuários. A aparência renderizada integral do site antigo em desktop/celular e o desempenho real ainda precisam de avaliação específica; a leitura HTML não permite afirmar tamanhos visuais ou pontuações de performance.

## Base conceitual aplicada

- **Nielsen, usabilidade**: feedback do filtro, termos do negócio, controles convencionais, retorno à visão completa, prevenção de promessas indevidas e conteúdo enxuto. [Referência primária](https://www.nngroup.com/articles/ten-usability-heuristics/).
- **Krug, Don't Make Me Think, Revisited** (2014): rótulos claros, hierarquia e leitura por varredura. Aplicação: poucas entradas principais e textos curtos. É uma decisão de projeto, não prova de conversão.
- **Norman, The Design of Everyday Things**, edição revisada (2013): affordances, sinalização, feedback e modelos conceituais. Aplicação: botões distinguíveis, links explícitos e categorias por tarefa.
- **Gestalt**: proximidade e similaridade orientam o agrupamento de máquina, descrição e orçamento. Evitar misturar serviços, institucional e oferta no mesmo bloco.
- **Fitts e Hick–Hyman**: alvos confortáveis e menos decisões simultâneas. Aplicação qualitativa, sem alegar cálculo ou experimento: botões de filtro com pelo menos 44 px de altura e quatro itens principais de navegação.
- **WCAG 2.2**: referência para contraste, teclado, reflow e nome acessível. [W3C](https://www.w3.org/TR/WCAG22/). Meta de projeto: nível AA, ainda dependente de auditoria completa.

Não há uma regra universal de quantidade de opções ou uma cor que garanta vendas. Testar as escolhas com o público da empresa.

## Arquitetura de informação

Landing: proposta e cidade → máquina por aplicação → serviços → empresa → como contratar → dúvidas → contato. Caminhos futuros: `/maquinas/retroescavadeira`, `/maquinas/minicarregadeira`, `/maquinas/mini-escavadeira`, `/maquinas/caminhao-cacamba`, `/maquinas/vassoura-coletora`, `/servicos/terraplenagem`, `/servicos/demolicao`, `/servicos/perfuracao-de-solo`, `/servicos/limpeza-de-terrenos`, `/materiais`, `/obras`, `/contato`.

No protótipo, as seções usam âncoras reais e somente `/` é uma página pública nova. Os caminhos futuros acima **não estão implementados**.

## Plano para ser encontrado

1. Publicar a versão aprovada em `cavalcante.olivaw.com.br`, com HTTPS, título e descrição próprios e HTML público prerenderizado.
2. Remover `noindex` e liberar `robots.txt` somente na versão oficial aprovada. O protótipo bloqueia indexação deliberadamente.
3. Confirmar endereço, nome comercial, horários, WhatsApp e região atendida com o cliente; manter consistência com o Google Business Profile.
4. Preparar sitemap com URLs oficiais, canonical e dados `LocalBusiness` reais; testar os dados antes de publicar. Não inserir horários, notas ou área de atendimento fictícios.
5. Definir a relação com `locacoesdemaquinas.com.br`: manter duas versões exige estratégia de canonical; migração definitiva exige controle do domínio antigo e redirecionamentos 301 por intenção. Não redirecionar centenas de URLs indistintamente para a home.
6. Publicar páginas úteis por serviço e máquina, com condições confirmadas e fotos próprias. Evitar replicar a mesma página trocando apenas bairro ou cidade.
7. Configurar Search Console, validar propriedade e acompanhar indexação, consultas e contatos qualificados. SEO é processo contínuo, sem garantia de posição.

Fontes: [Google — SEO Starter Guide](https://developers.google.com/search/docs/fundamentals/seo-starter-guide), [dados de negócio local](https://developers.google.com/search/docs/appearance/structured-data/local-business), [detalhes da empresa](https://developers.google.com/search/docs/appearance/establish-business-details).

## Validação com pessoas

Realizar sessões curtas com 3 a 5 participantes representativos como diagnóstico qualitativo inicial: encontrar máquina para área pequena; descobrir como solicitar terraplenagem; localizar atendimento; pedir orçamento com local e data; entender que disponibilidade e operador exigem confirmação. Observar sucesso sem ajuda, hesitações e dúvidas. Nicolas acompanha e registra fatos, sem induzir respostas. Comparar protótipo e site atual sob as mesmas tarefas; definir metas após uma linha de base. A amostra não sustenta inferência estatística de conversão.
