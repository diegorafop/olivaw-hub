# Catálogo de planos, composição de custos e versões mensais

## user-history

Como operador da Olivaw, quero planos simples e acessíveis com histórico mensal por cliente, para vender recorrência sem perder condições contratadas.

## acceptance-criteria

- Quatro cards fixos: Bronze, Prata, Ouro e Diamante; detalhes navegáveis e breadcrumb.
- Separar preço comercial vigente, proposta e custo estimado. Ausência de valor não significa zero.
- Publicar alterações como versões imutáveis; preservar contratos e competências anteriores.
- Vincular cliente, competência ano/mês, versão e valor acordado; impedir dois vínculos efetivos concorrentes na mesma competência.
- Sugestões de upgrade futuras devem usar dados reais e nunca executar mudança automática.

## ai-prompt

Responsável: Codex. Issue026, branch feature/issue026-admin-client, workspace atual. Desenvolvimento concluído e validado localmente: catálogo, composição de custos, versões e vínculos. Sem commit, push ou publicação. Migration PostgreSQL complementar `Issue026PlansAndFinance` gerada e revisada localmente; preserva a `Issue026` publicada. Recomendações automáticas de upgrade e aplicação de quotas nos containers permanecem futuras. Não apresentar propostas locais como preço já cadastrado.

### Interface

Formulários permitem publicar versão explicitamente aprovada e vincular cliente/versão a uma competência; clientes e estudos são selecionados pelo nome e têm paginação. GET /plans/versions separa versão vigente e histórico; POST /plans/versions é idempotente por PublicationKey e digest, sem alterar contratos. POST /plans/assignments é idempotente para o mesmo acordo e rejeita outra condição na mesma competência. Plano pode vincular PlanStudy imutável: custo conhecido + horas × valor/hora + reserva ficam visíveis somente quando não há custos ausentes. O detalhe mostra as linhas de rateio do estudo e o formulário simplificado de composição; não confundir acumulado parcial de outubro com previsão mensal ao preencher as bases.

/admin/plans mostra quatro cards. /admin/plans/bronze, prata, ouro e diamante mostram detalhes. O estudo anterior permanece acessível em /admin/plans/simulator; /catalog redireciona ao catálogo principal. Todos seguem rootGuard. Bronze R$49 é hipótese solicitada, não valor contratual aprovado. Preços anteriores de 299/699/1299 são estudos históricos, não tabela vigente. Prata e Ouro aguardam novo cálculo. Todos os valores de apresentação em reais.

### Escopo comercial para a primeira venda

Bronze de R$49 é alvo de estudo, não oferta automaticamente aprovada. Site estático em endereço Olivaw, criação inicial em proposta separada, sem API/banco de negócio, sem pacote mensal de desenvolvimento evolutivo. Defeitos do escopo entregue continuam corrigidos sem cobrança adicional. Prata tem domínio próprio permitido, conteúdo gerenciável, API e PostgreSQL; Ouro tem gestão e múltiplas contas; Diamante é contrato customizado. Limites e franquias descritos inicialmente continuam propostas até serem registrados nas condições aprovadas da versão.

Antes de oferecer: preencher as bases reais, comparar cenário com clientes pagantes atuais e projetados, definir criação inicial e condições de atendimento, publicar explicitamente a versão aprovada e registrar o aceite do cliente. Cadastro de cliente não o torna pagante nem cria contrato automaticamente. Não divulgar capacidade da VM ou reserva dedicada sem medir. Nenhum preço foi publicado ou atribuído a cliente real nesta execução.

### Modelo implementado e evolução

As quatro chaves estáveis são bronze/prata/ouro/diamante, sem criar uma tabela redundante de nomes fixos. PlanVersion guarda versão publicada imutável, capacidades, condições, mensalidade, aprovação e vigência mensal. Não há edição de uma publicação. Reutilizamos CustomerPlan para vincular cliente, PlanVersionId opcional, vigência no primeiro dia do mês, valor contratado BRL e aprovação. A condição permanece até a próxima alteração; assim não se exige copiar o mesmo contrato em todos os meses. Restrição única cliente+início de vigência preserva concorrência. Condições legadas permanecem válidas, sem associação automática a um catálogo. Se houver múltiplos serviços independentes por cliente, definir assinatura como dimensão adicional antes de implementar. Não confundir vigência do catálogo com renovação automática do contrato. Cancelamento, mudança no meio do mês e proporcionalidade precisam de regra explícita; proposta inicial: mudanças na próxima competência, sem rateio intramês.

### Composição implementada

O detalhe inclui formulário simplificado com oito bases em BRL: VM, armazenamento, rede/IP, backup, domínio, IA/ferramentas, licenças/serviços e outros custos. Cada linha informa valor, período mensal/anual e clientes que dividem a base. Domínio começa com R$40/ano informados pelo operador; demais bases e quantidades precisam ser preenchidas. Zero é explícito, não substitui valor desconhecido. Horas de operação, custo/hora, reserva, margem e despesas comerciais entram na conta. Hipóteses iniciais 15/20/6% são identificadas como editáveis, sem afirmar alíquota fiscal.

POST `/plans/cost-sheets` persiste as oito versões de custo e o PlanStudy em uma única unidade de trabalho; chave e digest impedem duplicação por reenvio. `CostSheetData` string JSON preserva valor anual original, denominadores e demais entradas, além das parcelas mensais normalizadas. O operador pode recuperar um estudo vigente como base sem alterá-lo. Estudos antigos preservam despesas comerciais zero; o simulador anterior também mantém o novo campo ao copiar um cenário. Margem + despesas comerciais precisa ser menor que 100%. Resultado definitivo é confirmado pela API; prévia local é aproximada. Estudo salvo vincula o formulário da próxima versão, sem publicar sozinho. Condições da versão vigente mostram capacidade e preço reais separados da proposta do card. Guard protege saída com alterações não salvas. Ao escolher cliente e competência, o operador consulta a condição vigente no início do mês e o histórico paginado, com versão, valor, vigência e aprovação; a consulta é atualizada após registrar uma condição.

### Formação de preço proposta

Custo mensal = parcela de infraestrutura + armazenamento/backup + ferramentas compartilhadas + tempo esperado de operação e suporte + reserva de risco. Preço sugerido = custo / (1 - proporção de despesas comerciais - margem desejada). Percentuais precisam somar menos que 100%. Custos e despesas ficam em BRL; preservar dólar e taxa de conversão como evidência. Não usar automaticamente o câmbio de R$5,50 como cotação oficial. Em mês aberto, taxa provisória identificada; fechamento conforme cobrança ou política mensal documentada.

Ratear infraestrutura em grupos (estático, API/gestão e infraestrutura comum), com pesos medidos por RAM reservada, CPU observada em janelas e armazenamento provisionado. Não somar percentuais de unidades diferentes nem cobrar duas vezes o mesmo custo compartilhado. Custos fixos por pool devem ser reconciliados ao total real. Capacidade ociosa e risco não desaparecem ao distribuir pelos clientes projetados: mostrar cenários com clientes pagantes reais e pretendidos separadamente.

Bronze R$49: site estático, endereço Olivaw, 2 GiB propostos, sem API/DB de negócio e sem horas mensais garantidas de melhoria. Criação e evolução cobradas separadamente; defeitos do escopo continuam com correção sem cobrança adicional. A ausência de conteúdo dinâmico reduz trabalho, mas não elimina atualizações, incidentes, atendimento e backups. Hipótese de custo: infra R$82,50 + ferramenta R$550 = R$632,50/mês. Com 5 clientes iguais, R$126,50 por cliente antes de suporte; com 25, R$25,30. A R$49, com 6% de provisão ilustrativa e 20% de margem sobre receita, sobram R$36,26 para todos os custos; só os fixos exigiriam ao menos 18 clientes (632,50/36,26 arredondado para cima), antes de suporte/backup. Isso NÃO comprova que a VM comporta 18 ou 25 sites. Ferramenta compartilhada deve ser rateada entre todos os produtos que a utilizam, sem escondê-la no cálculo.

### Capacidade e evolução

Teste inicial de estático: teto 0,10 vCPU e 64 MiB por container, sujeito a medição e carga antes de aplicar. Não alterar containers reais nesta entrega. Somar limites/necessidades com proxy, PostgreSQL, APIs, sistema e margem operacional; a VM atual de 1 GiB já tem pressão de memória. CPU limitada é teto de uso, não núcleo dedicado. RAM insuficiente pode encerrar processos. Limite Docker de memória/CPU não impõe quota de arquivos/DB; armazenamento requer mecanismo e monitoramento próprios. Banco compartilhado não permite atribuir o limite inteiro do container a cada cliente. Build não deve consumir a reserva de runtime dos sites.

Upgrade futuro: armazenamento acima de 80% em leituras recentes ou saturação sustentada de CPU/RAM, combinada com latência/erros. Acessos isolados não justificam upgrade; podem ser bots e o site pode estar saudável. Exibir motivo, período, dados faltantes e opção de contato, sem mudar contrato ou limitar serviço automaticamente.

Fontes primárias consultadas em 27/10/2026: [limites Docker](https://docs.docker.com/engine/containers/resource_constraints/) e [hospedagem estática AWS](https://docs.aws.amazon.com/lightsail/latest/userguide/amazon-lightsail-static-site-hosting-tutorial.html). Usar medições próprias para dimensionamento; documentação não confirma capacidade desta VM.

### Publicação e validação

Sem commit, push ou deploy. A API aplicará a migration complementar automaticamente no startup após publicação pelo mantenedor; não houve conexão a banco real ou database update. Ela inclui as estruturas de faturas/controle financeiro iniciadas no mesmo worktree antes do foco ser restringido a Planos, além de seis colunas aditivas e índices/FKs restritivos; revisar o pacote completo antes de publicar. PostgreSQL é o único provedor com migration. Segurança e autorização testadas com identidades sintéticas; não foram alterados containers, limites reais ou contratos de clientes.

### Evidências de validação

446 testes da API Olivaw passaram, incluindo modelo PostgreSQL compatível com a migration, matemática do preço, idempotência, conflitos, histórico e isolamento do portal. 96 testes da API DevOps passaram, incluindo rejeição de outro issuer, audience inválida e token expirado. 22 testes E2E passaram para catálogo, composição de custos, publicação/vínculo de versão, configuração, faturas, financeiro manual e portal em desktop/celular. Build e lint do frontend passaram; somente avisos de orçamento SCSS já existentes. Não houve chamadas pagas de IA, banco real ou publicação.

A migration é `20261030222928_Issue026PlansAndFinance`: sete tabelas novas, seis colunas aditivas, índices e FKs RESTRICT. SQL gerado entre Issue026 e a complementar não remove tabelas/colunas no upgrade; defaults preservam estudos antigos e verificações manuais. `Down` não deve ser executado em produção como mecanismo de recuperação sem plano próprio, pois remove as estruturas novas e seus dados.

## user-feedback

entao já muda essa tela ai, teremos os cards fixos.

em cada card podemos ter uma visualização com mais informacao né como voce mesmo citou. o valor atual do plano e o valor estimado por exemplo.
valor estimado ainda vamos ver como calcular.

acho que os planos quando alterados geram uma nova versão do plano né, o qual é vinculado a um cliente no ano/mes especifico.
um mesmo cliente pode ter planos diferentes em diferentes meses ou ainda o mesmo plano com versoes diferentes.

o calculo do plano voce vai sugerir como fazer, baseado nas caraceristicas do plano, algo como 2gb de espaço, memoria etc. Ai limitariamos o uso de CPU e RAM por container de acordo com o plano.

e na UI do cliente, futuramente, poderiamos sugerir que ele aumente o plano pois tem muita gente acessando ,o espaço dele ta chegando ao fim por muitas fotos.

quero que pensemos em planos de facil acesso, tipo bronze por exemplo algo como 49 mes algo assim... VAMOS entender os valores mas penso que ganharemos na recorrencia as pessoas vao deixar o site estatico por meses ate anos e n teremos manutençao nenhuma, só pagar a conta da aws.


no calculo do custo tudo tem que ser colocado na conta, penso da gente ter alguma forma como 'details' do plano e lá uma tabela alguma coisa que faz a 'conta' tem gasto de X por mes com VM, gasto Y com o armazenameno, gasto Z com o dominio (esse voce n registrou né, mas pode lançar já, foi 40 reais em 12 meses começando em outubro, dive 40/12 e teremos o custo do dominio nos meses, ja pode lançar no futuro tbm

vamos nos concentrar em fechar as features dos planos para poder começar trabalhar em adquirir clientes
