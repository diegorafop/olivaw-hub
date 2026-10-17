import os
import shutil
import subprocess
import tempfile
import unittest
from pathlib import Path


class GitFlowCycleTest(unittest.TestCase):
    def test_native_finish_preserves_references_and_tags(self):
        if subprocess.run(["git", "flow", "version"], capture_output=True).returncode:
            self.skipTest("Git Flow AVH não instalado")
        source = Path(__file__).resolve().parents[3]
        with tempfile.TemporaryDirectory(prefix="olivaw-flow-test-", dir="/tmp") as temp:
            root = Path(temp) / "repo"
            root.mkdir()
            environment = os.environ.copy()
            environment["GIT_MERGE_AUTOEDIT"] = "no"

            def run(*args):
                result = subprocess.run(args, cwd=root, env=environment, capture_output=True, text=True)
                self.assertEqual(0, result.returncode, result.stdout + result.stderr)
                return result.stdout

            run("git", "init", "-q", "-b", "master")
            run("git", "config", "user.name", "Olivaw Test")
            run("git", "config", "user.email", "test@example.invalid")
            shutil.copytree(source / "scripts/gitflow", root / "scripts/gitflow", ignore=shutil.ignore_patterns("__pycache__"))
            shutil.copytree(source / ".githooks", root / ".githooks")
            run("git", "add", ".")
            run("git", "commit", "-qm", "initial setup")
            run("git", "flow", "init", "-d")
            run("python3", "scripts/gitflow/setup.py")
            run("git", "flow", "feature", "start", "issue001-exemplo")
            (root / "feature.txt").write_text("feature\n")
            run("git", "add", "feature.txt")
            run("git", "commit", "-qm", "feat: exemplo")
            self.assertIn("Refs #1", run("git", "log", "-1", "--format=%B"))
            run("git", "flow", "feature", "finish", "issue001-exemplo")
            self.assertEqual("develop", run("git", "branch", "--show-current").strip())
            run("git", "flow", "release", "start", "1.0.0")
            run("git", "flow", "release", "finish", "-m", "Release 1.0.0", "1.0.0")
            self.assertIn("Refs #1", run("git", "log", "master", "--format=%B"))
            self.assertIn("1.0.0", run("git", "tag", "--list"))
            run("python3", "scripts/gitflow/start_hotfix.py", "2", "1.0.1")
            (root / "hotfix.txt").write_text("fix\n")
            run("git", "add", "hotfix.txt")
            run("git", "commit", "-qm", "fix: exemplo")
            self.assertIn("Refs #2", run("git", "log", "-1", "--format=%B"))
            run("git", "flow", "hotfix", "finish", "-m", "Hotfix 1.0.1", "1.0.1")
            self.assertIn("1.0.1", run("git", "tag", "--list"))
            self.assertIn("Refs #2", run("git", "log", "master", "--format=%B"))
            self.assertIn("Refs #2", run("git", "log", "develop", "--format=%B"))
            remote = Path(temp) / "remote.git"
            run("git", "init", "--bare", "-q", str(remote))
            run("git", "remote", "add", "origin", str(remote))
            run("git", "push", "origin", "master", "develop", "--tags")


if __name__ == "__main__":
    unittest.main()
