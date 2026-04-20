## Exploration: 2026-04-19-startup-updater-prompt-fix

### Current State
1. **Automatic Startup Check**: The `UpdateHostedService` is scheduled at startup with bounded retries and runs `CheckForUpdateUseCase.ExecuteAsync(token)`. If successful, it fires the `UpdateAvailable` event. Then, `App.xaml.cs` handles this event and **silently** attempts to download the update and launch the Inno Setup installer via `ApplyUpdateUseCase`, followed immediately by an `ExitApp()`. The app literally downloads and quits in the background without prompting the user. There is no `Actualizar`/`Cancelar` dialog. If the background download fails, it falls back to a System Tray notification saying "Update Available".
2. **Manual Check ("Acerca de")**: The user clicks "Buscar actualizaciones" in `AboutViewModel.cs`, which invokes `CheckForUpdateUseCase.ExecuteAsync(token)` directly. If an update is available, the UI merely sets a text label: `UpdateStatusMessage = "Actualización disponible: v1.X.X"`. It does **not** download or start the update process.

### Affected Areas
- `app/src/MGG.Pulse.UI/App.xaml.cs` — Currently hard-codes silent execution. It needs to show a WinUI `ContentDialog` asking "Actualizar" or "Cancelar" when the event fires.
- `app/src/MGG.Pulse.UI/ViewModels/AboutViewModel.cs` — The manual path just shows a string message; it needs a way to actually trigger the `ApplyUpdateUseCase` (the true "update application path").
- `app/src/MGG.Pulse.UI/Views/AboutPage.xaml` — Needs a visible "Actualizar" action that is enabled only when an update is found.

### Approaches
1. **WinUI ContentDialog in App.xaml.cs**
   - Wire a `ContentDialog` with `Actualizar`/`Cancelar` buttons in `App.xaml.cs` (`OnUpdateAvailable`), bound to the Main Window's `XamlRoot`.
   - Wire an "Actualizar" button in the `AboutPage` that either handles the command by calling `ApplyUpdateUseCase` directly or by firing an event handled by `App.xaml.cs`.
   - Pros: Directly addresses both the missing UI dialogue for startup and connects the missing application flow to the manual check.
   - Cons: Putting `ContentDialog` logic in `App.xaml.cs` adds some UI concern there.
   - Effort: Low

2. **Move Update Application Flow to a DialogService**
   - Abstract `IDialogService` and move the dialog and the update applying concern into the application layer, or completely move it into `MainViewModel` rather than `App.xaml.cs`.
   - Pros: Cleaner separation of UI dialogs from `App.xaml.cs`.
   - Cons: `App.xaml.cs` currently has the main `XamlRoot` access and owns the application lifecycle (`ExitApp()`), making it naturally suited to orchestrate an update restart. Setting up an `IDialogService` just for one dialog might be overkill.
   - Effort: Medium

### Recommendation
**Approach 1: WinUI ContentDialog in App.xaml.cs**
It's the most pragmatic solution. We can create a simple `ContentDialog` inside `App.xaml.cs`'s `OnUpdateAvailable` handler:
```csharp
var dialog = new ContentDialog
{
    Title = "Actualización disponible",
    Content = $"MGG Pulse v{result.AvailableVersion} está disponible. ¿Deseás actualizar ahora?",
    PrimaryButtonText = "Actualizar",
    CloseButtonText = "Cancelar",
    XamlRoot = _mainWindow.Content.XamlRoot,
    RequestedTheme = _mainWindow.Content.RequestedTheme
};
```
If the user clicks "Actualizar", we proceed with `ApplyUpdateUseCase` and exit. If "Cancelar", we do nothing (it will naturally be deferred to the next startup check). We also add a command to `AboutViewModel` to trigger this same apply path manually.

### Risks
- `ContentDialog` requires a valid `XamlRoot`. Since the startup check has a 5-second delay, `_mainWindow` should be fully initialized, but we must handle edge cases where `_mainWindow.Content.XamlRoot` is null (e.g. app starting minimized and immediately hidden to tray without ever displaying). If `XamlRoot` is null, we should fall back to the System Tray balloon notification instead of crashing.
- WinUI 3 only allows one `ContentDialog` open per thread at a time. This shouldn't be an issue as this is the only dialog.

### Ready for Proposal
Yes. The orchestrator can proceed to `sdd-propose` using this exploration. The technical gap is fully understood (silent exit vs missing dialog), and the smallest sound architecture is ready.