# Relatório evolutivo

Produza JSON UTF-8 válido, sem cerca Markdown quando um consumidor pedir apenas JSON. Envelope mínimo: `schemaVersion`, `runId`, `agentId`, `hostId`, `startedAt`, `completedAt`, `executionStatus`, `result`.

- `schemaVersion`: inicialmente `1.0`; novos campos analíticos são aditivos. Mudanças de significado/obrigatoriedade do envelope exigem versão e compatibilidade explícitas.
- `runId`: GUID da execução; reutilizar no reenvio. `agentId`: `baley` quando executado pelo agente; no uso manual, identificar o executor sem inventar identidade autenticada.
- Timestamps UTC e ordered; status `completed`, `partial` ou `failed`, sem confundir com risco.
- `result`: objeto flexível com `summary`, `coverage`, `findings`, `limitations`. Campos desconhecidos são preservados; ausência de campo opcional significa desconhecido/não avaliado.
- Cada achado pode incluir `ruleId`, `title`, `severity`, `confidence`, `evidence`, `recommendation`, `sources`. Grave somente evidência sanitizada; não exigir coleção de fatos que não foi autorizada.
- Inclua versão/hash da skill e modelo/provedor quando disponíveis. Não inventar resultados de IA, medições, preço, testes ou persistência na API.
- Informações de falha podem explicar classe/código e operação segura; nunca stacktrace com segredo ou resposta de login.

O Baley usa `POST /security-reports` autenticado com `security.reports.write`; consulta e detalhes exigem root. A API persiste `RawData` string e metadados estáveis. Idempotência por agente/RunId e conteúdo; timestamps UTC. O JSON completo deve ter no máximo 220.000 bytes no agente, mantendo margem para o limite de transporte. Versão 1.0 admite campos analíticos adicionais; mudança do envelope exige evolução explícita. Não conectar ao banco. Até confirmação da API, manter o mesmo relatório na fila local limitada; falhas de envio não significam relatório persistido.
