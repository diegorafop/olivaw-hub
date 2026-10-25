import importlib.util
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import json
import subprocess

SPEC = importlib.util.spec_from_file_location('prepare', Path(__file__).parents[1] / 'scripts/prepare.py')
prepare = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(prepare)


class PreparationSafetyTests(unittest.TestCase):
    def fixture(self, root):
        return {'networks': {'postgres-net': {'name': 'postgres-net'}},
                'volumes': {'postgres-data': {'name': 'production-data'}},
                'services': {'ms-traefik-pr': {'volumes': [{'target': '/acme.json', 'source': str(root / 'acme.json')}]}}}

    def test_new_host_refuses_existing_infrastructure_before_creating_resources(self):
        with tempfile.TemporaryDirectory() as directory:
            config = self.fixture(Path(directory))
            with patch.object(prepare.subprocess, 'run', side_effect=[
                subprocess.CompletedProcess([], 0, json.dumps(config)),
                subprocess.CompletedProcess([], 0, 'ms-postgres-db\n')
            ]) as run, patch('sys.argv', ['prepare.py', '--new-host']):
                with self.assertRaisesRegex(SystemExit, 'existem containers'):
                    prepare.main()
                self.assertEqual(2, run.call_count)
                self.assertFalse((Path(directory) / 'acme.json').exists())

    def test_default_mode_never_creates_missing_volume_or_acme(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            config = self.fixture(root)
            with patch.object(prepare.subprocess, 'run', side_effect=[
                subprocess.CompletedProcess([], 0, json.dumps(config)),
                subprocess.CompletedProcess([], 0, ''),
                subprocess.CompletedProcess([], 0),
                subprocess.CompletedProcess([], 1)
            ]) as run, patch('sys.argv', ['prepare.py']):
                with self.assertRaisesRegex(SystemExit, 'production-data'):
                    prepare.main()
                for call in run.call_args_list:
                    self.assertNotIn('create', call.args[0])
                self.assertFalse((root / 'acme.json').exists())
