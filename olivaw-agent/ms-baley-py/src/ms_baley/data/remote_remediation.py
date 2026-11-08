"""Executor remoto de procedimentos fixos; nunca recebe comandos do modelo."""
import http.client
import json
import os
import re
import socket
import stat
import subprocess
import urllib.parse
from ms_baley.data.process_io import bounded_run

OPERATION = None
REQUIRED = ('ms-traefik-pr', 'ms-postgres-db', 'ms-rabbit-mq', 'ms-olivaw-api-prd', 'ms-devops-api-prd', 'ms-ltmj-api-prd')
READY_HOSTS = ('api.olivaw.com.br', 'api.devops.olivaw.com.br', 'api.ltmj.olivaw.com.br')


def run(args):
  if args and args[0] == "docker": args = ["docker", "-H", "unix:///var/run/docker.sock", *args[1:]]
  result = bounded_run(args, input='', timeout=12, maximum=200000)
  if result.returncode: raise ValueError('command-failed')
  return result.stdout


def health():
  ids = run(['docker', 'ps', '-a', '-q', '--no-trunc']).splitlines()
  if not ids or len(ids) > 60 or any(not re.fullmatch('[a-f0-9]{64}',identifier) for identifier in ids): raise ValueError('container-inventory-limit')
  template = '{"id":{{json .Id}},"name":{{json .Name}},"state":{{json .State.Status}},"health":{{if .State.Health}}{{json .State.Health.Status}}{{else}}"none"{{end}},"paused":{{json .State.Paused}},"restarts":{{json .RestartCount}},"startedAt":{{json .State.StartedAt}}}'
  rows = [json.loads(run(['docker', 'inspect', '--format', template, identifier])) for identifier in ids]
  names = {row['name'].lstrip('/') for row in rows}
  healthy = all(row['state'] == 'running' and not row['paused'] and row['health'] in ('healthy', 'none') and (row['health'] != 'none' or row['name'].lstrip('/') in REQUIRED) for row in rows) and set(REQUIRED).issubset(names)
  probes = []
  if healthy:
    for host in READY_HOSTS:
      connection = http.client.HTTPSConnection(host, timeout=5)
      passed = False
      try:
        connection.request('GET', '/ready')
        response = connection.getresponse()
        payload = response.read(16385)
        if response.status == 200 and len(payload) <= 16384:
          passed = json.loads(payload).get('status') == 'pass'
      except (OSError, ValueError, http.client.HTTPException): pass
      finally: connection.close()
      probes.append({'name':host+'/ready','passed':passed})
    # PostgreSQL checks readiness without connection strings or business queries.
    try: run(['docker','exec','ms-postgres-db','pg_isready','-U','postgres']); passed = True
    except (OSError, ValueError, subprocess.TimeoutExpired): passed = False
    probes.append({'name':'postgres-readiness','passed':passed})
    try:
      ip_template = '{{range .NetworkSettings.Networks}}{{.IPAddress}} {{end}}'
      addresses = run(['docker','inspect','--format',ip_template,'ms-rabbit-mq']).split()
      passed = False
      for address in addresses[:4]:
        try:
          with socket.create_connection((address,5672),timeout=2): passed = True
          if passed: break
        except OSError: pass
    except (OSError, ValueError, subprocess.TimeoutExpired): passed = False
    probes.append({'name':'rabbit-amqp-tcp','passed':passed})
    healthy = healthy and all(probe['passed'] for probe in probes)
  return {'healthy':healthy,'containers':rows,'checks':probes,'samples':1,
          'summary':'Saúde verificada.' if healthy else 'Infraestrutura incompleta, container parado, instável ou healthcheck não confirmado. Nenhuma manutenção autorizada nestas condições.'}


def target_path(target):
  if target == 'sshDirectory': return os.path.expanduser('~/.ssh')
  if target == 'authorizedKeys': return os.path.expanduser('~/.ssh/authorized_keys')
  if target == 'dockerSocket': return '/var/run/docker.sock'
  raise ValueError('unsupported-target')


def metadata(target):
  value = os.lstat(target_path(target))
  if stat.S_ISLNK(value.st_mode): raise ValueError('symlink-blocked')
  if target == 'sshDirectory' and not stat.S_ISDIR(value.st_mode): raise ValueError('not-directory')
  if target == 'authorizedKeys' and not stat.S_ISREG(value.st_mode): raise ValueError('not-file')
  if target == 'dockerSocket' and not stat.S_ISSOCK(value.st_mode): raise ValueError('not-socket')
  owner = 0 if target == 'dockerSocket' else os.getuid()
  if value.st_uid != owner: raise ValueError('unexpected-owner')
  if target == 'authorizedKeys' and value.st_nlink != 1: raise ValueError('hardlink-blocked')
  return {'inode':value.st_ino,'device':value.st_dev,'mode':stat.S_IMODE(value.st_mode),'owner':value.st_uid}


def corrected_mode(target, mode):
  if target == 'sshDirectory': return 0o700
  if target == 'authorizedKeys': return 0o600
  if target == 'dockerSocket': return mode & ~0o007
  raise ValueError('unsupported-target')


def set_mode(target, mode, expected):
  path = target_path(target)
  if target == 'dockerSocket': run(['sudo','-n','chmod',format(mode,'o'),path])
  else:
    descriptor = os.open(path,os.O_RDONLY | os.O_NOFOLLOW)
    try:
      opened = os.fstat(descriptor)
      if opened.st_ino != expected["inode"] or opened.st_dev != expected["device"] or opened.st_uid != expected["owner"]: raise ValueError("target-changed")
      os.fchmod(descriptor,mode)
    finally: os.close(descriptor)


def execute(operation):
  action = operation.get('operation')
  if action == 'health': return health()
  if operation.get('action') != 'restrict-permissions': raise ValueError('unsupported-action')
  target = operation['target']
  current = metadata(target)
  if action == 'prepare':
    if current['mode'] != operation.get('expectedMode'): raise ValueError('precondition-changed')
    return {'before':current,'appliedMode':corrected_mode(target,current['mode'])}
  if action == 'apply':
    # The immutable approval binds the mode observed in the report.
    if type(operation.get('expectedMode')) is not int or current['mode'] != operation['expectedMode']:
      raise ValueError('precondition-changed')
    receipt = operation['receipt']
    if any(current[key] != receipt['before'][key] for key in ('inode','device','owner','mode')): raise ValueError('precondition-changed')
    new_mode = corrected_mode(target,current['mode'])
    set_mode(target,new_mode,current)
    after = metadata(target)
    if any(after[key] != current[key] for key in ('inode','device','owner')) or after['mode'] != new_mode:
      raise ValueError('postcondition-failed')
    return {'before':current,'appliedMode':new_mode,'findingAbsent': True}
  if action == 'verify':
    expected = operation['receipt']
    unchanged = all(current[key] == expected['before'][key] for key in ('inode','device','owner'))
    return {'findingAbsent':unchanged and current['mode'] == expected['appliedMode']}
  if action == 'rollback':
    receipt = operation['receipt']
    if not all(current[key] == receipt['before'][key] for key in ('inode','device','owner')) or current['mode'] != receipt['appliedMode']:
      raise ValueError('rollback-precondition-changed')
    set_mode(target,receipt['before']['mode'],current)
    return {'rolledBack':metadata(target)['mode'] == receipt['before']['mode']}
  raise ValueError('unsupported-operation')


if __name__ == '__main__':
  try:
    print(json.dumps(execute(OPERATION)))
  except Exception:
    # No stderr, pathname contents, environment or secret value leaves this program.
    print(json.dumps({'error':'Procedure could not be confirmed safely.'}))
    raise SystemExit(1)
