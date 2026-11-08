# Diretrizes — Geração de clientes

Estas regras complementam o [guia do template](../AGENTS.md).

## Escopo

`new-microservice.sh` instala o template local e gera uma pasta de cliente. Preserve validação do nome, recusa de destino existente e exigência de saída diretamente na raiz do monorepo. Os caminhos relativos das bibliotecas dependem dessa organização.

## Alterações

- Mantenha consistência com `../.template.config/template.json`: nomes, parâmetros, renomeação de arquivos e exclusões.
- Não copie secrets JWT ou senhas root do template. O gerador cria segredos de identidade independentes para cada serviço/ambiente e grava os dotenv iniciais versionados. Preserve a configuração da infraestrutura compartilhada autorizada; nunca copie credenciais AWS, certificados privados ou metadados de ferramentas.
- A geração não deve criar banco, executar migrations, publicar aplicações ou alterar clientes existentes.
- Não duplique core ou identidade no destino: os projetos gerados usam `ProjectReference` para as bibliotecas compartilhadas.
- Atualize `../TEMPLATE.md` quando mudar uso ou estrutura de saída.

## Validação

Execute `bash -n ms-cavalcante-api/scripts/new-microservice.sh` na raiz. Para mudanças no gerador, gere em um destino descartável permitido, confira nomes e referências e compile a saída. Remova somente os arquivos de validação criados na tarefa; nunca reutilize uma pasta de cliente existente como destino de teste.
