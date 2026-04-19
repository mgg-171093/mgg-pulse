## Verification Report

**Change**: 2026-04-18-cicd-workflow-rethink  
**Version**: N/A  
**Mode**: Strict TDD

---

### Completeness
| Metric | Value |
|--------|-------|
| Tasks total | 15 |
| Tasks complete | 15 |
| Tasks incomplete | 0 |

All tasks are marked complete in `openspec/changes/2026-04-18-cicd-workflow-rethink/tasks.md`.

---

### Build & Tests Execution

**Build**: ✅ Passed
```text
Verified through the focused Release test run:
dotnet test app/tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj --configuration Release --filter "FullyQualifiedName~GitHubActionsWorkflowTests"

Observed successful restore + Release compilation for:
- MGG.Pulse.Domain
- MGG.Pulse.Application
- MGG.Pulse.Infrastructure
- MGG.Pulse.UI
- MGG.Pulse.Tests.Unit
```

**Tests**: ✅ All verify runs green
```text
Focused workflow contracts
- dotnet test app/tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj --configuration Release --filter "FullyQualifiedName~GitHubActionsWorkflowTests"
- Result: 11 passed / 0 failed / 0 skipped

CI-safe suite
- dotnet test app/tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj --configuration Release --no-build --filter "Category!=Integration" --collect:"XPlat Code Coverage"
- Result: 142 passed / 0 failed / 0 skipped

Focused ThemeService validation
- dotnet test app/tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj --configuration Release --no-build --filter "FullyQualifiedName~ThemeServiceTests"
- Result: 50 passed / 0 failed / 4 skipped (intentional local-only Integration tests)

Process-level script contract checks
- bump-version.ps1 missing props -> exit 10
- publish-release.ps1 missing Version -> exit 24
- publish-release.ps1 missing installer -> exit 20
- publish-release.ps1 dry-run happy path -> exit 0 with unchanged git status
- publish-release.ps1 simulated gh failure -> exit 21 with unchanged manifest hash and no git calls
```

**Coverage**: 10.72% / threshold: 0% → ✅ Above threshold

---

### TDD Compliance
| Check | Result | Details |
|-------|--------|---------|
| TDD Evidence reported | ✅ | Found in `openspec/changes/2026-04-18-cicd-workflow-rethink/apply-progress.md` (Batch B) |
| All tasks have tests | ✅ | 3/3 remediation tasks in the TDD table reference executable tests |
| RED confirmed (tests exist) | ✅ | `GitHubActionsWorkflowTests.cs` exists and contains the reported contract/process tests |
| GREEN confirmed (tests pass) | ✅ | Focused workflow contract suite is now 11/11 green |
| Triangulation adequate | ✅ | Native-command handling, exit-code mapping, and mutation-order coverage all have multiple cases |
| Safety Net for modified files | ✅ | Reported safety-net checks align with current passing runs |

**TDD Compliance**: 6/6 checks passed

---

### Test Layer Distribution
| Layer | Tests | Files | Tools |
|-------|-------|-------|-------|
| Unit | 61 | 2 | xUnit |
| Integration | 4 | 1 | xUnit (`[Trait("Category", "Integration")]`, skipped/local-only) |
| E2E | 0 | 0 | not installed |
| **Total** | **65** | **2 mixed files** | |

---

### Changed File Coverage
| File | Line % | Branch % | Uncovered Lines | Rating |
|------|--------|----------|-----------------|--------|
| `.github/scripts/publish-release.ps1` | — | — | — | ➖ Not instrumented by Coverlet |
| `.github/scripts/bump-version.ps1` | — | — | — | ➖ Not instrumented by Coverlet |
| `app/tests/MGG.Pulse.Tests.Unit/Infrastructure/Update/GitHubActionsWorkflowTests.cs` | — | — | — | ➖ Test file, not a production coverage target |

**Average changed file coverage**: N/A — changed assets in the remediation batch are PowerShell and test files, which this coverage tool does not measure meaningfully.

---

### Assertion Quality
**Assertion quality**: ✅ All assertions verify real behavior

---

### Quality Metrics
**Linter**: ➖ Not available  
**Type Checker**: ✅ No errors (Release compilation succeeded during verification runs)

### Spec Compliance Matrix

| Requirement | Scenario | Test | Result |
|-------------|----------|------|--------|
| Hosted-runner-safe validation | Pull request uses CI-safe suite | `GitHubActionsWorkflowTests.cs > CiWorkflow_UsesHostedRunnerSafePipeline_WithIntegrationFilter` + CI-safe suite run | ✅ COMPLIANT |
| Hosted-runner-safe validation | Unsafe test remains local-only | `GitHubActionsWorkflowTests.cs > ThemeServiceTests_ClassifyHeadlessUnsafeTests_AsIntegration` + focused `ThemeServiceTests` run (4 skipped local-only) | ✅ COMPLIANT |
| Test partitioning | New non-CI-safe coverage is added | `GitHubActionsWorkflowTests.cs > UnitTestProject_AndReadme_DocumentTraitFilteringContract` | ✅ COMPLIANT |
| Dependency minimization for validation | Stable tooling path exists | `GitHubActionsWorkflowTests.cs > CiWorkflow_UsesHostedRunnerSafePipeline_WithIntegrationFilter` | ⚠️ PARTIAL |
| Side-effect-free release validation | Pull request readiness is dry-run only | `GitHubActionsWorkflowTests.cs > ReleaseWorkflow_DefinesDryRunForPrsAndPublishPathForMain` + local dry-run execution with unchanged git status | ✅ COMPLIANT |
| Side-effect-free release validation | Dry-run validation fails safely | `GitHubActionsWorkflowTests.cs > PublishReleaseScript_Returns20_WhenInstallerMissing` + manual missing-installer execution (exit 20, unchanged git status) | ✅ COMPLIANT |
| Deterministic main publishing | Publish succeeds from one authoritative artifact | `GitHubActionsWorkflowTests.cs > ReleaseScripts_DeclareDeterministicContracts_AndAtomicMutationOrder` + `ReleaseWorkflow_UsesDeterministicScriptOutputs_AndNoInlineGitConfig` | ⚠️ PARTIAL |
| Deterministic main publishing | Publish fails before repository mutation | Manual stubbed `gh` failure: exit 21, `latest.json` hash unchanged, no git calls observed | ✅ COMPLIANT |
| latest.json remains repository state | Manifest is updated after successful publish | Static contract coverage + raw-main updater reference in `app/src/MGG.Pulse.UI/App.xaml.cs`; no live publish smoke yet | ⚠️ PARTIAL |
| Dependency minimization for publishing | Packaging tool acquisition is chosen | Workflow keeps direct-download fallback, but still prefers Chocolatey first | ⚠️ PARTIAL |

**Compliance summary**: 6/10 scenarios compliant

---

### Correctness (Static — Structural Evidence)
| Requirement | Status | Notes |
|------------|--------|-------|
| Hosted-runner-safe validation | ✅ Implemented | `ci.yml` is orchestration-only and runs `dotnet test ... --filter "Category!=Integration"` |
| Test partitioning | ✅ Implemented | `ThemeServiceTests.cs`, `MGG.Pulse.Tests.Unit.csproj`, and docs all encode the local-only integration contract |
| Side-effect-free release validation | ✅ Implemented | `release-readiness` uses `-DryRun`; local dry-run exited 0 and left repo status unchanged |
| Deterministic main publishing | ✅ Implemented | `publish-release.ps1` now gates native `gh`/`git` commands via `Invoke-NativeCommand` + `$LASTEXITCODE`; version/tag outputs stay deterministic |
| latest.json remains repository state | ✅ Implemented | `publish-release.ps1` writes `app/build/latest.json`; release asset upload still targets only the installer argument |
| Dependency minimization for publishing | ⚠️ Partial | Direct download fallback exists, but Chocolatey is still the preferred first path |
| Previously failing brittle contract area | ✅ Implemented | The old false-fail area is fixed: mutation-order check now keys off stable `commit -m`, and the focused workflow suite is 11/11 green |

---

### Coherence (Design)
| Decision | Followed? | Notes |
|----------|-----------|-------|
| Replace workflows from scratch | ✅ Yes | `ci.yml` and `release.yml` are both orchestration-first replacements |
| Extract logic to `.github/scripts/*.ps1` | ✅ Yes | `bump-version.ps1` and `publish-release.ps1` hold the operational logic |
| Keep two-workflow split | ✅ Yes | `ci.yml` handles CI; `release.yml` handles PR readiness + main publishing |
| CI test filtering via trait filter | ✅ Yes | Both workflows use `Category!=Integration`, and the tests/docs match |
| Release atomicity via single script | ✅ Yes | Native-command failures now stop deterministically before later native steps; `gh` failure probe exited 21 before manifest/git mutation |
| Loop prevention via actor guard + skip token | ✅ Yes | `github.actor != 'github-actions[bot]'` and `[skip release]` are both present |
| Inno Setup caching via `actions/cache` | ⚠️ Deviated | No cache step exists; acquisition uses Chocolatey first with direct-download fallback |

---

### Issues Found

**CRITICAL** (must fix before archive):
None

**WARNING** (should fix):
- No live GitHub smoke run has yet proven the successful end-to-end publish path on the real runner (`gh release create`, release asset upload, metadata commit/push, and loop guard behavior).
- Dependency minimization for publishing is only partially satisfied because Inno Setup acquisition still prefers Chocolatey before the pinned direct-download fallback.

**SUGGESTION** (nice to have):
- Add a temp-repo/process-level harness for the successful publish path so the GitHub-facing contract has one non-live behavioral proof in addition to the eventual smoke run.
- If Chocolatey flakes again, flip the acquisition order to prefer the pinned direct installer first.

---

### Verdict
PASS WITH WARNINGS

The previous blockers are cleared: native PowerShell failure handling is now deterministic, documented exit codes are actually honored, the formerly brittle workflow/script contract area is green, CI-safe filtering still holds, and the remaining practical risk is the expected live GitHub smoke validation of the real publish path.
