## Exploration: pulse-packify-alignment

### Executive Summary
This exploration analyzes `mgg-packify` (a Flutter/Python hybrid) to extract key structural and UX patterns (Splash Screen, Navigation Shell, Auto-updater via `latest.json`, InnoSetup installer pipeline, and Material Design branding) and map them to `mgg-pulse` (a WinUI 3 / .NET 8 application). The goal is to unify the branding, installer, update flow, and UI architecture under a single "packify" standard adapted to WinUI 3 with a Green 500 (#4CAF50) theme.

### Current mgg-pulse State
Currently, `mgg-pulse` is a standard single-page WinUI 3 application with the following characteristics:
- All features (tracking status, configurations, start/stop) appear to be combined into `MainPage.xaml` and `MainViewModel.cs`.
- No dedicated splash screen with minimum duration exists.
- No auto-updater or release tracking pipeline.
- Relies on default WinUI 3 styling without a dedicated Material Design Green 500 theme.
- The project lacks a bundled installer script (like InnoSetup) and an automated build script (`build.ps1`).

### Reference Patterns found in mgg-packify
- **Splash Screen**: `app/lib/screens/splash_screen.dart`
  - *Pattern*: 5-second minimum display duration, solid background, large centralized logo (`logo-full.png`), circular progress indicator, and text indicating startup status. Transitions to the main shell upon completion.
- **Auto-Updater**: `app/lib/providers/update_check_provider.dart` & `latest.json`
  - *Pattern*: Background polling every 4 hours checking a raw GitHub URL (`latest.json`). Defers initial check. Compares semantic versioning. Downloads update to `%TEMP%` displaying progress, stops background processes, and launches the InnoSetup installer with `/SILENT` before exiting the app.
- **Installer Pipeline**: `build.ps1` & `installer/mgg-packify.iss`
  - *Pattern*: PowerShell script coordinates building the app, copying artifacts to a `staging` directory, and running InnoSetup to produce a single distributable `.exe`.
- **Navigation Shell**: `app/lib/screens/` (implied by router usage)
  - *Pattern*: Lateral sidebar containing primary navigation links (e.g., Dashboard, Settings, About).
- **Branding Assets**: `app/assets/branding/`
  - *Pattern*: Standardized filenames (`logo-main.png`, `logo-sidebar.png`, `icon-app.png`, `banner-readme.png`) utilized consistently across the app and repository markdown.

### Recommended Architecture for mgg-pulse
To map these patterns into WinUI 3 / .NET 8:
1. **Shell and Navigation**:
   - Implement `ShellWindow` or `ShellPage` using `NavigationView` set to `Left` display mode.
   - Refactor `MainPage` to `DashboardPage`, strictly showing tracking state (status, idle time, last/next action).
   - Move configuration bindings to a new `SettingsPage`.
   - Create an `AboutPage` showing the current version and a manual "Check for Updates" button.
2. **Splash Screen**:
   - Create a `SplashWindow` (Window with no titlebar/ExtendsContentIntoTitleBar) displaying `logo-main.png` and a `ProgressRing`.
   - Use `Task.Delay(5000)` combined with any required service initialization before transitioning to `ShellWindow`.
3. **Auto-Updater**:
   - Implement an `IUpdateService` in `MGG.Pulse.Application` that polls a GitHub-hosted `latest.json` every 4 hours using `HttpClient`.
   - On update available, download to `%TEMP%`, report progress via events to the UI (e.g., an `UpdateDialog` or `InfoBar`), and launch the silent installer via `Process.Start`.
4. **Installer & Build Pipeline**:
   - Create a `build.ps1` executing `dotnet publish` targeting `win-x64` (unpackaged or self-contained) to a `staging` folder.
   - Create `installer/mgg-pulse.iss` using InnoSetup to bundle the `staging` contents into a Setup executable.
5. **Material Design & Branding**:
   - Define ResourceDictionaries in `Themes/` for Green 500 primary color (`#4CAF50`) and solid backgrounds.
   - Integrate the existing assets from `app/assets/branding/`.
   - Convert `icon-app.png` to `.ico` for the Window icon and InnoSetup.

### Proposed Subdomains for Proposal/Spec
I recommend breaking this large change into the following logical subdomains for the proposal and spec:
1. **Infrastructure & Pipeline**: Build script (`build.ps1`), icon generation, InnoSetup installer (`mgg-pulse.iss`), and `latest.json` structure.
2. **Core Services**: Auto-updater background service, polling logic, version comparison, and silent installation execution.
3. **UI Shell & Navigation**: `ShellPage` with lateral `NavigationView`, `DashboardPage` (simplified tracking), `SettingsPage`, and `AboutPage`.
4. **Splash & Theming**: `SplashWindow` with 5s delay, Material Green 500 ResourceDictionaries, and asset integration.
5. **Documentation**: Update README.md and documentation with standard banners and badges.

### Risks
- **WinUI 3 Windowing**: WinUI 3's window management can be tricky (e.g., closing SplashWindow and opening ShellWindow without tearing down the app process). Need to ensure `App.xaml.cs` handles window lifecycle correctly.
- **Silent Update Permissions**: If installed in `Program Files`, the silent updater will require UAC elevation. If installed in `AppData\Local` (like mgg-packify), it can update silently without UAC. *Recommendation*: Default installation to `%LocalAppData%\MGG Pulse` to mirror packify's smooth update flow.
- **Unpackaged vs Packaged**: WinUI 3 apps default to MSIX packaging. To use InnoSetup, the app must be configured for unpackaged distribution (`<WindowsPackageType>None</WindowsPackageType>`).

### Discovered Blockers
- **Icon Conversion**: We have `icon-app.png` but WinUI 3 and InnoSetup require `.ico`. We either need a Python script (like packify's `generate_icon.py`) or an external tool to generate the `.ico` file.
- **No Blockers**: Otherwise, the path forward is clear.

### Ready for Proposal
Yes. The orchestrator can proceed with launching `sdd-propose` using the subdomains listed above.