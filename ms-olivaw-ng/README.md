# ms-olivaw-ng

Frontend gerado do template **Olivaw**, com build e publicação independentes da API.

## Desenvolvimento

Node 24 e npm 11.

```bash
npm ci
npm start
npm run build
npm test
npm run lint
npm run format:check
npx playwright install chromium
npm run e2e
```

Settings públicos: `public/config.json`. Tema: `src/theme/_brand.scss`. Site: https://olivaw.com.br. API: https://api.olivaw.com.br.

Docker: `docker compose -f docker/docker-compose.yml -f docker/docker-compose.override.yml up --build -d`. Configure CORS na API para a origem do frontend. Não há proxy de API nem refresh token. A sessão é preservada no `sessionStorage` da aba até expirar, com revalidação na API ao recarregar. Consulte o [template](../ms-limpo-ng/README.md) para os padrões compartilhados. Arquivos existentes no destino são preservados; a geração recusa conflitos e não atualiza projetos já gerados.

## Landing page institucional

A landing page foi implementada como componentes Angular em `src/app/features/public`, a partir do template. `docs/prot_tipo_da_landing_page.html` é apenas referência visual. As decisões atuais estão em [Implementação](docs/implementacao.md).

- Site: `https://olivaw.com.br`.
- API futura e independente: `https://api.olivaw.com.br` em `public/config.json`.
- A pasta do projeto reúne o rascunho na aba. O envio explícito para análise usa `PUT /leads/{id}` da API Olivaw, sem cadastro.
- Telefone, e-mail, redes sociais, demonstrações e clientes não são inventados. Confirmar esses conteúdos antes de divulgar o canal de atendimento.
- Material permanece nos formulários e Tailwind no layout, conforme o padrão universal do template. A marca é configurada em `src/theme/_brand.scss`.
- DNS, TLS, hospedagem e API não foram criados/publicados por essa geração.

Os E2E simulam a API futura; não dependem de esse domínio responder. A raiz é prerenderizada e inclui o conteúdo institucional no HTML do build. Não há clientes ou resultados fictícios publicados.

## Ambientes AWS

Docker segue o padrão `dev/qas/rls/stg/prd` das APIs, com configurações públicas por ambiente e Traefik em HTTPS. Consulte [Docker e ambientes](docker/README.md) para executar o deploy com os dotenv já versionados. A URL da API é configurada por `API_URL` no container; `public/config.json` é usado no desenvolvimento com Angular CLI.

## Estúdio de ideias

A rota `/estudio` demonstra três intenções: apresentar a empresa, vender produtos e organizar o negócio. Cada intenção começa com uma prévia simples e quatro possibilidades desmarcadas. As opções adicionam e removem partes da demonstração, com animações que respeitam movimento reduzido.

“Quero criar do meu jeito” abre `/projeto`, a pasta visual com escolhas editáveis, requisitos escritos, serviços Olivaw, planejamento MVP/E01/E02 e planos conceituais Bronze/Prata/Ouro/Diamante. Preços, capacidades e franquias aguardam definição comercial.

A pasta conduz por cinco etapas: ideia, serviços, primeira entrega, parceria e revisão/envio. Nome do negócio (pode ser provisório) e ideia escrita habilitam o primeiro avanço. O indicador permite revisitar etapas já vistas; texto, escolhas e etapa atual são preservados na aba. Cards recebem cor gradualmente e uma linha percorre entregas e planos, na horizontal em desktop e vertical em telas menores. Os efeitos não bloqueiam o avanço e respeitam `prefers-reduced-motion`.

O `ProjectDraft` reúne um JSON versionado desde a entrada, com identificador aleatório, origem, opções, texto e até 40 eventos de navegação. O rascunho fica no estado e no `sessionStorage` da aba; gravações locais são agrupadas em 400 ms e não chamam a API. Recarregar preserva o rascunho. Fechar a sessão da aba encerra esse armazenamento, sujeito ao comportamento de restauração do navegador.

Somente “Enviar para análise da Olivaw”, com e-mail ou WhatsApp válido, persiste o snapshot na API. Uma chave aleatória exclusiva da jornada protege atualizações sem usar o JWT administrativo. Revisões permitem repetir um envio sem criar outro lead. Falhas preservam o texto e oferecem nova tentativa ou cópia do resumo. Não há uploads, orçamento automático ou análise automática de requisitos.

Validação específica: `npm run build` e `npm run e2e -- site-studio.spec.ts project-folder.spec.ts project-wizard.spec.ts landing.spec.ts`, dentro de `ms-olivaw-ng`.

## Dashboard interno da Olivaw

`/admin` apresenta a primeira versão visual da operação: navegação responsiva, identificação do operador, menu lateral recolhível para ícones no desktop, acolhimento com o mini Olivaw e regiões para clientes, sistemas, atendimento, prioridades e histórico. O título é `Olivaw - Operação`.

A entrada exige sessão válida da identidade interna e root ou a permissão `leads.manage`. Operadores com essa permissão entram diretamente em Leads; Clientes e Visão geral continuam exclusivos de root. Contas sem ambas as autorizações seguem para acesso negado. O guard de interface não substitui autorização na API; a consulta de leads descrita abaixo já é autorizada na API; o dashboard não oferece ações remotas.

Não há contadores ou gráficos simulados. O resumo de leads usa a API real. As áreas deixam explícito que cadastros de clientes, monitoramento, solicitações e histórico operacional ainda não foram integrados. O ambiente aparece como “Ambiente não informado”, pois não existe configuração pública verificada para esse contexto; a URL da API não é usada para inferir o ambiente.

O login armazena somente token e expiração no `sessionStorage`, com chave própria por URL de API. O bootstrap carrega a configuração e consulta `/identity/authentication/access` antes de restaurar a sessão e suas permissões atuais. Recarregar, inclusive com cache desabilitado, preserva o login durante a validade do token. Falha na revalidação encerra a sessão local; não existe renovação automática. Logout, expiração e respostas 401 autenticadas removem o token armazenado. Falha na revogação informa a limitação e conclui a saída local. Atalhos apontam somente para a própria visão geral e para o site público.

Validação: `npm run build`, `npm run lint` e `npm run e2e -- auth.spec.ts admin-dashboard.spec.ts`. Testes de navegador simulam a identidade, sem credenciais ou chamadas à API real. Ver [escopo da primeira feature](../docs/features/done/01-dashboard-administrativo.md).

## Leads administrativos

`/admin/leads` lista os contatos recebidos pelo Estúdio e pelo formulário direto, com pesquisa por negócio/título/e-mail/WhatsApp, origem e datas de recebimento. A paginação é executada pela API, com 20 itens por página. Selecionar um lead abre sua ficha e preserva o recorte na navegação. No celular, a ficha vem antes da lista. Datas são escolhidas no fuso do navegador e enviadas como instantes com limites inclusivo/exclusivo à API.

A ficha apresenta ideia, possibilidades incluídas/excluídas, plano de interesse e histórico, aceitando contatos antigos sem WhatsApp. Links de e-mail e WhatsApp apenas abrem o canal por ação do operador; não enviam mensagens automaticamente. Etapas e notas internas foram acrescentadas no acompanhamento descrito adiante. Não há responsável atribuído ou exportação; a conversão manual em cliente está disponível para root.

O dashboard consulta `GET /leads/summary`, exibindo totais recebidos e dos últimos sete dias, com data de consulta. Não são contadores de não lidos. Se a API não estiver disponível/atualizada, aparece falha com nova tentativa, sem números demonstrativos. O layout e o encerramento da sessão ficam no componente pai das rotas administrativas, mantendo o menu recolhido ao navegar entre páginas.

A API exige root ou `leads.manage` vigente também no servidor. O acompanhamento exige a API atualizada e a migration PostgreSQL `Issue017`; somente atualizar o site não basta. Ver [contratos da API](../ms-olivaw-api/README.md#consulta-administrativa-de-leads) e [estado da feature](../docs/features/done/02-leads-administrativos.md).

Verificação: `npm run build`, `npm run lint` e `npm run e2e -- admin-leads.spec.ts admin-dashboard.spec.ts auth.spec.ts`. Os testes usam dados simulados e não acessam leads reais ou a API remota.

## Clientes e contato

O menu oferece “Clientes” e “Contato” no desktop e no celular. A seção de clientes fica após soluções, em uma faixa compacta e clara, com a marca tipográfica da LTMJ Eventos em cinza e link externo para `https://ltmj.olivaw.com.br/`. A marca segue o lettering usado no cabeçalho do próprio cliente. A LTMJ foi confirmada pelo mantenedor; não há logos fictícios ou vagas de clientes simuladas. As capturas reais em `public/assets/clients/ltmj-desktop.jpg` e `ltmj-mobile.jpg`, realizadas em 20/10/2026, ficam preservadas como material de referência e não são exibidas na faixa.

A seção de contato começa pelo formulário direto de e-mail e mensagem. “Quero organizar minha ideia” abre a descoberta guiada, preservando suas opções. Digitação e troca de caminho não enviam dados. “Enviar mensagem” usa o mesmo contrato `PUT /leads/{id}`, com identificação própria, sem sobrescrever o rascunho do estúdio. O recebimento é confirmado apenas após resposta da API; falhas mantêm a mensagem e permitem repetir a tentativa sem criar outro lead. Nenhum telefone ou e-mail da Olivaw foi inventado ou exposto.

Validação específica: `npm run e2e -- clients-contact.spec.ts landing.spec.ts`. O envio é interceptado nos testes, sem gravação em API remota. A persistência em banco continua dependendo das migrations documentadas na API.

## Imagem de compartilhamento

As tags Open Graph e Twitter apontam para `public/assets/brand/olivaw-share-v3.jpg`, uma cópia JPEG de 720 × 720 do avatar atual, com fundo claro. A família visual agora usa a órbita mais fina com arco ciano no lugar da lâmpada circular: símbolo `olivaw-avatar-web-v4.png`, reação da carta `olivaw-message-welcome-v3.webp`, coordenador administrativo `olivaw-operations-coordinator-v3.webp`, favicon PNG/ICO e Apple Touch dedicados. Originais e prompts ficam em [Mini Olivaws](design/brand/avatars/README.md).

Para regenerar a prévia com os mesmos pixels de origem, sem redesenhar o robô:

```bash
node scripts/generate-social-preview.mjs
npm run build
```

O script usa o Chromium do Playwright já instalado no projeto e limita a saída a 150 KB. Após publicar, conferir se a URL da imagem responde como `image/jpeg` e testar um novo compartilhamento. O nome separado diferencia o asset da imagem antiga, mas não garante atualização imediata do cache do WhatsApp para a página. Alterações locais não atualizam a prévia publicada.

## Clientes administrativos

`/admin/customers` permite buscar por empresa, razão social ou CNPJ e filtrar situação, com paginação pela API. `/admin/customers/new` prepara cadastro; `/admin/customers/:id` apresenta edição, contatos, situação e histórico mínimo. A ficha do lead oferece “Cadastrar como cliente”: apenas prepara os campos e conserva origem, sem gravar até a confirmação. Nome do contato precisa ser conferido/preenchido pela equipe. Clientes ficam sem login nesta etapa.

A Visão geral consulta `/customers/summary`; números representam situação cadastral, não saúde técnica. Falha de API aparece como indisponibilidade, sem dados demonstrativos. A persistência no ambiente depende da migration e publicação descritas no [README da API](../ms-olivaw-api/README.md#cadastro-administrativo-de-clientes); a migration PostgreSQL `Issue013Customers` foi gerada e revisada, mas não aplicada em banco nesta entrega.

O formulário aceita até oito contatos com nome e pelo menos um canal válido; WhatsApp tem máscara e é enviado só com dígitos. Revisão evita sobrescrita concorrente; erro conserva os campos. Mudança de situação exige confirmação e não altera cobrança ou sistemas. Sair com alterações não salvas pede confirmação; expiração da sessão e logout continuam liberando a navegação de segurança.

```bash
npm run build
npm run lint
npm run e2e -- admin-customers.spec.ts admin-dashboard.spec.ts admin-leads.spec.ts
```

Os testes interceptam a API e não cadastram empresas reais, enviam convites ou criam usuários.

## Acompanhamento de leads

A ficha oferece Novo contato, Em conversa, Em análise, Em desenvolvimento, Em validação, Entregue e Sem continuidade. Todas as transições são livres e não há responsável atribuído. A lista compacta ocupa cerca de 30% do espaço ao abrir uma ficha no desktop; os detalhes recebem o restante. Em telas menores, os detalhes ficam antes da lista.

Notas internas de até 3.000 caracteres entram no histórico com autor e data; não podem ser editadas ou excluídas pela interface. O histórico é consultado em blocos de 20 alterações, com navegação para anteriores e retorno às mais recentes. Não são notas de versão públicas.

Revisão própria evita sobrescrever uma alteração concorrente. Em conflito, a interface mantém a nota digitada e exige recarregar/conferir a ficha antes de salvar novamente. Filtro por etapa e contagens gerais usam estados persistidos; as contagens gerais não acompanham os filtros da lista. Snapshot, etapa pública e revisão do visitante são independentes do acompanhamento.

Root e operadores internos com `leads.manage` podem consultar e atualizar o acompanhamento. A permissão é conferida no servidor usando a identidade existente; nenhum perfil ou usuário externo é criado automaticamente. O portal e os perfis propostos continuam no planejamento.

Validação local desta entrega: `npm test`, `npm run lint`, `npm run build` e `npm run e2e -- e2e/admin-leads.spec.ts e2e/admin-dashboard.spec.ts e2e/admin-customers.spec.ts`. O navegador usa contratos simulados, sem gravar dados remotos. A API e sua migration PostgreSQL precisam ser publicadas antes de usar o acompanhamento no ambiente.
