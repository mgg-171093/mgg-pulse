# Tasks: pulse-packify-alignment

## Phase 1: Foundation & Branding Pipeline

- [x] 1.1 Add `assets/branding/logo.svg` and align `assets/branding/logo.png`, `logo-main.png`, `logo-sidebar.png`, `icon-app.png` naming/usage in `app/src/MGG.Pulse.UI/MGG.Pulse.UI.csproj`.
- [x] 1.2 Create `tools/gen-icon.ps1` to generate `assets/branding/icon.ico` (16/32/48/256) from `assets/branding/logo.png` with ImageMagick fallback handling.
- [x] 1.3 Add `build/latest.json` baseline schema (`version`, `url`, `sha256`, `notes`) and document required release values in file comments.
- [x] 1.4 Create `build/build.ps1` to run `dotnet publish` (`win-x64`, Release), stage outputs, call `tools/gen-icon.ps1`, and produce versioned artifacts.
- [x] 1.5 Create `build/pulse.iss` (Inno Setup) consuming staged publish output and emitting `MGGPulse-Setup-{version}.exe`.

## Phase 2: Domain/Application Update Contracts (TDD)

- [x] 2.1 RED: Add `tests/MGG.Pulse.Tests.Unit/Application/Updates/CheckForUpdateUseCaseTests.cs` covering newer/same/older version scenarios and manual-check behavior.
- [x] 2.2 GREEN: Add `src/MGG.Pulse.Domain/Ports/IUpdateService.cs` and `src/MGG.Pulse.Application/UseCases/CheckForUpdateUseCase.cs` with `CancellationToken` support.
- [x] 2.3 REFACTOR: Add `src/MGG.Pulse.Application/Updates/VersionComparer.cs` and simplify use case branching while keeping tests green.
- [x] 2.4 RED: Add `tests/MGG.Pulse.Tests.Unit/Application/Updates/UpdateManifestValidationTests.cs` for missing fields and invalid semver in manifest data.
- [x] 2.5 GREEN: Add `src/MGG.Pulse.Application/Updates/UpdateManifest.cs` and validation logic used by `CheckForUpdateUseCase`.

## Phase 3: Infrastructure Update Service & Timers

- [x] 3.1 Create `src/MGG.Pulse.Infrastructure/Update/GithubReleaseUpdateService.cs` implementing `IUpdateService` via `HttpClient` against `build/latest.json` schema.
- [x] 3.2 Create `src/MGG.Pulse.Application/Updates/UpdateHostedService.cs` with startup deferred check and 4-hour periodic timer (lives in Application for testability).
- [x] 3.3 Add `ITimeProvider` abstraction (`Application/Abstractions/`) and `SystemTimeProvider` (`Infrastructure/Update/`) for timer mocking.
- [x] 3.4 Wire update services in `src/MGG.Pulse.UI/App.xaml.cs` DI and lifecycle start/stop without blocking UI thread.

## Phase 4: UI Shell, Splash, Dashboard, Settings, About

- [x] 4.1 Create shell navigation: `src/MGG.Pulse.UI/Views/ShellPage.xaml` + `src/MGG.Pulse.UI/ViewModels/ShellViewModel.cs` using `NavigationView` (Dashboard/Settings/About).
- [x] 4.2 Simplify dashboard by migrating status/log cards into `src/MGG.Pulse.UI/Views/DashboardPage.xaml` and trimming `src/MGG.Pulse.UI/Views/MainPage.xaml` root role.
- [x] 4.3 Create `src/MGG.Pulse.UI/Views/SettingsPage.xaml` + `src/MGG.Pulse.UI/ViewModels/SettingsViewModel.cs` for config controls extracted from dashboard.
- [x] 4.4 Create `src/MGG.Pulse.UI/Views/AboutPage.xaml` + `src/MGG.Pulse.UI/ViewModels/AboutViewModel.cs` with app version, changelog link, and manual check updates command.
- [x] 4.5 Update `src/MGG.Pulse.UI/Windows/SplashWindow.xaml` and `.xaml.cs` for branded layout, version overlay, and hard 5-second minimum display.
- [x] 4.6 Update `src/MGG.Pulse.UI/App.xaml.cs` and `src/MGG.Pulse.UI/Windows/MainWindow.xaml` to bootstrap Splash → Shell flow and preserve tray behavior.

## Phase 5: Verification & Documentation Alignment

- [x] 5.1 RED/GREEN: Add `tests/MGG.Pulse.Tests.Unit/Application/Updates/UpdateHostedServiceTests.cs` for startup trigger and 4-hour scheduling behavior (time-provider/mocked clock). — 5 tests passing.
- [x] 5.2 Run `dotnet test MGG.Pulse.Tests.Unit` and fix regressions in new update/domain/application tests.
- [ ] 5.3 Perform manual verification checklist: shell navigation, splash 5s minimum, dashboard-only content, settings persistence, about version/update action.
- [x] 5.4 Update `README.md` with packify-aligned branding, shell structure, update flow (`latest.json`), and installer pipeline (`build.ps1` + `pulse.iss`).
