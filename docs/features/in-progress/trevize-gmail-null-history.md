# Trevize — histórico JSON no teste Gmail

## Problema encontrado

No teste controlado, a autorização OAuth funcionou, mas o agente não conseguiu confirmar a preparação no CRM. Leads novos podiam trazer `communications=null`; após gravar um evento, o MVC/Newtonsoft serializava o `JsonElement` do contrato como `valueKind`, sem o conteúdo da lista.

## Correção

- Trevize trata histórico nulo como lista vazia, mantendo a exigência de confirmação após a escrita.
- Conversor registrado somente no MVC da API Olivaw preserva o JSON bruto de `JsonElement` e seu nullable. Cliente e Core não recebem dependências ou mudanças globais.
- Teste HTTP autenticado grava uma preparação e relê os eventos pela mesma serialização usada em produção. Testes cobrem também objetos, arrays, null e valores indefinidos.

83 testes Python e 502 testes da API aprovados. Sem migrations, envio, push ou deploy nesta correção. Publicar a API atualizada antes de retomar o teste; reutilizar o mesmo lead e a mesma intenção preparada, sem criar outra abordagem.
