# Análise técnica

## Leitura do pedido

O cliente prioriza custo e praticidade em uma plataforma existente. A proposta deve começar pela verificação da plataforma e pelo reaproveitamento de recursos nativos, plugins ou serviços compatíveis. Desenvolvimento completo é alternativa de maior investimento.

## Solução recomendada por cenário

- **WordPress confirmado:** avaliar comentários nativos com wpDiscuz para interface e votos, mais componente mantido de reações aos conteúdos. Validar as quatro reações e contagens em prova local antes de selecionar o segundo componente. Não prometer que um único plugin atende tudo.
- **Outro CMS:** avaliar primeiro extensões oficiais ou mantidas para a versão instalada, incluindo permissões, exportação dos dados, compatibilidade e licenças.
- **Plataforma própria:** comparar componente incorporável com integração à identidade atual e extensão do backend existente. Serviço externo exige avaliar assinatura, publicidade, tráfego, armazenamento e dependência do fornecedor. Não escolher fornecedor sem conhecer esses limites.

O wpDiscuz oferece votos positivos/negativos e contagem separada; votos de visitantes por IP podem agrupar pessoas na mesma rede. Isso exige validar a regra de identidade com o cliente, em vez de prometer unicidade por pessoa. Filtros de palavras e antispam devem encaminhar suspeitas à revisão humana; não garantem detecção completa de ofensas ou preconceito.

## Perguntas para fechar a proposta

- Qual o endereço, CMS/framework, versões, hospedagem e forma de acesso ao código? Há ambiente de homologação e backup recuperável?
- Visitantes precisam entrar em uma conta? Já existe login? Aceitam login para garantir uma interação por conta?
- Qual o volume mensal de páginas vistas, comentários e picos simultâneos?
- Quais páginas recebem os recursos? Há vários tipos de conteúdo, idiomas ou sites?
- As quatro reações são definitivas? Pode trocar/remover reação e voto? Exibir positivos e negativos separados?
- Há comentários antigos para migrar? Respostas em árvore, anexos, notificações ou edição são necessários?
- Quem modera, qual política será aplicada e haverá aprovação prévia? Quem configura exemplos de termos inadequados?
- Qual o orçamento disponível, prazo desejado e aceitação de custos recorrentes?
- Há restrições de cookies, IP, envio de conteúdo a terceiros e retenção? Confirmar requisitos de privacidade antes da configuração.

## Premissas do recorte econômico

Um site, um idioma, um modelo de página de conteúdo e um fluxo de administração existente. Sem migração, anexos, notificações, novo login ou reformulação visual. Moderação humana sob responsabilidade do cliente; configuração inicial de filtros e treinamento incluídos. Homologação, credenciais temporárias e autorização de publicação devem ser combinadas na contratação.

## Verificações previstas

- Comentários vinculados ao conteúdo correto, paginação e visibilidade conforme moderação.
- Voto e reação persistentes, troca/remoção e repetição conforme regra acordada; requisições simultâneas sem duplicar contagens.
- Administração protegida; visitante não aprova, oculta ou exclui comentários.
- Conteúdo malicioso tratado como texto; limites de tamanho e frequência, proteção contra requisições indevidas e cenários de bot.
- Filas de suspeitas e revisão de falsos positivos com amostras acordadas.
- Celular, computador, teclado, rótulos acessíveis, erros de rede e recuperação; regressão da página e efeito do cache.
- Ensaio de backup e reversão em homologação, sem bancos reais nesta análise.

## Fontes consultadas em 31/10/2026

- [wpDiscuz no diretório WordPress](https://wordpress.org/plugins/wpdiscuz/): candidato para comentários; núcleo gratuito não implica todos os complementos gratuitos.
- [Documentação de votos do wpDiscuz](https://wpdiscuz.com/docs/wpdiscuz-7/plugin-settings/comment-thread-features/): votos negativos, contagens separadas e limitações da identificação por IP.
- [WP Reactions Lite](https://wordpress.org/plugins/wp-reactions-lite/): diretório informa encerramento permanente; descartado como candidato.

A recomendação é condicional. Não houve inspeção da plataforma, instalação ou teste de compatibilidade. Revalidar manutenção, segurança, funcionalidades e preços na contratação.
