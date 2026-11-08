#!/usr/bin/env python3
"""Create initial versioned service settings with independent identity secrets."""
import re
import secrets
import sys
from pathlib import Path

source, target, slug, owner = sys.argv[1:]
source = Path(source)
target = Path(target)
schema = 'ms_' + slug.replace('-', '_')
for environment in ('dev', 'qas', 'rls', 'stg', 'prd'):
    settings = (source / 'docker' / f'.env.{environment}').read_text()
    settings = settings.replace('ms-limpo', f'ms-{slug}').replace('ms_limpo', schema)
    settings = settings.replace('Database=olivaw', f'Database={owner}')
    settings = settings.replace('api.limpo', f'api.{slug}')
    values = {
        'Identity__Issuer': f'{slug}-{environment}',
        'Identity__Audience': f'{slug}-{environment}',
        'Identity__SigningKey': secrets.token_urlsafe(48),
        'Identity__Bootstrap__Password': secrets.token_urlsafe(36),
        'Identity__Bootstrap__Email': 'diegorafop@gmail.com',
    }
    for key, value in values.items():
        settings = re.sub(rf'^{re.escape(key)}=.*$', f'{key}={value}', settings, flags=re.MULTILINE)
    (target / 'docker' / f'.env.{environment}').write_text(settings)
(target / 'docker' / '.env').write_text(
    (target / 'docker' / '.env.dev').read_text().replace('OLIVAW_ENV_FILE=.env.dev', 'OLIVAW_ENV_FILE=.env'))
