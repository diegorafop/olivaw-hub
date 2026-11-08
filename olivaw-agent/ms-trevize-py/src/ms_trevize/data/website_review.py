"""Leitura limitada de HTML público com destino fixado por IP e sem executar scripts."""
import http.client
from html.parser import HTMLParser
import ipaddress
import re
import socket
import ssl
from datetime import datetime, timezone
from urllib.parse import urlsplit, urljoin, unquote, urldefrag
from urllib.robotparser import RobotFileParser


class PageParser(HTMLParser):
  def __init__(self):
    super().__init__()
    self.title, self.text, self.links = [], [], []
    self.h1 = self.images = self.images_without_alt = self.forms = 0
    self.description = self.viewport = False
    self.in_title = False
    self.ignore = 0
    self.contact_links, self.form_fields = [], []
    self.current_link = None
    self.current_form = None

  def handle_starttag(self, tag, attrs):
    values = {key:value or "" for key,value in attrs}
    if self.ignore and tag not in ("script", "style", "noscript"): return
    if tag in ("script", "style", "noscript"):
      self.ignore += 1
    if tag == "title": self.in_title = True
    if tag == "h1": self.h1 += 1
    if tag == "form":
      self.forms += 1
      self.current_form = []
      self.form_fields.append(self.current_form)
    if tag in ("input", "textarea", "select") and self.current_form is not None:
      kind = values.get("type", "text").lower()
      if kind not in ("hidden", "submit", "button", "reset"):
        self.current_form.append((tag, kind,
          " ".join(str(values.get(key) or "") for key in ("name", "id", "placeholder", "aria-label")).lower()))
    if tag == "meta":
      self.description |= values.get("name", "").lower() == "description" and bool(values.get("content"))
      self.viewport |= values.get("name", "").lower() == "viewport"
    if tag == "img":
      self.images += 1
      self.images_without_alt += not bool(values.get("alt", "").strip())
    if tag == "a" and values.get("href"):
      self.links.append(values["href"])
      self.current_link = values["href"]
      if re.search(r"contato|contact|fale[-_ ]?conosco", values["href"], re.I):
        self.contact_links.append(values["href"])

  def handle_endtag(self, tag):
    if tag in ("script", "style", "noscript"): self.ignore = max(0, self.ignore - 1)
    if tag == "title": self.in_title = False
    if tag == "a": self.current_link = None
    if tag == "form": self.current_form = None

  def handle_data(self, text):
    if not self.ignore:
      self.text.append(text.strip())
      if self.in_title: self.title.append(text)
      if self.current_link and re.search(r"contato|contact|fale\s+conosco", text, re.I):
        self.contact_links.append(self.current_link)

  @property
  def has_contact_form(self):
    for fields in self.form_fields:
      identity = any(kind == "email" or re.search(r"e[-_]?mail|telefone|phone", name) for _, kind, name in fields)
      message = any(tag == "textarea" or re.search(r"mensagem|message|assunto|subject", name) for tag, _, name in fields)
      if identity and message and not any(kind == "password" for _, kind, _ in fields): return True
    return False


EMAIL_PATTERN = re.compile(r"(?<![\w.+-])[A-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[A-Z0-9](?:[A-Z0-9.-]*[A-Z0-9])?\.[A-Z]{2,63}(?![\w-])", re.I)


def public_emails(parser, source):
  text = " ".join(parser.text)
  mailto = []
  for link in parser.links:
    if link.lower().startswith("mailto:"):
      mailto.append(unquote(link[7:].split("?", 1)[0]))
  result = []
  for value in mailto + [text]:
    for match in EMAIL_PATTERN.finditer(value):
      email = match.group().lower()
      if len(email) > 254 or email.rsplit(".", 1)[-1] in ("png", "jpg", "jpeg", "webp", "svg", "gif"): continue
      if email not in [item["email"] for item in result]: result.append({"email":email, "sourceUrl":source})
      if len(result) == 5: return result
  return result


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
      robots = RobotFileParser(); robots.parse(rules.splitlines() if status == 200 else [])
      if status == 200:
        if not robots.can_fetch("Olivaw-Trevize", url): return {"status":"not-checked", "reason":"robots-restringe-leitura"}
      status, content_type, content, target = self.fetch(url, self.settings["timeoutSeconds"], self.settings["maxBytes"])
      if status != 200 or "text/html" not in content_type:
        return {"status":"inconclusive", "httpStatus":status, "reason":"uma-tentativa-não-prova-indisponibilidade"}
      parser = PageParser(); parser.feed(content)
      whatsapp = next((re.search(r"(?:wa\.me/|api\.whatsapp\.com/send\?phone=)(?:55)?([1-9][0-9]9[0-9]{8})(?:\D|$)", link) for link in parser.links if re.search(r"wa\.me/|api\.whatsapp\.com/send\?phone=", link)), None)
      contacts = self.contacts(parser, target, robots, base.netloc)
      return {"status":"checked", "url":target, "httpStatus":status, "title":" ".join(parser.title)[:300],
              "hasForm":bool(parser.forms), "hasDescription":parser.description, "hasViewport":parser.viewport, "h1Count":parser.h1,
              "imagesWithoutAlt":parser.images_without_alt, "whatsapp":whatsapp.group(1) if whatsapp else "",
              **contacts, "restrictedAreaUrl":next((urljoin(target,link) for link in parser.links
                if re.search(r"(?:^|[/#])(?:login|entrar|admin|painel|area-do-cliente)(?:[/#?.]|$)",link,re.I)
                and urlsplit(urljoin(target,link)).netloc.lower() == urlsplit(target).netloc.lower()), ""), "publicText":" ".join(parser.text)[:6000],
              "limitations":"Leitura passiva da página inicial e até três páginas de contato do mesmo site; sem medição de desempenho, teste visual ou envio de formulário. Formulários via JavaScript podem não ser detectados."}
    except (OSError, ValueError, http.client.HTTPException):
      return {"status":"inconclusive", "reason":"leitura-indisponível-ou-destino-restrito"}

  def contacts(self, parser, target, robots, robots_host):
    emails = public_emails(parser, target)
    form_url = target if parser.has_contact_form else ""
    contact_page, candidates = form_url, []
    host = urlsplit(target).netloc.lower()
    for link in parser.contact_links:
      absolute = urldefrag(urljoin(target, link))[0]
      parsed = urlsplit(absolute)
      if parsed.scheme not in ("http", "https") or parsed.netloc.lower() != host or parsed.username or parsed.password: continue
      if len(absolute) > 2048 or absolute == urldefrag(target)[0] or absolute in candidates: continue
      candidates.append(absolute)
      if len(candidates) == 3: break
    complete = True
    # As regras só valem para a origem consultada; redirects de origem exigem nova leitura.
    if host != robots_host.lower():
      try:
        origin = urlsplit(target)
        status, _, rules, _ = self.fetch(origin.scheme + "://" + origin.netloc + "/robots.txt",
          self.settings["timeoutSeconds"], min(65536, self.settings["maxBytes"]))
        if status not in (200, 404): candidates, complete = [], False
        else:
          robots = RobotFileParser(); robots.parse(rules.splitlines() if status == 200 else [])
      except (OSError, ValueError, http.client.HTTPException): candidates, complete = [], False
    for page in candidates:
      if not robots.can_fetch("Olivaw-Trevize", page):
        complete = False
        continue
      try:
        status, kind, content, final = self.fetch(page, self.settings["timeoutSeconds"], self.settings["maxBytes"])
        if status != 200 or "text/html" not in kind or urlsplit(final).netloc.lower() != host:
          complete = False
          continue
        detail = PageParser(); detail.feed(content)
        if not contact_page: contact_page = final
        for item in public_emails(detail, final):
          if len(emails) < 5 and not any(saved["email"] == item["email"] for saved in emails): emails.append(item)
        if detail.has_contact_form and not form_url: form_url = final
      except (OSError, ValueError, http.client.HTTPException): complete = False
    return {"publicEmails":emails, "contactPage":contact_page, "contactFormUrl":form_url,
      "contactFormStatus":"found" if form_url else "not-detected" if complete else "inconclusive",
      "contactCheckedAt":datetime.now(timezone.utc).isoformat()}
