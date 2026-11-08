"""CLI do piloto Gmail; comandos de rede/contato sempre explícitos."""
import argparse
import json
from pathlib import Path
import sys

from ms_trevize.data.gmail_store import GmailStore


def parser_for():
  parser = argparse.ArgumentParser(description="Trevize Gmail — piloto com destinatários de teste")
  parser.add_argument("--directory", type=Path, default=Path(__file__).resolve().parents[4]/".state"/"gmail")
  commands = parser.add_subparsers(dest="operation", required=True)
  configure = commands.add_parser("configure", help="Importar cliente OAuth e limitar destinatários de teste")
  configure.add_argument("--client-secrets", type=Path, required=True)
  configure.add_argument("--sender", required=True)
  configure.add_argument("--test-recipient", action="append", required=True)
  commands.add_parser("status", help="Estado local; sem rede, sem iniciar worker")
  commands.add_parser("authorize", help="Consentimento OAuth no navegador, sem senha no CLI")
  commands.add_parser("profile", help="Conferir conta autorizada por API")
  inbox = commands.add_parser("inbox", help="Listar metadados da caixa, sem marcar como lido")
  inbox.add_argument("--limit", type=int, default=10, choices=range(1,21))
  thread = commands.add_parser("thread", help="Ler uma conversa explicitamente selecionada")
  thread.add_argument("--id", required=True)
  thread.add_argument("--include-body", action="store_true")
  prepare = commands.add_parser("prepare", help="Preparar intenção local; não cria rascunho remoto nem envia")
  prepare.add_argument("--key", required=True)
  prepare.add_argument("--to", required=True)
  prepare.add_argument("--subject", required=True)
  prepare.add_argument("--body-file", type=Path, required=True)
  prepare.add_argument("--lead-id")
  reply = commands.add_parser("reply", help="Preparar resposta local a uma mensagem, sem enviar")
  reply.add_argument("--message-id", required=True)
  reply.add_argument("--body-file", type=Path, required=True)
  reply.add_argument("--key", required=True)
  reply.add_argument("--lead-id")
  send = commands.add_parser("send", help="Enviar uma intenção revisada, somente para destinatário de teste")
  send.add_argument("--id", required=True)
  send.add_argument("--confirm", action="store_true")
  reconcile = commands.add_parser("reconcile", help="Conferir Enviados sem reenviar")
  reconcile.add_argument("--id", required=True)
  commands.add_parser("replies", help="Consultar respostas novas aos fios do piloto; sem responder automaticamente")
  return parser


def main(argv=None):
  args = parser_for().parse_args(argv)
  try:
    store = GmailStore(args.directory)
    if args.operation == "configure":
      result = store.configure(args.sender, args.test_recipient, args.client_secrets)
    elif args.operation == "status":
      result = store.status()
    elif args.operation == "authorize":
      from ms_trevize.data.gmail_client import authorize
      with store.lock():
        result = authorize(store)
    else:
      store.config()
      from ms_trevize.features.gmail.pilot import GmailPilot
      if args.operation == "prepare":
        api = None  # Preparação local não usa OAuth, rede ou dependência opcional.
      else:
        from ms_trevize.data.gmail_client import GoogleGmail
        api = GoogleGmail.connect(store)
      pilot = GmailPilot(store, api)
      if args.operation in ("prepare", "reply"):
        if args.body_file.stat().st_size > 32000:
          raise ValueError("Corpo excede 32 KB.")
        body = args.body_file.read_text(encoding="utf-8")
        result = pilot.prepare(args.key, body, getattr(args,"to",None), getattr(args,"subject",None),
          getattr(args,"message_id",None), args.lead_id)
      elif args.operation == "profile": result = pilot.profile()
      elif args.operation == "inbox": result = pilot.inbox(args.limit)
      elif args.operation == "thread": result = pilot.thread(args.id,args.include_body)
      elif args.operation == "send": result = pilot.send(args.id,args.confirm)
      elif args.operation == "reconcile": result = pilot.reconcile(args.id)
      else: result = pilot.replies()
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 2 if result.get("state") == "confirmation-pending" else 0
  except ImportError:
    message = "Instale o extra Gmail no venv: python -m pip install -e '.[gmail]'."
  except FileNotFoundError:
    message = "Configuração, arquivo ou autorização ausente. Execute configure e authorize; consulte GMAIL.md."
  except (OSError, ValueError, KeyError, TypeError):
    # Não imprimir str(error): bibliotecas podem incluir tokens, URLs OAuth ou respostas privadas.
    message = "Operação Gmail não concluída. Confira configuração privada, conta, destinatários, chave e estado da intenção. Nenhum reenvio automático foi realizado."
  print(json.dumps({"status":"failed","operation":args.operation,"message":message},ensure_ascii=False,indent=2),file=sys.stderr)
  return 1
