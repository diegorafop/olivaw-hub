"""Configuração JSON sem credenciais e sem efeitos externos."""
import json
import math
from pathlib import Path


def unique_keys(pairs):
  result = {}
  for key, value in pairs:
    if key in result:
      raise ValueError("Chave JSON duplicada: " + key)
    result[key] = value
  return result


def load_settings(path):
  path = Path(path).expanduser().resolve(strict=True)
  if path.stat().st_size > 65536:
    raise ValueError("setting.json excede 64 KiB.")
  settings = json.loads(path.read_text(encoding="utf-8"), object_pairs_hook=unique_keys,
                        parse_constant=lambda value: (_ for _ in ()).throw(ValueError("Número JSON inválido.")))
  if not isinstance(settings, dict) or type(settings.get("settingsVersion")) is not int or settings["settingsVersion"] != 1:
    raise ValueError("Versão de configuração não suportada.")
  for section in ("agent", "schedule", "execution", "ai", "skill", "localState"):
    if not isinstance(settings.get(section), dict):
      raise ValueError("Seção obrigatória: " + section)
  for section in ("schedule", "execution"):
    if type(settings[section].get("enabled")) is not bool:
      raise ValueError("enabled deve ser booleano: " + section)
  for section, key in (("schedule", "intervalHours"), ("execution", "timeoutSeconds")):
    value = settings[section].get(key)
    if type(value) not in (int, float) or not math.isfinite(value) or value <= 0:
      raise ValueError("Limite positivo obrigatório: " + key)
  if not settings["agent"].get("id") or not settings["agent"].get("module"):
    raise ValueError("Identidade do agente obrigatória.")
  if not isinstance(settings["localState"].get("directory"), str) or not settings["localState"]["directory"]:
    raise ValueError("Diretório de estado obrigatório.")
  if settings["execution"]["enabled"]:
    for section in ("ssh", "audit", "reportApi"):
      if not isinstance(settings.get(section), dict):
        raise ValueError("Seção de execução obrigatória: " + section)
    if type(settings["audit"].get("logWindowHours")) is not int or not 1 <= settings["audit"]["logWindowHours"] <= 24:
      raise ValueError("Janela de logs deve ser inteira entre 1 e 24 horas.")
    if not isinstance(settings["audit"].get("hostId"), str) or not 1 <= len(settings["audit"]["hostId"]) <= 128:
      raise ValueError("Identificador do host obrigatório.")
    if settings["ssh"].get("enabled") is not True or settings["ssh"].get("strictHostKeyChecking") is not True:
      raise ValueError("SSH habilitado com host verificado é obrigatório.")
    if type(settings["reportApi"].get("enabled")) is not bool:
      raise ValueError("Habilitação da API deve ser booleana.")
    if type(settings["ai"].get("enabled", False)) is not bool:
      raise ValueError("Habilitação da IA deve ser booleana.")
    if settings["ai"].get("enabled"):
      if settings["ai"].get("provider") != "codex-cli":
        raise ValueError("Provedor IA não suportado.")
      if type(settings["ai"].get("timeoutSeconds")) is not int or not 10 <= settings["ai"]["timeoutSeconds"] <= 180:
        raise ValueError("Timeout IA deve ficar entre 10 e 180 segundos.")
    if type(settings["ssh"].get("port")) is not int or not 1 <= settings["ssh"]["port"] <= 65535:
      raise ValueError("Porta SSH inválida.")
    ports = settings["audit"].get("expectedPublicTcpPorts", [22, 80, 443])
    if not isinstance(ports, list) or len(ports) > 32 or any(type(port) is not int or not 1 <= port <= 65535 for port in ports):
      raise ValueError("Portas TCP previstas inválidas.")
    if settings["schedule"]["intervalHours"] < 0.05:
      raise ValueError("Intervalo mínimo de execução: três minutos.")
    for key, minimum, maximum in (("maxBytes", 262144, 104857600), ("retentionDays", 1, 30)):
      value = settings["localState"].get(key)
      if type(value) is not int or not minimum <= value <= maximum:
        raise ValueError("Retenção local inválida: " + key)
  remediation = settings.get("remediation", {"enabled": False})
  if not isinstance(remediation, dict) or type(remediation.get("enabled")) is not bool:
    raise ValueError("Configuração de correção inválida.")
  if remediation["enabled"]:
    value = remediation.get("pollSeconds")
    if type(value) is not int or not 15 <= value <= 300:
      raise ValueError("Consulta de aprovações deve ficar entre 15 e 300 segundos.")
  return settings
