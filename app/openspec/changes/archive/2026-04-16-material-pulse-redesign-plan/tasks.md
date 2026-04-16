# Tasks: Material Pulse Redesign Plan (v3 — Final Closure)

## Batch A: Domain & Infrastructure — Auto support (no UI dependency)

- [x] A.1 Update `SimulationConfig.NormalizeAppearanceTheme` to accept `"Auto"` as a third valid value alongside `"Dark"` and `"Light"`.
- [x] A.2 Update `JsonConfigRepository` DTO to serialize/deserialize `"Auto"`.
- [x] A.3 Add `string ResolveEffectiveTheme(string appearance)` to `IThemeService` port.
- [x] A.4 Unit tests: `SimulationConfig` round-trips Auto; `NormalizeAppearanceTheme("Auto")` returns `"Auto"`.

## Batch B: ThemeService — Auto resolution (depends on A)

- [x] B.1 Implement `ResolveEffectiveTheme` in `ThemeService`: when `"Auto"`, use `UISettings.GetColorValue(UIColorType.Background)` luminance to resolve `"Dark"`/`"Light"`.
- [x] B.2 Update `ThemeService.ApplyTheme` to call `ResolveEffectiveTheme` internally, then swap dictionary AND set `App.Current.RequestedTheme`.
- [x] B.3 Unit tests: `ResolveEffectiveTheme("Dark")` → `"Dark"`, `("Light")` → `"Light"`, `("Auto")` → Dark or Light based on mock.

## Batch C: Startup flow — apply before splash (depends on B)

- [x] C.1 In `App.xaml.cs`, move config load + `ThemeService.ApplyTheme` to before `SplashWindow` creation.
- [x] C.2 Update `SplashWindow` to accept a resolved theme string; set `this.RequestedTheme` before `Activate()`.
- [x] C.3 Replace hardcoded dark gradient in `SplashWindow.xaml` with `ThemeResource`-based background.
- [x] C.4 Add `AppWindow.SetIcon(iconPath)` to `SplashWindow.ConfigureWindow()`, matching `MainWindow` pattern.

## Batch D: StaticResource → ThemeResource migration (depends on B)

- [x] D.1 Replace `{StaticResource XxxBrush}` with `{ThemeResource XxxBrush}` in `ShellPage.xaml`.
- [x] D.2 Replace in `DashboardPage.xaml`.
- [x] D.3 Replace in `SettingsPage.xaml`.
- [x] D.4 Replace in `AppearancePage.xaml`.
- [x] D.5 Replace in `LogsPage.xaml`.
- [x] D.6 Replace in `AboutPage.xaml`.
- [x] D.7 Replace in `Themes/SharedStyles.xaml`.
- [x] D.8 Update `ShellPage.xaml.cs` sidebar brush resolution.

## Batch E: Shell navigation cleanup + Spanish labels (depends on D.1)

- [x] E.1 Remove the manual `NavigationViewItem Content="Settings"` from `ShellPage.xaml`.
- [x] E.2 Set `IsSettingsVisible="True"` and override Settings Content to `"Configuración"`.
- [x] E.3 Update `ShellPage.xaml.cs` `NavView_SelectionChanged` to handle built-in Settings item.
- [x] E.4 Rename nav labels to Spanish.
- [x] E.5 Update page headers to Spanish.

## Batch F: Appearance UI — Save-and-restart flow (depends on B, D.4)

- [x] F.1 Add `IsAutoTheme` property to `AppearanceViewModel` with three-way radio binding support.
- [x] F.2 Update `AppearancePage.xaml` to show three radio options: Oscuro, Claro, Automático.
- [x] F.3 Update `AppearanceViewModel`: save config immediately on selection (no live theme swap). Add `ShowRestartBanner` observable property (bool, default false, set true after any change).
- [x] F.4 Add `RestartCommand` to `AppearanceViewModel` that calls `Microsoft.Windows.AppLifecycle.AppInstance.Restart("")`.
- [x] F.5 Add `InfoBar` to `AppearancePage.xaml`: `Severity="Informational"`, message "El cambio de apariencia se aplicará al reiniciar la aplicación.", action button `Reiniciar`. Bind visibility to `ShowRestartBanner`.
- [x] F.6 Update `SettingsViewModel` to align with Auto support.

## Batch G: Icon regeneration + wiring (depends on C)

- [x] G.1 Regenerate `app/assets/branding/icon.ico` from `app/assets/branding/icon-app.png` as multi-size ICO (16/32/48/256px).
- [x] G.2 Verify `MainWindow` and `SplashWindow` both call `SetIcon` with `icon.ico`.
- [x] G.3 Wire `icon.ico` for tray icon (`ITrayService` implementation) — ensure consistent branding across window, taskbar, and tray.
- [x] G.4 Run `dotnet test MGG.Pulse.Tests.Unit` — fix regressions.
- [x] G.5 Manual validation: appearance save shows InfoBar + Reiniciar restarts and applies; splash matches theme; icon in title bar + taskbar + tray; no duplicate Settings; Spanish labels. (Completed via user validation override during archive closure)
