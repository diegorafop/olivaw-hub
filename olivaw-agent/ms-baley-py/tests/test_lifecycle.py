import json
import os
from pathlib import Path
from tempfile import TemporaryDirectory
from concurrent.futures import ThreadPoolExecutor
import unittest

from ms_baley.data.local_runtime import LocalRuntime

ROOT = Path(__file__).resolve().parents[1]


@unittest.skipUnless(os.name == "posix", "Primeira versão Linux/POSIX")
class LifecycleTests(unittest.TestCase):
  def setUp(self):
    self.directory = TemporaryDirectory()
    self.addCleanup(self.directory.cleanup)
    self.path = Path(self.directory.name) / "setting.json"
    settings = json.loads((ROOT / "setting.json").read_text())
    settings["execution"]["enabled"] = False
    settings["schedule"]["enabled"] = False
    self.path.write_text(json.dumps(settings))
    self.runtime = LocalRuntime(self.path)
    self.addCleanup(self.runtime.stop)

  def test_start_is_idempotent_and_stop_is_cooperative(self):
    self.assertFalse(self.runtime.status()["running"])
    self.assertFalse(self.runtime.state.exists())
    first = self.runtime.start()
    second = self.runtime.start()
    self.assertEqual(first["instanceId"], second["instanceId"])
    self.assertEqual("waiting-configuration", first["state"])
    self.assertFalse(first["analysisEnabled"])
    self.assertFalse(self.runtime.stop()["running"])
    self.assertFalse(self.runtime.stop()["running"])
    self.assertEqual(0o700, self.runtime.state.stat().st_mode & 0o777)

  def test_concurrent_starts_share_one_worker(self):
    with ThreadPoolExecutor(max_workers=2) as executor:
      states = list(executor.map(lambda _: LocalRuntime(self.path).start(), range(2)))
    self.assertEqual(states[0]["instanceId"], states[1]["instanceId"])
    self.assertTrue(self.runtime.status()["running"])

  def test_stale_pid_file_never_authorizes_killing_another_process(self):
    self.runtime.prepare()
    (self.runtime.state / "status.json").write_text(json.dumps({"pid": os.getpid(), "instanceId": "old", "state": "running"}))
    self.assertFalse(self.runtime.stop()["running"])

  def test_invalid_analysis_config_fails_without_starting_or_network(self):
    settings = json.loads(self.path.read_text())
    settings["execution"]["enabled"] = True
    del settings["ssh"]
    self.path.write_text(json.dumps(settings))
    with self.assertRaises(ValueError):
      LocalRuntime(self.path).start()
    self.assertFalse(self.runtime.state.exists())
