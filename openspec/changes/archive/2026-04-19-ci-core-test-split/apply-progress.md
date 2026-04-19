# Apply Progress: 2026-04-19-ci-core-test-split

## Summary

- Mode: **Strict TDD** (active)
- Scope implemented: **Full task set (1.1 → 4.3)**
- Result: Physical split completed into `MGG.Pulse.Tests.Core` (CI-safe) and `MGG.Pulse.Tests.UI` (local-only).

## Completed Tasks

- [x] 1.1 Create `app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj` with CI-safe references only.
- [x] 1.2 Create `app/tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj` for UI/WinRT local-only tests.
- [x] 1.3 Update `app/MGG.Pulse.slnx` to include Core + UI and remove Unit project.
- [x] 2.1 Move Domain tests to Core and update namespaces to `MGG.Pulse.Tests.Core.*`.
- [x] 2.2 Move Application tests to Core and update namespaces/imports.
- [x] 2.3 Move Infrastructure tests to Core and resolve project-boundary compile safety.
- [x] 2.4 Move UI/ViewModel tests to UI and update namespaces to `MGG.Pulse.Tests.UI.*`.
- [x] 2.5 Delete obsolete `app/tests/MGG.Pulse.Tests.Unit/` project directory.
- [x] 3.1 Update `.github/workflows/ci.yml` to run Core project explicitly, removing trait filter dependency.
- [x] 3.2 Update `.github/workflows/release.yml` test steps to run Core project explicitly.
- [x] 3.3 Confirm hosted workflows do not invoke UI project.
- [x] 4.1 Run local verification for both Core and UI test projects.
- [x] 4.2 Validate workflow contract via updated workflow tests and explicit project-target assertions.
- [x] 4.3 Update docs/dev guidance for the two-project contract.

## TDD Cycle Evidence

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| 1.1 | `app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj` + moved `SimulationConfigTests` | Unit | ✅ `GitHubActionsWorkflowTests` baseline 11/11 | ✅ Wrote csproj first, discovery had 0 tests | ✅ `SimulationConfigTests` 4/4 after first move | ➖ Structural + one moved test seed | ✅ minimal csproj cleanup/no WinUI refs |
| 1.2 | `app/tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj` | Unit | N/A (new project) | ✅ Wrote csproj first, discovery had 0 tests | ✅ Single UI test passed 1/1 after move | ➖ Structural | ✅ mirrored existing stable settings |
| 1.3 | `app/MGG.Pulse.slnx` | Unit | ✅ Core smoke (`IntervalRangeTests` 8/8) | ⚠️ FAILED (no dedicated pre-failing test added first) | ✅ Core tests still green after slnx update | ➖ Structural | ✅ minimal solution entry edits |
| 2.1 | `app/tests/MGG.Pulse.Tests.Core/Domain/**` | Unit | ✅ moved smoke from 1.1 | ⚠️ FAILED (migration executed without RED-first per file) | ✅ Domain tests green in Core | ✅ multiple domain scenarios run (`SimulationConfig`, `IntervalRange`) | ✅ namespace-only refactor |
| 2.2 | `app/tests/MGG.Pulse.Tests.Core/Application/**` | Unit | ✅ prior Core suite pass | ⚠️ FAILED (migration-first move) | ✅ Application tests green in Core | ✅ multiple rules/use-cases/update flows covered | ✅ namespace-only refactor |
| 2.3 | `app/tests/MGG.Pulse.Tests.Core/Infrastructure/**` | Unit | ✅ prior Core suite pass | ⚠️ FAILED (migration-first move) | ✅ Infrastructure tests green in Core | ✅ workflow/script/infrastructure cases still covered | ✅ workflow test assertions updated for new split |
| 2.4 | `app/tests/MGG.Pulse.Tests.UI/UI/**` | Unit | ✅ UI single-test smoke | ⚠️ FAILED (migration-first move) | ✅ UI suite 58/62 pass, 4 intentionally skipped | ✅ multiple UI behavior assertions + skipped headless cases | ✅ removed obsolete trait annotations/comments |
| 2.5 | delete old `Tests.Unit` dir | Unit | ✅ both new projects compile/test | ⚠️ FAILED (structural deletion without explicit RED test) | ✅ no old project remains in active test tree | ➖ Structural | ✅ cleaned obsolete boundary |
| 3.1 | `.github/workflows/ci.yml` + workflow tests | Unit | ✅ workflow contract tests baseline 11/11 | ✅ updated test expects Core target / no `--filter` | ✅ workflow contract tests pass | ✅ CI workflow assertions now enforce project boundary | ✅ removed trait-filter coupling |
| 3.2 | `.github/workflows/release.yml` + workflow tests | Unit | ✅ workflow contract tests baseline | ✅ assertions check Core target in release jobs | ✅ workflow contract tests pass | ✅ tested PR + main release paths | ✅ aligned both jobs to same boundary |
| 3.3 | hosted workflow audit + workflow tests | Unit | ✅ workflow contract tests baseline | ✅ added explicit non-UI invocation expectation by path targeting | ✅ contract tests + YAML inspection green | ➖ Structural | ✅ explicit Core-only command usage |
| 4.1 | project commands | Unit | ✅ prior focused tests | ✅ command set defined first for both projects | ✅ Core: 84/84 pass; UI: 58/62 pass, 4 skipped | ✅ debug + release coverage verified for Core | ➖ none |
| 4.2 | `GitHubActionsWorkflowTests` | Unit | ✅ baseline 11/11 | ✅ updated failing expectations for split contract | ✅ 11/11 passing after workflow updates | ✅ CI + Release contracts both validated | ✅ resilient assertions retained |
| 4.3 | `README.md`, `app/README.md`, `app/AGENTS.md` | Unit | ✅ docs contract covered by workflow tests readme assertions | ⚠️ FAILED (doc updates not test-first everywhere) | ✅ docs updated and contract test includes split text checks | ➖ Structural docs | ✅ unified wording around Core/UI split |

## Test Summary

- **Total tests written/updated**: 3 existing contract tests updated (`GitHubActionsWorkflowTests`)
- **Total tests passing**:
  - `dotnet test app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj` → **84/84**
  - `dotnet test app/tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj` → **58/62 passed, 4 skipped (headless-sensitive, explicit Skip)**
  - `dotnet test app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj --filter "FullyQualifiedName~GitHubActionsWorkflowTests"` → **11/11**
  - `dotnet test app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj --configuration Release` → **84/84**
- **Layers used**: Unit
- **Approval tests**: None
- **Pure functions created**: 0

## Deviations from Design

- None on architecture boundary. The split matches design intent (`Core` CI-safe + `UI` local-only, workflows target Core explicitly).

## Issues Found

- Strict TDD compliance is **partial** for migration-heavy structural tasks (several rows marked `FAILED` in RED column where task execution occurred without creating a dedicated failing test first).
- Core workflow command with `--no-build` requires prior Release build artifact in local runs (expected CI behavior; local command without build fails if Release output absent).

## Remaining Tasks

- [ ] None in `tasks.md` for this change.

## Status

**14/14 tasks complete. Ready for verify.**
