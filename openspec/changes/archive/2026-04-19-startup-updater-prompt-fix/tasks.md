# Tasks: Startup Updater Prompt Fix

## Phase 1: Foundation (Prompt Reuse Contract)

- [x] 1.1 In `app/src/MGG.Pulse.UI/App.xaml.cs`, extract prompt logic to `ShowUpdatePromptAsync(...)` returning user decision (`update`/`cancel`) and wiring `XamlRoot` from `_mainWindow.Content`.
- [x] 1.2 In `app/src/MGG.Pulse.UI/App.xaml.cs`, guard `XamlRoot == null` by skipping dialog and reusing current tray-notification fallback path.
- [x] 1.3 In `app/src/MGG.Pulse.UI/App.xaml.cs`, expose a reusable method (`TryApplyAvailableUpdateAsync(...)`) that can be called by startup event and manual flow without duplicating `ApplyUpdateUseCase` orchestration.

## Phase 2: Core Implementation (Startup + Manual Apply)

- [x] 2.1 Update `OnUpdateAvailable` in `app/src/MGG.Pulse.UI/App.xaml.cs` to call `ShowUpdatePromptAsync`; only call `ApplyUpdateUseCase` + `ExitApp()` when user confirms.
- [x] 2.2 Preserve cancel behavior in `OnUpdateAvailable`: if user cancels, continue app execution and show tray notification indicating update remains available.
- [x] 2.3 In `app/src/MGG.Pulse.UI/ViewModels/AboutViewModel.cs`, add install-intent state (`CanInstallUpdate`, selected update payload/result cache) set only when `CheckForUpdatesAsync` finds applicable updates.
- [x] 2.4 In `app/src/MGG.Pulse.UI/ViewModels/AboutViewModel.cs`, add `InstallUpdateAsync` command that triggers the shared apply path (via callback/event/service already used by `App`) and handles success/error status messages.

## Phase 3: UI Wiring (About Page)

- [x] 3.1 In `app/src/MGG.Pulse.UI/Views/AboutPage.xaml`, add `Instalar ahora` button bound to the new install command and enabled state.
- [x] 3.2 In `app/src/MGG.Pulse.UI/Views/AboutPage.xaml`, keep existing “Buscar actualizaciones” behavior and update text/state so install CTA appears only when update is available.

## Phase 4: Verification (Fast, Scenario-Based)

- [x] 4.1 Add/extend tests for `app/tests/MGG.Pulse.Tests.UI/UI/ViewModels/AboutViewModel*` to verify: after update found, install command becomes enabled and invokes apply path.
- [x] 4.2 Add/extend startup update flow tests (in existing UI/core test project) to verify: startup does not silently apply without positive prompt decision.
- [x] 4.3 Add/extend cancel-path test to verify: when prompt is canceled, app does not exit and fallback notification path executes.
- [x] 4.4 Run targeted test suites for updated ViewModel/update flow and confirm proposal success criteria are covered (startup prompt, cancel behavior, manual “Instalar ahora”, shared apply use case).

## Phase 5: Cleanup

- [x] 5.1 Remove dead/duplicated update-apply branches left in `App.xaml.cs` and `AboutViewModel.cs` after shared path extraction.
- [x] 5.2 Review user-facing strings in dialog/button/status for consistent Spanish copy in startup and About flows.
