# Verificação da entrega inicial

## Resultado local

Execução em 24/10/2026, branch `feature/issue024-cavalcante`. Sem início do host da API, acesso a bancos, aplicação de migrations, finish, push ou implantação.

| Verificação | Resultado |
| --- | --- |
| Build da API e dependências, Release `-m:1` | Aprovado, 0 avisos e 0 erros |
| Testes .NET herdados do esqueleto | 8 aprovados |
| Build Angular de produção | Aprovado; 1 rota pública prerenderizada |
| Testes unitários Angular herdados | 6 aprovados em 3 arquivos |
| ESLint | Aprovado |
| Prettier | Aprovado |
| Playwright | 5 aprovados: 3 de identidade herdados e 2 da landing |
| Compose de produção do frontend | Configuração válida |
| Revisão visual desktop e celular | Prévias capturadas e conferidas |
| Browser console | Sem erros de página durante captura |
| Fotos locais | Nenhuma imagem quebrada na captura ou no teste móvel |
| Overflow horizontal | Ausente em 1440 × 1000 e 390 × 844 |
| HTML sem execução de JavaScript | Conteúdo público presente no arquivo prerenderizado |
| npm audit | 0 vulnerabilidades após patch compatível de fast-uri |

O aviso inicial era transitivo, em `fast-uri` 3.1.8. `npm update fast-uri` atualizou somente o pacote para 3.1.9 no novo cliente. O template e as famílias .NET não foram atualizados.

## Comandos executados

Na raiz:

```bash
bash ms-limpo-api/scripts/new-microservice.sh cavalcante
node ms-limpo-ng/schematics/generate.cjs --name ms-cavalcante-ng
dotnet build ms-cavalcante-api/tests/Owh.Cavalcante.Tests/Owh.Cavalcante.Tests.csproj -c Release -m:1
dotnet test ms-cavalcante-api/tests/Owh.Cavalcante.Tests/Owh.Cavalcante.Tests.csproj -c Release --no-build --no-restore
docker compose --env-file ms-cavalcante-ng/docker/.env.prd -f ms-cavalcante-ng/docker/docker-compose.yml -f ms-cavalcante-ng/docker/docker-compose.prd.yml config --quiet
git diff --check
```

Dentro de `ms-cavalcante-ng`:

```bash
npm ci
npm update fast-uri
npm run lint
npm run format:check
npm test
npm run build
npm run e2e
```

Aumentado apenas no cliente o orçamento de CSS por componente de 4/8 kB para 16/18 kB, para acomodar a landing responsiva completa. O limite inicial de JavaScript/CSS permanece 500 kB/1 MB; o build resultou em aproximadamente 394 kB iniciais (103 kB estimados na transferência). Valores são saída de build, não medição de Core Web Vitals.

Os testes da landing verificam filtro, retorno a todas as máquinas, contexto da mensagem, FAQ, menu móvel, fotos e ausência de chamadas de identidade na página pública. Os links externos são inspecionados; nenhuma mensagem é enviada.

## Contraste de cores sólidas

Cálculo de luminância relativa sRGB conforme WCAG:

| Combinação | Razão |
| --- | --- |
| Grafite / amarelo | 9.86:1 |
| Grafite / mineral | 13.88:1 |
| Texto secundário / mineral | 5.75:1 |
| Branco / grafite | 14.92:1 |

Todas essas combinações superam 4,5:1. Esse cálculo não certifica o contraste sobre fotografias nem substitui avaliação de todos os estados.

## Evidências visuais

- [Desktop](preview-desktop.png), 1440 px de largura.
- [Celular](preview-mobile.png), 390 px de largura.
- [Avatar vetorial](../../ms-cavalcante-ng/public/tractor.svg).

## Limites e pendências

- Não foi executada a solução inteira nem os testes de outros clientes; a validação .NET se concentrou no esqueleto gerado e suas dependências. Oracle é somente reserva de projeto; não foi validado como provedor funcional.
- Não foi construída imagem Docker local nem testado runtime Nginx em container. Foi validado o Compose de produção e a configuração será verificada pelo workflow quando for enviado e executado; nenhuma Action foi executada remotamente nesta tarefa.
- Sem testes com leitores de tela, usuários reais ou medição Lighthouse/CrUX. Foco, semântica e responsividade são uma base para auditoria AA, sem declaração de conformidade completa.
- Sem DNS, TLS ou disponibilidade de produção verificados. O protótipo continua sem indexação. Horários, cobertura, condições comerciais e direitos das fotos precisam de confirmação antes da publicação.
