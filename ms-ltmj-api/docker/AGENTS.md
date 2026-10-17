# Diretrizes — Docker do template

Estas regras complementam o [guia do template](../AGENTS.md).

## Estrutura

Os arquivos Compose desta pasta apontam para o Dockerfile em `../src/Mps.Ltmj.Api/Dockerfile`. O contexto de build é a raiz do monorepo, necessária para resolver os `ProjectReference` do core e da identidade. Preserve essa relação ao alterar caminhos ou gerar novos clientes.

## Configuração

- Mantenha valores comuns no Compose base e diferenças de ambiente nos overrides.
- Os dotenv dos ambientes são versionados no repositório privado por decisão do mantenedor; Key Vault será adotado futuramente. Não imprima seus valores. `.env.example` permanece como referência.
- Preserve conexão, emissor, destinatário e chave JWT exclusivos por cliente. Não reintroduza dependência de autenticação corporativa.
- Migrations e bootstrap são opções explícitas, desabilitadas por padrão; não habilite alterações de banco como efeito de uma mudança de container.
- Preserve execução sem root, porta documentada e exclusão de certificados e segredos do contexto de build.

## Validação

Confira a configuração com `docker compose -f ms-ltmj-api/docker/docker-compose.yml config --no-interpolate --quiet`. Quando alterar build ou execução, valide o container se houver acesso ao daemon e registre limitações reais. Não implante nem publique imagens apenas para verificar o Compose.
