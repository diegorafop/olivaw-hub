# Diretrizes — Scripts de infraestrutura

- Complemente as [instruções do módulo](../AGENTS.md).
- Scripts de auditoria e validação são somente leitura e não imprimem conteúdo de segredos.
- Preparação de VM nova exige argumento explícito; não criar volumes automaticamente em migração de VM existente.
- Não usar shell com entrada externa interpolada; comandos têm argumentos explícitos.
