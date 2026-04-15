## Exploration: pulse-material-ui-refinement

### Current State
- **Splash Screen**: Fixed at 400x280. Logo is 80x80 with "MGG Pulse" and subtitle text.
- **Main Window**: Starts at 420x640, centered. Does not explicitly set a `.ico` for the WinUI window or taskbar.
- **Navigation**: Uses `NavigationView` in `ShellPage.xaml`. Shows Dashboard, Settings, About, plus a default Settings item at the bottom (duplicate).
- **Themes**: Hardcoded to `DarkTheme.xaml` in `App.xaml` merged dictionaries.
- **Logs**: Currently displayed at the bottom of the `DashboardPage`.
- **Branding**: The logo is loaded from `assets/branding/logo.png`.

### Affected Areas
- `Windows/SplashWindow.xaml` — Needs resize to >=600x750, large 400x450 logo, text removed.
- `Windows/MainWindow.xaml.cs` — Needs to resize to >=800x600, apply `.ico` to taskbar/window title.
- `Views/ShellPage.xaml` & `.cs` — Add Appearance/Logs menu items, hide duplicate settings, add Sidebar Logo Header, distinguish pane vs body backgrounds, add Status bar.
- `Views/DashboardPage.xaml` & `ViewModels/MainViewModel.cs` — Remove logs area. Move logic to a new `LogsPage`.
- `Views/LogsPage.xaml` (NEW) — Move log viewer here.
- `Views/AppearancePage.xaml` (NEW) — Controls for Light/Dark mode.
- `Themes/DarkTheme.xaml` & `Themes/LightTheme.xaml` — Refine colors per Material Design (near-black dark, light-gray light, Green 500 primary).
- `Infrastructure/Tray/SystemTrayService.cs` — Apply `.ico`.

### Approaches

1. **Material Styles Customization (Recommended)**
   - Pros: No external UI library dependencies; fully relies on WinUI3 styling but tweaks colors, padding, and corner radius (`8px` is already standard in project) to mimic Material guidelines.
   - Cons: Real Material ripples/shadows might be limited by WinUI 3 default primitives.
   - Effort: Medium

2. **Full Material UI Library**
   - Pros: Authentic Material UI components.
   - Cons: High risk of incompatibility with WinUI 3 / CommunityToolkit. Requires massive rewrite of UI layers.
   - Effort: High

### Recommendation
Use **Approach 1**. We manually refine the XAML styles in `DarkTheme.xaml` and `LightTheme.xaml`, adding `CornerRadius="8"` to borders and buttons, updating colors to match Material Design specs, and using WinUI's `ThemeDictionaries` to dynamically switch between Light and Dark mode without heavy third-party libraries.

For the theme state persistence, use a simple `LocalSettings` or add it to a UI-specific config layer rather than `SimulationConfig`.

### Risks
- Moving the `LogText` string from `DashboardViewModel` to `LogsViewModel` requires ensuring the `ILoggerService` or an intermediary can push log events to the new ViewModel when it's active.
- WinUI 3 requires specific interop to set the Taskbar `.ico` (`AppWindow.SetIcon()`), which must be physically present as a `.ico` file in the assets.

### Ready for Proposal
Yes. The scope is well understood and isolated mostly to the `UI` and `Infrastructure` layers.
