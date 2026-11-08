import json
import os
from pathlib import Path
from tempfile import TemporaryDirectory
import unittest
from ms_trevize.data.local_runtime import LocalRuntime

ROOT=Path(__file__).resolve().parents[1]


@unittest.skipUnless(os.name=='posix','Lifecycle Linux/POSIX')
class LifecycleTests(unittest.TestCase):
  def setUp(self):
    self.temp=TemporaryDirectory();self.addCleanup(self.temp.cleanup)
    self.path=Path(self.temp.name)/'setting.json'
    settings=json.loads((ROOT/'setting.json').read_text());settings['execution']['enabled']=False
    settings['outreach']['enabled']=False
    self.path.write_text(json.dumps(settings));self.runtime=LocalRuntime(self.path)
    self.addCleanup(self.runtime.stop)

  def test_status_never_starts_and_start_is_explicit_and_idempotent(self):
    self.assertFalse(self.runtime.status()['running']);self.assertFalse(self.runtime.state.exists())
    self.assertFalse(self.runtime.status()['autoStart'])
    first=self.runtime.start();second=self.runtime.start()
    self.assertTrue(first['running']);self.assertEqual(first['instanceId'],second['instanceId'])
    self.assertFalse(self.runtime.stop()['running'])
    self.assertFalse(self.runtime.status()['running'])

  def test_stale_state_after_reboot_never_restarts_or_kills_other_process(self):
    self.runtime.prepare()
    (self.runtime.state/'status.json').write_text(json.dumps({'state':'collecting','instanceId':'previous-boot','pid':os.getpid()}))
    self.assertFalse(self.runtime.status()['running'])
    self.assertEqual('stopped',self.runtime.stop()['state'])


class ScheduleTests(unittest.TestCase):
  def test_first_cycle_is_immediate_and_next_waits_24_hours_after_completion(self):
    from ms_trevize.domain.schedule import Schedule
    timer=Schedule(24,100)
    self.assertTrue(timer.due(100))
    timer.finish(500)
    self.assertFalse(timer.due(500+86400-1))
    self.assertTrue(timer.due(500+86400))
    # Uma nova sessão não retoma o timer de um processo morto.
    self.assertTrue(Schedule(24,1000).due(1000))

  def test_no_repeat_only_allows_one_cycle_per_start(self):
    from ms_trevize.domain.schedule import Schedule
    timer=Schedule(24,0,False)
    timer.finish(1)
    self.assertFalse(timer.due(999999))
