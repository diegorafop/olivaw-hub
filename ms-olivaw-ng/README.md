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

Docker: `docker compose -f docker/docker-compose.yml -f docker/docker-compose.override.yml up --build -d`. Configure CORS na API para a origem do frontend. Não há proxy de API, refresh token nem persistência da sessão. Consulte o [template](../ms-limpo-ng/README.md) para os padrões compartilhados. Arquivos existentes no destino são preservados; a geração recusa conflitos e não atualiza projetos já gerados.

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

Somente “Enviar para análise da Olivaw”, com e-mail, persiste o snapshot na API. Uma chave aleatória exclusiva da jornada protege atualizações sem usar o JWT administrativo. Revisões permitem repetir um envio sem criar outro lead. Falhas preservam o texto e oferecem nova tentativa ou cópia do resumo. Não há uploads, orçamento automático ou análise automática de requisitos.

Validação específica: `npm run build` e `npm run e2e -- site-studio.spec.ts project-folder.spec.ts project-wizard.spec.ts landing.spec.ts`, dentro de `ms-olivaw-ng`.

## Clientes e contato

O menu oferece “Clientes” e “Contato” no desktop e no celular. A seção de clientes fica após soluções, em uma faixa compacta e clara, com a marca tipográfica da LTMJ Eventos em cinza e link externo para `https://ltmj.olivaw.com.br/`. A marca segue o lettering usado no cabeçalho do próprio cliente. A LTMJ foi confirmada pelo mantenedor; não há logos fictícios ou vagas de clientes simuladas. As capturas reais em `public/assets/clients/ltmj-desktop.jpg` e `ltmj-mobile.jpg`, realizadas em 20/10/2026, ficam preservadas como material de referência e não são exibidas na faixa.

A seção de contato começa pelo formulário direto de e-mail e mensagem. “Quero organizar minha ideia” abre a descoberta guiada, preservando suas opções. Digitação e troca de caminho não enviam dados. “Enviar mensagem” usa o mesmo contrato `PUT /leads/{id}`, com identificação própria, sem sobrescrever o rascunho do estúdio. O recebimento é confirmado apenas após resposta da API; falhas mantêm a mensagem e permitem repetir a tentativa sem criar outro lead. Nenhum telefone ou e-mail da Olivaw foi inventado ou exposto.

Validação específica: `npm run e2e -- clients-contact.spec.ts landing.spec.ts`. O envio é interceptado nos testes, sem gravação em API remota. A persistência em banco continua dependendo das migrations documentadas na API.
