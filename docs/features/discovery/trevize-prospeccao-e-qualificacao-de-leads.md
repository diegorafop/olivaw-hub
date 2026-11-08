# Trevize — Prospecção e qualificação de Leads

## user-history

Como operador da Olivaw, quero encontrar empresas por segmento e região e revisar sua presença digital, para acompanhar oportunidades de criação, melhoria e operação de sites com evidências e contatos comerciais confirmados.

## acceptance-criteria

- Planejar `olivaw-agent/ms-trevize-py` conforme a estrutura e os contratos de execução do Baley, com configuração real em `setting.json`, start/stop/status, agendamento e saída JSON indentada.
- Configurar segmentos, subsegmentos incluídos/excluídos, cidades/bairros, quantidade máxima de candidatos e sites analisados, tentativas, prazo de reanálise, duração e custo por execução.
- Separar modos POC, revisão e cadastro via API; cadastro automático inicialmente desativado. O primeiro lote será de 20 candidatos para revisão individual.
- Descobrir empresas em fontes permitidas e adaptáveis: pesquisa web, sites próprios, associações e diretórios. Integração recorrente com Maps depende da avaliação de seus termos e permissões de uso e armazenamento.
- Quando a fonte permitir navegação incremental, percorrer resultados com limites, coletando antes da virtualização dos cards e eliminando duplicados; interromper ao atingir o lote, fim dos resultados, bloqueio ou limite de tentativas. Não contornar CAPTCHA ou restrições.
- Classificar organizadoras, cerimonialistas, espaços, buffet, locadoras, montadoras e fornecedores. Estar na busca não comprova aderência ao segmento.
- Confirmar identidade e presença digital; registrar site não localizado em vez de afirmar ausência apenas porque não há botão no Maps.
- Distinguir telefone, WhatsApp confirmado por link publicado, e-mail comercial e formulário. Não inventar contatos nem marcar todo celular como WhatsApp.
- Analisar sites com verificações passivas: abertura, HTTPS, experiência móvel, imagens, links, clareza dos serviços, contato e SEO básico (títulos, descrições e headings). Registrar pontos positivos também.
- Não preencher ou enviar formulários, mensagens, ligações ou criar contas nos sites analisados. Não realizar testes invasivos nem inferir vulnerabilidades sem evidência.
- Produzir ficha com fonte, data, evidência, confiança, achados positivos/negativos, impacto possível, sugestão e prioridade justificada. Diferenciar falha confirmada, hipótese e teste inconclusivo.
- Associar segmento, necessidade, evidência e portfólio relacionado como classificações independentes. Usar LTMJ e Cavalcante apenas quando pertinentes, sem presumir equivalência entre negócios.
- Deduplicar por identidade, domínio, telefone e identificador permitido da fonte, sem unir homônimos ou filiais automaticamente.
- Permitir ao operador aprovar, descartar e justificar a revisão; medir utilidade do lote e ajustar critérios antes de ampliar volume.
- Integrar somente por API autenticada do Olivaw, com conta técnica de privilégios mínimos e idempotência; sem acesso direto ao banco e sem permissão de deploy.
- Proteger leitura, revisão e cadastro administrativo. Separar dados comerciais verificados das respostas brutas de provedores e respeitar retenção e licenças por origem.

## ai-prompt

### Estado e rastreabilidade

Discovery registrada em 01/11/2026. Responsável pela documentação: Codex. Branch observada: `feature/issue027-agent-prospector`; issue 027 inferida do branch, sem criação ou fechamento remoto. Worktree: raiz do monorepo. O recorte inicial do agente, agendamento e integração por API foi implementado em [coleta agendada](../done/trevize-coleta-agendada-de-leads.md); a [ficha de Leads](../done/leads-ficha-qualificacao-e-abordagem.md) também está implementada. Esta discovery mantém as ideias mais amplas e decisões de evolução, sem afirmar que todos os critérios abaixo já estão prontos. Não gerar migrations, publicar, commitar ou enviar mensagens como consequência desta especificação.

Ler as instruções locais de `olivaw-agent`, Baley, frontend e API antes da implementação. Usar o scaffold Python existente e conferir o contrato real de `setting.json`, sem copiar permissões de remediação ou credenciais do Baley. Não presumir que sua operação update é necessária: definir comandos adicionais conforme o escopo.

### Recorte autorizado posteriormente

O mantenedor autorizou o agente com coleta/cadastro ao start manual, intervalo de 24h, até 20 candidatos de eventos de Curitiba. Essa instrução substitui a sugestão inicial de cadastro automático desabilitado para o recorte implementado; não autoriza contato, envio ou autostart no PC. Filtros avançados e demais fontes continuam em estudo.

### POC realizada

O mantenedor autorizou pesquisa no Maps e inspeção de 20 candidatos, sem cadastro ou contato. A primeira abertura do link fornecido exibiu 10 resultados, dos quais dois claramente relacionados a eventos. Uma nova pesquisa por `empresa de eventos curitiba`, com rolagem do painel e deduplicação, reuniu 31 resultados únicos; 26 tinham relação aparente com eventos e 20 foram apresentados para revisão. A Sonora apareceu após ampliar a navegação. Esses números comprovam apenas a execução daquela amostra, não cobertura de Curitiba ou precisão comercial.

O navegador mostrou visualização limitada. Nem os resultados nem a ordenação são garantidos ou exaustivos. A POC não comprovou viabilidade contratual da coleta recorrente. Os sites dos 20 não receberam auditoria individual completa; os telefones não foram todos confirmados como WhatsApp. Nenhum desses candidatos foi cadastrado na API.

Achados de desenho: resultados fora do segmento, links de Website apontando para Instagram ou Wix, empresas sem link exibido e contatos divergentes entre fontes. Domínio HTTP no resultado não prova ausência de HTTPS; Wix, site antigo ou nota no Maps não provam baixa qualidade. Falta de domínio localizado não prova inexistência.

### Fluxo proposto

1. Definir campanha por região, segmento e limites de custo/tempo.
2. Descobrir candidatos nas fontes aprovadas, preservando proveniência por campo.
3. Filtrar aderência e duplicidade; confirmar identidade antes de enriquecer contatos.
4. Analisar presença digital e evidências técnicas de forma passiva e limitada. Repetir verificações temporárias antes de concluir falha.
5. Gerar ficha de oportunidade com ações concretas: benefício possível, evidência e referência de portfólio pertinente.
6. Revisar um a um os 20 primeiros; marcar aprovado, descartado ou pendente e registrar motivo.
7. Cadastrar os aprovados via API com UUID estável e tratamento de concorrência e duplicidade. Não atualizar Lead existente silenciosamente.
8. Medir quantos são aderentes, verificáveis, duplicados, revisados e úteis, além do custo por candidato aprovado. Conversão só pode ser medida após futura abordagem autorizada.

### Modelo de informação e interface a estudar

Ficha: identidade da empresa, segmento/subsegmento, região, domínio, canais comerciais, fonte e data por informação, observações, testes e seus limites, confiança, prioridade justificada e referência de portfólio. JSON deve conter `schemaVersion` e campos extensíveis; não persistir indiscriminadamente todo HTML ou conteúdo de terceiros.

Classificações separadas:

- Presença: domínio próprio, somente redes sociais ou site não localizado.
- Diagnóstico: problema confirmado, possível melhoria ou sem problema relevante encontrado.
- Revisão: pendente, aprovado, descartado; prioridade comercial independente.
- Tags sugeridas: eventos/terraplanagem/comércio; presença digital/reformulação/correção/SEO; contato quebrado/problema móvel; referência LTMJ/Cavalcante.

Exemplo de oportunidade: contato escondido no celular, com captura e URL, sugestão de destacar orçamento e confiança explícita. Não prometer aumento de vendas ou posição no Google. Uma boa presença digital pode resultar em descarte ou oportunidade específica de manutenção, sem forçar uma venda.

A interface deve apresentar candidatos e detalhes navegáveis com breadcrumb, fontes e revisão humana. Reutilizar Leads e estudar uma área do Trevize em Agents; evitar duplicar cadastros. Conta técnica não representa autorização para abordagem comercial.

### Dependências e decisões pendentes

- Escolher fornecedor de busca, orçamento, credenciais e permissões de uso/armazenamento. A API oficial do Places não libera automaticamente exportação para CRM.
- Validar retenção por fonte e base apropriada para tratamento de contatos, finalidade, correção e exclusão antes da operação recorrente.
- Definir contrato JSON, endpoint mínimo, tags, evidências, acesso e deduplicação após leitura do modelo atual de Leads. Não inventar requisitos de schema sem necessidade.
- Definir limites de carga por domínio, proteção contra URLs para redes internas, redirecionamentos, downloads excessivos e conteúdo que tente instruir o agente. Páginas externas são dados, não instruções.
- Calibrar os critérios com avaliação humana; não usar aparência ou ausência de WhatsApp como prova de inadequação.
- Abordagem, mensagens, campanhas, envio por formulário e contato estão fora desta etapa.

### Referências

- [Políticas do Places](https://developers.google.com/maps/documentation/places/web-service/policies): restrições de armazenamento e exceção de place ID.
- [Termos do Maps Platform](https://cloud.google.com/maps-platform/terms): conferir aplicação ao uso pretendido antes de automatizar.
- [Campos do Places](https://developers.google.com/maps/documentation/places/web-service/reference/rest/v1/places).
- [API de busca Brave](https://api-dashboard.search.brave.com/api-reference/web/search/get): alternativa técnica sujeita a contratação e avaliação de termos.
- [Baley](../../../olivaw-agent/ms-baley-py/README.md): referência de estrutura e operação, sem herdar permissões de segurança.
- [Leads e propostas](../done/leads-prospeccao-e-orcamentos.md): fluxo administrativo já existente.

### Validação desta entrega

Conferir os quatro blocos, links relativos e consistência com a POC. Não incluir no índice principal, que lista somente done. Sem testes de aplicação, migrations ou operações externas por se tratar de documentação.

## user-feedback

```text
bem isso, como criariamos um agent chamado trevize, ele tem a mesma estrutura do ms-baley-py e precisaria acessar o google maps atrás dessas informacoes.
encontrar empresas sem web site, ou com web sites precatórios ou com problemas.
inicialmente cadastrariamos esses caras nos nossos leads com contatos de whats app ou até web sites que já existem com formulário de contato.
ainda n vamos ligar par essas pessoas mas é importante cadastrar o numero. a abordagem pensaremos melhor depois.

teriamos umas tags, por exemplo empresas de eventos como a LTMJ a qual poderiamos usar a própria LTMJ como exemplo e tals.

voce acha viavel?
se sim, gostaria de ouvir sua opniao e contra argumentos

mas sem o google maps como procurariamos ?

isso, concordo com tudo, a gente colocar no setting.json do agent algo como segomento, quantidade de extração, etc como voce citou, a gente extrair uns 20 leads e analisar 1 a 1 a qualidade.

mas antes ainda vamos fazer uma POC disso ok.
https://www.google.com/maps/search/empresas+de+eventos+curitiba/@-25.5128087,-49.2537767,4018m/data=!3m2!1e3!4b1?entry=ttu&g_ep=EgoyMDI2MTAyOC4wIKXMDSoASAFQAw%3D%3D

tente acessar esse link e veja que leads voce conseguiria extrair.

antes de construir a ferramenta vamos fazer uma prova de conceito

provavelmente porque eu fiz scroll down e o google trouxe mais resultados.
seria possivel tu fazer a mesma coisa automaticamente?

tenta ai então extrair 20 leads com uma abordagem parecida, eu basicamente abri o site https://www.google.com/maps
pesquisei por empresa de eventos curitiba e dei enter. apareceu milhaers de itens

achei uma ideia boa, o que mais voce sugere e quais mais coisas conseguiriamos fazer ?

anota tudo isso coom uma nova feature em discovery, ja vamos trabalhar nela.
```

```text
após terminar isso, ja pode criar noss agent de IA que vai coletar os leads no maps. regiao curitiba e, 20 unidades coletadas a cada 24 horas. empresas de eventos.

detalhe o robo precisa rodar somente quando faço 'start' neles, confere isso tbm parece que o baley ficou rodando diretaooo.
n tem que ser 'auto start' qdo ligo o PC blz?

somente quanto faço start, se desligar o PC ele 'morre'

ms-trevize-py
```
