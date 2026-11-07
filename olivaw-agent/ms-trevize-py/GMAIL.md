# Gmail e acompanhamento de leads

## Estado da entrega

Integração local por API oficial, com OAuth e comandos manuais. Inclui coleta pequena de validação, enriquecimento de leads já cadastrados, teste com destinatários controlados, abordagem aprovada, registro de respostas no CRM e reconciliação de falhas. Não inicia campanha, worker, envio ou resposta ao instalar/importar. Não usa a sessão do Firefox nem pede senha da conta.

Os testes usam mocks, fixtures e API em memória. A autorização da conta real exige criar o cliente OAuth e consentir no navegador. Não foi realizado deploy, coleta real, enriquecimento remoto ou envio real nesta entrega.

## Preparar o ambiente

Execute na raiz do monorepo, com as dependências do venv já instaladas. Para instalar o extra usando o pip do próprio ambiente:

```bash
olivaw-agent/ms-trevize-py/.venv/bin/python -m pip install -e 'olivaw-agent/ms-trevize-py[gmail]'
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads --help
olivaw-agent/ms-trevize-py/.venv/bin/trevize gmail status
```

Se o venv não tiver pip, preparar pip/venv antes; não instalar dependências no Python global. O extra Gmail é opcional para a descoberta de contatos.

A API atualizada precisa estar publicada pelo mantenedor antes de `enrich --apply` ou de qualquer operação comercial. O novo endpoint é `PUT /leads/{id}/communications`, protegido por `leads.manage`. A conta técnica continua sem root. A API e o agente usam o snapshot e o histórico existentes, sem migration nova.

## Validar uma coleta pequena

```bash
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads collect-preview --limit 3
```

Lê o Maps e os sites de até três novos candidatos, sem IA paga, sem autenticar/gravar no CRM e sem contato comercial. `previewEvidence` mostra os canais na saída para conferência. O estado local guarda apenas o resumo e IDs, sem contatos. Não modifica o setting versionado nem marca candidatos como entregues. Respeita a configuração de habilitação e o lock do worker: encerre um worker em execução antes de usar esta prévia.

Também é possível validar só os sites dos leads já cadastrados:

```bash
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads inspect --limit 3
```

Essa operação autentica no CRM para leitura e não altera fichas. Não confundir a ausência de formulário no HTML com inexistência de formulário dinâmico.

## Enriquecer os leads existentes

```bash
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads enrich --limit 40
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads enrich --limit 40 --apply
```

A primeira execução é uma prévia. A segunda lê novamente os sites e grava os canais. Sem `--id`, seleciona até o limite solicitado entre os leads de prospecção (`entry=prospecting`), na ordem da API; não presume que todos sejam do Trevize. Para escolher precisamente, use `--id UUID` repetido e um limite que comporte os IDs. Limite máximo: 100 por comando. Fichas sem qualificação são sinalizadas e não recebem texto gerado automaticamente.

Preserva abordagem, observações, tags, notas, etapa e links já revisados. Mescla até cinco e-mails, sem apagar os anteriores. Leituras inconclusivas não apagam canais. Usa a revisão atual e relê a ficha para confirmar a gravação. Conflitos interrompem o comando e exigem conferir a ficha; não sobrescrevem silenciosamente outra edição. O e-mail principal já existente permanece intacto; os endereços descobertos ficam em `qualification.publicEmails`, disponíveis para o piloto.

## Configurar OAuth do Google

1. No Google Cloud Console, escolha/crie um projeto e habilite a **Gmail API**.
2. Configure o Google Auth Platform: identidade do aplicativo, público e permissões. Para uma conta Gmail pessoal em modo de teste, adicione a conta Olivaw como usuário de teste.
3. Crie um cliente OAuth do tipo **Aplicativo para computador** e baixe o JSON. Mantenha o original fora do repositório.
4. Configure o remetente e um endereço de teste que você controle:

```bash
olivaw-agent/ms-trevize-py/.venv/bin/trevize gmail configure \
  --client-secrets /caminho/fora-do-repositorio/client_secret.json \
  --sender conta-olivaw@gmail.com \
  --test-recipient seu-email-de-teste@example.com

olivaw-agent/ms-trevize-py/.venv/bin/trevize gmail authorize
olivaw-agent/ms-trevize-py/.venv/bin/trevize gmail profile
```

`authorize` abre o consentimento no navegador, com callback local, PKCE e verificação do e-mail autorizado antes de salvar o token. Não é login por senha no terminal. São solicitados `gmail.send` e `gmail.readonly`: este último permite tecnicamente ler a caixa inteira; `inbox` é leitura explícita e a sincronização comercial consulta somente threads conhecidas. Não altera lido/não lido nem exclui mensagens.

Cliente, token, intenções e aprovações ficam em `.state/gmail/`, diretório 0700 e arquivos 0600 ignorados pelo Git. `--directory` permite outro diretório privado e deve aparecer antes do subcomando. Não compartilhar tokens, senhas ou o JSON OAuth em chats/logs. Tokens podem expirar/requerer novo consentimento, especialmente com o aplicativo externo em modo de teste. Distribuição para outros usuários pode exigir revisão do aplicativo e dos escopos pelo Google.

## Testar envio e leitura com nossos endereços

Crie `/tmp/trevize-teste.txt` com um texto de teste sem dados de clientes:

```bash
olivaw-agent/ms-trevize-py/.venv/bin/trevize gmail prepare \
  --key teste-inicial-001 --to seu-email-de-teste@example.com \
  --subject 'Teste de integração Olivaw' --body-file /tmp/trevize-teste.txt
```

Revise o conteúdo retornado e copie o `id` da intenção. Só então:

```bash
olivaw-agent/ms-trevize-py/.venv/bin/trevize gmail send --id ID_DA_INTENCAO --confirm
olivaw-agent/ms-trevize-py/.venv/bin/trevize gmail inbox --limit 5
olivaw-agent/ms-trevize-py/.venv/bin/trevize gmail replies
olivaw-agent/ms-trevize-py/.venv/bin/trevize gmail thread --id ID_DA_THREAD --include-body
```

Responda pelo endereço de teste para verificar a leitura. Estes comandos de teste não atualizam o CRM. Sem `--lead-id` neste teste. Para preparar uma resposta de teste, use `gmail reply --message-id ID_RECEBIDO --key teste-resposta-001 --body-file /tmp/trevize-teste.txt`, seguido de `gmail send --id NOVA_INTENCAO --confirm`. Respostas preservam assunto, thread e cabeçalhos de referência.

## Abordagem comercial piloto

Após validar o teste, escolha até cinco leads e revise seus e-mails e o texto individualmente. Cada endereço deve constar no e-mail principal ou nos e-mails públicos da ficha. Não é permitido aprovar o mesmo destinatário para dois leads no mesmo diretório. Leads inbound, encerrados, fora de Novo contato/Em conversa ou com tags `opt-out`, `do-not-contact`, `não contatar` ou `nao contatar` não entram no piloto.

```bash
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads prepare \
  --lead-id UUID_DO_LEAD --to contato@empresa.com.br \
  --subject 'Olivaw — tecnologia para a rotina da sua empresa' \
  --body-file /caminho/texto-revisado.txt --approve

olivaw-agent/ms-trevize-py/.venv/bin/trevize leads outbox
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads send --id ID_DA_INTENCAO --confirm
```

`prepare --approve` registra a aprovação e a intenção no CRM, sem enviar. O texto aprovado fica imutável; repetir o comando com o mesmo conteúdo recupera a mesma intenção. Alterar conteúdo depois da aprovação é bloqueado. A abordagem inicial tem chave canônica por lead, e o CRM impede outra intenção inicial para a mesma ficha. Esta versão não inclui cancelamento/reaprovação de intenções: revisar o arquivo antes de aprovar.

`send --confirm` confere novamente conta, destinatário, etapa e limites; registra uma reserva exclusiva/idempotente no CRM antes do Gmail. Somente a primeira reserva pode enviar. Sem API disponível ou persistência confirmada, não envia. A confirmação procura a mensagem específica em Enviados, sem afirmar entrega ou leitura pelo destinatário.

Cinco tentativas por dia UTC, compartilhadas entre testes e leads no diretório; cinco leads comerciais aprovados por piloto. São limites locais de um operador, não uma cota global distribuída. Não operar várias instalações para ampliar esses limites. A API oficial também está sujeita a políticas, quotas e filtros do Gmail.

## Sincronizar respostas e responder com aprovação

```bash
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads sync
```

Recupera registros de envios confirmados localmente, consulta as threads conhecidas e grava as respostas no histórico do lead antes de reconhecê-las localmente. Uma falha no CRM não consome a resposta. Repetições usam IDs idempotentes. Não depende do estado lido/não lido. Mensagens automáticas, remetentes inesperados ou threads associadas a mais de um lead exigem revisão. Uma mensagem enviada manualmente na thread registra `human-takeover` e bloqueia novos envios pelo agente para a ficha.

Eventos aparecem no histórico administrativo com data, operador, assunto, IDs Gmail/thread e trecho de até 1.500 caracteres. Não se armazenam anexos ou a caixa inteira no CRM. HTML sem alternativa texto simples exige consulta manual. Os metadados idempotentes ficam em `snapshot.communications`. Envio confirmado ou resposta recebida move apenas Novo contato para Em conversa; nunca regride uma etapa posterior. Preparação e reserva não avançam a etapa.

Depois de ler a resposta e revisar um novo texto:

```bash
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads prepare-reply \
  --lead-id UUID_DO_LEAD --to contato@empresa.com.br \
  --message-id ID_GMAIL_RECEBIDO --body-file /caminho/resposta-revisada.txt --approve
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads send --id ID_DA_RESPOSTA --confirm
```

Exige uma resposta já registrada pelo `sync`, no mesmo fio e do destinatário aprovado. Bloqueia `Reply-To` divergente, automações, assunto/referências inválidos e intervenção posterior à preparação. Não há resposta autônoma por IA nem envio periódico. Solicitações de não contato devem ser revisadas e marcadas com uma das tags de bloqueio antes de qualquer nova aprovação.

## Recuperação sem duplicar mensagens

Se houver timeout, erro ou `confirmation-pending`, não prepare outra abordagem e não apague a intenção:

```bash
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads outbox
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads reconcile --id ID_DA_INTENCAO
olivaw-agent/ms-trevize-py/.venv/bin/trevize leads sync
```

`reconcile` apenas procura a mensagem em Enviados e atualiza o CRM. Não reenvia. Uma queda após reservar no CRM e antes de enviar pode deixar uma intenção pendente sem mensagem: exige decisão manual; a ausência em uma consulta não autoriza repetir. Se o Gmail confirmar e o CRM falhar, `sync` pode completar o registro depois. Para testes sem lead, use `gmail reconcile --id ID`.

O snapshot comporta até 40 eventos de comunicação por lead, sujeito ao limite total de 32 KB; o diretório guarda até 500 intenções e 1.000 IDs recebidos. Atingir limites interrompe a operação para revisão. Não apagar histórico para contornar proteção contra duplicidade. Guardar backup privado do estado ao trocar de máquina; recuperação automática sem o diário local não está incluída.

## Validação local

```bash
PYTHONPATH=olivaw-agent/ms-trevize-py/src python3 -m unittest discover -s olivaw-agent/ms-trevize-py/tests -v
dotnet build ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj -m:1 --no-restore
dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj -m:1 --no-restore --no-build --filter FullyQualifiedName~LeadFeatureTest
```

Fontes: [Python quickstart](https://developers.google.com/workspace/gmail/api/quickstart/python), [envio](https://developers.google.com/workspace/gmail/api/guides/sending), [threads](https://developers.google.com/workspace/gmail/api/guides/threads), [OAuth para aplicativos instalados](https://developers.google.com/identity/protocols/oauth2/native-app), [escopos](https://developers.google.com/workspace/gmail/api/auth/scopes).
