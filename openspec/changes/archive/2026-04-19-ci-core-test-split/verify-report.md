## Verification Report

**Change**: 2026-04-19-ci-core-test-split
**Version**: N/A
**Mode**: Strict TDD

---

### Completeness
| Metric | Value |
|--------|-------|
| Tasks total | 14 |
| Tasks complete | 14 |
| Tasks incomplete | 0 |

All tasks in `openspec/changes/2026-04-19-ci-core-test-split/tasks.md` are marked complete.

---

### Build & Tests Execution

**Build**: ➖ No standalone build command was configured in `openspec/config.yaml` (file absent). Implicit restore/build succeeded through the executed test commands below, including Release for the Core project.

**Tests**:
- ✅ `dotnet test app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj` → 84 passed, 0 failed, 0 skipped
- ✅ `dotnet test app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj --configuration Release` → 84 passed, 0 failed, 0 skipped
- ✅ `dotnet test app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj --filter "FullyQualifiedName~GitHubActionsWorkflowTests"` → 11 passed, 0 failed, 0 skipped
- ✅ `dotnet test app/tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj` → 58 passed, 0 failed, 4 skipped

Skipped UI tests:
- `ThemeServiceTests.GetSavedTheme_WhenPreferenceMissing_ReturnsDarkDefault`
- `ThemeServiceTests.SaveTheme_WhenSupportedValue_PersistsPreference`
- `ThemeServiceTests.ApplyTheme_UpdatesCurrentTheme_WithNormalizedValue`
- `ThemeServiceTests.GetSavedTheme_WhenSavedValueIsInvalid_FallsBackToDark`

**Coverage**: 53.1% / threshold: 0% → ✅ Above threshold

---

### TDD Compliance
| Check | Result | Details |
|-------|--------|---------|
| TDD Evidence reported | ✅ | `apply-progress.md` contains a full TDD Cycle Evidence table |
| All tasks have tests | ✅ | 14/14 task rows point to executable test evidence or structural verification artifacts |
| RED confirmed (tests exist) | ⚠️ | 7/14 rows explicitly show RED-first evidence; 7/14 rows openly admit migration-first or docs-first execution |
| GREEN confirmed (tests pass) | ✅ | Referenced Core/UI/workflow tests pass now: 84 Core Debug, 84 Core Release, 11 workflow contract, 58 UI (+4 skipped) |
| Triangulation adequate | ⚠️ | Structural migration tasks mostly rely on single-path contract checks; future placement rules are only partially enforced by tests |
| Safety Net for modified files | ✅ | Safety-net evidence exists for modified tasks; the new UI project row is correctly marked N/A |

**TDD Compliance**: 4/6 checks passed

---

### Test Layer Distribution
| Layer | Tests | Files | Tools |
|-------|-------|-------|-------|
| Unit | 146 | 20 | xUnit + Moq |
| Integration | 0 | 0 | not installed / not used |
| E2E | 0 | 0 | not installed / not used |
| **Total** | **146** | **20** | |

Notes:
- `MGG.Pulse.Tests.Core` contains 16 files / 84 tests covering Domain, Application, Infrastructure-safe logic.
- `MGG.Pulse.Tests.UI` contains 4 files / 62 tests covering local-only UI/ViewModel behavior.
- Many UI tests are still structural/source-inspection tests rather than runtime WinUI harness tests.

---

### Changed File Coverage
| File | Line % | Branch % | Uncovered Lines | Rating |
|------|--------|----------|-----------------|--------|
| `.github/workflows/ci.yml` | ➖ | ➖ | N/A (workflow YAML not emitted in Cobertura) | ➖ N/A |
| `.github/workflows/release.yml` | ➖ | ➖ | N/A (workflow YAML not emitted in Cobertura) | ➖ N/A |
| `README.md` | ➖ | ➖ | N/A (docs not emitted in Cobertura) | ➖ N/A |
| `app/README.md` | ➖ | ➖ | N/A (docs not emitted in Cobertura) | ➖ N/A |
| `app/AGENTS.md` | ➖ | ➖ | N/A (docs not emitted in Cobertura) | ➖ N/A |
| `app/MGG.Pulse.slnx` | ➖ | ➖ | N/A (solution file not emitted in Cobertura) | ➖ N/A |
| `app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj` | ➖ | ➖ | N/A (project file not emitted in Cobertura) | ➖ N/A |
| `app/tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj` | ➖ | ➖ | N/A (project file not emitted in Cobertura) | ➖ N/A |

**Average changed file coverage**: N/A — this change is dominated by workflow/docs/test-project boundary files, which are outside Cobertura’s production-code report.

---

### Assertion Quality
| File | Line | Assertion | Issue | Severity |
|------|------|-----------|-------|----------|
| `app/tests/MGG.Pulse.Tests.Core/Infrastructure/Update/GitHubActionsWorkflowTests.cs` | 31-43 | `Assert.Contains(...)` over `release.yml` text | Structural contract test only; it does not execute GitHub Actions on a hosted runner | WARNING |
| `app/tests/MGG.Pulse.Tests.Core/Infrastructure/Update/GitHubActionsWorkflowTests.cs` | 152-163 | `Assert.Contains(...)` over README + csproj text | Verifies split wiring/doc strings, not runtime workflow behavior | WARNING |
| `app/tests/MGG.Pulse.Tests.UI/UI/Services/ThemeServiceTests.cs` | 57-63 | `Assert.Contains(...)` over `App.xaml.cs` text | Source-inspection assertion; proves wiring text, not WinUI runtime behavior | WARNING |
| `app/tests/MGG.Pulse.Tests.UI/UI/Services/ThemeServiceTests.cs` | 66-79 | `IndexOf(...)` ordering checks over `App.xaml.cs` text | Structural-only startup proof; useful for regression, but not live app execution | WARNING |

**Assertion quality**: 0 CRITICAL, 4 WARNING

---

### Quality Metrics
**Linter**: ➖ Not available
**Type Checker**: ➖ Not available as a standalone configured step

---

### Spec Compliance Matrix

| Requirement | Scenario | Test | Result |
|-------------|----------|------|--------|
| Hosted-runner-safe validation | Pull request uses CI-safe project | `app/tests/MGG.Pulse.Tests.Core/Infrastructure/Update/GitHubActionsWorkflowTests.cs > CiWorkflow_UsesHostedRunnerSafePipeline_WithCoreProjectBoundary` | ✅ COMPLIANT |
| Hosted-runner-safe validation | Discovery avoids UI-bound types | `GitHubActionsWorkflowTests > ThemeServiceTests_AreIsolatedInUiOnlyTestProject` + `GitHubActionsWorkflowTests > TestProjects_AndReadme_DocumentProjectSplitContract` + full Core suite (84/84) | ✅ COMPLIANT |
| Test partitioning | New non-CI-safe coverage is added | `GitHubActionsWorkflowTests > ThemeServiceTests_AreIsolatedInUiOnlyTestProject` | ⚠️ PARTIAL |
| Test partitioning | Core logic remains covered | Full Core suite: `dotnet test app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj` (84/84) | ✅ COMPLIANT |
| Side-effect-free release validation | Pull request readiness is dry-run only | `GitHubActionsWorkflowTests > ReleaseWorkflow_DefinesDryRunForPrsAndPublishPathForMain` | ✅ COMPLIANT |
| Side-effect-free release validation | Dry-run validation fails safely | `GitHubActionsWorkflowTests > BumpVersionScript_Returns10_WhenPropsFileMissing` + `PublishReleaseScript_Returns24_WhenRequiredArgumentMissing` + `PublishReleaseScript_Returns20_WhenInstallerMissing` | ⚠️ PARTIAL |
| Side-effect-free release validation | Release validation uses CI-safe project | `GitHubActionsWorkflowTests > ReleaseWorkflow_DefinesDryRunForPrsAndPublishPathForMain` | ⚠️ PARTIAL |
| Deterministic main publishing | Publish succeeds from one authoritative artifact | `GitHubActionsWorkflowTests > ReleaseScripts_DeclareDeterministicContracts_AndAtomicMutationOrder` | ✅ COMPLIANT |
| Deterministic main publishing | Publish fails before repository mutation | `GitHubActionsWorkflowTests > ReleaseScripts_DeclareDeterministicContracts_AndAtomicMutationOrder` + `PublishReleaseScript_MapsDeterministicExitCodes_ForKnownFailureModes` | ✅ COMPLIANT |
| Deterministic main publishing | Main publish uses CI-safe project | `GitHubActionsWorkflowTests > ReleaseWorkflow_DefinesDryRunForPrsAndPublishPathForMain` | ⚠️ PARTIAL |

**Compliance summary**: 6/10 scenarios compliant

Why partial instead of failing on the release-path scenarios? The implementation is statically correct in `release.yml`, but the executed workflow contract tests do not assert the Core test-project path as explicitly as the CI workflow test does. So the technical objective is present, but the automated proof is weaker than it should be.

---

### Correctness (Static — Structural Evidence)
| Requirement | Status | Notes |
|------------|--------|-------|
| Hosted-runner-safe validation | ✅ Implemented | `ci.yml` restores/builds the solution, then runs only `app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj`; no trait filter remains |
| Test partitioning | ✅ Implemented | `MGG.Pulse.Tests.Core` and `MGG.Pulse.Tests.UI` exist; old `MGG.Pulse.Tests.Unit` tree is gone; solution points only to Core + UI |
| Core project stays free of UI/WinUI refs | ✅ Implemented | `MGG.Pulse.Tests.Core.csproj` references Domain/Application/Infrastructure only; no `MGG.Pulse.UI`, `UseWinUI`, WinRT, or Windows App SDK references were found |
| Side-effect-free release validation | ✅ Implemented | `release-readiness` remains dry-run oriented and now points tests at Core only |
| Deterministic main publishing | ✅ Implemented | `release` job uses Core tests before publish and retains atomic publish ordering assertions/tests |
| Docs and developer guidance reflect the split | ⚠️ Partial | `README.md`, `app/README.md`, and `app/AGENTS.md` reflect Core/UI split, but `ARCHITECTURE.md` still references removed `MGG.Pulse.Tests.Unit`, and README test guidance has one stale sentence |

---

### Coherence (Design)
| Decision | Followed? | Notes |
|----------|-----------|-------|
| Two physical test projects replace trait-filter-only strategy | ✅ Yes | Core/UI split is present on disk and in the solution |
| Core project targets `net8.0-windows` | ✅ Yes | `MGG.Pulse.Tests.Core.csproj` uses `net8.0-windows` |
| Core project omits RuntimeIdentifier | ✅ Yes | No `<RuntimeIdentifier>` exists in Core csproj |
| Local project keeps UI-capable Windows target and UI reference | ✅ Yes | `MGG.Pulse.Tests.UI.csproj` targets `net8.0-windows10.0.19041.0`, keeps RID, and references `MGG.Pulse.UI` |
| CI and Release workflows target Core explicitly | ✅ Yes | `ci.yml` and both release jobs call `dotnet test ...MGG.Pulse.Tests.Core.csproj --configuration Release --no-build` |

---

### Issues Found

**CRITICAL** (must fix before archive):
- None.

**WARNING** (should fix):
- `ARCHITECTURE.md` still references the removed `MGG.Pulse.Tests.Unit` project, so docs are not fully consistent with the split.
- `README.md` still says “Infrastructure and UI are excluded from automated tests”; that is now misleading because Core includes Infrastructure tests and UI tests are local-only rather than absent.
- Release workflow contract tests do not explicitly assert the Core-project test path in `release.yml`; manual/static verification says it is correct, but automated proof is weaker than for `ci.yml`.
- Strict TDD purity is partial: 7/14 task rows in `apply-progress.md` admit migration-first or docs-first execution without a dedicated failing test first.
- The UI local-only suite still relies heavily on source-inspection assertions. That is acceptable for preserving legacy coverage during the split, but it is weaker than true runtime UI verification.

**SUGGESTION** (nice to have):
- Add an explicit `release.yml` Core-target assertion to `GitHubActionsWorkflowTests` so release-path compliance is proven as strongly as CI-path compliance.
- Update `ARCHITECTURE.md` and the stale README testing sentence before archive so the developer story is completely aligned.

---

### Verdict
PASS WITH WARNINGS

The technical objective is achieved: hosted workflows now target the physically separated Core project, UI/WinRT-bound tests are isolated locally, Core has no UI references, and Release also uses Core. Remaining issues are mostly documentation drift, weaker automated proof on the release workflow path, and partial TDD purity for migration-heavy structural tasks.
