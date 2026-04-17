# Verification Report

**Change**: 2026-04-16-updater-relaunch-hardening
**Version**: N/A
**Mode**: Strict TDD

---

### Completeness
| Metric | Value |
|--------|-------|
| Tasks total | 7 |
| Tasks complete | 7 |
| Tasks incomplete | 0 |

All tasks in `tasks.md` are marked complete.

---

### Build & Tests Execution

**Build**: ✅ Passed
```text
dotnet build tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj

Build succeeded.
0 Warning(s)
0 Error(s)
```

**Tests**: ✅ 140 passed / ❌ 0 failed / ⚠️ 0 skipped
```text
dotnet test tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj

Passed!  - Failed: 0, Passed: 140, Skipped: 0, Total: 140, Duration: 4 s
```

**Targeted verification tests**: ✅ 13 passed / ❌ 0 failed
```text
dotnet test tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj --filter "FullyQualifiedName~UpdateHostedServiceTests|FullyQualifiedName~InnoSetupInstallerLauncherTests|FullyQualifiedName~InnoSetupScriptTests|FullyQualifiedName~CheckForUpdateUseCaseTests.ExecuteAsync_ManualCheck_InvokesSameFlowAsAutomaticCheck|FullyQualifiedName~ThemeServiceTests.OnLaunched_WhenStartMinimized_ActivatesThenHidesMainWindow"

Passed: 13, Failed: 0, Skipped: 0
```

**Coverage**: 9.77% / threshold: N/A → ➖ No threshold configured

Notes:
- Coverage tooling is available and executed via `dotnet test --collect:"XPlat Code Coverage"`.
- Total coverage is low because the test suite intentionally focuses on Domain/Application and does not execute WinUI runtime flows.

---

### TDD Compliance
| Check | Result | Details |
|-------|--------|---------|
| TDD Evidence reported | ✅ | `apply-progress` contains a full TDD Cycle Evidence table |
| All tasks have tests | ✅ | 7/7 task rows map to test files |
| RED confirmed (tests exist) | ✅ | 7/7 referenced test files exist |
| GREEN confirmed (tests pass) | ✅ | targeted hardening tests passed; full suite is 140/140 green |
| Triangulation adequate | ✅ | retry behavior has multiple paths; only guard-style tasks remain single-case by design |
| Safety Net for modified files | ✅ | existing modified test files had baseline green runs; new files were correctly marked `N/A (new)` |

**TDD Compliance**: 6/6 checks passed

---

### Test Layer Distribution
| Layer | Tests | Files | Tools |
|-------|-------|-------|-------|
| Unit | 13 | 5 | xUnit + Moq |
| Integration | 0 | 0 | not installed |
| E2E | 0 | 0 | not installed |
| **Total** | **13** | **5** | |

Relevant files: `UpdateHostedServiceTests.cs`, `InnoSetupInstallerLauncherTests.cs`, `InnoSetupScriptTests.cs`, `ThemeServiceTests.cs`, `CheckForUpdateUseCaseTests.cs`.

---

### Changed File Coverage
| File | Line % | Branch % | Uncovered Lines | Rating |
|------|--------|----------|-----------------|--------|
| `src/MGG.Pulse.Application/Updates/UpdateHostedService.cs` | 83.33% | 40.9% | L50-51, L55-56, L97-101, L159-160, L162, L164-167 | ⚠️ Acceptable |
| `src/MGG.Pulse.Infrastructure/Update/InnoSetupInstallerLauncher.cs` | 61.53% | 100% | L25-30 | ⚠️ Low |
| `src/MGG.Pulse.UI/App.xaml.cs` | 0% | 0% | L44-104, L107-225, L235-323 | ⚠️ Low |
| `build/pulse.iss` | N/A | N/A | Static installer script; not included in .NET coverage | ➖ Static artifact |

**Average changed file coverage (instrumented code files)**: 48.29%

---

### Assertion Quality
| File | Line | Assertion | Issue | Severity |
|------|------|-----------|-------|----------|
| `tests/MGG.Pulse.Tests.Unit/UI/Services/ThemeServiceTests.cs` | 86-96 | `Assert.Contains(...)` + `IndexOf(...)` over `App.xaml.cs` text | Source-inspection test only; it does not execute WinUI startup/minimized behavior | CRITICAL |
| `tests/MGG.Pulse.Tests.Unit/Infrastructure/Update/InnoSetupScriptTests.cs` | 14-15 | `Assert.Contains(...)` / `Assert.DoesNotContain(...)` over raw `pulse.iss` text | Static artifact inspection only; it validates wiring but not an actual installer run/relaunch | WARNING |

**Assertion quality**: 1 CRITICAL, 1 WARNING

---

### Quality Metrics
**Linter**: ✅ No errors (`dotnet build` / Roslyn analyzers)
**Type Checker**: ✅ No errors (`dotnet build` / C# compiler)

---

### Spec Compliance Matrix

| Requirement | Scenario | Test | Result |
|-------------|----------|------|--------|
| latest.json Contract and Polling | Startup retry finds a newer release | `tests/MGG.Pulse.Tests.Unit/Application/Updates/UpdateHostedServiceTests.cs > StartAsync_WhenRetrySucceedsWithNewerVersion_RaisesUpdateAvailableEvent` | ✅ COMPLIANT |
| latest.json Contract and Polling | Startup retries are exhausted | `tests/MGG.Pulse.Tests.Unit/Application/Updates/UpdateHostedServiceTests.cs > StartAsync_WhenStartupCheckKeepsFailing_StopsAfterThreeAttempts` + `StartAsync_SchedulesPeriodicTimerWithFourHourPeriod` | ⚠️ PARTIAL |
| latest.json Contract and Polling | Periodic check handles no-update or invalid manifest | `tests/MGG.Pulse.Tests.Unit/Application/Updates/UpdateHostedServiceTests.cs > PeriodicCallback_WhenCheckFails_DoesSingleAttemptWithoutInternalRetry` + `tests/MGG.Pulse.Tests.Unit/Application/Updates/CheckForUpdateUseCaseTests.cs > ExecuteAsync_WhenSameVersion_ReturnsUpdateAvailableFalse` / `ExecuteAsync_WhenOlderVersionInManifest_ReturnsUpdateAvailableFalse` / `ExecuteAsync_WhenManifestVersionIsEmpty_ReturnsFailResult` / `ExecuteAsync_WhenManifestUrlIsEmpty_ReturnsFailResult` | ⚠️ PARTIAL |
| Silent LocalAppData Installer Flow | Interactive install uses LocalAppData target | (none found) | ❌ UNTESTED |
| Silent LocalAppData Installer Flow | Updater launches silent install | `tests/MGG.Pulse.Tests.Unit/Infrastructure/Update/InnoSetupInstallerLauncherTests.cs > CreateStartInfo_WhenCalled_UsesSilentAndRestartApplicationFlags` + `tests/MGG.Pulse.Tests.Unit/Infrastructure/Update/InnoSetupScriptTests.cs > RunEntry_WhenUpdaterUsesSilentInstall_DoesNotSkipRelaunchInSilentMode` | ⚠️ PARTIAL |
| Tray Icon Always Visible When Running | App minimized to tray | (none found) | ❌ UNTESTED |
| Tray Icon Always Visible When Running | App started minimized | `tests/MGG.Pulse.Tests.Unit/UI/Services/ThemeServiceTests.cs > OnLaunched_WhenStartMinimized_ActivatesThenHidesMainWindow` | ⚠️ PARTIAL |
| About Version Surface | About page shows current version | (none found) | ❌ UNTESTED |
| About Version Surface | Manual check reuses updater flow | `tests/MGG.Pulse.Tests.Unit/Application/Updates/CheckForUpdateUseCaseTests.cs > ExecuteAsync_ManualCheck_InvokesSameFlowAsAutomaticCheck` | ⚠️ PARTIAL |

**Compliance summary**: 1/9 scenarios compliant

---

### Correctness (Static — Structural Evidence)
| Requirement | Status | Notes |
|------------|--------|-------|
| latest.json Contract and Polling | ✅ Implemented | `UpdateHostedService` now performs startup retry attempts with bounded delays and keeps the 4-hour timer path separate |
| Silent LocalAppData Installer Flow | ✅ Implemented | `pulse.iss` sets `DefaultDirName={localappdata}\MGG Pulse`, `PrivilegesRequired=lowest`, and relaunch wiring removes `skipifsilent`; launcher passes `/SILENT /RESTARTAPPLICATION` |
| Tray Icon Always Visible When Running | ⚠️ Partial | `App.xaml.cs` activates-then-hides the main window for `StartMinimized`, but actual WinUI/tray survival is not runtime-proven |
| About Version Surface | ✅ Implemented | `AboutViewModel` exposes installed version and manual check command; `AboutPage.xaml` binds version text and update button/status |

---

### Coherence (Design)
| Decision | Followed? | Notes |
|----------|-----------|-------|
| Retry inside `UpdateHostedService.RunCheckSafeAsync` with 3 attempts + exponential back-off | ✅ Yes | Implemented in `RunStartupCheckWithRetryAsync` with 5s/20s/80s defaults |
| Add `/RESTARTAPPLICATION` in `InnoSetupInstallerLauncher` | ✅ Yes | `CreateStartInfo()` now emits `/SILENT /RESTARTAPPLICATION` |
| Remove `skipifsilent` from `pulse.iss` relaunch entry | ⚠️ Deviated | Silent relaunch wiring is present, but design text also called for `shellexec`; current script uses `Flags: nowait postinstall` only |
| Start-minimized path keeps the process alive after splash | ⚠️ Deviated | Design text says “skip Activate + Hide”; implementation actually `Activate()`s then immediately hides, which matches the proposal/tasks and is likely the correct WinUI behavior |

---

### Issues Found

**CRITICAL** (must fix before archive):
- `app/openspec/changes/2026-04-16-updater-relaunch-hardening/proposal.md` is missing on disk in HYBRID mode; verify had to fall back to Engram for the proposal artifact.
- 3 spec scenarios are completely untested at verification time: interactive installer LocalAppData path, minimize-to-tray runtime behavior, and About page version/button surface.
- The only proof for `StartMinimized` survival is a source-inspection test (`ThemeServiceTests.OnLaunched_WhenStartMinimized_ActivatesThenHidesMainWindow`); there is still no runtime WinUI proof that splash closes, process stays alive, and tray remains visible.

**WARNING** (should fix):
- Silent installer relaunch is correctly wired statically (`/RESTARTAPPLICATION` + `pulse.iss` flags) but there is still no manual or automated installer execution proving post-install relaunch end-to-end.
- Manual update-check path remains intact at use-case level, but the About UI contract (“show result inline without restart / silent install”) is only partially covered.
- `App.xaml.cs` has 0% executable coverage; this change still relies on code-structure assertions instead of runtime UI verification.
- Design doc and implementation diverge on the `StartMinimized` and `pulse.iss` details noted above.

**SUGGESTION** (nice to have):
- Add a lightweight manual verification checklist for: startup transient failure then success, silent install relaunch, splash→tray survival with `StartMinimized`, and About page manual check messaging.
- If WinUI automation remains out of scope, isolate startup/minimized decisions behind testable adapters so behavior can be verified without source-text assertions.

---

### Verdict
FAIL

Core updater retry wiring exists and unit tests are green, but the change is NOT ready to archive because runtime-sensitive installer/tray/About scenarios are still unproven and several spec scenarios remain untested.
