# Diretrizes — Domínio de identidade

Estas regras complementam o [guia do módulo](../../AGENTS.md).

## Responsabilidade

Este projeto é uma biblioteca `netstandard2.1`, C# 10. Contém as cinco entidades de identidade e o contrato `IIdentityStore`; referencia o núcleo do Olivaw Core. Não introduza dependências de ASP.NET Core, EF Core, transporte HTTP ou implementações de persistência.

## Contratos

- Reutilize `Entity` para identidade e auditoria. Preserve a distinção entre `Id` numérico interno e `ExternalId` público.
- Perfil representa role e autorização representa permissão. Concessões passam por `UsuarioPerfilAcesso` e `PerfilAcessoAutorizacao`.
- Mantenha hash, estado ativo e versão de segurança com sua semântica atual. Alterações afetam autenticação, revogação e persistência.
- Novas operações de armazenamento devem permanecer independentes do EF e propagar `CancellationToken`; não exponha `DbContext` ou `IQueryable` pela interface.
- Navegações e nulabilidade devem refletir carregamento e persistência reais; não adicione entidades específicas de um cliente.

## Validação

Ao mudar entidades ou interfaces, compile as camadas dependentes e teste os comportamentos afetados. Documente possíveis efeitos no schema e nos consumidores. Migrations continuam nos projetos dos clientes; não pertencem a esta biblioteca.
