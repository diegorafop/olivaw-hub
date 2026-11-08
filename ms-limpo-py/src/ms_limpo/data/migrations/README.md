# Migrações PostgreSQL

Adicione arquivos `0001_descricao.sql`, `0002_descricao.sql`, em sequência.
O template não inclui tabelas de negócio nem uma migração fictícia.
Não altere arquivos aplicados: acrescente outra migração.
Use SQL transacional, sem BEGIN/COMMIT e sem operações incompatíveis com transações.
O executor gerencia transação por arquivo, checksum e lock por schema.
O schema e os usuários devem existir; o executor não provisiona infraestrutura.
