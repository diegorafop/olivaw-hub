#!/usr/bin/env python3
"""Verifica arquivos rastreados sem imprimir o conteúdo de chaves."""
from pathlib import Path
import re
import subprocess
import sys

PRIVATE_HEADER = re.compile(rb'^-----BEGIN (?:OPENSSH |RSA |EC |DSA |ENCRYPTED )?PRIVATE KEY-----', re.MULTILINE)


def find_private_keys(root):
    paths = subprocess.check_output(['git', '-C', str(root), 'ls-files', '-z']).split(b'\0')
    found = []
    for raw in paths:
        if not raw:
            continue
        relative = raw.decode('utf-8', errors='surrogateescape')
        path = root / relative
        if path.is_symlink() or not path.is_file():
            continue
        with path.open('rb') as source:
            # Leitura em blocos limita memória e detecta cabeçalho entre blocos.
            tail = b''
            while chunk := source.read(65536):
                content = tail + chunk
                if PRIVATE_HEADER.search(content):
                    found.append(relative)
                    break
                tail = content[-100:]
    return found


if __name__ == '__main__':
    root = Path(__file__).resolve().parents[2]
    matches = find_private_keys(root)
    for path in matches:
        print('Chave privada rastreada: ' + path, file=sys.stderr)
    print('Arquivos rastreados com chave privada: ' + str(len(matches)))
    sys.exit(1 if matches else 0)
