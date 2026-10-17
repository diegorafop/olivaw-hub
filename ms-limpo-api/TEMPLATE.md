# Gerar um microservice de cliente

Na raiz do monorepo:

```bash
bash ms-limpo-api/scripts/new-microservice.sh pastelaria-joao ./ms-pastelaria-joao-api pastelaria
```

O script instala o template local do `dotnet new` e gera `ms-pastelaria-joao-api`, com solução, projetos e namespaces em `Mps.PastelariaJoao`. O destino não pode existir e deve ser uma pasta diretamente na raiz do monorepo, ao lado de `olivaw-identity` e `ms-olivaw-core-api`, para preservar os caminhos dos `ProjectReference`.

O cliente importa as bibliotecas compartilhadas; as features de identidade não são copiadas. O cliente mantém seu `DbContext`, suas migrations, suas funcionalidades e a conexão com o banco da organização e seu schema exclusivo. O projeto `.Client` é uma biblioteca base, sem implementação automática de cliente REST.

Após gerar:

1. Adicione os projetos do cliente à `OlivawHub.sln` se ele fizer parte da solução de trabalho.
2. Configure conexão, emissor/destinatário JWT e chave exclusiva por ambiente ou secret store.
3. Crie e revise a migration inicial no projeto de provedor do cliente; não há schema herdado do antigo sistema.
4. Prepare o banco e execute o bootstrap de root uma vez, conforme [Olivaw Identity](../olivaw-identity/README.md).
5. Compile e teste o cliente e os módulos compartilhados antes de implantar.

O template exclui `.env`, certificados, binários e metadados de ferramentas. O Dockerfile gerado usa a raiz do monorepo como contexto; não exige feed NuGet privado nem chave Gitea para consumir core/identidade.

## Infraestrutura e configuração de ambientes

O dono do banco é o terceiro argumento do script e usa snake_case; o padrão é `olivaw` para serviços internos. O exemplo acima configura `pastelaria_dev` até `pastelaria_prd`, com schema `ms_pastelaria_joao`. Sem o terceiro argumento, `cloud-validacao` usa `olivaw_dev` até `olivaw_prd` e schema `ms_cloud_validacao`. A geração apenas configura os nomes: não cria bancos ou schemas. Serviços, imagens, routers e domínios preservam o nome em kebab-case. Produção usa `<nome-do-servico>.fintalk-server.duckdns.org`; ambientes adicionais inserem seu sufixo antes da base DuckDNS.

O gerador exclui todos os dotenv locais com credenciais e inclui apenas `.env.example`. Prepare credenciais por serviço e configure o JWT conforme o emissor principal da organização; o template atual inclui identidade local e serviços agregados precisarão de adaptação antes de usá-lo. Configure os segredos antes de executar o Compose. Veja [configuração de nuvem](../docs/cloud.md).

A identidade é mantida somente no serviço principal do cliente. Serviços agregados compartilharão o emissor e validarão seus tokens, sem duplicar usuários. Veja as limitações atuais e a governança dos schemas no [guia de nuvem](../docs/cloud.md).
