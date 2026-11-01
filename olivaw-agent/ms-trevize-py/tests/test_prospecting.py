import json
from pathlib import Path
import socket
from tempfile import TemporaryDirectory
import unittest
from unittest.mock import Mock, patch
from ms_trevize.configuration import load_settings
from ms_trevize.data.batch_state import BatchState
from ms_trevize.data.local_runtime import LocalRuntime
from ms_trevize.data.website_review import WebsiteReview, public_get
from ms_trevize.data.ai_qualification import AIQualification
from ms_trevize.data.maps_collector import collect_panel
from ms_trevize.domain.candidate import parse_card
from ms_trevize.features.prospecting.commands.create_prospect_batch import CreateProspectBatchHandler

ROOT=Path(__file__).resolve().parents[1]


def card(name="Eventos Aurora", identity="test-place", website="https://example.test"):
  return {"text":name+"\n5,0\nEmpresa de organização de eventos · Rua de exemplo, 10\n(41) 99897-0017",
    "links":[{"url":"https://www.google.com/maps/place/Example/data=!19s"+identity}]+([{"url":website}] if website else [])}


class ProspectingTests(unittest.TestCase):
  def setUp(self):
    self.temp=TemporaryDirectory();self.addCleanup(self.temp.cleanup)
    self.path=Path(self.temp.name)/"setting.json"
    settings=json.loads((ROOT/"setting.json").read_text());settings["search"]["limit"]=1
    self.path.write_text(json.dumps(settings));self.runtime=LocalRuntime(self.path)

  def test_settings_do_not_allow_boot_start_invalid_limits_or_duplicate_keys(self):
    self.assertFalse(load_settings(self.path)["lifecycle"]["autoStart"])
    settings=json.loads(self.path.read_text());settings["lifecycle"]["autoStart"]=True
    self.path.write_text(json.dumps(settings))
    with self.assertRaises(ValueError):load_settings(self.path)
    self.path.write_text('{"settingsVersion":1,"settingsVersion":1}')
    with self.assertRaises(ValueError):load_settings(self.path)

  def test_identity_is_stable_and_phone_does_not_become_whatsapp(self):
    candidate=parse_card(card(),[])
    other=parse_card(card(),[])
    self.assertEqual(candidate.external_id,other.external_id)
    self.assertEqual('(41) 99897-0017',candidate.phone)
    self.assertIsNone(parse_card({**card(),"text":"Contábil\nAssessoria contábil · Rua 10"},[]))

  def test_scrolling_collects_new_cards_and_deduplicates(self):
    page=Mock();body=Mock();articles=Mock();feed=Mock()
    body.inner_text.return_value="Resultados"
    articles.evaluate_all.side_effect=[[card("Eventos A","a")],[card("Eventos A","a"),card("Eventos B","b")]]
    page.locator.side_effect=lambda selector:body if selector=="body" else articles if selector=='[role="article"]' else feed
    feed.first.count.return_value=1
    settings={**self.runtime.settings["search"],"limit":2}
    result=collect_panel(page,settings,set(),float('inf'))
    self.assertEqual(["Eventos A","Eventos B"],[item.name for item in result])
    feed.first.evaluate.assert_called_once()
    known={item.key for item in result}
    self.assertEqual(2,len(known))

  def test_maps_blocking_stops_without_bypass(self):
    page=Mock();page.locator.return_value.inner_text.return_value="Tráfego incomum"
    with self.assertRaises(ValueError):collect_panel(page,self.runtime.settings["search"],set(),float('inf'))

  def test_website_checks_are_passive_and_do_not_infer_visual_failure(self):
    fetch=Mock(side_effect=[(404,'text/plain','','https://example.test/robots.txt'),
      (200,'text/html','<title>Eventos</title><h1>Serviços</h1><a href="https://wa.me/5541999990000">Contato</a><img src="x">','https://example.test')])
    result=WebsiteReview(self.runtime.settings["website"],fetch).analyze('https://example.test')
    self.assertEqual('checked',result['status']);self.assertEqual('41999990000',result['whatsapp'])
    self.assertEqual(1,result['imagesWithoutAlt']);self.assertFalse(result['hasForm'])
    self.assertIn('sem medição de desempenho',result['limitations'])
    blocked=Mock(return_value=(200,'text/plain','User-agent: *\nDisallow: /',''))
    self.assertEqual('not-checked',WebsiteReview(self.runtime.settings['website'],blocked).analyze('https://example.test')['status'])
    self.assertEqual(1,blocked.call_count)

  def test_private_network_destinations_never_connect(self):
    with patch('socket.getaddrinfo',return_value=[(socket.AF_INET,socket.SOCK_STREAM,6,'',('127.0.0.1',80))]), patch('socket.create_connection') as connect:
      with self.assertRaises(ValueError):public_get('http://example.test',1,1024)
      connect.assert_not_called()

  def test_delivery_is_confirmed_before_marking_seen_and_no_raw_contacts_in_state(self):
    candidate=parse_card(card(),[])
    collector=Mock();collector.collect.return_value=[candidate]
    site=Mock();site.analyze.return_value={'status':'not-checked'}
    ai=Mock();ai.analyze.return_value={'status':'disabled'}
    api=Mock();api.publish.return_value='delivered'
    handler=CreateProspectBatchHandler(BatchState(self.runtime),collector,site,ai,api)
    report=handler.handle()
    self.assertEqual(1,report['delivered']);self.assertFalse(report['contactPerformed'])
    self.assertIn(candidate.key,json.loads((self.runtime.state/'seen.json').read_text()))
    qualification=api.publish.call_args.args[1]
    self.assertEqual('',qualification['whatsapp']);self.assertEqual('draft',qualification['approachStatus'])
    self.assertEqual('pending',qualification['confidence'])
    self.assertNotIn(candidate.phone,(self.runtime.state/'last-cycle.json').read_text())
    api.publish.side_effect=ValueError('SECRET')
    (self.runtime.state/'seen.json').unlink()
    report=handler.handle()
    self.assertEqual(1,report['failed']);self.assertFalse((self.runtime.state/'seen.json').exists())
    self.assertNotIn('SECRET',json.dumps(report))

  def test_ai_has_no_tools_and_rejects_tool_events_or_invalid_schema(self):
    import subprocess
    result={"positives":"Serviços publicados.","observations":"Análise limitada.","questions":"O que querem facilitar?","approach":"Olá, somos da Olivaw."}
    runner=Mock(return_value=subprocess.CompletedProcess([],0,json.dumps({"type":"item.completed","item":{"type":"agent_message","text":json.dumps(result)}})))
    review=AIQualification(self.runtime.settings['ai'],'Não enviar mensagens.',runner)
    data=review.analyze(parse_card(card(),[]),{'status':'checked'},'Curitiba')
    self.assertEqual('completed',data['status'])
    args=runner.call_args.args[0]
    self.assertIn('read-only',args);self.assertIn('shell_tool',args)
    self.assertNotIn('password',runner.call_args.kwargs['input'])
    runner.return_value=subprocess.CompletedProcess([],0,json.dumps({"type":"item.completed","item":{"type":"command_execution"}}))
    with self.assertRaises(ValueError):review.analyze(parse_card(card(),[]),{},'Curitiba')
