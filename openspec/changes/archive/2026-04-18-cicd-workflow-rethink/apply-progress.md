# Apply Progress — 2026-04-18-cicd-workflow-rethink

## Batches (merged)

### Batch A — Initial full implementation (preserved)
- Scope: **Full implementation for all planned phases (1.1 → 5.3)**
- Mode: **Strict TDD**
- Result: **15/15 tasks completed**

### Batch B — Verify blocker remediation (current)
- Scope: **Fix strict-verify blockers without regressing redesigned workflow contracts**
- Mode: **Strict TDD**
- Focus:
  1. Native-command atomicity in `.github/scripts/publish-release.ps1`
  2. Deterministic script exit-code behavior
  3. Replace brittle string-coupled workflow/script contract checks with hardened contract coverage
  4. Preserve CI-safe filtering and `latest.json` raw-main contract behavior

## Completed Tasks (cumulative)

- [x] 1.1 Create `.github/scripts/bump-version.ps1` with machine-readable outputs (`version`, `tag`).
- [x] 1.2 Create `.github/scripts/publish-release.ps1` with atomic publish flow and fail-fast mutation rules.
- [x] 1.3 Add script contract docs (parameters + exit code contracts).
- [x] 2.1 Mark WinRT/UI-bound tests as non-CI-safe with `[Trait("Category", "Integration")]`.
- [x] 2.2 Document trait filtering contract in test project conventions.
- [x] 2.3 Add local-only guidance for headless-unsafe tests.
- [x] 3.1 Replace `.github/workflows/ci.yml` as orchestration-only CI-safe pipeline.
- [x] 3.2 Replace `.github/workflows/release.yml` with PR dry-run + push-to-main publish split.
- [x] 3.3 Add release loop guard (`github-actions[bot]` + skip-token policy).
- [x] 4.1 Update workflow contract tests for new structure and script references.
- [x] 4.2 Add/adjust assertions for CI filtering + dry-run behavior.
- [x] 4.3 Add/adjust assertions for deterministic publish and mutation ordering.
- [x] 5.1 Validate `latest.json` remains committed on `main` and is excluded from release assets.
- [x] 5.2 Verify script/workflow variable/output and failure-gate consistency.
- [x] 5.3 Keep task checklist status updated as source-of-truth ordering.

## Files Changed in Batch B

| File | Action | Notes |
|------|--------|-------|
| `.github/scripts/publish-release.ps1` | Modified | Added `Invoke-NativeCommand` wrapper with explicit `$LASTEXITCODE` gates for `gh`/`git`; introduced `Exit-WithCode` deterministic exits; hardened fail-fast behavior before mutation continuation. |
| `.github/scripts/bump-version.ps1` | Modified | Switched to deterministic `Exit-WithCode` path so documented exit code `10`/`11`/`12`/`13` is actually returned instead of generic process code 1 from `Write-Error` under `Stop`. |
| `app/tests/MGG.Pulse.Tests.Unit/Infrastructure/Update/GitHubActionsWorkflowTests.cs` | Modified | Replaced brittle `"git commit"` exact text assumption with resilient ordering token (`"commit -m"`), added native-command contract assertions, and added process-level exit-code tests for script contracts. |

## TDD Cycle Evidence (Batch B)

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| 1.2 | `GitHubActionsWorkflowTests.cs` | Unit (contract) | ✅ Baseline `GitHubActionsWorkflowTests` = 5/6 (known pre-existing failure) | ✅ Added `PublishReleaseScript_UsesNativeExitChecking_ForGhAndGitCommands` first (failing) | ✅ Targeted suite green after implementing `Invoke-NativeCommand` + explicit `$LASTEXITCODE` checks | ✅ Added coverage for `gh release create`, `git add`, `git commit`, `git push` call contracts | ✅ Extracted reusable `Exit-WithCode` and centralized native error handling |
| 1.3 | `GitHubActionsWorkflowTests.cs` | Unit (process-level script contract) | ✅ Existing focused tests running | ✅ Added process-level tests first: `BumpVersionScript_Returns10_WhenPropsFileMissing`, `PublishReleaseScript_Returns24_WhenRequiredArgumentMissing`, `PublishReleaseScript_Returns20_WhenInstallerMissing` (initially failing for bump script) | ✅ Green after deterministic exit-code fix in `bump-version.ps1` | ✅ Covered multiple failure classes (missing props, missing required args, missing installer) | ✅ Added shared script execution helper and narrowed assertions to contract outputs |
| 4.3 | `GitHubActionsWorkflowTests.cs` | Unit (contract) | ✅ Baseline showed brittle false-fail on exact `git commit` text | ✅ Updated brittle check to fail first when robust ordering marker absent | ✅ `ReleaseScripts_DeclareDeterministicContracts_AndAtomicMutationOrder` now passes with valid `git -c ... commit -m` form | ✅ Order now validated via stable mutation + commit markers rather than fragile literal text | ✅ Minor cleanup for ordering and regex imports |

## Test Summary (Batch B)

- Safety net before changes: `dotnet test app/tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj --filter "FullyQualifiedName~GitHubActionsWorkflowTests"` → **5 passed / 1 failed** (known blocker baseline)
- RED (new targeted tests):
  - Native command + exit map tests initially failing (**2 failed / 1 passed**)
  - Process-level exit-code tests initially failing on bump script contract (**1 failed / 2 passed**)
- GREEN:
  - Targeted blocker tests: **all passing**
  - Full `GitHubActionsWorkflowTests` suite: **11 passed / 0 failed**

## Deviations from Design

- None — changes reinforce the original design intent: script-level atomicity, deterministic contracts, and orchestration-only workflows.

## Issues Found

- Requested path `app/openspec/changes/2026-04-18-cicd-workflow-rethink/apply-progress.md` does not exist in repo; canonical artifact remains under `openspec/changes/...`. Merge was performed from the existing canonical apply-progress artifact.

## Remaining Tasks

- [ ] None (apply fixes complete; ready for verify re-run)

## Status

**15 / 15 tasks remain complete. Verify blockers addressed. Ready for `sdd-verify`.**
