# Archive Report: 2026-04-19-ci-core-test-split

**Change**: 2026-04-19-ci-core-test-split  
**Archived at**: 2026-04-19T00:00:00Z  
**Archived to**: `openspec/changes/archive/2026-04-19-ci-core-test-split/`  
**Mode**: HYBRID (filesystem + Engram)  
**Status**: PASS WITH WARNINGS

---

## Verification Status

### Closure Gate: ✅ PASS WITH WARNINGS

The technical objective is achieved. Hosted workflows now target the physically separated Core project, UI/WinRT-bound tests are isolated locally, Core has no UI references, and Release also uses Core.

**Remaining issues** (non-blocking for archive):
- `ARCHITECTURE.md` still references removed `MGG.Pulse.Tests.Unit` project (docs drift)
- `README.md` still says "Infrastructure and UI are excluded from automated tests" (stale guidance)
- Release workflow contract tests do not explicitly assert Core-project test path in `release.yml` (proof is weaker than for `ci.yml`, though static correctness verified)
- Strict TDD purity is partial: 7/14 task rows in `apply-progress.md` admit migration-first or docs-first execution without dedicated failing test first
- UI local-only suite relies heavily on source-inspection assertions (acceptable for legacy preservation, but weaker than runtime verification)

---

## Specs Synced

### github-actions-ci

**Status**: ✅ **Updated** (2 modified requirements)

| Domain | Action | Details |
|--------|--------|---------|
| github-actions-ci | Updated | "Hosted-runner-safe validation" now explicitly requires CI-safe test PROJECT boundary and discovery safety, not just trait filters. "Test partitioning" now requires CI-safe project to PRESERVE meaningful core-layer coverage. Added "Core logic remains covered" scenario. |

**Changed requirements**:
- `Requirement: Hosted-runner-safe validation` — Enhanced to require explicit CI-safe test project discovery path
- `Requirement: Test partitioning` — Enhanced to require CI-safe project preserve core-layer coverage

**Source of truth**:
```
openspec/specs/github-actions-ci/spec.md — lines 9-43
```

---

### github-actions-release

**Status**: ✅ **Updated** (2 modified requirements + new scenarios)

| Domain | Action | Details |
|--------|--------|---------|
| github-actions-release | Updated | "Side-effect-free release validation" now requires test step target CI-safe project explicitly. "Deterministic main publishing" now requires CI-safe project for hosted tests. Added "Release validation uses CI-safe project" and "Main publish uses CI-safe project" scenarios. |

**Changed requirements**:
- `Requirement: Side-effect-free release validation` — Added explicit CI-safe test project requirement (3 scenarios: dry-run, fail-safe, CI-safe target)
- `Requirement: Deterministic main publishing` — Added explicit CI-safe test project requirement (3 scenarios: atomic artifact, pre-publish abort, CI-safe target)

**Source of truth**:
```
openspec/specs/github-actions-release/spec.md — lines 9-57
```

---

## Archive Contents

✅ All artifacts present in `openspec/changes/archive/2026-04-19-ci-core-test-split/`:

| Artifact | Path | Status |
|----------|------|--------|
| Proposal | `proposal.md` | ✅ Present |
| Specs | `specs/github-actions-ci/spec.md` | ✅ Present (delta) |
| Specs | `specs/github-actions-release/spec.md` | ✅ Present (delta) |
| Design | `design.md` | ✅ Present |
| Tasks | `tasks.md` | ✅ Present (14/14 complete) |
| Apply Progress | `apply-progress.md` | ✅ Present |
| Verify Report | `verify-report.md` | ✅ Present |
| State | `state.yaml` | ✅ Present |

---

## Task Completion Summary

**Total tasks**: 14  
**Completed**: 14  
**Incomplete**: 0  

**Status**: ✅ **All tasks complete**

---

## Implementation Highlights

### What was Built

1. **Physical test project split**
   - New `MGG.Pulse.Tests.Core` (CI-safe, no WinUI/WinRT)
   - Renamed `MGG.Pulse.Tests.Unit` → `MGG.Pulse.Tests.UI` (local-only, WinRT-capable)
   - Solution file updated to reference both

2. **Test migration**
   - Domain, Application, Infrastructure tests moved to Core
   - UI, ViewModel tests moved to UI project
   - Namespaces updated (`Tests.Unit` → `Tests.Core` / `Tests.UI`)

3. **Workflow updates**
   - `ci.yml` now targets `MGG.Pulse.Tests.Core.csproj` exclusively
   - `release.yml` (both `release-readiness` and `release` jobs) target Core explicitly
   - No trait filters remain; physical boundary enforces CI-safe discovery

4. **Verification**
   - Core suite: 84 tests passing (Debug & Release)
   - UI suite: 58 tests passing (4 UI-only skipped as expected)
   - Workflow contract tests: 11 passing (GitHub Actions wiring verified)
   - Coverage: 53.1% (above 0% threshold)

### Test Layer Distribution

| Layer | Tests | Files | Tools |
|-------|-------|-------|-------|
| Unit | 146 | 20 | xUnit + Moq |
| Integration | 0 | 0 | Not used |
| E2E | 0 | 0 | Not used |

---

## Spec Compliance

| Requirement | Scenario | Result | Notes |
|-------------|----------|--------|-------|
| Hosted-runner-safe validation | Pull request uses CI-safe project | ✅ COMPLIANT | `ci.yml` targets Core, no UI assemblies in hosted discovery |
| Hosted-runner-safe validation | Discovery avoids UI-bound types | ✅ COMPLIANT | Tests confirm Core suite is CI-safe |
| Test partitioning | New non-CI-safe coverage is added | ⚠️ PARTIAL | UI tests isolated locally; proof exists but coverage metrics are light |
| Test partitioning | Core logic remains covered | ✅ COMPLIANT | 84 Core tests + 11 workflow tests cover core logic |
| Side-effect-free release validation | Pull request readiness is dry-run only | ✅ COMPLIANT | `release-readiness` uses Core tests, no publish |
| Side-effect-free release validation | Dry-run validation fails safely | ⚠️ PARTIAL | Tests verify failure paths but don't execute on hosted runner |
| Side-effect-free release validation | Release validation uses CI-safe project | ⚠️ PARTIAL | Static check confirms; automated assertion could be stronger |
| Deterministic main publishing | Publish succeeds from one authoritative artifact | ✅ COMPLIANT | Release scripts assert atomic ordering |
| Deterministic main publishing | Publish fails before repository mutation | ✅ COMPLIANT | Pre-publish tests gate mutation; abort logic preserved |
| Deterministic main publishing | Main publish uses CI-safe project | ⚠️ PARTIAL | Static check confirms; automated assertion could be stronger |

**Compliance summary**: 6 fully compliant, 4 partial (non-blocking)

---

## Known Warnings (Preserved in Archive)

### Documentation Drift (Suggestion priority)

1. **ARCHITECTURE.md** still references `MGG.Pulse.Tests.Unit`
   - File was not updated during the change
   - Action: Update before next release or in a separate docs maintenance change

2. **README.md** stale guidance
   - Still says "Infrastructure and UI are excluded from automated tests"
   - Now misleading: Core includes Infrastructure tests; UI tests are local-only
   - Action: Update the testing section with two-project contract

### Assertion Quality (Limitation, not blocker)

1. **Release workflow tests are structural only**
   - `GitHubActionsWorkflowTests` verifies text presence in `release.yml`
   - Does not execute the workflow on a hosted runner
   - Static verification confirms correctness; automated proof is weaker than CI tests
   - Acceptance: This is acceptable for now. Full hosted workflow test would be future hardening.

### TDD Purity (Process note, not blocker)

1. **7/14 tasks used migration-first or docs-first approach**
   - These tasks moved existing tests or updated configurations
   - For migration work, true RED-first is not always applicable
   - TDD purity is 7/14 strict RED-first; remainder is acceptable for structural work

### UI Test Coverage (Known limitation)

1. **UI tests rely on source-inspection assertions**
   - Tests inspect `App.xaml.cs` text and check initialization order
   - Not true runtime WinUI execution verification
   - Acceptable: Preserves legacy coverage during the split
   - Future: Full UI harness tests could replace/supplement these

---

## Rollback Instructions

If needed, change can be rolled back:

1. Delete the two test projects:
   ```bash
   Remove-Item -Recurse "app/tests/MGG.Pulse.Tests.Core"
   Remove-Item -Recurse "app/tests/MGG.Pulse.Tests.UI"
   ```

2. Restore `app/tests/MGG.Pulse.Tests.Unit/` from previous commit

3. Update solution file to remove Core/UI and re-add Unit

4. Revert workflow changes to `.github/workflows/ci.yml` and `release.yml`

Data loss: None — all test code is preserved in version control

---

## SDD Cycle Complete

The change has been fully planned, implemented, verified, and archived.

- **Proposal**: ✅ Defined intent, scope, and approach
- **Specs**: ✅ Enhanced delta specs with physical boundary requirement
- **Design**: ✅ Documented architecture decisions for test split
- **Tasks**: ✅ 14/14 implementation tasks complete
- **Apply**: ✅ Full implementation with TDD evidence
- **Verify**: ✅ PASS WITH WARNINGS — no critical blockers
- **Archive**: ✅ Deltas merged into main specs, change archived with audit trail

The project is ready for the next change.

---

## Next Steps

**Recommended actions** (optional, for future sessions):

1. **Update ARCHITECTURE.md** to remove stale `Tests.Unit` references
2. **Update README.md** testing section with two-project contract and explicit CI-safe guidance
3. **Add explicit `release.yml` Core-target assertion** to `GitHubActionsWorkflowTests` for release-path proof symmetry
4. **Monitor live GitHub Actions** to verify Core-only test invocation performs reliably on hosted runners

---

## Change Metadata

| Key | Value |
|-----|-------|
| Change ID | 2026-04-19-ci-core-test-split |
| Archive Date | 2026-04-19 |
| Archive Path | openspec/changes/archive/2026-04-19-ci-core-test-split/ |
| Mode | HYBRID |
| Verification Status | PASS WITH WARNINGS |
| Critical Blockers | None |
| Total Warnings | 5 (docs drift, assertion quality, TDD purity, UI test coverage) |
| All Tasks Complete | ✅ Yes (14/14) |
| Specs Synced | ✅ Yes (2 domains, 5 modified/new requirements) |

---

**Report generated**: 2026-04-19  
**Archived by**: SDD Archive Phase (sdd-archive skill)
