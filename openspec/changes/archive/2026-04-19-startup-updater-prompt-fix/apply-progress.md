# Apply Progress: 2026-04-19-startup-updater-prompt-fix

## Summary

- Mode: **Strict TDD** (active from `app/openspec/config.yaml`)
- Previous scope preserved: **Full task set (1.1 → 5.2)**
- Verify-gap remediation scope (latest): add missing behavioral proof for manual `Acerca de` flow when no applicable update exists.
- Result: startup/manual update behavior now has runtime proof for confirm/cancel/unavailable/manual-shared-path **and** manual no-applicable-update (no install exposure, no install trigger).

## Completed Tasks (Cumulative)

- [x] 1.1 In `app/src/MGG.Pulse.UI/App.xaml.cs`, extract prompt logic to `ShowUpdatePromptAsync(...)` returning user decision (`update`/`cancel`) and wiring `XamlRoot` from `_mainWindow.Content`.
- [x] 1.2 In `app/src/MGG.Pulse.UI/App.xaml.cs`, guard `XamlRoot == null` by skipping dialog and reusing current tray-notification fallback path.
- [x] 1.3 In `app/src/MGG.Pulse.UI/App.xaml.cs`, expose a reusable method (`TryApplyAvailableUpdateAsync(...)`) that can be called by startup event and manual flow without duplicating `ApplyUpdateUseCase` orchestration.
- [x] 2.1 Update `OnUpdateAvailable` in `app/src/MGG.Pulse.UI/App.xaml.cs` to call `ShowUpdatePromptAsync`; only call `ApplyUpdateUseCase` + `ExitApp()` when user confirms.
- [x] 2.2 Preserve cancel behavior in `OnUpdateAvailable`: if user cancels, continue app execution and show tray notification indicating update remains available.
- [x] 2.3 In `app/src/MGG.Pulse.UI/ViewModels/AboutViewModel.cs`, add install-intent state (`CanInstallUpdate`, selected update payload/result cache) set only when `CheckForUpdatesAsync` finds applicable updates.
- [x] 2.4 In `app/src/MGG.Pulse.UI/ViewModels/AboutViewModel.cs`, add `InstallUpdateAsync` command that triggers the shared apply path (via callback/event/service already used by `App`) and handles success/error status messages.
- [x] 3.1 In `app/src/MGG.Pulse.UI/Views/AboutPage.xaml`, add `Instalar ahora` button bound to the new install command and enabled state.
- [x] 3.2 In `app/src/MGG.Pulse.UI/Views/AboutPage.xaml`, keep existing “Buscar actualizaciones” behavior and update text/state so install CTA appears only when update is available.
- [x] 4.1 Add/extend tests for `app/tests/MGG.Pulse.Tests.UI/UI/ViewModels/AboutViewModel*` to verify: after update found, install command becomes enabled and invokes apply path.
- [x] 4.2 Add/extend startup update flow tests (in existing UI/core test project) to verify: startup does not silently apply without positive prompt decision.
- [x] 4.3 Add/extend cancel-path test to verify: when prompt is canceled, app does not exit and fallback notification path executes.
- [x] 4.4 Run targeted test suites for updated ViewModel/update flow and confirm proposal success criteria are covered (startup prompt, cancel behavior, manual “Instalar ahora”, shared apply use case).
- [x] 5.1 Remove dead/duplicated update-apply branches left in `App.xaml.cs` and `AboutViewModel.cs` after shared path extraction.
- [x] 5.2 Review user-facing strings in dialog/button/status for consistent Spanish copy in startup and About flows.

## Files Changed (Current Remediation Batch)

| File | Action | Notes |
|------|--------|-------|
| `app/src/MGG.Pulse.UI/Updates/UpdateApplyCoordinator.cs` | Created | New minimal seam that encapsulates behavioral update orchestration (confirm/apply/defer) and exposes `UpdatePromptDecision` for deterministic tests. |
| `app/src/MGG.Pulse.UI/App.xaml.cs` | Modified | Rewired startup/manual entry points to shared coordinator while preserving UX (`ShowUpdatePromptAsync`, deferred tray notification, and exit-on-success installer handoff). |
| `app/src/MGG.Pulse.UI/ViewModels/AboutViewModel.cs` | Modified | Added optional callback seam (`Func<UpdateCheckResult, Task<bool>>`) defaulting to `App.TryApplyAvailableUpdateAsync` so manual flow can be behavior-tested without file-text assertions. |
| `app/tests/MGG.Pulse.Tests.UI/UI/Updates/UpdaterPromptFlowTests.cs` | Modified | Added missing behavioral tests for manual no-applicable-update branches (`NoUpdate` and non-applicable payload), proving install UI/action remain unavailable. |
| `openspec/changes/2026-04-19-startup-updater-prompt-fix/design.md` | Modified | Aligned design text with accepted seam/callback implementation (`UpdateApplyCoordinator` + `AboutViewModel` callback) to remove stale assumptions. |

## TDD Cycle Evidence (Verify-blocker remediation)

| Task | Test File | Layer | Safety Net | RED | GREEN | TRIANGULATE | REFACTOR |
|------|-----------|-------|------------|-----|-------|-------------|----------|
| 4.1 | `app/tests/MGG.Pulse.Tests.UI/UI/Updates/UpdaterPromptFlowTests.cs` (`ManualAboutFlow_TriggersSharedApplyPathAfterDetection`) | Unit (behavioral) | ✅ prior targeted UI safety net preserved (`ThemeServiceTests` baseline from previous apply batch) | ✅ Written first (behavior test referenced `UpdateApplyCoordinator` namespace before seam existed) | ✅ `dotnet test ... --filter "FullyQualifiedName~UpdaterPromptFlowTests"` → 5/5 pass | ✅ Covers update-detected/install-enabled + shared apply callback invocation | ✅ Constructor kept backward-compatible via optional callback default |
| 4.2 | `UpdaterPromptFlowTests.cs` (`StartupAvailableUpdate_AsksForConfirmationBeforeApplying`, `ConfirmPath_InvokesApplyPath`) | Unit (behavioral) | ✅ same safety net context | ✅ Fail-first compile (`CS0234` missing `MGG.Pulse.UI.Updates`) | ✅ 5/5 pass after seam implementation | ✅ Happy path (confirm) + non-apply on prompt gate | ✅ Startup handler simplified to coordinator call |
| 4.3 | `UpdaterPromptFlowTests.cs` (`CancelPath_DoesNotApplyAndDefersNotification`, `UnavailablePromptHost_DoesNotApplyAndDefersSafely`) | Unit (behavioral) | ✅ same safety net context | ✅ Written first with deferred-notification assertions | ✅ 5/5 pass | ✅ Both decision branches (`Cancel` and `Unavailable`) prove no apply + defer | ➖ None needed |
| 4.4 | `UpdaterPromptFlowTests.cs` + targeted `ThemeServiceTests` safety checks | Unit (behavioral) | ✅ `dotnet test ... --filter "FullyQualifiedName~UpdaterPromptFlowTests|FullyQualifiedName~ThemeServiceTests.AboutViewModel_LocalizesUpdateMessagesToSpanish|FullyQualifiedName~ThemeServiceTests.App_ExposesRequestExitStaticAccessorThatReusesExitApp"` → 7/7 pass | ✅ Failing run captured before seam (`CS0234`) | ✅ Passing targeted suite captured | ✅ All requested scenarios covered behaviorally | ➖ None needed |
| 4.1 (verify-gap closure) | `UpdaterPromptFlowTests.cs` (`ManualAboutFlow_NoApplicableUpdate_DoesNotExposeOrTriggerInstall`, `ManualAboutFlow_NonApplicablePayload_DoesNotExposeOrTriggerInstall`) | Unit (behavioral) | ✅ Baseline run before edits: `UpdaterPromptFlowTests` 5/5 pass | ✅ Tests written first for manual no-install behavior before runtime changes | ✅ `dotnet test ... --filter "FullyQualifiedName~UpdaterPromptFlowTests"` → 7/7 pass | ✅ Triangulated both no-update and non-applicable-update branches; assert hidden install CTA + no apply invocation | ➖ None needed (no runtime gap found) |

## Test Summary

- **RED evidence:**
  - `dotnet test tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj --filter "FullyQualifiedName~UpdaterPromptFlowTests"` → **compile failure `CS0234`** (missing seam namespace before implementation)
- **GREEN evidence:**
  - `dotnet test tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj --filter "FullyQualifiedName~UpdaterPromptFlowTests"` → **5/5 pass**
  - `dotnet test tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj --filter "FullyQualifiedName~UpdaterPromptFlowTests|FullyQualifiedName~ThemeServiceTests.AboutViewModel_LocalizesUpdateMessagesToSpanish|FullyQualifiedName~ThemeServiceTests.App_ExposesRequestExitStaticAccessorThatReusesExitApp"` → **7/7 pass**
- **GREEN evidence (latest batch):**
  - `dotnet test tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj --filter "FullyQualifiedName~UpdaterPromptFlowTests"` → **7/7 pass**
  - `dotnet test tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj --filter "FullyQualifiedName~UpdaterPromptFlowTests|FullyQualifiedName~ThemeServiceTests.AboutViewModel_LocalizesUpdateMessagesToSpanish|FullyQualifiedName~ThemeServiceTests.App_ExposesRequestExitStaticAccessorThatReusesExitApp"` → **9/9 pass**
- **Layers used:** Unit (behavioral logic + ViewModel command flow)
- **Approval tests:** None
- **Pure functions created:** 0

## Deviations from Design

- Previously added `UpdateApplyCoordinator` seam and callback-based manual apply path differed from early design text.
  - **Current status:** design artifact updated in this batch to match accepted implementation; no outstanding design mismatch remains.

## Issues Found

- None in this remediation batch.

## Remaining Tasks

- [ ] None in `tasks.md` for this change.

## Status

**15/15 tasks complete. Missing manual no-applicable-update behavioral proof added. Ready for verify.**
