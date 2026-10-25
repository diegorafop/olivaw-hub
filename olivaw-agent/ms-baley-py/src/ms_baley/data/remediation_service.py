"""Consome aprovações explícitas, executa allowlist e confirma saúde antes do sucesso."""
from datetime import datetime, timezone
import json
from pathlib import Path
import time
from urllib.parse import urlencode
from uuid import UUID, uuid4

from ms_baley.data.audit_service import AuditService, ReportClient, ReportHTTPError
from ms_baley.data.local_runtime import atomic_json, exclusive_lock


def same_snapshot(before, after):
  def snapshot(health):
    return sorted((row['id'],row['name'],row['restarts'],row['startedAt']) for row in health['containers'])
  return snapshot(before) == snapshot(after)


class RemediationService:
  def __init__(self, runtime, transport=None, client=None, sleeper=time.sleep):
    self.runtime = runtime
    self.transport = transport or AuditService(runtime)
    self.client = client or ReportClient(runtime.settings['reportApi'],runtime.path.parent)
    self.sleeper = sleeper

  def remote(self, operation):
    source = Path(__file__).with_name('remote_remediation.py').read_text()
    source = source.replace('from ms_baley.data.process_io import bounded_run','')
    source = source.replace('OPERATION = None','OPERATION = '+repr(operation))
    source = Path(__file__).with_name('process_io.py').read_text()+'\n'+source
    result = self.transport.remote(source)
    if not isinstance(result,dict) or result.get('error'): raise ValueError('Procedimento não confirmado.')
    return result

  def update(self, job, action, result):
    identifier = str(UUID(job['externalId']))
    payload = {'action':action,'leaseToken':job['leaseToken'],'resultJson':json.dumps(result,ensure_ascii=False)}
    self.client.request('/security-remediations/'+identifier,payload,self.token,method='PUT')

  def process(self, task, directory):
    # No arbitrary command, filename, hostname or script comes from a report or model.
    if task['hostId'] != self.runtime.settings['audit']['hostId']: raise ValueError('Alvo diferente do autorizado.')
    plan = json.loads(task['planJson'])
    if plan.get('Action') != 'restrict-permissions' or plan.get('Target') not in ('sshDirectory','authorizedKeys','dockerSocket') or type(plan.get('ExpectedMode')) is not int:
      raise ValueError('Plano não implementado com segurança.')
    identifier = str(UUID(task['externalId']))
    operation = {'action':'restrict-permissions','target':plan['Target'],'expectedMode':plan['ExpectedMode']}
    job = {'externalId':identifier,'leaseToken':str(uuid4()),'phase':'claiming','result':{}}
    path = directory/(job['externalId']+'.json')
    atomic_json(path,job)
    receipt = None
    baseline = None
    changed = False
    try:
      try:
        self.update(job,'claim',{})
      except ReportHTTPError as error:
        if error.status in (400,401,403,404,409,429):
          path.unlink()
          return {'externalId':job['externalId'],'status':'not-claimed'}
        raise
      job['phase']='claimed';atomic_json(path,job)
      baseline = self.remote({'operation':'health'})
      if baseline.get('healthy') is not True:
        raise ValueError('Infraestrutura sem saúde confirmada; alteração bloqueada.')
      receipt = self.remote({**operation,'operation':'prepare'})
      job['receipt']=receipt; job['phase']='prepared'; atomic_json(path,job)
      # All backup metadata is durable before the potentially mutating call.
      changed = True
      self.remote({**operation,'operation':'apply','receipt':receipt})
      job['phase']='changed';atomic_json(path,job)
      self.update(job,'validating',{'healthBefore':baseline})
      samples=[]
      for _ in range(2):
        self.sleeper(2)
        samples.append(self.remote({'operation':'health'}))
      proof = self.remote({**operation,'operation':'verify','receipt':receipt})
      stable = all(sample.get('healthy') is True and same_snapshot(baseline,sample) for sample in samples)
      if not stable or proof.get('findingAbsent') is not True:
        raise ValueError('Saúde ou correção não confirmada após alteração.')
      after=samples[-1];after['samples']=2
      result={'healthBefore':baseline,'healthAfter':after,'snapshotUnchanged':True,'findingAbsent':True,
              'summary':'Permissões corrigidas; infraestrutura estável e verificação sem o achado.'}
      job.update(phase='result',action='resolved',result=result);atomic_json(path,job)
    except Exception:
      rollback = {'attempted':False}
      if changed and receipt:
        rollback={'attempted':True,'confirmed':False}
        try: rollback['confirmed']=self.remote({**operation,'operation':'rollback','receipt':receipt}).get('rolledBack') is True
        except Exception: pass
      result={'healthBefore':baseline or {'healthy':False},'rollback':rollback,'findingAbsent':False,
              'executionUncertain':changed and rollback.get('confirmed') is not True,
              'summary':'Intervenção necessária: pré-condição, saúde, procedimento ou comunicação não pôde ser confirmada com segurança.'}
      job.update(phase='result',action='intervention',result=result);atomic_json(path,job)
    # Failed delivery never repeats the remote mutation; only this immutable receipt is retried.
    self.update(job,job['action'],job['result'])
    path.unlink()
    return {'externalId':job['externalId'],'status':job['action']}

  def poll(self):
    if not self.runtime.settings['execution']['enabled'] or not self.runtime.settings.get('remediation',{}).get('enabled'):
      return {'status':'disabled'}
    self.runtime.prepare()
    directory=self.runtime.state/'remediations'
    if directory.is_symlink(): raise ValueError('Estado de remediação não pode ser link.')
    directory.mkdir(mode=0o700,exist_ok=True)
    with exclusive_lock(self.runtime.state/'audit.lock'):
      try:
        self.token=self.client.login()
        pending=list(directory.glob('*.json'))
        if len(pending)>100: return {'status':'intervention','message':'Fila de confirmações exige revisão.'}
        for path in pending:
          if path.is_symlink() or path.stat().st_size>65536: raise ValueError('Recibo inválido.')
          job=json.loads(path.read_text())
          if job.get('phase')!='result':
            # A crash may have happened after mutation: do not replay it or claim success.
            job.update(phase='result',action='intervention',result={'summary':'Execução interrompida; revisão manual obrigatória antes de nova ação.','findingAbsent':False,'executionUncertain':job.get('phase') in ('prepared','changed')})
            atomic_json(path,job)
          self.update(job,job['action'],job['result']);path.unlink()
        query=urlencode({'hostId':self.runtime.settings['audit']['hostId']})
        tasks=self.client.request('/security-remediations/queue?'+query,None,self.token,method='GET')['items']
        if not tasks:return {'status':'idle','processed':0}
        # One approved action per poll, and no overlap with audits or another local process.
        result=self.process(tasks[0],directory)
        return {'status':'completed','processed':1,'result':result}
      except Exception:
        return {'status':'intervention','message':'Fila ou confirmação indisponível; nenhuma nova ação será iniciada.'}
