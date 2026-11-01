"""Leitura limitada de HTML público com destino fixado por IP e sem executar scripts."""
import http.client
from html.parser import HTMLParser
import ipaddress
import re
import socket
import ssl
from urllib.parse import urlsplit, urljoin
from urllib.robotparser import RobotFileParser


class PageParser(HTMLParser):
  def __init__(self):
    super().__init__()
    self.title, self.text, self.links = [], [], []
    self.h1 = self.images = self.images_without_alt = self.forms = 0
    self.description = self.viewport = False
    self.in_title = False
    self.ignore = 0

  def handle_starttag(self, tag, attrs):
    values = dict(attrs)
    if tag in ("script", "style", "noscript"):
      self.ignore += 1
    if tag == "title": self.in_title = True
    if tag == "h1": self.h1 += 1
    if tag == "form": self.forms += 1
    if tag == "meta":
      self.description |= values.get("name", "").lower() == "description" and bool(values.get("content"))
      self.viewport |= values.get("name", "").lower() == "viewport"
    if tag == "img":
      self.images += 1
      self.images_without_alt += not bool(values.get("alt", "").strip())
    if tag == "a" and values.get("href"): self.links.append(values["href"])

  def handle_endtag(self, tag):
    if tag in ("script", "style", "noscript"): self.ignore = max(0, self.ignore - 1)
    if tag == "title": self.in_title = False

  def handle_data(self, text):
    if not self.ignore:
      self.text.append(text.strip())
      if self.in_title: self.title.append(text)


def public_get(url, timeout, maximum, redirects=3):
  parsed = urlsplit(url)
  if parsed.scheme not in ("https", "http") or not parsed.hostname or parsed.username or parsed.password:
    raise ValueError("URL externa inválida.")
  port = parsed.port or (443 if parsed.scheme == "https" else 80)
  if port not in (80, 443): raise ValueError("Porta externa não permitida.")
  addresses = socket.getaddrinfo(parsed.hostname, port, type=socket.SOCK_STREAM)
  if not addresses or any(not ipaddress.ip_address(item[4][0]).is_global for item in addresses):
    raise ValueError("Destino de rede não público.")
  # Conecta ao IP já validado; TLS mantém hostname e verificação de certificado.
  connection = http.client.HTTPConnection(parsed.hostname, port, timeout=timeout)
  sock = socket.create_connection(addresses[0][4][:2], timeout=timeout)
  try:
    if parsed.scheme == "https": sock = ssl.create_default_context().wrap_socket(sock, server_hostname=parsed.hostname)
    connection.sock = sock
    path = parsed.path or "/"
    if parsed.query: path += "?" + parsed.query
    connection.request("GET", path, headers={"User-Agent":"Olivaw-Trevize/0.1", "Accept":"text/html,text/plain", "Accept-Encoding":"identity"})
    response = connection.getresponse()
    if response.status in (301, 302, 303, 307, 308):
      if redirects <= 0: raise ValueError("Redirecionamentos excedidos.")
      target = urljoin(url, response.getheader("Location", ""))
      return public_get(target, timeout, maximum, redirects - 1)
    body = response.read(maximum + 1)
    if len(body) > maximum: raise ValueError("Página excede limite de leitura.")
    return response.status, response.getheader("Content-Type", ""), body.decode("utf-8", errors="replace"), url
  finally:
    connection.close()
    sock.close()


class WebsiteReview:
  def __init__(self, settings, fetch=public_get):
    self.settings, self.fetch = settings, fetch

  def analyze(self, url):
    if not url or not self.settings["enabled"]:
      return {"status":"not-checked", "reason":"site-não-informado-ou-análise-desabilitada"}
    host = (urlsplit(url).hostname or "").lower()
    if any(host == domain or host.endswith("." + domain) for domain in ("instagram.com", "facebook.com", "wa.me", "whatsapp.com", "shopee.com.br")):
      return {"status":"social-only", "url":url}
    try:
      base = urlsplit(url)
      robots_url = base.scheme + "://" + base.netloc + "/robots.txt"
      status, _, rules, _ = self.fetch(robots_url, self.settings["timeoutSeconds"], min(65536,self.settings["maxBytes"]))
      if status not in (200, 404): return {"status":"inconclusive", "reason":"robots-indisponível"}
      if status == 200:
        robots = RobotFileParser(); robots.parse(rules.splitlines())
        if not robots.can_fetch("Olivaw-Trevize", url): return {"status":"not-checked", "reason":"robots-restringe-leitura"}
      status, content_type, content, target = self.fetch(url, self.settings["timeoutSeconds"], self.settings["maxBytes"])
      if status != 200 or "text/html" not in content_type:
        return {"status":"inconclusive", "httpStatus":status, "reason":"uma-tentativa-não-prova-indisponibilidade"}
      parser = PageParser(); parser.feed(content)
      whatsapp = next((re.search(r"(?:wa\.me/|api\.whatsapp\.com/send\?phone=)(?:55)?([1-9][0-9]9[0-9]{8})(?:\D|$)", link) for link in parser.links if re.search(r"wa\.me/|api\.whatsapp\.com/send\?phone=", link)), None)
      contact = next((urljoin(target, link) for link in parser.links if re.search(r"contato|contact", link, re.I)), "")
      return {"status":"checked", "url":target, "httpStatus":status, "title":" ".join(parser.title)[:300],
              "hasForm":bool(parser.forms), "hasDescription":parser.description, "hasViewport":parser.viewport, "h1Count":parser.h1,
              "imagesWithoutAlt":parser.images_without_alt, "whatsapp":whatsapp.group(1) if whatsapp else "",
              "contactPage":contact, "publicText":" ".join(parser.text)[:6000],
              "limitations":"Leitura passiva de uma página; sem medição de desempenho, teste visual ou envio de formulário."}
    except (OSError, ValueError, http.client.HTTPException):
      return {"status":"inconclusive", "reason":"leitura-indisponível-ou-destino-restrito"}
