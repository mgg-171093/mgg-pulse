# Design: Startup Updater Prompt Fix

## Technical Approach

Introduce a single `ShowUpdatePromptAsync` helper on `App.xaml.cs` that shows a WinUI `ContentDialog` asking the user to confirm before applying. Startup and manual flows converge through a shared `UpdateApplyCoordinator` seam that centralizes confirm/apply/defer behavior while keeping WinUI dialog rendering in `App`. `AboutViewModel.InstallUpdateCommand` delegates to this shared path via callback.

## Architecture Decisions

| Decision | Alternatives | Rationale |
|----------|-------------|-----------|
| Dialog lives as `App.ShowUpdatePromptAsync(UpdateCheckResult)` static helper | Separate `UpdatePromptDialog` class; Dialog service | Prompt rendering remains a UI concern in `App`; no extra dialog class/service needed for two call sites. |
| Shared orchestration lives in `UpdateApplyCoordinator` | Keep all flow inline in `App`; duplicate manual/startup branches | A focused coordinator seam enables deterministic behavioral tests for confirm/cancel/unavailable/manual flows without WinUI automation, while preserving UX wiring in `App`. |
| `AboutViewModel` uses callback seam (`Func<UpdateCheckResult, Task<bool>>`) defaulting to `App.TryApplyAvailableUpdateAsync(...)` | Direct `ApplyUpdateUseCase` call + static exit; messenger/event bus | Callback keeps VM decoupled from installer orchestration details and lets tests prove manual flow behavior without source-text assertions. |
| Startup path shows dialog via `_mainWindow.Content.XamlRoot` | Defer dialog to next navigation; Use tray balloon only | `_mainWindow` is always created before `UpdateHostedService` fires. `Content.XamlRoot` is available after `Activate()`. Safe and direct. |

## Data Flow

```
Startup path:
  UpdateHostedService ──(event)──→ App.OnUpdateAvailable
    ──(dispatcher)──→ UpdateApplyCoordinator.TryApplyAvailableUpdateAsync(showPrompt: true)
      ├─ prompt via App.ShowUpdatePromptAsync(result)
      ├─ user accepts ──→ App.ApplyUpdateAndExitAsync → ApplyUpdateUseCase.ExecuteAsync → ExitApp()
      └─ user cancels/unavailable ──→ TrayService.ShowNotification (existing fallback)

Manual path:
  AboutPage [Instalar ahora] ──→ AboutViewModel.InstallUpdateCommand
    ──→ callback (`App.TryApplyAvailableUpdateAsync`) ──→ UpdateApplyCoordinator shared path
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `app/src/MGG.Pulse.UI/Updates/UpdateApplyCoordinator.cs` | Add | Encapsulates shared update apply orchestration (prompt decision + apply/defer behavior) for startup and manual paths. |
| `app/src/MGG.Pulse.UI/App.xaml.cs` | Modify | Keep `ShowUpdatePromptAsync(UpdateCheckResult)` with Spanish copy and `_mainWindow.Content.XamlRoot` guard; wire startup/manual entry points into coordinator and existing tray fallback/exit handoff. |
| `app/src/MGG.Pulse.UI/ViewModels/AboutViewModel.cs` | Modify | Keep cached `UpdateCheckResult` install-intent state and `InstallUpdateCommand`; call shared apply callback seam instead of directly owning `ApplyUpdateUseCase` + exit. |
| `app/src/MGG.Pulse.UI/Views/AboutPage.xaml` | Modify | Add "Instalar ahora" `Button` bound to `InstallUpdateCommand`, visible when `IsUpdateInstallable` is true. Place below existing status message. |

## Interfaces / Contracts

No new interfaces. `AboutViewModel` constructor gains an optional callback seam:

```csharp
public AboutViewModel(CheckForUpdateUseCase checkUseCase, Func<UpdateCheckResult, Task<bool>>? tryApplyUpdateAsync = null)
```

When callback is omitted, it defaults to `App.TryApplyAvailableUpdateAsync`.

## Testing Strategy

| Layer | What | Approach |
|-------|------|----------|
| Unit (Core) | `AboutViewModel` sets `IsUpdateInstallable` correctly after check | Mock both use cases; assert property after `CheckForUpdateCommand` |
| Unit (Core) | `InstallUpdateCommand` calls apply + exit callback | Mock `ApplyUpdateUseCase`; inject exit callback; verify sequence |
| Manual | Startup prompt appears; cancel falls back to tray | Run app with outdated version against real manifest |

## Migration / Rollout

No migration required.

## Open Questions

- (none)
