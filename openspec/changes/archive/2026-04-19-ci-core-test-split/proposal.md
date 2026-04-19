# Proposal: CI Core Test Split

## Intent

GitHub Actions CI still hangs intermittently because xUnit's reflection-based discovery loads WinUI/WinRT-bound types even when trait-based exclusion filters are applied. The discovery phase itself is the trigger — no filter fires before type initialization. The only reliable fix is to ensure CI-hosted runners **never load UI-bound assemblies at all**: physically separate test projects.

## Scope

### In Scope
- Create a new `MGG.Pulse.Tests.Core` project containing only pure-logic, WinRT-free tests (view-model logic, service layer, helpers)
- Move or re-author any existing CI-safe tests from `MGG.Pulse.Tests` into `Core`
- Keep `MGG.Pulse.Tests` (renamed or relabeled "UI / local-only") for WinUI-bound and integration tests
- Update CI workflow to run `dotnet test` only against the `Core` project
- Update local developer instructions (README / contributing notes) to describe the two-project contract

### Out of Scope
- Writing new test coverage beyond migrating existing CI-safe tests
- Converting UI tests to headless-safe (future work)
- Changing the release workflow

## Capabilities

### New Capabilities
- None

### Modified Capabilities
- `github-actions-ci`: Test partitioning requirement now enforced by project boundary, not by trait filters. Spec scenarios for "unsafe test remains local-only" and "CI-safe suite is independently runnable" gain concrete implementation contract.

## Approach

1. Add `MGG.Pulse.Tests.Core` csproj — no WinUI SDK, no `<UseWinUI>` — referencing only the production assemblies it tests.
2. Migrate existing trait-filtered or pure-logic tests into the new project.
3. `MGG.Pulse.Tests` retains UI-bound and integration tests; no CI references it.
4. CI `dotnet test` targets `MGG.Pulse.Tests.Core` exclusively; runner never sees WinUI types.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `MGG.Pulse.Tests/` | Modified | Becomes local-only; remove from CI step |
| `MGG.Pulse.Tests.Core/` | New | CI-safe project; pure-logic tests only |
| `.github/workflows/ci.yml` | Modified | Point `dotnet test` at `Tests.Core` |
| `MGG.Pulse.sln` | Modified | Add new project |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Migrated tests carry hidden WinUI deps | Med | Build `Tests.Core` without WinUI SDK — compile error surfaces immediately |
| Initial coverage regression in CI | Low | All previously CI-excluded tests already didn't run in CI |
| Developer confusion about two projects | Low | README section + project naming makes boundary explicit |

## Rollback Plan

Delete `MGG.Pulse.Tests.Core`, remove it from the solution, and revert `ci.yml` to the previous `dotnet test` invocation. No data loss; all test code remains in `MGG.Pulse.Tests`.

## Dependencies

- None — change is entirely within the repository

## Success Criteria

- [ ] `MGG.Pulse.Tests.Core` builds without WinUI SDK on a stock Windows runner
- [ ] CI workflow completes without hanging on xUnit discovery
- [ ] All tests in `Tests.Core` pass green on GitHub-hosted runner
- [ ] `MGG.Pulse.Tests` still runs locally without issues
