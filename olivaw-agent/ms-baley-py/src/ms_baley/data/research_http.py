"""HTTP público de leitura, TLS verificado, DNS fixado e sem redirects."""
import http.client
import ipaddress
import socket
import ssl
from urllib.parse import urlsplit


def public_url(url):
  if not isinstance(url, str) or len(url) > 2048 or any(ord(c) < 32 for c in url):
    raise ValueError("URL inválida.")
  parsed = urlsplit(url)
  if parsed.scheme != "https" or not parsed.hostname or parsed.username or parsed.password or parsed.port not in (None, 443):
    raise ValueError("Somente HTTPS público sem credenciais.")
  host = parsed.hostname.lower()
  if host == "localhost" or host.endswith((".localhost", ".local", ".internal")):
    raise ValueError("Destino local não permitido.")
  try:
    address = ipaddress.ip_address(host)
  except ValueError:
    address = None
  if address is not None and not address.is_global:
    raise ValueError("IP privado não permitido.")
  return parsed


class ResearchHTTPError(ValueError):
  def __init__(self, status):
    super().__init__("HTTP " + str(status))
    self.code = status


class PinnedHTTPS(http.client.HTTPSConnection):
  def connect(self):
    addresses = socket.getaddrinfo(self.host, 443, type=socket.SOCK_STREAM)
    public = []
    for _, _, _, _, sockaddr in addresses:
      if not ipaddress.ip_address(sockaddr[0]).is_global:
        raise ValueError("DNS retornou endereço não público.")
      if sockaddr[0] not in public:
        public.append(sockaddr[0])
    if not public:
      raise ValueError("DNS sem destino público.")
    # Conecta ao IP validado; TLS continua conferindo o hostname original.
    raw = socket.create_connection((public[0], 443), self.timeout)
    try:
      self.sock = self._context.wrap_socket(raw, server_hostname=self.host)
    except Exception:
      raw.close()
      raise


class ResearchHTTP:
  def fetch(self, url, timeout, maximum):
    parsed = public_url(url)
    connection = PinnedHTTPS(parsed.hostname, timeout=timeout, context=ssl.create_default_context())
    try:
      path = parsed.path or "/"
      if parsed.query:
        path += "?" + parsed.query
      connection.request("GET", path, headers={"User-Agent": "Olivaw-Baley/0.2", "Accept": "application/json,text/html,application/rss+xml", "Accept-Encoding": "identity"})
      response = connection.getresponse()
      if response.status != 200:
        raise ResearchHTTPError(response.status)
      body = response.read(maximum + 1)
      if len(body) > maximum:
        raise ValueError("Resposta excede limite.")
      return body
    finally:
      connection.close()
