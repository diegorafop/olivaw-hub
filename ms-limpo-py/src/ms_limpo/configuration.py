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
  return settings
