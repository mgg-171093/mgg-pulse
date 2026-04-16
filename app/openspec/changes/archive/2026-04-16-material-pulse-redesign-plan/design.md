# Design: Material Pulse Redesign Plan (v3 — Final Closure)

## Technical Approach

Seven closure workstreams: (A) `ThemeResource` bindings across XAML for startup-time theme resolution, (B) `Auto` appearance mode with system-theme detection, (C) apply persisted theme before splash/main window, (D) theme-aware splash, (E) remove duplicate Settings nav + Spanish labels, (F) icon regeneration from `icon-app.png` → `icon.ico` + consistent wiring, (G) **save-and-restart** appearance flow with UI messaging + `Reiniciar` action.

## Architecture Decisions

| Decision | Choice | Alternatives | Rationale |
|----------|--------|-------------|-----------|
| Appearance apply strategy | **Save immediately, apply on restart**. Persist choice to config.json on selection; show info banner with `Reiniciar` button. | Live swap via `RequestedTheme` + dictionary hot-reload | WinUI 3 `RequestedTheme` change after window activation is fragile — some controls cache `StaticResource` at load. Restart guarantees a clean, consistent state. Simpler code, no `ActualThemeChanged` subscriptions needed. |
| Restart mechanism | `AppearanceViewModel` calls `Microsoft.Windows.AppLifecycle.AppInstance.Restart("")` | Kill+relaunch via Process.Start; manual user restart | `AppInstance.Restart` is the official WinUI 3/Windows App SDK API for in-place restart. Clean, single-call. |
| Restart UI messaging | `InfoBar` (WinUI 3 control) in `AppearancePage` — `Severity=Informational`, message in Spanish, `Reiniciar` action button. Visible only after a change is made. | Dialog; Toast notification | `InfoBar` is non-blocking, inline, and part of the WinUI 3 control set. Keeps user in context. |
| ThemeResource usage | Keep `ThemeResource` bindings in all XAML. Theme is resolved once at startup — no runtime swap needed, but `ThemeResource` ensures correct palette per `RequestedTheme`. | `StaticResource` since no runtime swap | `ThemeResource` is the correct pattern even for startup-only theming — it resolves based on `RequestedTheme` which we set before window creation. |
| Auto appearance mode | `"Auto"` stored in config. At startup, `ThemeService.ResolveEffectiveTheme("Auto")` reads `UISettings.GetColorValue(UIColorType.Background)` luminance → resolves Dark/Light. | Resolve at save time | Storing `"Auto"` preserves intent; system theme may change between sessions. |
| Icon source of truth | Regenerate `icon.ico` from `app/assets/branding/icon-app.png` (multi-size ICO: 16, 32, 48, 256). Wire via `AppWindow.SetIcon` for `MainWindow`, `SplashWindow`, and tray. | Keep existing icon.ico; embed in manifest only | `icon-app.png` is the canonical branding asset per project standards. `SetIcon` is explicit and works for unpackaged WinUI 3 across window, taskbar, and tray. |
| Spanish labels | Hardcode Spanish strings in XAML `Content` attributes | Resource files (`.resw`) | Single-language app; `.resw` is overkill. |

## Data Flow

```
App() constructor
  │ ConfigureServices()
  │ Load config.json (sync)
  │ ThemeService.ApplyTheme(config.AppearanceTheme)
  │   ├─ "Auto" → UISettings luminance → resolve "Dark"/"Light"
  │   ├─ Swap MergedDictionary
  │   └─ Set App.RequestedTheme
  │
  ▼
SplashWindow(resolvedTheme) → sets RequestedTheme before Activate()
  │
  ▼ (after init)
MainWindow → inherits App.RequestedTheme
  │
User changes appearance (AppearancePage)
  │
AppearanceViewModel.SelectTheme(choice)
  │ config.UpdateAppearanceTheme(choice)
  │ ConfigRepository.SaveAsync(config)    ← persisted immediately
  │ ShowRestartBanner = true              ← InfoBar becomes visible
  │
  ▼ User clicks "Reiniciar"
AppInstance.Restart("") → app restarts with new theme
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `app/assets/branding/icon-app.png` | Source | Canonical source for icon regeneration |
| `app/assets/branding/icon.ico` | Regenerate | Multi-size ICO (16/32/48/256) from `icon-app.png` |
| `Views/AppearancePage.xaml` | Modify | Add `InfoBar` for restart messaging; three radio options (Oscuro/Claro/Automático); `ThemeResource` bindings |
| `ViewModels/AppearanceViewModel.cs` | Modify | Save-on-select; `ShowRestartBanner` property; `RestartCommand` calling `AppInstance.Restart` |
| `Views/ShellPage.xaml` | Modify | `ThemeResource` bindings; remove duplicate Settings nav; Spanish labels |
| `Views/ShellPage.xaml.cs` | Modify | Handle built-in Settings nav item |
| `Views/DashboardPage.xaml` | Modify | `ThemeResource` bindings |
| `Views/SettingsPage.xaml` | Modify | `ThemeResource`; Spanish header |
| `Views/LogsPage.xaml` | Modify | `ThemeResource`; Spanish header |
| `Views/AboutPage.xaml` | Modify | `ThemeResource`; Spanish header |
| `Themes/SharedStyles.xaml` | Modify | `ThemeResource` in style setters |
| `Windows/SplashWindow.xaml` | Modify | Theme-aware background via `ThemeResource` |
| `Windows/SplashWindow.xaml.cs` | Modify | Accept resolved theme; set `RequestedTheme`; `SetIcon` with new `icon.ico` |
| `App.xaml.cs` | Modify | Theme apply before splash creation |
| `Services/ThemeService.cs` | Modify | Handle `"Auto"` resolution; no runtime swap logic needed |
| `Domain/Entities/SimulationConfig.cs` | Modify | `NormalizeAppearanceTheme` accepts `"Auto"` |
| `Infrastructure/Persistence/JsonConfigRepository.cs` | Modify | Serialize `"Auto"` |

## Testing Strategy

| Layer | What | Approach |
|-------|------|----------|
| Unit | `SimulationConfig` accepts Auto, persists it | xUnit |
| Unit | `ThemeService.ResolveEffectiveTheme` logic | xUnit + mock UISettings |
| Unit | Config round-trip with Auto value | xUnit |
| Manual | Appearance change shows InfoBar + Reiniciar works | Restart test |
| Manual | Splash + main window match persisted theme on cold start | Visual |
| Manual | New icon.ico visible in title bar, taskbar, and tray | Visual |
| Manual | No duplicate Settings entry; labels are Spanish | Visual |

## Migration / Rollout

No migration. Existing `config.json` without `"Auto"` falls back to `"Dark"` via default.

## Open Questions

- [x] `IThemeService` lives in Domain — confirmed.
- [x] Runtime vs restart for appearance — **restart** (final decision).
- [ ] Should Auto listen for system theme changes at runtime? Recommendation: defer; apply Auto only at launch.
