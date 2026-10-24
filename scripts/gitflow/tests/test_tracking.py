import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path
from unittest.mock import Mock

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from tracking import issue_for_branch, reference_commit, referenced_issues, validate_branch
from github_tracking import GitHub, synchronize
from hooks import pre_push

CONFIG = {"production": "master", "development": "develop"}


class TrackingTests(unittest.TestCase):
    def test_branch_and_message_references(self):
        self.assertEqual(1, issue_for_branch("feature/issue001-devops-api"))
        self.assertEqual(12, issue_for_branch("bugfix/issue12-validacao"))
        self.assertEqual("feat: exemplo\n\nRefs #1\n", reference_commit("feat: exemplo\n", 1))
        self.assertEqual("feat: exemplo (#1)\n", reference_commit("feat: exemplo (#1)\n", 1))
        self.assertEqual("# cancelado\n", reference_commit("# cancelado\n", 1))

    def test_reference_is_inserted_before_verbose_diff(self):
        text = "feat: exemplo\n# ------------------------ >8 ------------------------\ndiff --git a/a b/a\n"
        result = reference_commit(text, 1)
        self.assertIn("Refs #1\n\n# ------------------------", result)
        self.assertTrue(result.endswith("diff --git a/a b/a\n"))

    def test_standard_branches_and_finish_pushes_are_allowed(self):
        for branch in ["master", "develop", "feature/issue001-api", "hotfix/1.0.1", "release/1.0.0"]:
            validate_branch(branch, "master", "develop")
            pre_push([f"refs/heads/{branch} abc refs/heads/{branch} def"])
        with self.assertRaises(ValueError):
            validate_branch("feature/sem-issue", "master", "develop")

    def test_release_collects_refs_and_merge_branch_names(self):
        self.assertEqual([1, 2, 3], referenced_issues(["feat: algo\n\nRefs #1", "Merge branch 'feature/issue002-api' into develop", "fix: falha\nRefs: #3", "Refs #1"]))

    def test_feature_is_referenced_and_never_closed(self):
        api = Mock()
        api.request.return_value = {"state": "open"}
        synchronize(api, "owner/repo", "feature/issue001-api", {}, CONFIG)
        api.comment_once.assert_called_once()
        self.assertEqual([("GET", "/issues/1")], [call.args for call in api.request.call_args_list])

    def test_master_delivery_closes_only_newly_referenced_issues(self):
        api = Mock()
        api.commits.return_value = ["fix: algo\n\nRefs #1"]
        api.request.return_value = {"state": "open"}
        synchronize(api, "owner/repo", "master", {"before": "aaa", "after": "bbb"}, CONFIG)
        api.commits.assert_called_once_with("aaa", "bbb")
        api.request.assert_any_call("PATCH", "/issues/1", {"state": "closed", "state_reason": "completed"})
        api.comment_once.assert_called_once()

    def test_already_closed_issue_is_not_commented_again(self):
        api = Mock()
        api.commits.return_value = ["Refs #1"]
        api.request.return_value = {"state": "closed"}
        synchronize(api, "owner/repo", "master", {"before": "aaa", "after": "bbb"}, CONFIG)
        api.comment_once.assert_not_called()
        self.assertEqual(1, api.request.call_count)

    def test_force_push_and_initial_master_do_not_close_history(self):
        api = Mock()
        synchronize(api, "owner/repo", "master", {"before": "0" * 40}, CONFIG)
        api.request.assert_not_called()
        with self.assertRaises(ValueError):
            synchronize(api, "owner/repo", "master", {"before": "aaa", "after": "bbb", "forced": True}, CONFIG)
        api.commits.assert_not_called()

    def test_pull_request_number_is_not_accepted_as_issue(self):
        api = Mock()
        api.request.return_value = {"pull_request": {}}
        with self.assertRaises(ValueError):
            synchronize(api, "owner/repo", "feature/issue001-api", {}, CONFIG)
        api.comment_once.assert_not_called()

    def test_versioned_hotfix_requires_commit_reference(self):
        api = Mock()
        api.commits.return_value = []
        with self.assertRaises(ValueError):
            synchronize(api, "owner/repo", "hotfix/1.0.1", {}, CONFIG)

    def test_compare_pagination(self):
        api = GitHub("owner/repo", "fake-token")
        api.request = Mock(side_effect=[{"commits": [{"commit": {"message": "Refs #1"}}] * 100}, {"commits": [{"commit": {"message": "Refs #2"}}]}])
        self.assertEqual(101, len(api.commits("aaa", "bbb")))
        self.assertEqual(2, api.request.call_count)

    def test_comment_is_idempotent_across_pages(self):
        api = GitHub("owner/repo", "fake-token")
        api.request = Mock(side_effect=[[{"body": "outro"}] * 100, [{"body": "<!-- marker -->"}]])
        api.comment_once(1, "<!-- marker -->", "texto")
        self.assertEqual(2, api.request.call_count)
        self.assertTrue(all(call.args[0] == "GET" for call in api.request.call_args_list))


if __name__ == "__main__":
    unittest.main()
