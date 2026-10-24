# Testes da API Olivaw

Esta pasta reúne os projetos de testes da API. Ela não é exclusiva de testes unitários: poderá receber projetos de integração, ponta a ponta (E2E) e outros tipos de verificação, conforme a necessidade.

## Estrutura atual

- [Owh.Olivaw.Tests](Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj): projeto .NET 10 com xUnit, testes unitários e verificações HTTP em memória. Suas regras estão no [AGENTS.md](Owh.Olivaw.Tests/AGENTS.md).

## Evolução da estrutura

Novos projetos devem ficar em subpastas irmãs, com nomes que indiquem seu propósito. Exemplos de organização futura:

- `Owh.Olivaw.IntegrationTests`: integração entre componentes, persistência e dependências controladas.
- `Owh.Olivaw.E2ETests`: fluxos completos da API em ambiente isolado.

Esses nomes são possibilidades, não projetos existentes. O projeto atual mantém seu nome e conteúdo; a criação de uma nova suíte não implica mover os testes existentes.

Cada novo projeto deve documentar como executar seus testes, quais dependências utiliza e como preparar e limpar os dados. Diferencie testes executáveis sem serviços externos daqueles que exigem banco, containers ou uma aplicação em execução. Use somente ambientes isolados, sem bancos de clientes ou implantação remota como efeito da validação.

## Execução

A partir da raiz do monorepo, para o projeto atual:

```bash
dotnet test ms-olivaw-api/tests/Owh.Olivaw.Tests/Owh.Olivaw.Tests.csproj -c Release
```

Para todos os projetos registrados na solução da API:

```bash
dotnet test ms-olivaw-api/Olivaw.sln -c Release
```

Ao adicionar uma suíte, inclua o projeto na solução apropriada e atualize este índice com sua finalidade e instruções. Dependências adicionais de integração e E2E devem ser descritas antes dos comandos de execução.
