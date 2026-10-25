# Primeiros passos do Nicolas

## Objetivo desta etapa

Aprender a transformar uma necessidade real em conteúdo, interface e critérios verificáveis. Primeiro entender o que o visitante procura; depois escolher componentes. O estudo distingue fatos lidos no site, decisões do protótipo e dúvidas para o cliente.

## Rodar o frontend

Na raiz do monorepo, com Node 24 e npm 11:

```bash
cd ms-cavalcante-ng
npm ci
npm start
```

Abrir `http://localhost:4200`. O catálogo, menu, dúvidas e links funcionam sem iniciar a API. Links de WhatsApp, telefone e mapa são reais; para verificar, inspecione o endereço e não envie mensagens de teste ao cliente.

## Encontrar cada parte

| Arquivo/pasta | Responsabilidade |
| --- | --- |
| `ms-cavalcante-ng/src/app/features/public/public-page.ts` | Conteúdo estático, filtro e estado do menu com Signals |
| `ms-cavalcante-ng/src/app/features/public/public-page.html` | Estrutura semântica e bindings Angular |
| `ms-cavalcante-ng/src/app/features/public/public-page.scss` | Composição e adaptação ao celular |
| `ms-cavalcante-ng/src/theme/_brand.scss` | Tokens de identidade |
| `ms-cavalcante-ng/public` | Imagens, avatar, favicon e configuração pública |
| `ms-cavalcante-ng/e2e/landing.spec.ts` | Interações principais em navegador |
| `ms-cavalcante-api/src/Owh.Cavalcante` | Futuras entidades no Domain e contratos nas pastas próprias |
| `ms-cavalcante-api/src/Owh.Cavalcante.Api` | Host e infraestrutura herdados; sem nova regra de negócio |

Frontend apresenta a experiência; backend futuramente valida regras e acessos. A solução `Olivaw.sln` agrupa projetos, sem ser uma aplicação. O HTML da landing é produzido no build; alterar conteúdo exige novo build.

`public/config.json` é público. Segredos da identidade pertencem à API e não podem aparecer no frontend. Os `.env` gerados seguem a decisão do mantenedor para este repositório privado; não imprimir seus valores.

## Exercícios sugeridos

- Explicar por que serviço e máquina são grupos diferentes e localizar cada exemplo no estudo.
- Usar apenas Tab, Shift+Tab e Enter para chegar a um orçamento e abrir uma dúvida.
- Filtrar por transporte e explicar `signal`, `aria-pressed` e o retorno para “Todas”.
- Conferir o site em 390 px e com zoom de 200%; registrar onde o conteúdo deixa de ser legível.
- Comparar três combinações de cor usando contraste calculado, sem escolher só pela preferência pessoal.
- Escrever uma história e critérios de aceite para página de máquina antes de criar código.
- Simular uma entrevista: o cliente sabe a máquina ou apenas o problema do terreno? Não sugerir a resposta.

Exercícios não autorizam alterar templates compartilhados, gerar migrations, publicar ou implementar o backlog.

## Verificar uma mudança

Dentro de `ms-cavalcante-ng`:

```bash
npm run lint
npm run format:check
npm test
npm run build
npm run e2e
```

O build deve manter conteúdo prerenderizado em `dist/ms-cavalcante-ng/browser/index.html`. Testes de identidade herdados simulam HTTP; não precisam de usuário ou banco real. Ler o [Git Flow](../../gitflow.md) antes de iniciar outra tarefa. A tarefa atual usa issue #24; não criar branch ou commit sem combinar o fluxo com o responsável.
