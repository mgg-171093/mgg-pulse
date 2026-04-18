# Apply Progress — 2026-04-17-github-actions-cicd

## Batch

- Scope: **Cumulative progress across implementation + verify-blocker remediation**
- Previously completed: **1.1 → 1.6**
- Completed in this batch: **2.1 → 2.2**
- Mode: **Strict TDD** (`dotnet test tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj --filter FullyQualifiedName~GitHubActionsWorkflowTests`)

## Completed Tasks

- [x] 1.1 Create `.github/workflows/ci.yml` for PR/develop validation (restore, build, test, workflow syntax checks) with least-privilege permissions
- [x] 1.2 Create `.github/workflows/release.yml` with PR-to-main release-readiness validation and push-to-main real release automation on Windows runners
- [x] 1.3 Implement release metadata automation (patch semver bump in `app/Directory.Build.props`, installer hash + URL, update `app/build/latest.json`, create GitHub release)
- [x] 1.4 Prevent workflow recursion from bot-authored metadata commits
- [x] 1.5 Add focused unit-level workflow contract tests under `app/tests/MGG.Pulse.Tests.Unit`
- [x] 1.6 Update docs minimally so CI/CD model and raw-main `latest.json` contract are explicit
- [x] 2.1 Make release metadata publication atomic by pushing version/manifest only after tests/build/release creation/`latest.json` update succeed
- [x] 2.2 Add workflow contract test for failure-ordering / no-early-push expectation

## Files Changed

| File | Action | Notes |
|------|--------|-------|
| `.github/workflows/ci.yml` | Created | Added PR/develop CI gates on Windows runner with restore/build/test and YAML parse validation. |
| `.github/workflows/release.yml` | Created | Added split jobs for main PR readiness and main-push real release; includes semver bump + release + latest.json update flow. |
| `app/tests/MGG.Pulse.Tests.Unit/Infrastructure/Update/GitHubActionsWorkflowTests.cs` | Created | Added workflow/docs contract tests for trigger model, loop prevention, and raw-main latest.json policy. |
| `app/README.md` | Modified | Added minimal CI/CD model section and explicit raw-main manifest contract. |
| `README.md` | Modified | Updated release docs row to clarify latest.json is raw-main and automation-managed by release workflow. |
| `app/openspec/changes/2026-04-17-github-actions-cicd/tasks.md` | Created | Backfilled missing tasks artifact and marked implemented items complete. |
| `.github/workflows/release.yml` | Modified | Removed early version-bump push, switched to single post-success metadata commit (`Directory.Build.props` + `latest.json`). |
| `app/tests/MGG.Pulse.Tests.Unit/Infrastructure/Update/GitHubActionsWorkflowTests.cs` | Modified | Added ordering contract test to enforce no metadata push before release + latest.json steps. |
| `app/openspec/changes/2026-04-17-github-actions-cicd/tasks.md` | Modified | Added Phase 2 verify-blocker remediation tasks and marked complete. |

## TDD Cycle Evidence

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| 1.1 | `GitHubActionsWorkflowTests.cs` | Unit | N/A (new workflow file) | ❌ FAILED (test was added after workflow draft) | ✅ 3/3 passing | ➖ Single structural workflow assertions | ➖ None needed |
| 1.2 | `GitHubActionsWorkflowTests.cs` | Unit | N/A (new workflow file) | ❌ FAILED (test was added after workflow draft) | ✅ 3/3 passing | ✅ Trigger/job/guard assertions | ➖ None needed |
| 1.3 | `GitHubActionsWorkflowTests.cs` | Unit | N/A (new workflow file) | ❌ FAILED (test was added after workflow draft) | ✅ 3/3 passing | ✅ Assertions for props/latest.json/release metadata path | ➖ None needed |
| 1.4 | `GitHubActionsWorkflowTests.cs` | Unit | N/A (new workflow file) | ❌ FAILED (test was added after workflow draft) | ✅ 3/3 passing | ✅ Actor + commit-message recursion guards | ➖ None needed |
| 1.5 | `GitHubActionsWorkflowTests.cs` | Unit | N/A (new test file) | ✅ Written | ✅ 3/3 passing | ✅ Three independent contract scenarios | ✅ Shared repo path resolver helper |
| 1.6 | `GitHubActionsWorkflowTests.cs` | Unit | N/A (docs only) | ❌ FAILED (docs update came after initial failing assertion) | ✅ 3/3 passing | ➖ Structural docs clause | ➖ None needed |
| 2.1 | `GitHubActionsWorkflowTests.cs` | Unit | Existing workflow contract suite | ✅ Written first (new ordering assertions added before workflow edits) | ✅ 3/3 passing | ✅ Step-order indices + no early push string guard | ➖ None needed |
| 2.2 | `GitHubActionsWorkflowTests.cs` | Unit | Existing workflow contract suite | ✅ Written first | ✅ 3/3 passing | ✅ Asserts single metadata push step and combined `git add` scope | ➖ None needed |

## Test Summary

- **Focused test command:** `dotnet test tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj --filter FullyQualifiedName~GitHubActionsWorkflowTests`
- **RED evidence:** first run failed on missing README CI/CD documentation assertion.
- **GREEN evidence (current batch):** `dotnet test app/tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj --no-build --filter FullyQualifiedName~GitHubActionsWorkflowTests` passed **3/3**.
- **Layers used:** Unit only.
- **Approval tests:** None.
- **Pure functions created:** 0.

## Deviations from Design

- Design mentioned `softprops/action-gh-release@v2`; implementation uses `gh release create` to keep explicit control over target SHA and avoid extra action dependency.
- Proposal said no auto-bump; spec/design requested bump logic. Implementation follows current task request/spec and bumps patch in workflow.
- Release now creates tag/release against `${{ github.sha }}` from the triggering push and only pushes metadata after all publish prerequisites finish, prioritizing atomic metadata visibility over immediate in-branch version bump visibility.

## Issues Found

- Historical strict-TDD gaps from batch 1 remain documented (tasks 1.1–1.4 and 1.6).
- Atomicity risk for premature `main` mutation is remediated in current workflow definition.

## Remaining Tasks

- [ ] None in this change batch.

## Status

**8 / 8 tasks complete.** Verify blockers addressed for release atomicity + failure-ordering contract coverage. Change is **ready for verify**.
