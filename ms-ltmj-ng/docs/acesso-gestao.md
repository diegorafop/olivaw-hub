# Acesso da equipe LTMJ

## Decisão

Login solicitado: `ltmj`, criado na identidade de `ms-ltmj-api`. Perfil dedicado somente com `portfolio.manage`; não conceder `identity.gerenciar` nem root. Root de cada cliente é exclusivo do Olivaw Hub. Chave JWT de serviço não é a senha de usuário; a LTMJ tem assinatura, emissor e destinatário próprios por ambiente.

## Pré-requisitos

API e schema preparados. Usar token root emitido pela própria API LTMJ. O contrato exige e-mail com formato válido, nome, login e senha não vazia de até 128 caracteres, sem mínimo adicional. E-mail definido para esta conta: `ltmj@teste.com.br`. Não há envio ou confirmação por e-mail como requisito de login. A senha informada pelo responsável deve ser digitada no prompt; não é registrada nesta documentação. Nenhum usuário foi criado no servidor.

## Criar usuário por curl

Executar em Bash, com `curl` e Python 3. Informar e-mail confirmado e senha compatível. Os prompts não exibem token ou senha; não ativar `set -x`.

```bash
LTMJ_API_URL='https://api.ltmj.olivaw-hub.duckdns.org'
read -r -s -p 'Token root da API LTMJ: ' LTMJ_ROOT_TOKEN
printf '\n'
LTMJ_USER_EMAIL='ltmj@teste.com.br'
read -r -s -p 'Senha do usuário ltmj: ' LTMJ_USER_PASSWORD
printf '\n'
export LTMJ_USER_EMAIL LTMJ_USER_PASSWORD

python3 - <<'PY' | curl --fail-with-body --silent --show-error \
  --request POST "$LTMJ_API_URL/identity/usuarios" \
  --header "Authorization: Bearer $LTMJ_ROOT_TOKEN" \
  --header 'Content-Type: application/json' \
  --data-binary @-
import json, os
print(json.dumps({
    'login': 'ltmj',
    'nome': 'Equipe LTMJ',
    'email': os.environ['LTMJ_USER_EMAIL'],
    'senha': os.environ['LTMJ_USER_PASSWORD'],
    'ativo': True,
}))
PY
unset LTMJ_USER_PASSWORD
```

Guardar o `registro.id` retornado como `LTMJ_USER_ID`. A criação não concede permissão automaticamente. Não repetir a criação se a conta já existe; consultar a identidade e reutilizar seu ID.

## Conceder somente a gestão de portfólio

Todas as chamadas abaixo usam o mesmo token root LTMJ. Se autorização, perfil ou vínculos já existem, consultar as listagens paginadas de identidade e reutilizar seus IDs, evitando duplicação. Nos cadastros, o ID de retorno fica em `registro.id`.

1. `POST /identity/autorizacoes`: `{ "chave": "portfolio.manage", "nome": "Gerenciar portfólio LTMJ" }`. Guardar ID como `LTMJ_PERMISSION_ID`.
2. `POST /identity/perfis`: `{ "nome": "Gestão de portfólio LTMJ", "ativo": true }`. Guardar ID como `LTMJ_PROFILE_ID`.
3. `POST /identity/perfis-autorizacoes`: `{ "perfilAcessoId": "<LTMJ_PROFILE_ID>", "autorizacaoId": "<LTMJ_PERMISSION_ID>" }`.
4. `POST /identity/usuarios-perfis`: `{ "usuarioId": "<LTMJ_USER_ID>", "perfilAcessoId": "<LTMJ_PROFILE_ID>" }`.

Modelo para cada POST, usando o JSON do passo correspondente:

```bash
curl --fail-with-body --silent --show-error \
  --request POST "$LTMJ_API_URL/identity/perfis" \
  --header "Authorization: Bearer $LTMJ_ROOT_TOKEN" \
  --header 'Content-Type: application/json' \
  --data '{"nome":"Gestão de portfólio LTMJ","ativo":true}'
```

Depois dos vínculos, fazer login em `/gestao/entrar`. A consulta `GET /identity/autenticacao/acesso`, autenticada como `ltmj`, deve indicar `root: false` e incluir `portfolio.manage` em `autorizacoes`. O usuário não recebe acesso às operações de identidade. Ao terminar os curls, executar `unset LTMJ_ROOT_TOKEN`.
