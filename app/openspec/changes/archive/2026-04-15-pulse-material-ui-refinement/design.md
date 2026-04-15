# Design: pulse-material-ui-refinement

## Technical Approach

Refine the WinUI 3 shell using the existing MVVM + Hexagonal architecture. No new domain ports or external libraries — changes are confined to UI and Infrastructure layers. Theme switching uses WinUI 3's `ResourceDictionary` hot-swap via `Application.Current.Resources.MergedDictionaries`. Log relay uses the existing `FileLoggerService.LogEntryAdded` event, subscribed from `LogsViewModel`. The `.ico` asset already exists at `assets/branding/icon.ico` — just needs csproj inclusion and `AppWindow.SetIcon()` calls.

## Architecture Decisions

| Decision | Choice | Alternatives | Rationale |
|----------|--------|-------------|-----------|
| Theme persistence | `Windows.Storage.ApplicationData.Current.LocalSettings` | JSON config file, Registry | LocalSettings is sandboxed, fast, no domain coupling. Config.json is for simulation settings only. |
| Theme swap mechanism | Replace `MergedDictionaries[1]` at runtime in `App.xaml.cs` | `RequestedTheme` enum, full restart | WinUI 3 `RequestedTheme` doesn't support custom palettes. Dictionary swap is instant, no restart needed. Surfaces using `{StaticResource}` require `DynamicResource`-like pattern via re-navigation or binding refresh. |
| Log relay to LogsViewModel | Subscribe to `FileLoggerService.LogEntryAdded` event | CommunityToolkit Messenger, shared service | `LogEntryAdded` already exists and fires on every log. Direct event subscription is simpler; Messenger adds indirection with no benefit here. |
| LogText ownership | Move `LogText` + `AddLogEntry` from `MainViewModel` to `LogsViewModel` | Keep in MainViewModel and bind from both | Single responsibility — `MainViewModel` manages simulation, `LogsViewModel` manages log display. Dashboard already binds simulation-action logs through `CycleOrchestrator` events. |
| Sidebar logo | `Image` element in `NavigationView.PaneHeader` | Custom `NavigationView` header template | PaneHeader is the idiomatic WinUI 3 slot for sidebar branding. Simpler, no custom templates. |
| Status bar location | `Grid` row below `NavigationView` in `ShellPage` | Separate UserControl, inside NavigationView footer | Grid row keeps it as shared shell chrome, always visible regardless of navigation. |

## Data Flow

```
FileLoggerService ──(LogEntryAdded event)──→ LogsViewModel.OnLogEntry()
       │                                           │
       │                                    DispatcherQueue.TryEnqueue
       │                                           │
       └───────────────────────────────────→  LogText property ──→ LogsPage binding

AppearanceViewModel ──(SaveTheme)──→ LocalSettings["AppTheme"]
       │
       └──→ ThemeService.ApplyTheme() ──→ App.Resources.MergedDictionaries[1] = new theme
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `Windows/SplashWindow.xaml` | Modify | Resize Grid to 600x750, logo 400x450, remove text elements |
| `Windows/SplashWindow.xaml.cs` | Modify | Update `AppWindow.Resize` to 600x750, center calc, remove TextPanel animation |
| `Windows/MainWindow.xaml.cs` | Modify | Resize to 800x600, add `AppWindow.SetIcon()` with `.ico` path |
| `Views/ShellPage.xaml` | Modify | Add `PaneHeader` with logo, add Appearance/Logs nav items, add status bar Grid row |
| `Views/ShellPage.xaml.cs` | Modify | Add Appearance/Logs to nav switch, bind status bar to `ShellViewModel` |
| `Views/DashboardPage.xaml` | Modify | Remove Activity Log card (lines 119-135) |
| `Views/LogsPage.xaml` | Create | Dedicated log view with `ScrollViewer` + monospace `TextBlock` bound to `LogsViewModel.LogText` |
| `Views/LogsPage.xaml.cs` | Create | Code-behind resolving `LogsViewModel` from DI |
| `Views/AppearancePage.xaml` | Create | Theme toggle (RadioButtons or ToggleSwitch for Light/Dark) |
| `Views/AppearancePage.xaml.cs` | Create | Code-behind resolving `AppearanceViewModel` from DI |
| `ViewModels/LogsViewModel.cs` | Create | Subscribes to `FileLoggerService.LogEntryAdded`, exposes `LogText` |
| `ViewModels/AppearanceViewModel.cs` | Create | Theme selection, persist to LocalSettings, call `ThemeService.ApplyTheme()` |
| `ViewModels/MainViewModel.cs` | Modify | Remove `LogText`, `AddLogEntry()`, and `_logHandler` orchestrator subscription |
| `ViewModels/ShellViewModel.cs` | Modify | Add `StatusText` property for status bar |
| `Services/ThemeService.cs` | Create | Static helper: `ApplyTheme(string themeName)` swaps MergedDictionaries entry |
| `Themes/LightTheme.xaml` | Create | Restore from `.bak`, add `PrimaryButtonStyle` and `CardStyle` (missing in .bak) |
| `Themes/DarkTheme.xaml` | Modify | Add `CornerRadius` token `<CornerRadius x:Key="CardCornerRadius">8</CornerRadius>` |
| `Infrastructure/Tray/SystemTrayService.cs` | Modify | Use `.ico` file via `new Icon(path)` instead of `Bitmap.GetHicon()` |
| `App.xaml.cs` | Modify | Register `LogsViewModel`, `AppearanceViewModel`, `ThemeService` in DI. Call `ThemeService.ApplyTheme()` on launch with persisted preference. |
| `MGG.Pulse.UI.csproj` | Modify | Add `icon.ico` as Content asset |

## Interfaces / Contracts

```csharp
// UI/Services/ThemeService.cs — static, no interface needed (UI-only concern)
public static class ThemeService
{
    public static void ApplyTheme(string themeName); // "Dark" or "Light"
    public static string GetSavedTheme();            // reads LocalSettings
    public static void SaveTheme(string themeName);  // writes LocalSettings
}
```

```csharp
// LogsViewModel subscribes directly to existing event
public partial class LogsViewModel : ObservableObject
{
    // Constructor takes FileLoggerService (concrete) — it's UI-layer, not domain port
    // Subscribes to LogEntryAdded, accumulates into LogText
}
```

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Unit | `ThemeService.GetSavedTheme` default fallback | Mock `LocalSettings` or test static method with known state |
| Unit | `LogsViewModel` log accumulation & truncation | Supply mock `FileLoggerService`, fire events, assert `LogText` |
| Manual | Theme swap visual correctness | Toggle Light/Dark, verify all surfaces update |
| Manual | Splash sizing, icon surfaces, status bar | Visual inspection on launch |

## Migration / Rollout

No data migration required. `LightTheme.xaml.bak` is renamed to `.xaml` — no data loss. `LocalSettings["AppTheme"]` key is new; missing value defaults to "Dark" (current behavior). Rollback via `git revert` — no persistent state changes beyond the new LocalSettings key which is harmless if orphaned.

## Open Questions

- None — all assets exist, patterns are established, no blocking unknowns.
