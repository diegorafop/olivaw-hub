"""Configuração validada antes de operações externas."""
import json
import math
from pathlib import Path
from urllib.parse import urlsplit


def unique_keys(pairs):
  result = {}
  for key, value in pairs:
    if key in result:
      raise ValueError("Chave de configuração duplicada.")
    result[key] = value
  return result


def load_settings(path):
  path = Path(path).expanduser().resolve(strict=True)
  if path.stat().st_size > 65536:
    raise ValueError("Configuração excede limite.")
  data = json.loads(path.read_text(), object_pairs_hook=unique_keys,
                    parse_constant=lambda _: (_ for _ in ()).throw(ValueError("Número inválido.")))
  if data.get("settingsVersion") != 1 or data.get("agent") != {"id": "trevize", "module": "ms_trevize"}:
    raise ValueError("Identidade ou versão inválida.")
  if data.get("lifecycle", {}).get("autoStart") is not False:
    raise ValueError("Trevize só permite início manual.")
  for section in ("schedule", "execution", "ai", "website", "leadApi"):
    if type(data.get(section, {}).get("enabled")) is not bool:
      raise ValueError("Habilitação inválida: " + section)
  for section, key, minimum, maximum in (
      ("schedule", "intervalHours", 1, 720), ("execution", "timeoutSeconds", 60, 7200),
      ("search", "limit", 1, 100), ("search", "maxScrolls", 1, 100),
      ("search", "maxCandidates", 20, 500), ("search", "waitMilliseconds", 1000, 10000),
      ("website", "timeoutSeconds", 1, 30), ("website", "maxBytes", 1024, 1048576),
      ("ai", "timeoutSeconds", 10, 180), ("ai", "maxCallsPerCycle", 1, 100),
      ("localState", "retentionDays", 1, 30), ("localState", "maxBytes", 262144, 104857600)):
    value = data.get(section, {}).get(key)
    if type(value) not in (int, float) or not math.isfinite(value) or not minimum <= value <= maximum:
      raise ValueError("Limite inválido: " + section + "." + key)
    if section != "schedule" and type(value) is not int:
      raise ValueError("Limite inteiro obrigatório.")
  search = data["search"]
  for key in ("region", "queryLocation", "segment"):
    if not isinstance(search.get(key), str) or not 1 <= len(search[key]) <= 120:
      raise ValueError("Busca incompleta.")
  if search.get("provider") != "google-maps-browser" or type(search.get("headless")) is not bool:
    raise ValueError("Provedor de busca inválido.")
  if not isinstance(search.get("excludeTerms"), list) or any(not isinstance(v, str) for v in search["excludeTerms"]):
    raise ValueError("Exclusões inválidas.")
  if data["ai"].get("provider") != "codex-cli":
    raise ValueError("Provedor IA inválido.")
  if data["ai"].get("model") is not None and not isinstance(data["ai"]["model"], str):
    raise ValueError("Modelo inválido.")
  for section, key in (("skill", "path"), ("localState", "directory"), ("leadApi", "credentialsPath"), ("leadApi", "credentialsSection")):
    if not isinstance(data.get(section, {}).get(key), str) or not data[section][key]:
      raise ValueError("Caminho ou seção obrigatório.")
  url = urlsplit(data["leadApi"].get("baseUrl", ""))
  if url.scheme != "https" or not url.hostname or url.username or url.password or url.query or url.fragment:
    raise ValueError("API deve usar HTTPS sem credenciais na URL.")
  return data
