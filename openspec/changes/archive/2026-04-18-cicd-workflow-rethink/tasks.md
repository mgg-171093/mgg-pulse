# Tasks: CI/CD Workflow Rethink

## Phase 1: Foundation and Script Contracts

- [x] 1.1 Create `.github/scripts/bump-version.ps1` to read/update `Directory.Build.props` patch version and emit machine-readable outputs (`version`, `tag`) for workflow consumption.
- [x] 1.2 Create `.github/scripts/publish-release.ps1` that performs atomic release flow (hash, release publish, `app/build/latest.json` update, commit, push) and exits before repo mutation on any failure.
- [x] 1.3 Add script parameter/exit-code contract comments in both `.ps1` files so `release.yml` can treat them as deterministic steps.

## Phase 2: CI-safe Test Partitioning

- [x] 2.1 Mark WinRT/UI-bound tests as non-CI-safe using `[Trait("Category","Integration")]` in `app/tests/MGG.Pulse.Tests.Unit/UI/Services/ThemeServiceTests.cs` and any additional UI/WinRT test files.
- [x] 2.2 Ensure `app/tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj` and test conventions document support for trait-based filtering without creating a second test project.
- [x] 2.3 Update test guidance comments/docs near affected test files so new headless-unsafe tests are classified local-only by default (spec: Test partitioning).

## Phase 3: Workflow Replacement from Scratch

- [x] 3.1 Replace `.github/workflows/ci.yml` with orchestration-only YAML: checkout, setup .NET, restore, build, and `dotnet test --filter "Category!=Integration"`; remove external lint/tool install steps.
- [x] 3.2 Replace `.github/workflows/release.yml` with trigger split: PR-to-`main` dry-run validation (no publish/mutation) and push-to-`main` publish path invoking `.github/scripts/bump-version.ps1` + `.github/scripts/publish-release.ps1`.
- [x] 3.3 Add release loop guard in `.github/workflows/release.yml` using actor check (`github-actions[bot]`) and skip-token commit policy so bot metadata commits do not retrigger release.

## Phase 4: Contract and Regression Tests

- [x] 4.1 Update `app/tests/MGG.Pulse.Tests.Unit/Infrastructure/Update/GitHubActionsWorkflowTests.cs` assertions for new workflow structure, step names, and `.github/scripts/*.ps1` references.
- [x] 4.2 Add/adjust contract assertions for CI-safe filtering and release dry-run behavior to cover spec scenarios: hosted-runner-safe validation and side-effect-free PR release validation.
- [x] 4.3 Add/adjust contract assertions for deterministic main publish and atomic mutation ordering (manifest/commit only after successful release publish).

## Phase 5: Verification and OpenSpec Sync

- [x] 5.1 Validate `app/build/latest.json` remains repository state on `main` and is excluded from release assets by checking release workflow/script asset list.
- [x] 5.2 Verify script/workflow consistency by reviewing environment variables, outputs, and failure gates across `.github/workflows/release.yml` and `.github/scripts/*.ps1`.
- [x] 5.3 Update this checklist statuses during implementation (`sdd-apply`) and keep task ordering as dependency source of truth.
