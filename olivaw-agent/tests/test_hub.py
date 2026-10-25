import importlib.util
import json
from pathlib import Path
import shutil
from tempfile import TemporaryDirectory
import unittest

ROOT = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location("hub", ROOT / "hub.py")
hub = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(hub)


class HubTests(unittest.TestCase):
  def project(self, root, name):
    path = root / ("ms-" + name + "-py")
    module = "ms_" + name
    package = path / "src" / module
    package.mkdir(parents=True)
    (path / "setting.json").write_text(json.dumps({"agent": {"id": name, "module": module}}))
    (path / "pyproject.toml").write_text('[project]\nname = "ms-' + name + '-py"\n')
    (package / "__main__.py").write_text('import sys,json;print(json.dumps({"operation":sys.argv[1]}))')
    return path

  def test_all_projects_receive_operation_from_their_own_module(self):
    with TemporaryDirectory() as directory:
      root = Path(directory)
      self.project(root, "baley")
      self.project(root, "future")
      result = hub.operate(root, "start")
      self.assertTrue(result["success"])
      self.assertEqual(["baley", "future"], [r["agent"] for r in result["agents"]])
      self.assertTrue(all(r["result"]["operation"] == "start" for r in result["agents"]))

  def test_symlink_and_inconsistent_identity_do_not_execute(self):
    with TemporaryDirectory() as directory:
      root = Path(directory)
      self.project(root, "baley")
      (root / "ms-alias-py").symlink_to(root / "ms-baley-py", target_is_directory=True)
      bad = self.project(root, "wrong")
      (bad / "setting.json").write_text('{"agent":{"id":"other","module":"os"}}')
      result = hub.operate(root, "status")
      self.assertFalse(result["success"])
      self.assertEqual(2, len(result["rejectedProjects"]))
      self.assertEqual(["baley"], [r["agent"] for r in result["agents"]])
