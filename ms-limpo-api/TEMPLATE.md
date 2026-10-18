# Gerar um microservice

Na raiz do monorepo:

```bash
bash ms-limpo-api/scripts/new-microservice.sh cliente-novo ./ms-cliente-novo-api cliente_novo
```

O gerador usa `dotnet new owh-microservice` e cria projetos e namespaces `Owh.ClienteNovo`. O destino deve ser uma pasta nova diretamente na raiz do monorepo. Ele referencia core e identidade sem duplicar as bibliotecas.

O projeto começa com identidade local, migration inicial, diretórios `Domain`, `Repositories`, `Services` e `Requests` e suas instruções. A API também contém `Domain/AGENTS.md` para orientar a separação entre modelos internos e entidades do núcleo. Não inclui entidades ou endpoints demonstrativos.

`configure-generated-service.py` cria os dotenv iniciais versionados com emissor, audiência, secret e senha de root independentes por serviço/ambiente. Esse passo ocorre somente ao gerar o projeto: não existem scripts para reconstruir configurações em cada clone. A infraestrutura compartilhada permanece configurada como no template; revise os bancos e domínios do novo cliente antes de publicar. O gerador não conecta ao banco nem publica containers.

As migrations do template descrevem exclusivamente o modelo básico de identidade. Cada novo serviço mantém seu próprio histórico e deve gerar migrations adicionais para suas entidades. O startup aplica migrations e garante root automaticamente; não gera arquivos de migrations no runtime. Novos clientes mantêm bancos e usuários próprios.

Para um serviço interno consumidor do Olivaw, registre `AddOlivawIdentityConsumer` em vez de `AddOlivawIdentity`, remova `AddOlivawIdentity()` do modelo de dados e o bootstrap do startup e gere a migration própria. Configure o mesmo emissor, audiência e secret do Olivaw. Esse consumidor usa apenas o JWT; não acessa o banco de identidade nem consulta o Olivaw por requisição.

Veja [Olivaw Identity](../olivaw-identity/README.md) e [arquitetura de identidade](../docs/identity-architecture.md).
