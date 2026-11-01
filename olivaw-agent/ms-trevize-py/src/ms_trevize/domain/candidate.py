"""Identidade estável e normalização de candidatos, sem I/O."""
from dataclasses import dataclass
import hashlib
import re
from urllib.parse import urlsplit
from uuid import UUID, uuid5

NAMESPACE = UUID("492223ad-b5bb-401c-95ea-d4f88449f214")


@dataclass(frozen=True)
class Candidate:
  key: str
  name: str
  category: str
  address: str
  phone: str
  website: str
  source: str

  @property
  def external_id(self):
    return str(uuid5(NAMESPACE, self.key))


def web_url(value):
  try:
    parsed = urlsplit(value)
    if parsed.scheme not in ("http", "https") or not parsed.hostname or parsed.username or parsed.password:
      return ""
    return value[:2048]
  except ValueError:
    return ""


def parse_card(card, exclusions):
  text = card.get("text", "")[:6000]
  lines = text.splitlines()
  links = card.get("links", [])
  source = next((item.get("url", "") for item in links if "/maps/place/" in item.get("url", "")), "")
  if not source or not lines:
    return None
  name = lines[0].strip()[:120]
  category = next((line.split("·")[0].strip() for line in lines if "·" in line and not re.search(r"\([0-9]{2}\)", line)), "")[:120]
  if any(term.casefold() in (category + " " + name).casefold() for term in exclusions):
    return None
  if not re.search(r"evento|feira|festa|cerimonial|buffet|formatura", name + " " + category, re.I):
    return None
  address_line = next((line for line in lines if line.startswith(category) and "·" in line), "")
  address = address_line.split("·")[-1].strip()[:300]
  phone = re.search(r"\([0-9]{2}\)\s*[0-9]{4,5}-[0-9]{4}", text)
  website = next((item.get("url", "") for item in links if "/maps/place/" not in item.get("url", "")), "")
  identifier = re.search(r"!19s([^!?&]+)", source) or re.search(r"!1s([^!]+)", source)
  identity = identifier.group(1) if identifier else name.casefold() + "|" + address.casefold()
  key = hashlib.sha256(identity.encode()).hexdigest()
  return Candidate(key, name, category, address, phone.group() if phone else "", web_url(website), web_url(source))
