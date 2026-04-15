# Design: pulse-packify-alignment

## Technical Approach

We will migrate `mgg-pulse` from a single-page layout to a WinUI 3 Navigation Shell with discrete pages, aligning its architecture and deployment strategy with the established patterns from `mgg-packify`. This involves extracting the current `MainPage.xaml` into a `DashboardPage`, adding `SettingsPage` and `AboutPage`, and introducing a robust unpackaged update pipeline with a background updater service and Inno Setup installer. 

## Architecture Decisions

### Decision: Navigation Shell Framework
**Choice**: WinUI 3 `NavigationView` with a central `Frame`.
**Alternatives considered**: Template Studio for WinUI 3's complex MVVM navigation, or an external library like Prism.
**Rationale**: `NavigationView` provides native Windows 11 styling out-of-the-box and fits perfectly for a 3-page app without over-engineering with heavy frameworks. 

### Decision: Update Service Model (Port vs Adapter)
**Choice**: Clean Architecture approach with an `IUpdateService` port and a `GithubReleaseUpdateAdapter`.
**Alternatives considered**: Hardcoding `HttpClient` checks inside `App.xaml.cs` or the `AboutViewModel`.
**Rationale**: Allows easy mocking for testing update dialogs and decoupling the release JSON schema from UI logic. The UI will only know about `UpdateInfo` domain models.

### Decision: Installer Pipeline
**Choice**: `dotnet publish` (Unpackaged, single-file) + Inno Setup (`.iss` script).
**Alternatives considered**: MSIX packaging or WiX Toolset.
**Rationale**: Aligns strictly with `mgg-packify`'s deployment strategy. MSIX restricts background execution and file access, which clashes with `pulse`'s tray behaviors. Inno Setup is simple and handles unpackaged apps perfectly.

## Data Flow

```text
  [Timer / UI] ─→ IUpdateService ─→ GithubReleaseUpdateAdapter
                                              │
                                              ▼
                                         latest.json (GitHub)
                                              │
                                              ▼
  [Dialog UI] ◄── UpdateInfo ◄───────── Parsed Response
       │
       ▼
 [Downloader] ─→ %TEMP%\pulse-setup.exe ─→ [Execute /SILENT]
```

## Shell Architecture

- **MainWindow.xaml**: Contains only the root `Frame`.
- **ShellPage.xaml / ShellViewModel**: Houses the `NavigationView`. The main `Frame` inside this page handles routing to child pages.
- **DashboardPage**: The primary view (formerly `MainPage.xaml`).
- **SettingsPage**: Configurations and system tray behaviors.
- **AboutPage**: Displays app version, branding, and manual update trigger.

## Splash Lifecycle

1. `App.xaml.cs` starts.
2. `SplashWindow.xaml` is created and activated (Borderless, center screen).
3. A `DispatcherTimer` starts (e.g., 3-second minimum duration) alongside initialization tasks (checking DB/Health).
4. Once both the timer and tasks complete, `MainWindow` is created and activated.
5. `SplashWindow.Close()` is called.

## Update Service Architecture

### Interfaces and Models
```csharp
public interface IUpdateService
{
    Task<UpdateCheckResult> CheckForUpdatesAsync(bool isManualCheck);
    Task DownloadAndInstallUpdateAsync(string downloadUrl);
}

public class UpdateCheckResult
{
    public bool IsUpdateAvailable { get; set; }
    public string LatestVersion { get; set; }
    public string DownloadUrl { get; set; }
    public string ReleaseNotes { get; set; }
}
```

### Infrastructure Adapters
- `GithubReleaseUpdateAdapter`: Implements `IUpdateService`.
- **Polling Timer**: `DispatcherTimer` in `App.xaml.cs` or a background `IHostedService` checking every 4 hours.
- **Startup Check**: Invoked quietly on launch. If an update exists, a notification badge or toast is shown.
- **Manual Check**: Invoked from `AboutPage`. Shows loading state, then an explicit dialog with results.

### `latest.json` Schema
```json
{
  "version": "1.2.0",
  "url": "https://github.com/org/mgg-pulse/releases/download/v1.2.0/pulse-setup.exe",
  "notes": "- Added new shell navigation\n- Fixed tray issues"
}
```

## Installer & Branding Pipeline

- **Build Pipeline**: A `build.ps1` script will execute `dotnet publish -c Release -r win-x64 --self-contained`.
- **Icon Generation**: A base `logo.png` will be converted to `.ico` for the executable and Inno Setup installer. 
- **Inno Setup**: `installer.iss` compiles the published artifacts into `pulse-setup.exe`, handling previous version uninstalls and shortcut creation.
- **Branding Assets**: Located in `Assets/Branding/` (logo, splash image, tray icons).

## Documentation File Plan
- `README.md`: Updated with build and installer instructions.
- `docs/architecture.md`: High-level overview of the shell and update pipeline.
- `docs/deployment.md`: Instructions for cutting a release and updating `latest.json`.

## Sequence Diagrams

### Startup Update Check
```text
App        UpdateService       GitHub        UI
 │              │                 │          │
 ├──CheckAsync()─▶│                 │          │
 │              ├─── GET latest ───▶│          │
 │              │◄────── JSON ──────┤          │
 │◄──Result─────┤                 │          │
 │              │                 │          │
 ├─[If Update]───────────────────────────────▶ Show Badge
```

### Manual Update Flow
```text
AboutPage       UpdateService       GitHub        Dialog
 │                    │                 │           │
 ├──CheckAsync(true)──▶│                 │           │
 │                    ├─── GET latest ───▶│           │
 │                    │◄────── JSON ──────┤           │
 │◄────Result─────────┤                 │           │
 │                    │                 │           │
 ├──────────────────────────────────────────────────▶ Show Update Dialog
 │                    │                 │           │
 │                    │                 │           │
 ├─[User Clicks Update]─────────────────────────────▶
 │                    │                 │           │
 ├─DownloadAndInstall()▶                 │           │
 │                    ├── Download Exe ──▶           │
 │                    │◄── Save to %TEMP%─           │
 │                    ├── Execute /SILENT            │
 │                    │                 │           │
 ─ ─ ─ ─ App Terminates for Installation ─ ─ ─ ─ ─ ─
```

## Risks and Mitigation

- **WinUI Threading**: The update downloader must run on background threads (`Task.Run`). Any UI updates (progress bar, dialogs) MUST be dispatched back to the main UI thread using `DispatcherQueue.TryEnqueue` to avoid `0x8001010E` exceptions.
- **Unpackaged App Updates**: The app cannot overwrite its own `.exe` while running. The updater must download the installer to `%TEMP%`, launch it with `/SILENT`, and then immediately `Application.Current.Exit()`. Inno Setup will wait for the lock to release or forcibly terminate if configured.

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `app/mgg-pulse/App.xaml.cs` | Modify | Add splash lifecycle and background update polling. |
| `app/mgg-pulse/MainWindow.xaml` | Modify | Strip out main page content, add root `Frame`. |
| `app/mgg-pulse/Views/ShellPage.xaml` | Create | `NavigationView` shell wrapping the main frame. |
| `app/mgg-pulse/Views/DashboardPage.xaml` | Create | Migrate old `MainPage.xaml` content here. |
| `app/mgg-pulse/Views/SettingsPage.xaml` | Create | Dedicated settings configuration page. |
| `app/mgg-pulse/Views/AboutPage.xaml` | Create | App version and manual update trigger. |
| `app/mgg-pulse/Services/IUpdateService.cs` | Create | Interface for update operations. |
| `app/mgg-pulse/Services/GithubReleaseUpdateAdapter.cs` | Create | Implementation reading `latest.json`. |
| `scripts/build.ps1` | Create | Orchestrates `dotnet publish` and Inno Setup compilation. |
| `installer/setup.iss` | Create | Inno Setup configuration for unpackaged deployment. |

## Open Questions
- None.
