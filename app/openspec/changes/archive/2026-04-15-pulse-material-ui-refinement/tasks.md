# Tasks: pulse-material-ui-refinement

## Phase 1: Foundation & Assets

- [x] 1.1 Include `app/assets/branding/icon.ico` in `app/src/MGG.Pulse.UI/MGG.Pulse.UI.csproj` as Content with output copy metadata.
- [x] 1.2 Create `app/src/MGG.Pulse.UI/Services/ThemeService.cs` with `ApplyTheme`, `GetSavedTheme`, and `SaveTheme` using `ApplicationData.Current.LocalSettings`.
- [x] 1.3 Create `app/src/MGG.Pulse.UI/ViewModels/AppearanceViewModel.cs` to expose Light/Dark selection, call `ThemeService`, and handle reselect-noop behavior.
- [x] 1.4 Create `app/src/MGG.Pulse.UI/ViewModels/LogsViewModel.cs` subscribing to `FileLoggerService.LogEntryAdded`, append entries on UI thread, and preserve session text state.

## Phase 2: Shell & Navigation Refinement

- [x] 2.1 Update `app/src/MGG.Pulse.UI/Views/ShellPage.xaml` to add sidebar logo (`NavigationView.PaneHeader`), visible status bar row, and unique nav items including Appearance and Logs.
- [x] 2.2 Update `app/src/MGG.Pulse.UI/Views/ShellPage.xaml.cs` navigation switch to resolve and route `AppearancePage` and `LogsPage` without duplicate Settings entries.
- [x] 2.3 Add `StatusText` in `app/src/MGG.Pulse.UI/ViewModels/ShellViewModel.cs` and bind it from the shell status bar.
- [x] 2.4 Create `app/src/MGG.Pulse.UI/Views/AppearancePage.xaml` and `.xaml.cs`, wiring DI resolution for `AppearanceViewModel`.
- [x] 2.5 Create `app/src/MGG.Pulse.UI/Views/LogsPage.xaml` and `.xaml.cs`, binding log transcript UI to `LogsViewModel.LogText`.

## Phase 3: Startup Windows, Themes, and Log Ownership

- [x] 3.1 Update `app/src/MGG.Pulse.UI/Windows/SplashWindow.xaml` and `.xaml.cs` to minimum 600×750, centered, large logo, and no supporting text/animations.
- [x] 3.2 Update `app/src/MGG.Pulse.UI/Windows/MainWindow.xaml.cs` to minimum 800×600 and set branded icon through `AppWindow.SetIcon()`.
- [x] 3.3 Update `app/src/MGG.Pulse.Infrastructure/Tray/SystemTrayService.cs` to load tray icon from `.ico` path safely with fallback logging if missing.
- [x] 3.4 Remove dashboard log transcript UI from `app/src/MGG.Pulse.UI/Views/DashboardPage.xaml` and keep dashboard-only content.
- [x] 3.5 Remove `LogText`/log handlers from `app/src/MGG.Pulse.UI/ViewModels/MainViewModel.cs`, leaving simulation responsibilities only.
- [x] 3.6 Restore/create `app/src/MGG.Pulse.UI/Themes/LightTheme.xaml` from `.bak` and update both Light/Dark themes with near-black/light-gray surfaces, green primary, and `CornerRadius=8` tokens.
- [x] 3.7 Update `app/src/MGG.Pulse.UI/App.xaml.cs` DI registrations for new viewmodels/services and apply persisted theme on startup.

## Phase 4: Verification

- [x] 4.1 Add unit tests under `app/tests/MGG.Pulse.Tests.Unit` for `ThemeService` persisted preference and invalid-value fallback-to-default behavior.
- [x] 4.2 Add unit tests under `app/tests/MGG.Pulse.Tests.Unit` for `LogsViewModel` live accumulation and continuity after page re-navigation in-session.
- [x] 4.3 Run manual validation: startup sizes, icon on window/taskbar/tray, shell nav uniqueness, status bar visibility, Dashboard without logs, Logs live updates, Appearance runtime swap + persisted restore.

## Dependencies

- 1.1 is prerequisite for 2.1/3.2/3.3 (icon availability).
- 1.2 is prerequisite for 1.3 and 3.7.
- 1.4 is prerequisite for 2.5 and 4.2.
- 2.4 and 2.5 depend on 1.3 and 1.4 respectively.
- 3.5 should complete before 3.4/2.5 verification to avoid duplicate log ownership.
- 4.x starts after Phases 1–3 are complete.
