# Gestão de portfólio — issue 009

## Acesso e uso

- Login: `/gestao/entrar`; painel: `/gestao`. Não há link no menu público nem rota `/admin`.
- A rota discreta não substitui segurança: JWT válido e permissão `portfolio.manage` são exigidos. Root mantém o acesso previsto pela identidade compartilhada.
- Por decisão do responsável, a equipe usará o login `ltmj`, na identidade da própria API LTMJ, com a autorização `portfolio.manage` em um perfil dedicado. Root de cada cliente é exclusivo do Olivaw Hub. A conta ainda não foi criada no servidor; e-mail definido como `ltmj@teste.com.br`, sem confirmação por e-mail, e senha obrigatória sem mínimo adicional.
- Sessão permanece em memória, conforme o template. Recarregar a página exige novo login; refresh token continua fora do escopo.
- Cadastro exige título (até 150 caracteres), descrição (até 4.000) e uma das quatro categorias acordadas.
- Novos eventos começam ocultos. Salvar, conferir e ativar **Exibir no site**. A edição preserva a visibilidade atual.
- É possível adicionar fotos ao editar. Exclusão definitiva do evento pede confirmação e remove suas fotos; não há lixeira.

## Fotos

Selecionar várias imagens no celular, revisar as prévias e remover seleções antes de salvar. A primeira foto cadastrada é a capa. Upload em `multipart/form-data`, campo repetido `files`; o navegador define o boundary. JWT e loading global passam pelos interceptors existentes.

Limites: JPEG, PNG ou WebP, 10 MB por foto, 20 fotos e 100 MB por requisição, 100 fotos por evento. O frontend divide o envio em lotes de até 10 fotos. A API valida tamanhos e assinatura inicial dos formatos, gera nomes próprios e não utiliza o nome enviado como caminho. Não faz recodificação, redimensionamento ou remoção de metadados nesta versão.

Se o upload falhar depois de criar o rascunho, o formulário mantém seu identificador e as fotos ainda não enviadas. Repetir o envio reutiliza o mesmo evento. Se a página for fechada, o rascunho aparece na listagem para edição; seleções locais de arquivos precisam ser feitas novamente.

Os uploads ficam no volume Docker `portfolio-data`, em `/app/data/portfolio`, e as referências ficam no banco. A interface `IPhotoStorage` permite outra implementação; S3 ainda não foi configurado. O volume sobrevive à recriação do container, mas precisa de backup e transferência ao trocar a VM. Não executar `docker compose down -v` se deseja preservar as fotos. Múltiplas réplicas exigem armazenamento compartilhado.

`Portfolio__StoragePath` configura o diretório. Sem override, a API usa `data/portfolio` sob a pasta do executável. As imagens de eventos não pertencem ao Git; apenas os arquivos iniciais do protótipo continuam em `public/assets` para importação.

## Mural público e conteúdo inicial

O mural lê somente eventos visíveis pela API, em páginas de 12, com botão para carregar mais. Imagens de eventos ocultos também exigem permissão na API. Respostas usam `Cache-Control: no-store`. Ocultar impede novas consultas; conteúdo já visto ou baixado não pode ser recolhido do dispositivo do visitante.

Não existe fallback para os eventos fixos: ocultar/excluir um evento não o faz reaparecer a partir de `assets`. A seção é carregada no navegador; o restante da landing continua prerenderizado.

Para avaliar os exemplos sem API: acessar `/previa-portfolio#eventos`. Há o churrasco inicial e quatro seleções demonstrativas com fotos reais; agrupamentos e categorias precisam de confirmação. A prévia não interfere na home, em eventos ocultos nem nas consultas da API.

Para migrar uma seleção: abrir **Migrar conteúdo inicial do protótipo**, escolher uma das cinco seleções, carregar no formulário, revisar, salvar e ativar a visibilidade. A importação é uma ação explícita; conferir a listagem antes de repeti-la para não duplicar o conteúdo.

## Contrato HTTP

Base: URL absoluta de `ms-ltmj-api`, configurada em `public/config.json`.

| Método e recurso | Uso | Acesso |
| --- | --- | --- |
| `GET portfolio/events?page=1&pageSize=12` | Eventos publicados | Público |
| `GET portfolio/events/management?page=1&pageSize=12` | Eventos visíveis e ocultos | `portfolio.manage` |
| `POST portfolio/events` | Criar rascunho: `title`, `description`, `category` | `portfolio.manage` |
| `PUT portfolio/events/{id}` | Editar os mesmos campos | `portfolio.manage` |
| `PATCH portfolio/events/{id}/visibility` | Body `{ "isVisible": true }` ou `false` | `portfolio.manage` |
| `POST portfolio/events/{id}/photos` | Upload multipart de várias fotos | `portfolio.manage` |
| `DELETE portfolio/events/{id}` | Apagar evento e arquivos | `portfolio.manage` |
| `GET portfolio/events/{eventId}/photos/{photoId}` | Ler imagem | Público se visível; gestão se oculto |

Listagens retornam `{ "items": [...] }`; fotos contêm `id` e `url` relativa à API. IDs expostos são os identificadores externos. Auditoria é obtida do usuário autenticado, nunca do body. Leituras usam queries `ReadEventsByFilter` e `ReadEventPhoto`, seguindo Create/Read/Update/Delete; HTTP mantém seus verbos próprios.

## Preparação antes de implantar

1. Revisar e aplicar a migration inicial PostgreSQL, gerada a pedido do mantenedor; detalhes em [migrations da API](../../ms-ltmj-api/docs/migrations.md). O modelo inclui `portfolioevent` e a coleção própria `portfoliophoto`, além do contexto existente. Os arquivos foram gerados; a aplicação remota não foi executada nesta alteração.
2. O startup aplica migrations pendentes e garante root automaticamente. Preparar usuários da equipe e perfil de autorização; revisar CORS para a origem do frontend.
3. Publicar a API com o volume persistente e backup configurado; depois publicar o frontend.
4. Importar o churrasco inicial, revisar e publicar pelo painel. Sem a API/banco preparados, o mural mostra erro de carregamento, sem simular sucesso.

## Validação local

Na raiz do monorepo:

```bash
npm --prefix ms-ltmj-ng test
npm --prefix ms-ltmj-ng run build
npm --prefix ms-ltmj-ng run e2e
dotnet build ms-ltmj-api/Ltmj.sln -c Release
dotnet test ms-ltmj-api/tests/Mps.Ltmj.Tests/Mps.Ltmj.Tests.csproj -c Release
```

Testes de UI usam API simulada, sem credenciais reais. O Playwright compila e serve o build estático em `127.0.0.1:4317`, com fallback CSR e 404 para arquivos ausentes; não reutiliza o servidor de desenvolvimento. Testes de mapeamento validam os modelos dos provedores PostgreSQL e SQL Server sem conectar ao banco; isso não substitui aplicação da migration e teste integrado no ambiente de destino.
