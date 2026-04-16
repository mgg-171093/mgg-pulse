# Changelog

All notable changes to MGG Pulse are documented here.  
Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)  
Versioning: [Semantic Versioning](https://semver.org/spec/v2.0.0.html)

---

## [Unreleased]

---

## [1.3.1] — 2026-04-16

### Fixed
- Corrected updater manifest resolution to the raw `main` branch `latest.json` model, improving reliability of update checks against release metadata.

---

## [1.3.0] — 2026-04-16

### Added
- **material-pulse-redesign-plan**: Added Auto appearance mode with save-and-restart UX flow, restart InfoBar action, and shared interaction polish spec coverage.

### Changed
- Applied launch-time appearance synchronization so splash and shell render with persisted theme from first frame.
- Unified branded icon pipeline and runtime wiring across window chrome, taskbar, and tray.
- Updated shell/navigation and settings surfaces with Spanish labels and removed duplicate Settings entry.

---

## [1.2.0] — 2026-04-15

### Added
- **pulse-material-ui-refinement**: Material-inspired UI refinement with dedicated Appearance and Logs pages, persisted theme preference, sidebar branding, status bar, and consistent icon surfaces.

### Changed
- Enlarged splash experience to focus on the brand logo and loader.
- Removed dashboard log transcript in favor of a dedicated Logs surface.
- Refined dark/light palettes and shell layout for clearer sidebar/body separation.

---

## [1.0.0] — 2026-04-08

### Added
- **ch-001**: Project foundation — hexagonal architecture, Domain/Application/Infrastructure/UI layers, Rule Engine, xUnit test project
- **ch-002**: Windows App SDK upgrade — migrated to WinUI 3 with Windows App SDK 1.5+
- **ch-003**: Crash diagnostics — global exception handler, structured logging, crash report on startup
- **ch-004**: UI wiring and ComboBox fix — MainDashboard ViewModel bindings, mode/input type selectors

### Architecture
- Domain layer with zero external dependencies (Entities, Value Objects, Ports, Enums)
- Application layer: StartSimulationUseCase, StopSimulationUseCase, RuleEngine (IdleRule, AggressiveModeRule, IntervalRule), CycleOrchestrator
- Infrastructure layer: Win32InputSimulator (SendInput), Win32IdleDetector (GetLastInputInfo), JsonConfigRepository, FileLoggerService, TrayService
- UI layer: WinUI 3 with Material Design-inspired dark/light theme, System Tray integration, Splash Screen, MainDashboard, Logs panel

---

[Unreleased]: https://github.com/mgg-171093/mgg-pulse/compare/v1.3.1...HEAD
[1.3.1]: https://github.com/mgg-171093/mgg-pulse/releases/tag/v1.3.1
[1.3.0]: https://github.com/mgg-171093/mgg-pulse/releases/tag/v1.3.0
[1.2.0]: https://github.com/mgg-171093/mgg-pulse/releases/tag/v1.2.0
[1.0.0]: https://github.com/mgg-171093/mgg-pulse/releases/tag/v1.0.0
