# Proposal: CI/CD Workflow Rethink

## Intent

The implemented workflows (`ci.yml` / `release.yml`) required 3 hotfix commits after initial delivery: flaky `actionlint` via Chocolatey, WinRT tests hanging headlessly in CI, and broken YAML validation approach. The current model is fragile on GitHub-hosted Windows runners. This change hardens the existing workflows — removing the brittle pieces and making the remaining pipeline reliable by default, rather than replacing the two-workflow model which is fundamentally sound.

## Scope

### In Scope
- Remove `actionlint` CI step entirely (was flaky, low value on a solo project)
- Guard `dotnet test` against WinRT/UI tests that hang headlessly in CI (skip or isolate)
- Audit `release.yml` for any remaining failure-prone steps (Chocolatey install reliability)
- Verify `[skip release]` + actor guard prevents bot-commit loops in practice
- Update specs to reflect the stabilized design

### Out of Scope
- Switching to self-hosted runners
- MSIX / code signing
- Semantic versioning from commit messages (conventional-commits tooling)
- Multi-arch builds
- Replacing the two-workflow model or the `latest.json`-on-main updater pattern

## Recommendation: Salvage, Not Replace

The two-workflow split (`ci.yml` for PRs/develop, `release.yml` for main push) is the right model. The failures were all in **optional ancillary steps** (linting, UI tests) — not in the core build/publish/release flow. Full replacement would be waste. Targeted salvage is the right call.

## Capabilities

### New Capabilities
- None

### Modified Capabilities
- `github-actions-delivery`: Stabilized workflow behavior — removal of flaky steps, test isolation strategy for WinUI/WinRT in CI, verified loop-prevention guards

## Approach

1. **Strip actionlint from both workflows** — already done in `fix(ci)` commits; lock this as the permanent decision (do not re-add).
2. **WinRT test isolation** — annotate or filter UI/WinRT tests with a trait (`[Trait("Category", "WinRT")]`) and exclude them via `dotnet test --filter` in CI. Only pure domain + application tests run in CI.
3. **Chocolatey reliability** — `choco install innosetup` is only in the release job; verify it exits reliably on `windows-latest`. If flaky, switch to direct Inno Setup MSI download via `Invoke-WebRequest`.
4. **Release loop verification** — confirm the actor guard `github.actor != 'github-actions[bot]'` + `[skip release]` commit message pattern actually halts recursion; add an explicit `if:` condition to the release job.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `.github/workflows/ci.yml` | Modified | Remove lint step; add test filter for WinRT/UI tests |
| `.github/workflows/release.yml` | Modified | Harden Chocolatey step; tighten loop-prevention condition |
| `app/tests/MGG.Pulse.Tests.Unit/UI/Services/ThemeServiceTests.cs` | Modified | Already patched; formalize WinRT isolation pattern |
| `openspec/specs/github-actions-delivery/spec.md` | Modified | Reflect stabilized behavior as the canonical spec |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Chocolatey `innosetup` install still flaky | Med | Fallback: direct MSI download + `Start-Process -Wait` |
| WinRT trait filter misses new UI tests added later | Low | CI step documents filter convention; codified in spec |
| Bot commit still triggers release loop | Low | Belt-and-suspenders: actor guard + commit message filter both required |

## Rollback Plan

Each fix is isolated to workflow YAML and test annotations. Any step can be reverted independently via a revert commit. The `latest.json` on `main` is the only stateful artifact — revert the bot commit to restore previous release pointer. No data loss risk.

## Dependencies

- `windows-latest` GitHub-hosted runner (no change)
- `GITHUB_TOKEN` with `contents: write` on release job (no change)
- Inno Setup 6 available via Chocolatey or direct download

## Success Criteria

- [ ] CI workflow completes without flaky failures on 3 consecutive PR runs
- [ ] Release workflow publishes a GitHub Release and updates `latest.json` on a push to `main` without manual intervention
- [ ] No bot-commit release loop observed after a release run
- [ ] WinRT/UI tests are excluded from CI; only domain + application tests run in the runner
- [ ] Zero Chocolatey-related failures in release job logs
