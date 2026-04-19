# Archive Report: 2026-04-18-cicd-workflow-rethink

**Date Archived**: 2026-04-18  
**Change Name**: 2026-04-18-cicd-workflow-rethink  
**Status**: ✅ PASS WITH WARNINGS

---

## Executive Summary

The CI/CD workflow rethink change has been successfully completed and archived. All 15 tasks were implemented and verified. The two-workflow model (`ci.yml` for CI validation, `release.yml` for release publishing) has been hardened by removing flaky steps, introducing test partitioning for headless-unsafe tests, and adding deterministic release contracts.

**Warnings Preserved**:
- Live GitHub smoke validation of the successful publish path (`gh release create`, asset upload, metadata commit/push, loop guard behavior) remains outstanding — expected to validate in real repository environment
- Dependency minimization for publishing is partially satisfied; Inno Setup acquisition still prefers Chocolatey before the pinned direct-download fallback

**Next Action**: User will validate the implementation in the real repository environment, confirming end-to-end publish behavior.

---

## Specs Synced to Main

| Domain | Action | Requirements Count | Status |
|--------|--------|-------------------|--------|
| `github-actions-ci` | Created | 3 requirements, 7 scenarios | ✅ New spec added to source of truth |
| `github-actions-release` | Created | 4 requirements, 7 scenarios | ✅ New spec added to source of truth |

**Spec Paths**:
- `openspec/specs/github-actions-ci/spec.md` — Hosted-runner validation contract, test partitioning, dependency minimization
- `openspec/specs/github-actions-release/spec.md` — Dry-run PR validation, deterministic main publishing, manifest state, packaging tool selection

---

## Archive Contents

**Location**: `openspec/changes/archive/2026-04-18-cicd-workflow-rethink/`

- ✅ `proposal.md` — Intent, scope, affected areas, approach
- ✅ `specs/github-actions-ci/spec.md` — CI workflow specification
- ✅ `specs/github-actions-release/spec.md` — Release workflow specification
- ✅ `design.md` — Architecture decisions and technical approach
- ✅ `tasks.md` — 15/15 tasks completed (5 phases: foundation, test partitioning, workflow replacement, contracts, verification)
- ✅ `verify-report.md` — Comprehensive verification: 15/15 tasks complete, all tests green, 142 unit tests passing, 11/11 workflow contract tests passing

---

## Verification Status

**Build & Tests**: ✅ All passing
- Focused workflow contract suite: 11 passed / 0 failed
- CI-safe unit suite: 142 passed / 0 failed / 0 skipped
- ThemeService validation: 50 passed / 4 skipped (intentional local-only)
- Code coverage: 10.72% (above 0% threshold)

**Spec Compliance**: 6/10 scenarios fully compliant
- ✅ Hosted-runner-safe validation
- ✅ Test partitioning  
- ✅ Side-effect-free release validation
- ✅ Deterministic main publishing (partial — atomic ordering verified)
- ✅ Release loop prevention (actor guard + skip token verified)
- ⚠️ Latest.json repository state (structural evidence, no live smoke test yet)
- ⚠️ Dependency minimization for publishing (Chocolatey-first still in place)

**TDD Compliance**: 6/6 checks passed
- All tasks have accompanying contract/process tests
- Test failure modes documented (exit codes)
- Triangulation covers native-command handling, exit-code mapping, mutation-order

---

## Key Implementation Details

### 1. Script Contracts (Foundation)
- **`bump-version.ps1`**: Reads/updates `Directory.Build.props`, emits `version` and `tag` outputs
- **`publish-release.ps1`**: Atomic release flow with deterministic exit codes (20, 21, 24 for failure modes; 0 for success)
- Exit before repository mutation on any step failure

### 2. CI Test Partitioning
- WinRT/UI tests marked with `[Trait("Category", "Integration")]`
- CI workflow filters with `dotnet test --filter "Category!=Integration"`
- CI-safe suite: 142 tests passing reliably on hosted runners
- Local-only tests: 4 ThemeService integration tests (skipped in CI)

### 3. Workflow Structure
- **`ci.yml`**: Orchestration-only — checkout, setup .NET, restore, build, CI-safe test filter
- **`release.yml`**: Two paths: PR-to-main (dry-run, no mutation) + push-to-main (publish)
- Release loop guard: `github.actor != 'github-actions[bot]'` + `[skip release]` commit message policy

### 4. Deterministic Publishing
- Single authoritative artifact built once, checksummed, and released
- `app/build/latest.json` updated with version, URL, SHA-256 AFTER successful publish
- Latest.json committed back to main, NOT uploaded as release asset
- All pre-publish steps must succeed before any repository mutation

---

## Warnings in Archive

### ⚠️ WARNING: Live GitHub Smoke Validation Outstanding

**Issue**: The successful end-to-end publish path has not been validated against live GitHub Actions runners yet.

**What's Covered**:
- Native PowerShell failure handling (exit codes 20, 21, 24 tested locally)
- Dry-run validation (local test with unchanged repo state)
- Test suite contracts (11/11 workflow tests passing)
- Script parameter contracts (atomicity, no mutation on failure)

**What's Pending**:
- Live `gh release create` success path
- Release asset upload to GitHub
- `latest.json` metadata commit/push from live workflow
- Release loop guard behavior on real GitHub infrastructure

**Next Step**: Deploy change to main branch and validate full publish flow against real repository.

### ⚠️ WARNING: Dependency Minimization Partially Satisfied

**Issue**: Inno Setup acquisition still prefers Chocolatey before the pinned direct-download fallback.

**What's Implemented**:
- Direct-download fallback URL is defined (`https://jrsoftware.org/download.php/is.exe`)
- Chocolatey first path is documented
- No external feed dependency is REQUIRED (fallback exists)

**Current State**: Chocolatey-first acquisition path is live. If Chocolatey flakes again, flip the acquisition order to prefer pinned direct installer first.

**Next Step**: User to validate Chocolatey reliability in live publish; if issues resurface, switch to direct-download-first strategy.

---

## Source of Truth Updated

The following specs are now the canonical reference for CI/CD behavior:

- **`openspec/specs/github-actions-ci/spec.md`**
  - Hosted-runner-safe validation (CI-safe tests only)
  - Test partitioning (local-only for headless-unsafe tests)
  - Dependency minimization for validation (prefer stable tooling paths)

- **`openspec/specs/github-actions-release/spec.md`**
  - Side-effect-free PR validation (dry-run only)
  - Deterministic main publishing (single artifact, atomic mutation)
  - Latest.json repository state (committed on main, not as asset)
  - Dependency minimization for publishing (prefer pinned/stable paths)

---

## SDD Cycle Complete

✅ **Change Archived**

The CI/CD Workflow Rethink change has completed the full Spec-Driven Development cycle:

1. ✅ **Proposed** (`proposal.md`) — Intent, scope, approach
2. ✅ **Specified** (`specs/`) — Requirement scenarios for CI and release workflows
3. ✅ **Designed** (`design.md`) — Architecture decisions and technical approach
4. ✅ **Tasked** (`tasks.md`) — 15 implementation tasks across 5 phases
5. ✅ **Applied** (`apply-progress.md`) — All tasks completed with TDD evidence
6. ✅ **Verified** (`verify-report.md`) — PASS WITH WARNINGS status
7. ✅ **Archived** (`openspec/changes/archive/2026-04-18-cicd-workflow-rethink/`) — Specs synced to main, change moved to archive

---

## Next Change Ready

The SDD infrastructure is ready for the next change. Main specs in `openspec/specs/` now include the new CI/CD domains and are ready to be delta'd or merged with future workflow changes.

**Recommendation**: Monitor the live GitHub publish validation (pending smoke test) and be prepared to flip Chocolatey/direct-download order if reliability issues resurface.
