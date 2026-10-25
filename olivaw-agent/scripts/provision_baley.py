#!/usr/bin/env python3
"""Provisionamento explícito da conta técnica, exclusivamente pela API Olivaw."""
import argparse
import json
import os
from pathlib import Path
import secrets
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'olivaw-agent/ms-baley-py/src'))
from ms_baley.data.audit_service import ReportClient, private_file
from ms_baley.data.local_runtime import atomic_json


def provision(root_login, root_password):
  client = ReportClient({'baseUrl': 'https://api.olivaw.com.br'}, ROOT)
  token = client.request('/identity/authentication/login', {'login': root_login, 'password': root_password})['accessToken']
  def request(path, data=None):
    return client.request('/identity/'+path, data, token, method='GET' if data is None else 'POST')
  def items(resource):
    rows=[]
    for page in range(1,11):
      batch=request(resource+'?page='+str(page)+'&pageSize=100')['items'];rows.extend(batch)
      if len(batch)<100:return rows
    raise ValueError('Limite de consulta de identidades excedido.')
  path=ROOT/'_secrets/admin.json'
  administrative = json.loads(private_file(path).read_text()) if path.exists() else {}
  if "baley" in administrative:
    credentials=administrative["baley"]
  else:
    credentials={'login':'baley-agent','password':secrets.token_urlsafe(32)}
    # Persist before external creation, so interrupted provisioning never loses the generated password.
    path.parent.mkdir(mode=0o700,exist_ok=True)
    administrative["baley"]=credentials
    atomic_json(path,administrative)
  user=next((row for row in items('userprofiles') if row['login']==credentials['login']),None)
  if user is None:
    user=request('userprofiles', {**credentials,'name':'Baley','email':'baley@olivaw.com.br','isServiceAccount':True,'isActive':True})['item']
  if not user.get('isServiceAccount') or not user.get('isActive'):
    raise ValueError('Conta existente não é a conta técnica ativa esperada.')
  required_keys={'security.reports.write','security.remediations.execute'}
  permissions=[]
  existing_permissions=items('authorizations')
  for key in sorted(required_keys):
    permission=next((row for row in existing_permissions if row['key']==key),None)
    if permission is None: permission=request('authorizations',{'key':key,'name':key})['item']
    permissions.append(permission)
  profile=next((row for row in items('accessprofiles') if row['name']=='Baley — Relatórios de segurança'),None)
  if profile is None:profile=request('accessprofiles',{'name':'Baley — Relatórios de segurança','description':'Somente envio de relatórios pelo agente Baley.'})['item']
  links=items('accessprofileauthorizations')
  existing=[row for row in links if row['accessProfileId']==profile['id']]
  allowed_ids={permission['id'] for permission in permissions}
  if any(row['authorizationId'] not in allowed_ids for row in existing):raise ValueError('Perfil existente possui permissões adicionais; revisar manualmente.')
  for permission in permissions:
    if not any(row['authorizationId']==permission['id'] for row in existing):
      request('accessprofileauthorizations',{'accessProfileId':profile['id'],'authorizationId':permission['id']})
  links=items('userprofileaccessprofiles')
  existing=[row for row in links if row['userProfileId']==user['id']]
  if any(row['accessProfileId']!=profile['id'] for row in existing):raise ValueError('Conta possui outros perfis; revisar manualmente.')
  if not existing:request('userprofileaccessprofiles',{'userProfileId':user['id'],'accessProfileId':profile['id']})
  access_token=client.request('/identity/authentication/login',credentials)['accessToken']
  access=client.request('/identity/authentication/access',None,access_token,method='GET')
  if access.get('isRoot') or set(access.get('permissions',[]))!=required_keys:
    raise ValueError('Permissões efetivas diferentes do escopo esperado.')
  return {'account':'baley-agent','isRoot':False,'permissions':sorted(required_keys),'credentialsFile':'_secrets/admin.json','credentialsSection':'baley'}


def main():
  parser=argparse.ArgumentParser(description=__doc__)
  group=parser.add_mutually_exclusive_group(required=True)
  group.add_argument('--bootstrap-env',type=Path)
  group.add_argument('--admin-env',type=Path)
  args=parser.parse_args()
  try:
    values={}
    for line in (args.admin_env or args.bootstrap_env).read_text().splitlines():
      if '=' in line and not line.lstrip().startswith('#'):
        key,value=line.split('=',1);value=value.strip()
        if len(value)>1 and value[0]==value[-1] and value[0] in '\"\'':value=value[1:-1]
        values[key.strip()]=value
    result=provision(values['OLIVAW_LOGIN'],values['OLIVAW_PASSWORD']) if args.admin_env else provision('root',values['Identity__Bootstrap__Password'])
    print(json.dumps(result,ensure_ascii=False,indent=2))
    return 0
  except Exception as error:
    print(json.dumps({'status':'failed','error':type(error).__name__,'httpStatus':getattr(error,'status',None),'message':'Provisionamento incompleto; verificar identidade e configuração sem expor credenciais.'},indent=2))
    return 1

if __name__=='__main__':raise SystemExit(main())
