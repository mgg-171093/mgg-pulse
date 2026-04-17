# Design: Updater Relaunch Hardening

## Technical Approach

Three narrow fixes to the update lifecycle: (1) retry failed startup update checks with exponential back-off, (2) pass `/RESTARTAPPLICATION` to Inno Setup so the app relaunches after silent install, (3) hide `MainWindow` correctly when `StartMinimized` is true (create + hide instead of skip-activate which leaves an invisible but focusable window).

## Architecture Decisions

| # | Decision | Alternatives | Rationale |
|---|----------|-------------|-----------|
| 1 | Retry inside `UpdateHostedService.RunCheckSafeAsync` with 3 attempts + exponential back-off (5s, 20s, 80s) | Polly library; retry in `CheckForUpdateUseCase` | Keeps retry concern in the hosted service (orchestration layer), avoids new dependency. Use case stays pure single-attempt. |
| 2 | Add `/RESTARTAPPLICATION` to Inno Setup `/SILENT` args in `InnoSetupInstallerLauncher` | Custom restart script; scheduled task | Inno Setup natively supports this flag — it re-launches `[Run]` entry after install. Zero custom code. |
| 3 | Add `[Run]` `Flags: ... shellexec` to `pulse.iss` `postinstall` entry and mark it to also run on silent installs (remove `skipifsilent`) | Separate `[Run]` entry for silent mode | Single entry with correct flags covers both interactive and silent modes. |
| 4 | For `StartMinimized`: keep current pattern (create MainWindow, skip `Activate()`) but also call `_mainWindow.AppWindow.Hide()` immediately after creation | Don't create MainWindow at all; create lazily on tray-show | WinUI 3 requires at least one Window alive for message loop. Hide() ensures the window isn't in alt-tab or taskbar. Current code only skips Activate but window is still "visible" to the shell. |

## Data Flow

```
Startup
  │
  ├─ SplashWindow.Activate()
  ├─ InitializeServicesWithProgress()
  ├─ new MainWindow()
  │    ├─ if StartMinimized → AppWindow.Hide()
  │    └─ else → Activate()
  ├─ SplashWindow.Close()
  ├─ UpdateHostedService.StartAsync()
  │    └─ RunCheckSafeAsync() ─── fail? ──→ retry (3x, exp back-off)
  │         └─ success + update? ──→ ApplyUpdateUseCase
  │              └─ InnoSetupInstallerLauncher("/SILENT /RESTARTAPPLICATION")
  │                   └─ ExitApp() → Inno replaces files → relaunches app
  └─ Periodic timer (4h) ── same RunCheckSafeAsync
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `src/MGG.Pulse.Application/Updates/UpdateHostedService.cs` | Modify | Add retry loop (3 attempts, exponential back-off) inside `RunCheckSafeAsync` |
| `src/MGG.Pulse.Infrastructure/Update/InnoSetupInstallerLauncher.cs` | Modify | Change args from `"/SILENT"` to `"/SILENT /RESTARTAPPLICATION"` |
| `build/pulse.iss` | Modify | Remove `skipifsilent` from `[Run]` entry so app relaunches after silent install |
| `src/MGG.Pulse.UI/App.xaml.cs` | Modify | Add `_mainWindow.AppWindow.Hide()` in the `StartMinimized` branch |
| `tests/.../UpdateHostedServiceTests.cs` | Modify | Add tests for retry behavior (verify 3 attempts on transient failure) |

## Interfaces / Contracts

No new interfaces. Only behavioral changes to existing implementations:

```csharp
// UpdateHostedService — retry constants (internal, no public API change)
private const int MaxRetries = 3;
private static readonly TimeSpan[] RetryDelays = [
    TimeSpan.FromSeconds(5),
    TimeSpan.FromSeconds(20),
    TimeSpan.FromSeconds(80)
];
```

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Unit | Startup check retries 3x on failure then stops | Mock `CheckForUpdateUseCase` to fail N times, verify invocation count |
| Unit | Startup check stops retrying on success | Mock to fail once then succeed, verify exactly 2 calls |
| Unit | Retry does not apply to periodic timer checks | Verify periodic callback still does single attempt |
| Unit | `/RESTARTAPPLICATION` flag is passed | Assert `ProcessStartInfo.Arguments` in launcher test |

## Migration / Rollout

No migration required. The `/RESTARTAPPLICATION` flag only takes effect on the NEXT update install — existing users get the fix when they update to this version.

## Open Questions

None — all three fixes use existing Inno Setup and WinUI 3 capabilities.
