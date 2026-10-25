# Acesso da equipe LTMJ

## Conta provisionada

- Login: `ltmj`.
- E-mail: `ltmj@teste.com.br`.
- Perfil: `Gestão de portfólio LTMJ`.
- Permissão efetiva: somente `portfolio.manage`; a conta não é root e não administra identidades.
- A senha foi definida pelo responsável e não é registrada neste documento.
- Acesso: `https://ltmj.olivaw.com.br/gestao/entrar`.

## Verificação

Em 18/10/2026, o login foi validado contra `https://api.ltmj.olivaw.com.br`. A consulta de acesso confirmou `root: false` e somente `portfolio.manage`; a rota protegida de gestão do portfólio respondeu HTTP 200.

O root da API permanece exclusivo do Olivaw. As chaves JWT e credenciais de serviço não são as credenciais desta conta.
