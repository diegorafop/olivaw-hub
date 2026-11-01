"""Busca e rolagem limitada da interface pública, sem contornar bloqueios."""
import time
from urllib.parse import urlsplit
from ms_trevize.domain.candidate import parse_card

CARD_SCRIPT = """cards => cards.map(el => ({text:el.innerText, links:[...el.querySelectorAll('a[href]')].map(a=>({url:a.href}))}))"""


def collect_panel(page, settings, known, deadline):
  found, observed, stale = {}, set(), 0
  for _ in range(settings["maxScrolls"]):
    if time.monotonic() >= deadline:
      break
    text = page.locator("body").inner_text()[:12000].casefold()
    if any(term in text for term in ("unusual traffic", "tráfego incomum", "não sou um robô", "before you continue to google", "antes de continuar para o google")):
      raise ValueError("Maps exige intervenção humana; coleta interrompida.")
    cards = page.locator('[role="article"]').evaluate_all(CARD_SCRIPT)
    previous = len(observed)
    for card in cards:
      observed.add(card.get("text", "")[:500])
      candidate = parse_card(card, settings["excludeTerms"])
      if candidate and candidate.key not in known:
        found[candidate.key] = candidate
        if len(found) >= settings["limit"]:
          return list(found.values())
    stale = stale + 1 if len(observed) == previous else 0
    if stale >= 4 or len(observed) >= settings["maxCandidates"]:
      break
    feed = page.locator('[role="feed"]').first
    if feed.count():
      feed.evaluate("el => el.scrollBy(0, Math.max(el.clientHeight * .85, 700))")
    else:
      page.mouse.move(350, 700)
      page.mouse.wheel(0, 850)
    page.wait_for_timeout(settings["waitMilliseconds"])
  return list(found.values())


class MapsCollector:
  def __init__(self, settings, timeout):
    self.settings, self.timeout = settings, timeout

  def collect(self, known):
    from playwright.sync_api import sync_playwright
    deadline = time.monotonic() + self.timeout
    with sync_playwright() as playwright:
      browser = playwright.chromium.launch(headless=self.settings["headless"])
      try:
        context = browser.new_context(locale="pt-BR", viewport={"width":1440,"height":1000},
                                      accept_downloads=False, service_workers="block")
        def route(request):
          parsed = urlsplit(request.request.url)
          host = parsed.hostname or ""
          if parsed.scheme != "https" or not any(host == domain or host.endswith("." + domain) for domain in ("google.com", "gstatic.com", "googleapis.com", "googleusercontent.com")):
            request.abort()
          else:
            request.continue_()
        context.route("**/*", route)
        page = context.new_page()
        page.set_default_timeout(15000)
        page.goto("https://www.google.com/maps", wait_until="domcontentloaded", timeout=45000)
        search = page.locator('input[name="q"], #searchboxinput').first
        search.fill(self.settings["segment"] + " " + self.settings["queryLocation"])
        search.press("Enter")
        page.wait_for_timeout(5000)
        return collect_panel(page, self.settings, known, deadline)
      finally:
        browser.close()
