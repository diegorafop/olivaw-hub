"""Plano, ativação e pausa explícitos; nenhum comando ativa chaves globais de envio."""
import argparse
import json
from pathlib import Path
import sys


def main(argv=None):
  parser = argparse.ArgumentParser(description="Trevize — campanha aprovada com envio desligado por padrão")
  project = Path(__file__).resolve().parents[4]
  parser.add_argument("--settings",type=Path,default=project/"setting.json")
  parser.add_argument("--directory",type=Path,default=project/".state"/"gmail")
  commands = parser.add_subparsers(dest="operation",required=True)
  plan = commands.add_parser("plan")
  plan.add_argument("--id",action="append",required=True)
  plan.add_argument("--subject",required=True)
  plan.add_argument("--body-file",type=Path,required=True)
  plan.add_argument("--automatic-replies",action="store_true")
  plan.add_argument("--approve",action="store_true")
  activate = commands.add_parser("activate"); activate.add_argument("--approve",action="store_true")
  for command in ("stop","status","run"): commands.add_parser(command)
  args = parser.parse_args(argv)
  try:
    from ms_trevize.configuration import load_settings
    from ms_trevize.data.gmail_store import GmailStore
    from ms_trevize.features.leads.campaign import Campaign
    settings = load_settings(args.settings)
    store = GmailStore(args.directory)
    campaign = Campaign(store,None,settings["outreach"])
    if args.operation == "status": result = campaign.status()
    elif args.operation == "activate": result = campaign.activate(args.approve)
    elif args.operation == "stop": result = campaign.stop()
    elif args.operation == "run":
      from ms_trevize.bootstrap import run_outreach
      result = run_outreach(args.settings,args.directory)
    else:
      from ms_trevize.data.lead_api import LeadApi
      from ms_trevize.features.leads.outreach import LeadOutreach
      if not settings["leadApi"]["enabled"]: raise ValueError("API desabilitada.")
      crm = LeadApi(settings["leadApi"],args.settings.resolve().parent); crm.login()
      campaign.outreach = LeadOutreach(crm,store,None)
      if args.body_file.stat().st_size > 16000: raise ValueError("Modelo excede limite.")
      result = campaign.plan(args.id,args.subject,args.body_file.read_text(encoding="utf-8"),args.approve,args.automatic_replies)
    print(json.dumps(result,ensure_ascii=False,indent=2)); return 0
  except (OSError,ValueError,KeyError,TypeError,ImportError):
    print(json.dumps({"status":"failed","message":"Confira plano, configuração, OAuth e histórico; nenhuma chave de envio foi ativada."},ensure_ascii=False),file=sys.stderr)
    return 1
