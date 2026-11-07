"""Comandos explícitos para validar, enriquecer e acompanhar leads via Gmail."""
import argparse
import json
from pathlib import Path
import sys


def parser_for():
  parser = argparse.ArgumentParser(description="Trevize — contatos e piloto comercial com aprovação humana")
  project = Path(__file__).resolve().parents[4]
  parser.add_argument("--settings", type=Path, default=project/"setting.json")
  parser.add_argument("--directory", type=Path, default=project/".state"/"gmail")
  commands = parser.add_subparsers(dest="operation", required=True)
  commands.add_parser("outbox", help="Consultar intenções locais, sem rede ou credenciais CRM")
  preview = commands.add_parser("collect-preview", help="Coleta pequena sem IA, gravação no CRM ou contato")
  preview.add_argument("--limit", type=int, choices=range(1,6), default=3)
  for operation in ("inspect", "enrich"):
    command = commands.add_parser(operation, help="Ler canais existentes; enrich --apply grava só os canais")
    command.add_argument("--id", action="append", default=[])
    command.add_argument("--limit", type=int, choices=range(1,101), default=3)
    if operation == "enrich": command.add_argument("--apply", action="store_true")
  for operation in ("prepare", "prepare-reply"):
    command = commands.add_parser(operation, help="Aprovar texto e destinatário de um lead; não enviar")
    command.add_argument("--lead-id", required=True)
    command.add_argument("--to", required=True)
    command.add_argument("--body-file", type=Path, required=True)
    command.add_argument("--approve", action="store_true")
    if operation == "prepare": command.add_argument("--subject", required=True)
    else: command.add_argument("--message-id", required=True)
  send = commands.add_parser("send", help="Enviar uma intenção aprovada e registrar no CRM")
  send.add_argument("--id", required=True)
  send.add_argument("--confirm", action="store_true")
  reconcile = commands.add_parser("reconcile", help="Conferir Gmail e atualizar CRM sem reenviar")
  reconcile.add_argument("--id", required=True)
  commands.add_parser("sync", help="Registrar respostas e intervenção humana; não responder")
  return parser


def main(argv=None):
  args = parser_for().parse_args(argv)
  try:
    if args.operation == "outbox":
      from ms_trevize.data.gmail_store import GmailStore
      store = GmailStore(args.directory)
      result = {"items":[{"id":item["id"],"state":item["state"],"leadId":item["content"].get("leadId"),
        "to":item["content"]["to"],"subject":item["content"]["subject"]} for item in store.intents()]}
    elif args.operation == "collect-preview":
      from ms_trevize.bootstrap import compose
      from ms_trevize.data.local_runtime import LocalRuntime, exclusive_lock
      runtime = LocalRuntime(args.settings)
      runtime.prepare()
      with exclusive_lock(runtime.state/"worker.lock"):
        result = compose(args.settings, preview_limit=args.limit).handle()
    else:
      from ms_trevize.configuration import load_settings
      from ms_trevize.data.lead_api import LeadApi
      settings = load_settings(args.settings)
      if not settings["leadApi"]["enabled"]: raise ValueError("API de leads desabilitada.")
      crm = LeadApi(settings["leadApi"], args.settings.resolve().parent)
      crm.login()
      if args.operation in ("inspect", "enrich"):
        from ms_trevize.data.website_review import WebsiteReview
        from ms_trevize.features.leads.enrichment import LeadEnrichment
        result = LeadEnrichment(crm, WebsiteReview(settings["website"])).run(args.id, args.limit, getattr(args,"apply",False))
      else:
        from ms_trevize.data.gmail_store import GmailStore
        from ms_trevize.data.gmail_client import GoogleGmail
        from ms_trevize.features.leads.outreach import LeadOutreach
        store = GmailStore(args.directory)
        # A primeira abordagem pode ser preparada sem OAuth; o envio exige conta verificada.
        gmail = None if args.operation == "prepare" else GoogleGmail.connect(store)
        outreach = LeadOutreach(crm, store, gmail)
        if args.operation in ("prepare", "prepare-reply"):
          if args.body_file.stat().st_size > 32000: raise ValueError("Texto excede limite.")
          body = args.body_file.read_text(encoding="utf-8")
          result = outreach.prepare(args.lead_id, args.to, getattr(args,"subject",None), body, args.approve, getattr(args,"message_id",None))
        elif args.operation == "send": result = outreach.send(args.id, args.confirm)
        elif args.operation == "reconcile": result = outreach.reconcile(args.id)
        else: result = outreach.sync()
    print(json.dumps(result, ensure_ascii=False, indent=2))
    if result.get("state") == "confirmation-pending": return 2
    return 1 if result.get("status") in ("partial", "failed", "disabled") else 0
  except (OSError, ValueError, KeyError, TypeError, ImportError, RuntimeError):
    print(json.dumps({"status":"failed","operation":args.operation,
      "message":"Operação não confirmada. Confira configuração, versão da API e histórico. Em caso de dúvida no envio use leads reconcile; não prepare outra abordagem."},ensure_ascii=False),file=sys.stderr)
    return 1
