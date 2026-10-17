# Gerar um microservice de cliente

Na raiz do monorepo:

```bash
bash ms-limpo-api/scripts/new-microservice.sh pastelaria-joao
```

O script instala o template local do `dotnet new` e gera `ms-pastelaria-joao-api`, com solução, projetos e namespaces em `Mps.PastelariaJoao`. O destino não pode existir e deve ser uma pasta diretamente na raiz do monorepo, ao lado de `olivaw-identity` e `ms-olivaw-core-api`, para preservar os caminhos dos `ProjectReference`.

O cliente importa as bibliotecas compartilhadas; as features de identidade não são copiadas. O cliente mantém seu `DbContext`, suas migrations, suas funcionalidades e a conexão exclusiva com seu banco. O projeto `.Client` é uma biblioteca base, sem implementação automática de cliente REST.

Após gerar:

1. Adicione os projetos do cliente à `OlivawHub.sln` se ele fizer parte da solução de trabalho.
2. Configure conexão, emissor/destinatário JWT e chave exclusiva por ambiente ou secret store.
3. Crie e revise a migration inicial no projeto de provedor do cliente; não há schema herdado do antigo sistema.
4. Prepare o banco e execute o bootstrap de root uma vez, conforme [Olivaw Identity](../olivaw-identity/README.md).
5. Compile e teste o cliente e os módulos compartilhados antes de implantar.

O template exclui `.env`, certificados, binários e metadados de ferramentas. O Dockerfile gerado usa a raiz do monorepo como contexto; não exige feed NuGet privado nem chave Gitea para consumir core/identidade.
