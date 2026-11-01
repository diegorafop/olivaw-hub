"""Smoke explícito do navegador: fixture HTML local, sem rede ou API."""
import time
from playwright.sync_api import sync_playwright
from ms_trevize.data.maps_collector import collect_panel


def main():
  with sync_playwright() as playwright:
    browser=playwright.chromium.launch(headless=True)
    try:
      context=browser.new_context()
      context.route('**/*',lambda route:route.abort())
      page=context.new_page()
      page.set_content('''<main>Resultados</main><div role="feed" style="height:400px;overflow:auto"></div><script>
      const feed=document.querySelector('[role=feed]');let count=0;
      function append(n){for(let i=0;i<n;i++){count++;const card=document.createElement('article');card.setAttribute('role','article');card.style.height='120px';card.innerHTML='<a href="https://www.google.com/maps/place/Fixture/data=!19sfixture'+count+'">Eventos Fixture '+count+'</a><div>Empresa de organização de eventos · Rua de teste, '+count+'</div><div>(41) 99999-0000</div>';feed.append(card);}}
      append(10);feed.onscroll=()=>{if(count<25)append(5)};
      </script>''')
      result=collect_panel(page,{'limit':20,'maxScrolls':10,'maxCandidates':50,'excludeTerms':[],'waitMilliseconds':100},set(),time.monotonic()+15)
      assert len(result)==20
      assert len({item.key for item in result})==20
      print('Chromium local: 20 candidatos únicos após rolagem; rede bloqueada.')
    finally:browser.close()


if __name__=='__main__':main()
