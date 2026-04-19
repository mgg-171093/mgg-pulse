# Tasks: CI Core Test Split

## Phase 1: Project Boundary Foundation

- [x] 1.1 Create `app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj` with `net8.0-windows`, xUnit/Moq test packages, references to `app/src/MGG.Pulse.{Domain,Application,Infrastructure}`, and no WinUI/WinRT references.
- [x] 1.2 Create `app/tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj` from current unit project settings (`net8.0-windows10.0.19041.0`, UI reference) to keep local-only UI/WinRT tests runnable.
- [x] 1.3 Update `app/MGG.Pulse.slnx` to remove `tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj` and add `tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj` plus `tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj`.

## Phase 2: Test Migration and Namespace Alignment

- [x] 2.1 Move `app/tests/MGG.Pulse.Tests.Unit/Domain/**` into `app/tests/MGG.Pulse.Tests.Core/Domain/**` and update namespaces from `MGG.Pulse.Tests.Unit.*` to `MGG.Pulse.Tests.Core.*`.
- [x] 2.2 Move `app/tests/MGG.Pulse.Tests.Unit/Application/**` into `app/tests/MGG.Pulse.Tests.Core/Application/**` and update namespaces/imports to `MGG.Pulse.Tests.Core.*`.
- [x] 2.3 Move `app/tests/MGG.Pulse.Tests.Unit/Infrastructure/**` into `app/tests/MGG.Pulse.Tests.Core/Infrastructure/**` and resolve compile breaks caused by hidden UI dependencies.
- [x] 2.4 Move `app/tests/MGG.Pulse.Tests.Unit/UI/**` and `app/tests/MGG.Pulse.Tests.Unit/ViewModels/**` into `app/tests/MGG.Pulse.Tests.UI/**`, updating namespaces to `MGG.Pulse.Tests.UI.*`.
- [x] 2.5 Delete `app/tests/MGG.Pulse.Tests.Unit/` after migration is complete and both new projects compile.

## Phase 3: CI/Release Workflow Contract Updates

- [x] 3.1 Update `.github/workflows/ci.yml` test step to run `dotnet test app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj --configuration Release --no-build` and remove trait filter usage.
- [x] 3.2 Update `.github/workflows/release.yml` test steps in both `release-readiness` and `release` jobs to target `MGG.Pulse.Tests.Core.csproj` explicitly.
- [x] 3.3 Ensure no hosted workflow step invokes `app/tests/MGG.Pulse.Tests.UI/` so UI/WinRT-bound assemblies stay outside CI discovery.

## Phase 4: Verification and Developer Guidance

- [x] 4.1 Run local verification: `dotnet test app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj` and `dotnet test app/tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj`; capture failures before merge.
- [x] 4.2 Validate spec scenarios by checking that hosted workflows only report Core results (`github-actions-ci` and `github-actions-release` deltas) and never discover UI/WinRT tests.
- [x] 4.3 Update `README.md` (or contributing section) with the two-project contract: Core = CI-safe (Domain/Application/Infrastructure), UI = local-only (UI/WinRT-bound).
