# Proposal: pulse-packify-alignment

## Intent

mgg-pulse V1 shipped without an installer, auto-updater, navigation shell, or production-grade splash.
mgg-packify already solved these problems in Flutter/Python and has proven patterns for all of them.
This change ports those patterns to WinUI 3 / .NET 8, making mgg-pulse a complete, releasable desktop product with a consistent branding and release pipeline.

## Scope

### In Scope
- **Installer**: WiX/InnoSetup unpackaged build pipeline (`build.ps1`, NSIS or InnoSetup `.iss`, self-contained `dotnet publish`)
- **Release assets**: `latest.json` on GitHub, versioned `.exe` artifact naming
- **Auto-updater**: `IHostedService` polling (startup + 4-hour periodic) that reads `latest.json`, downloads to `%TEMP%`, launches `/SILENT` installer — Application port `IUpdateService`
- **Navigation shell**: `NavigationView` sidebar with pages: Dashboard, Settings, About — replaces flat `MainPage.xaml`
- **Settings page**: extracted from dashboard panel into own `SettingsPage` + `SettingsViewModel`
- **About page**: new `AboutPage` + `AboutViewModel` showing version, changelog link, manual update check button
- **Splash screen redesign**: enforce 5-second minimum display (matches packify), branded gradient background, version string overlay — delta over existing `splash-screen` spec
- **Branding assets**: `assets/branding/` — `logo.png` single source of truth (already exists), generate `icon.ico` (multi-res: 16/32/48/256) for installer and tray; `logo.svg` as design source
- **Icon generation**: PowerShell script `tools/gen-icon.ps1` using ImageMagick or .NET `System.Drawing` to produce `icon.ico` from `logo.png`
- **Documentation alignment**: `README.md` updated with architecture diagram, build instructions, and release workflow matching packify's README structure
- **Win32 constraints**: flag every Win32 API call (`SendInput`, `GetLastInputInfo`, `Shell_NotifyIcon`, `WinHTTP`/`HttpClient` for updater) and isolate in Infrastructure adapters

### Out of Scope
- MSIX packaging / code signing (deferred)
- Microsoft Store submission
- Multi-profile or multi-user support
- UI automation / E2E tests
- macOS / Linux port

## Capabilities

> Researched `openspec/specs/` — existing capabilities: config-persistence, crash-diagnostics, idle-detection, input-simulation, main-dashboard, simulation-engine, splash-screen, system-tray.

### New Capabilities
- `installer-pipeline`: Build script, InnoSetup config, dotnet publish profile for unpackaged distribution
- `release-manifest`: `latest.json` schema, GitHub release asset naming conventions
- `auto-updater`: Startup + periodic update check, download, and silent install trigger
- `navigation-shell`: `NavigationView` shell with Dashboard / Settings / About routing
- `settings-page`: Dedicated settings page extracted from main dashboard
- `about-page`: Version display, changelog link, manual update check
- `branding-assets`: Logo, icon generation pipeline, `.ico` multi-res output
- `documentation`: README structure aligned with packify patterns

### Modified Capabilities
- `splash-screen`: Add 5-second minimum hold, version string overlay, branded gradient — extends existing spec

## Approach

**Sequence (5 subdomains, can overlap after Infrastructure is stable):**

1. **Infrastructure** — `build.ps1`, InnoSetup `.iss`, `dotnet publish` profile, `latest.json` schema, icon generation script
2. **Core Services** — `IUpdateService` port (Domain), `UpdateService` adapter (Infrastructure), `IHostedService` wiring (UI composition root)
3. **UI Shell** — `NavigationView` shell, page routing, extract `SettingsPage` + `AboutPage` from `MainPage`
4. **Splash & Theming** — 5-second minimum, version overlay, gradient background (delta spec over existing)
5. **Documentation** — `README.md`, inline XML doc on public APIs, architecture diagram (Mermaid)

WinUI 3 constraints applied throughout:
- `NavigationView` uses `CompactPaneLength` — no custom Win32 sidebar
- Updater uses `HttpClient` (not WinHTTP) from Infrastructure; no Win32 HTTP stack exposed to Application
- Icon embedding uses Windows App SDK `AppWindow.SetIcon` + `Shell_NotifyIcon` (Infrastructure only)
- `SolidColorBrush` binding pattern (already established in ch-003/ch-004) must be preserved in new pages

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `src/MGG.Pulse.Domain/Ports/IUpdateService.cs` | New | Update service port — no external deps |
| `src/MGG.Pulse.Application/` | New | `CheckForUpdateUseCase` — orchestrates check + notify |
| `src/MGG.Pulse.Infrastructure/Update/` | New | `UpdateService` — HttpClient + download + launch |
| `src/MGG.Pulse.Infrastructure/Update/UpdateHostedService.cs` | New | `IHostedService` — startup + periodic timer (4h) |
| `src/MGG.Pulse.UI/Views/ShellPage.xaml` | New | `NavigationView` shell replacing `MainPage.xaml` as root |
| `src/MGG.Pulse.UI/Views/SettingsPage.xaml` | New | Extracted settings panel |
| `src/MGG.Pulse.UI/Views/AboutPage.xaml` | New | Version + update trigger |
| `src/MGG.Pulse.UI/Views/MainPage.xaml` | Modified | Stripped to dashboard-only content |
| `src/MGG.Pulse.UI/App.xaml.cs` | Modified | Composition root wires `IHostedService`, `IUpdateService` |
| `openspec/specs/splash-screen/spec.md` | Modified | Delta: 5s minimum + version overlay |
| `assets/branding/` | Modified | Add `logo.svg`, `icon.ico` |
| `tools/gen-icon.ps1` | New | Multi-res `.ico` generation script |
| `build/pulse.iss` | New | InnoSetup config |
| `build/build.ps1` | New | Release build orchestrator |
| `build/latest.json` | New | Release manifest schema + example |
| `README.md` | Modified | Architecture diagram, build + release guide |
| `tests/MGG.Pulse.Tests.Unit/` | Modified | Unit tests for `CheckForUpdateUseCase` + `UpdateService` mock |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| `NavigationView` shell breaks existing `MainPage` bindings | Med | Keep `MainPage` content intact; only wrap in shell frame |
| `UpdateService` downloads/launches executable from internet | High | Hash verification (`SHA-256`) against `latest.json` before launch; abort on mismatch |
| InnoSetup `.iss` path assumptions break on CI | Med | All paths relative to repo root; tested in dry-run before first release |
| 5s splash delay frustrates power users | Low | Make minimum configurable via `AppSettings`; default 5s |
| `Shell_NotifyIcon` tray icon path changes with new icon | Low | `ITrayService` adapter encapsulates path resolution; single constant |

## Rollback Plan

- **Installer / build pipeline**: pure additive (`build/` folder) — no rollback needed; delete folder to revert
- **Auto-updater**: guarded by `IUpdateService` port — disable by removing `UpdateHostedService` registration in `App.xaml.cs`; app boots without it
- **Navigation shell**: `ShellPage` is a new root; revert `App.xaml.cs` to point `MainWindow.Content` back to `MainPage` — one-line change
- **Splash delta**: `splash-screen` spec is versioned in `openspec`; revert delta by restoring previous `spec.md` from git
- **Icon changes**: `logo.png` is unchanged; `icon.ico` is additive — delete to revert tray/installer icon

## Dependencies

- ImageMagick CLI **or** .NET `System.Drawing` — for `gen-icon.ps1` (ImageMagick preferred; fallback to built-in)
- InnoSetup 6.x — for installer build (must be installed on build machine / CI agent)
- GitHub Release API — `latest.json` must be reachable from target machines
- `Microsoft.Extensions.Hosting` — already transitively available; explicit package reference needed if not yet declared

## Success Criteria

- [ ] Running `build.ps1` produces a versioned `MGGPulse-Setup-{version}.exe` installer
- [ ] `latest.json` is published alongside each release with correct `version`, `url`, `sha256` fields
- [ ] App checks for updates on startup and every 4 hours without blocking the UI thread
- [ ] `NavigationView` shell navigates cleanly between Dashboard, Settings, About
- [ ] Settings and About are separate pages with no content duplicated in Dashboard
- [ ] Splash displays for a minimum of 5 seconds and shows the current app version
- [ ] `icon.ico` is generated from `logo.png` with resolutions 16/32/48/256
- [ ] `README.md` documents the build and release workflow end-to-end
- [ ] All new Application logic has corresponding unit tests in `MGG.Pulse.Tests.Unit`
- [ ] No Infrastructure type leaks into Domain or Application layers
