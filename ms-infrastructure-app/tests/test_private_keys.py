import importlib.util
from pathlib import Path
import subprocess
import tempfile
import unittest

SPEC = importlib.util.spec_from_file_location('key_scan', Path(__file__).parents[1] / 'scripts/check-private-keys.py')
scan = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(scan)


class PrivateKeyScanTests(unittest.TestCase):
    def test_rejects_tracked_key_but_preserves_untracked_local_copy(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            subprocess.run(['git', 'init', '-q', str(root)], check=True)
            header = b'-----BEGIN OPENSSH PRIVATE KEY-----\nfixture-only-not-a-key\n'
            (root / 'tracked.pem').write_bytes(header)
            (root / 'local.pem').write_bytes(header)
            (root / 'public.pub').write_text('ssh-ed25519 fixture-only')
            subprocess.run(['git', '-C', str(root), 'add', 'tracked.pem', 'public.pub'], check=True)
            self.assertEqual(['tracked.pem'], scan.find_private_keys(root))
            subprocess.run(['git', '-C', str(root), 'rm', '--cached', '-q', 'tracked.pem'], check=True)
            self.assertEqual([], scan.find_private_keys(root))
            self.assertTrue((root / 'local.pem').exists())
            self.assertTrue((root / 'tracked.pem').exists())

    def test_finds_header_crossing_read_boundary(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            subprocess.run(['git', 'init', '-q', str(root)], check=True)
            (root / 'nested.dat').write_bytes(b'x' * 65525 + b'\n-----BEGIN RSA PRIVATE KEY-----\nfixture-only')
            subprocess.run(['git', '-C', str(root), 'add', 'nested.dat'], check=True)
            self.assertEqual(['nested.dat'], scan.find_private_keys(root))
