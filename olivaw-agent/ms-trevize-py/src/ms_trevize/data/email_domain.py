"""Verifica DNS público do domínio; não confirma existência da caixa nem envia mensagens."""
import json
import urllib.request
from urllib.parse import urlencode
from ms_trevize.domain.mail import address


def dns_record(domain, kind):
  url = 'https://dns.google/resolve?' + urlencode({'name':domain,'type':kind})
  request = urllib.request.Request(url,headers={'Accept':'application/dns-json'})
  with urllib.request.urlopen(request,timeout=8) as response:
    raw = response.read(32769)
    if len(raw)>32768: raise ValueError('Resposta DNS excede limite.')
    value = json.loads(raw)
    if not isinstance(value,dict): raise ValueError('Resposta DNS inválida.')
    return value


def check_email_domain(recipient, fetch=dns_record):
  domain = address(recipient).rsplit('@',1)[1]
  try:
    mx = fetch(domain,'MX')
    if mx.get('Status') == 3: return {'status':'invalid','reason':'domain-nxdomain'}
    if mx.get('Status') != 0: return {'status':'unknown','reason':'dns-unavailable'}
    records = [item.get('data','').split() for item in mx.get('Answer',[]) if item.get('type') == 15]
    if any(len(parts)==2 and parts[1] != '.' for parts in records): return {'status':'valid','reason':'mx-found'}
    if records: return {'status':'invalid','reason':'domain-rejects-mail'}
    # SMTP permite fallback de MX ausente para endereço A/AAAA do próprio domínio.
    for kind,code in (('A',1),('AAAA',28)):
      answer = fetch(domain,kind)
      if answer.get('Status') != 0: return {'status':'unknown','reason':'dns-unavailable'}
      if any(item.get('type') == code for item in answer.get('Answer',[])):
        return {'status':'valid','reason':'smtp-address-fallback'}
    return {'status':'invalid','reason':'no-mail-route'}
  except (OSError,ValueError,TypeError,KeyError):
    return {'status':'unknown','reason':'dns-unavailable'}
