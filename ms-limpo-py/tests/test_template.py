from pathlib import Path
from tempfile import TemporaryDirectory
import unittest
from uuid import uuid4

from ms_limpo.bootstrap import compose_example
from ms_limpo.data.apply_migrations import load_migrations, validate_history
from ms_limpo.features.execution.commands.create_execution import CreateExecutionCommand
from ms_limpo.features.execution.queries.get_execution import GetExecutionQuery


class ExecutionTests(unittest.TestCase):
    def test_command_creates_and_query_reads_same_execution(self):
        create, get = compose_example()
        execution = create.handle(CreateExecutionCommand("  review  "))
        self.assertEqual(execution.name, "review")
        self.assertEqual(get.handle(GetExecutionQuery(execution.id)), execution)
        self.assertIsNotNone(execution.created_at.utcoffset())

    def test_blank_name_is_rejected(self):
        create, _ = compose_example()
        with self.assertRaises(ValueError):
            create.handle(CreateExecutionCommand("  "))

    def test_unknown_execution_returns_none(self):
        _, get = compose_example()
        self.assertIsNone(get.handle(GetExecutionQuery(uuid4())))


class MigrationTests(unittest.TestCase):
    def test_duplicate_versions_are_rejected(self):
        with TemporaryDirectory() as directory:
            for name in ("0001_first.sql", "0001_second.sql"):
                Path(directory, name).write_text("SELECT 1;")
            with self.assertRaises(ValueError):
                load_migrations(Path(directory))

    def test_changed_or_missing_applied_migration_is_rejected(self):
        for available in ([], [("0001_first.sql", "changed", "SELECT 1;")]):
            with self.subTest(available=available), self.assertRaises(ValueError):
                validate_history(available, {"0001_first.sql": "original"})

    def test_out_of_order_history_is_rejected(self):
        migrations = [("0001_first.sql", "a", ""), ("0002_second.sql", "b", "")]
        with self.assertRaises(ValueError):
            validate_history(migrations, {"0002_second.sql": "b"})

    def test_unchanged_prefix_is_accepted(self):
        validate_history([("0001_first.sql", "a", ""), ("0002_second.sql", "b", "")],
                         {"0001_first.sql": "a"})


if __name__ == "__main__":
    unittest.main()
