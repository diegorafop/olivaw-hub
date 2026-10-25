# Separação de tokens JWT — Portal e DevOps

## user-history

Como responsável pela Olivaw,
quero separar as credenciais e permissões do Portal das operações DevOps,
para impedir que uma sessão de cliente autorize deployments ou que um token de deployment acesse dados do portal.

## acceptance-criteria

- Token do Portal não autoriza operações de deployment, mesmo emitido pela Olivaw.
- Token exclusivo de DevOps não autoriza endpoints do Portal, inclusive consultas de empresas e solicitações.
- Usar papéis distintos e políticas explícitas nas controllers/actions ASP.NET Core; autenticação válida sozinha não concede autorização.
- Validar assinatura, issuer, audience, validade e claims exigidas em cada API. Preferir audiences específicas por finalidade, além de roles/permissões distintas.
- Não elevar permissões por parâmetros enviados pelo cliente nem por papel declarado apenas na interface.
- Testar acesso cruzado nos dois sentidos, token ausente, expirado, audience incorreta, role ausente e acesso entre clientes; esperar 401 para autenticação inválida e 403 para identidade válida sem autorização.
- Restringir recursos do Portal às empresas autorizadas pela identidade autenticada, sem confiar somente em IDs recebidos.

## ai-prompt

Discovery: requisito registrado em 25/10/2026. Esta anotação não implementa nem comprova isolamento já aplicado. A implementação e os nomes finais das roles serão definidos em trabalho próprio após análise dos contratos atuais de identidade, emissão e consumo JWT.

O protótipo visual de `/portal` é uma prévia interna protegida pelos guards administrativos existentes; não representa a autenticação final do cliente. O portal real existente em `/meu-negocio` deve ser auditado antes da migração.

Levantar roles, scopes, audiences, emissores, chaves, contas técnicas e policies atuais em `ms-olivaw-api`, `ms-devops-api` e `olivaw-identity`. Documentar a matriz de acesso antes de alterar contratos. Separar papéis de cliente, operador administrativo e executor de deployment. Um token emitido para uma finalidade não deve ganhar outra finalidade por compartilhar emissor ou infraestrutura. Nomes ilustrativos: `PortalClient` e `DevOpsDeployer`; não são claims já aprovadas ou implementadas.

Aplicar `[Authorize(Roles = "...")]` ou políticas equivalentes nas controllers/actions, combinando audience e permissões específicas com isolamento por cliente. Revisar fallback policies e rotas anônimas. Não conceder bypass ao portal por root administrativo sem decisão explícita sobre o fluxo de suporte. O frontend apenas adapta a navegação; o bloqueio obrigatório é no backend.

Planejar compatibilidade de tokens existentes, contas técnicas e scripts de deployment. Consumidores continuam validando JWT localmente, sem consulta à Olivaw por requisição. Validar com tokens sintéticos e testes de integração, sem acessar bancos de clientes ou publicar alterações. Não gerar migrations nesta etapa.

## user-feedback

```text
deixa anotado ai que o portal terá uma Role diferente da ROle do devops api.
ou seja um token gerado pelo olivaw para o portal n consegue fazer deploy e vice versa.

vamos proteger bem isso, depois colocaremos um dotnet Role nas controllers para garantir isso.
deixa isso no nosso docs, analise de segurança tokens JWT com roles. etc
```
