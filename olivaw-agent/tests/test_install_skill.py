import importlib.util
from pathlib import Path
from tempfile import TemporaryDirectory
import unittest

ROOT = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location("install_skill", ROOT / "scripts/install_skill.py")
installer = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(installer)


class InstallTests(unittest.TestCase):
  def test_copy_matches_source_and_different_version_requires_explicit_replace(self):
    with TemporaryDirectory() as directory:
      destination = Path(directory) / "safety-docker"
      self.assertEqual("installed", installer.install(installer.SOURCE, destination))
      self.assertEqual(installer.files(installer.SOURCE), installer.files(destination))
      (destination / "SKILL.md").write_text("local edit")
      with self.assertRaises(ValueError):
        installer.install(installer.SOURCE, destination)
      self.assertEqual("local edit", (destination / "SKILL.md").read_text())
      installer.install(installer.SOURCE, destination, replace=True)
      self.assertEqual(installer.files(installer.SOURCE), installer.files(destination))
