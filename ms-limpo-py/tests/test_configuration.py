import json
from pathlib import Path
from tempfile import TemporaryDirectory
import unittest

from ms_limpo.configuration import load_settings

ROOT = Path(__file__).resolve().parents[1]


class ConfigurationTests(unittest.TestCase):
  def test_defaults_disable_external_execution_and_allow_optional_extensions(self):
    settings = load_settings(ROOT / "setting.json")
    self.assertFalse(settings["execution"]["enabled"])
    self.assertFalse(settings["schedule"]["enabled"])
    self.assertIsNone(settings["ai"]["model"])

  def test_invalid_versions_duplicate_keys_and_limits_fail_before_execution(self):
    original = json.loads((ROOT / "setting.json").read_text())
    with TemporaryDirectory() as directory:
      path = Path(directory) / "setting.json"
      for value in [True, 0, -1, "6", float("nan")]:
        settings = json.loads(json.dumps(original))
        settings["schedule"]["intervalHours"] = value
        path.write_text(json.dumps(settings))
        with self.subTest(value=value), self.assertRaises(ValueError):
          load_settings(path)
      path.write_text('{"settingsVersion":1,"settingsVersion":2}')
      with self.assertRaises(ValueError):
        load_settings(path)
