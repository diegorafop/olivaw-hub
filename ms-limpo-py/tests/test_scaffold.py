import importlib.util
import os
from pathlib import Path
import shutil
import subprocess
import sys
from tempfile import TemporaryDirectory
import unittest


ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("new_agent", ROOT / "scripts/new_agent.py")
scaffold = importlib.util.module_from_spec(spec)
spec.loader.exec_module(scaffold)


class ScaffoldTests(unittest.TestCase):
    def test_cli_applies_fixed_prefix_and_suffix(self):
        with TemporaryDirectory() as directory:
            process = subprocess.run(
                [sys.executable, str(ROOT / "scripts/new_agent.py"), "giskard", "--output", directory],
                cwd=directory, capture_output=True, text=True, timeout=30,
            )
            self.assertEqual(process.returncode, 0, process.stdout + process.stderr)
            result = Path(directory) / "ms-giskard-py"
            self.assertTrue((result / "src/ms_giskard/cli.py").is_file())
            metadata = (result / "pyproject.toml").read_text(encoding="utf-8")
            self.assertIn('name = "ms-giskard-py"', metadata)
            self.assertIn('giskard = "ms_giskard.cli:main"', metadata)
            readme = (result / "README.md").read_text(encoding="utf-8")
            self.assertIn("--schema agent_giskard", readme)
            self.assertNotIn("python scripts/new_agent.py", readme)

    def test_generated_project_runs_independently(self):
        with TemporaryDirectory() as directory:
            root = Path(directory)
            template = root / "template"
            template.mkdir()
            files = list(scaffold.ROOT_FILES) + [f"src/ms_limpo/{p}" for p in scaffold.PACKAGE_FILES]
            for source in files:
                target = template / source
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(ROOT / source, target)
            for unwanted in (".env", ".git/config", ".venv/secret", "src/ms_limpo/personal.py",
                             "src/ms_limpo/__pycache__/cached.pyc", "dist/private.whl"):
                path = template / unwanted
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text("PRIVATE", encoding="utf-8")
            result = scaffold.generate("daneel-olivaw", root, template)
            shutil.rmtree(template)
            self.assertEqual(result.name, "ms-daneel-olivaw-py")
            self.assertFalse((result / "scripts").exists())
            self.assertNotIn("## Criar novos agentes", (result / "AGENTS.md").read_text(encoding="utf-8"))
            self.assertEqual(len([p for p in result.rglob("*") if p.is_file()]), len(files))
            for path in result.rglob("*"):
                if path.is_file():
                    text = path.read_text(encoding="utf-8")
                    self.assertNotIn("PRIVATE", text)
                    self.assertNotIn("ms_limpo", text)
            metadata = (result / "pyproject.toml").read_text(encoding="utf-8")
            self.assertIn('name = "ms-daneel-olivaw-py"', metadata)
            self.assertIn('daneel-olivaw = "ms_daneel_olivaw.cli:main"', metadata)
            self.assertIn('daneel-olivaw-migrate = "ms_daneel_olivaw.data.apply_migrations:main"', metadata)
            self.assertIn("--schema agent_daneel_olivaw", (result / "README.md").read_text(encoding="utf-8"))
            migration = result / "src/ms_daneel_olivaw/data/apply_migrations.py"
            self.assertIn('daneel-olivaw:migrations:', migration.read_text(encoding="utf-8"))
            environment = dict(os.environ, PYTHONPATH=str(result / "src"))
            for arguments in (("-m", "ms_daneel_olivaw", "example"),
                              ("-m", "unittest", "discover", "-s", "tests", "-v")):
                process = subprocess.run([sys.executable, *arguments], cwd=result, env=environment,
                                         capture_output=True, text=True, timeout=30)
                self.assertEqual(process.returncode, 0, process.stdout + process.stderr)

    def test_invalid_names_do_not_write(self):
        with TemporaryDirectory() as directory:
            for name in ("", "../escape", "Giskard", "a_b", "a/b", "a\\b", "1agent",
                         "a--b", "a-", "limpo", "ms-giskard", "giskard-agent-py", "ms-giskard-py", "a" * 58, "á", "/absolute"):
                with self.subTest(name=name), self.assertRaises(ValueError):
                    scaffold.generate(name, directory)
            self.assertEqual(list(Path(directory).iterdir()), [])

    def test_existing_destinations_remain_untouched(self):
        with TemporaryDirectory() as directory:
            destination = Path(directory) / "ms-giskard-py"
            destination.mkdir()
            with self.assertRaises(ValueError):
                scaffold.generate("giskard", directory)
            original = destination / "existing.txt"
            original.write_bytes(b"keep")
            with self.assertRaises(ValueError):
                scaffold.generate("giskard", directory)
            self.assertEqual(original.read_bytes(), b"keep")
            self.assertEqual(list(destination.iterdir()), [original])
            original.unlink()
            destination.rmdir()
            destination.write_bytes(b"file")
            with self.assertRaises(ValueError):
                scaffold.generate("giskard", directory)
            self.assertEqual(destination.read_bytes(), b"file")

    def test_invalid_output_and_missing_template_do_not_write(self):
        with TemporaryDirectory() as directory:
            root = Path(directory)
            with self.assertRaises(OSError):
                scaffold.generate("giskard", root / "missing")
            template = root / "empty-template"
            template.mkdir()
            with self.assertRaises(OSError):
                scaffold.generate("giskard", root, template)
            self.assertEqual(list(root.iterdir()), [template])
            with self.assertRaises(ValueError):
                scaffold.generate("giskard", ROOT)

    @unittest.skipIf(os.name == "nt", "Links exigem privilégios específicos no Windows")
    def test_symlink_source_is_rejected_before_writing(self):
        with TemporaryDirectory() as directory:
            root = Path(directory)
            template = root / "template"
            template.mkdir()
            (template / ".editorconfig").symlink_to(ROOT / ".editorconfig")
            with self.assertRaises(ValueError):
                scaffold.generate("giskard", root, template)
            self.assertFalse((root / "ms-giskard-py").exists())

    @unittest.skipIf(os.name == "nt", "Links exigem privilégios específicos no Windows")
    def test_symlink_destination_is_rejected(self):
        with TemporaryDirectory() as directory:
            destination = Path(directory) / "ms-giskard-py"
            destination.symlink_to(Path(directory) / "missing")
            with self.assertRaises(ValueError):
                scaffold.generate("giskard", directory)
            self.assertTrue(destination.is_symlink())
