## Exploration: Updater and Startup Lifecycle Hardening

### Current State
1. **Startup Update Check**: Scheduled 5 seconds after startup. If the network is not yet connected (e.g., app launches at Windows boot), the check fails silently and the next check is 4 hours later.
2. **Installer Relaunch**: The silent updater runs Inno Setup (`/SILENT`). However, the `pulse.iss` script has the `skipifsilent` flag on the launch `[Run]` entry, meaning the installer finishes silently but never starts the application again.
3. **Startup Minimized Exit**: In WinUI 3, if `MainWindow` is created but never activated, closing the `SplashWindow` drops the visible window count to 0, which triggers an automatic application exit.

### Affected Areas
- `app/src/MGG.Pulse.Application/Updates/UpdateHostedService.cs` — Needs retry logic for initial boot check.
- `app/build/pulse.iss` — Needs a new conditional `[Run]` entry that supports silent relaunch.
- `app/src/MGG.Pulse.Infrastructure/Update/InnoSetupInstallerLauncher.cs` — Needs to pass the custom relaunch parameter to the installer.
- `app/src/MGG.Pulse.UI/App.xaml.cs` — Needs to activate and immediately hide `MainWindow` when starting minimized.

### Approaches

1. **Update Check Retry Loop**
   - Pros: Simple `for` loop (3 retries, 15s delay) handles brief network outages without complex Polly dependencies.
   - Cons: Slightly longer background task on failure, but harmless.
   - Effort: Low

2. **Installer Relaunch via ISS Parameter**
   - Pros: We modify `pulse.iss` to add an `IsUpdate` PascalScript function and a `[Run]` entry for `/UPDATE=1`. The launcher passes this flag. This is natively supported by Inno Setup and very robust.
   - Cons: Modifies the installation script, requires rebuilding the installer to take effect (standard CI/CD will handle this).
   - Effort: Low

3. **WinUI 3 Window Lifecycle Fix**
   - Pros: We unconditionally call `_mainWindow.Activate()` to register the Win32 handle and keep the message loop alive, then immediately call `_mainWindow.AppWindow.Hide()` if `StartMinimized` is true. 
   - Cons: Could cause a very brief visual flash on slower machines, though usually imperceptible since the splash screen is still visible or closing simultaneously.
   - Effort: Low

### Recommendation
Implement all three specific low-effort fixes. They directly address the regressions using the most robust native mechanisms available to the stack (WinUI 3 `Hide()`, InnoSetup `Check:` functions, and simple `Task` retries).

### Risks
- **Visual Flash**: `Activate()` followed by `Hide()` might show a brief flash of the main window before it minimizes to tray.
- **Installer Syntax**: PascalScript in Inno Setup is strict. The `[Code]` section must correctly read the parameter.

### Ready for Proposal
Yes — the fixes are scoped, safe, and ready to be proposed.