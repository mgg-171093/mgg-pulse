# Changelog

All notable changes to MGG Pulse are documented here.  
Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)  
Versioning: [Semantic Versioning](https://semver.org/spec/v2.0.0.html)

---

## [Unreleased]

### Added
- Dedicated Appearance page with persisted Light/Dark theme switching.
- Dedicated Logs page with in-session continuity for runtime logs.

### Changed
- Larger logo-first splash and refined shell chrome aligned to the Material-inspired visual pass.
- Shell navigation expanded to Dashboard / Settings / Appearance / Logs / About with status bar and stronger sidebar branding.

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

[Unreleased]: https://github.com/mgg-171093/mgg-pulse/compare/v1.2.0...HEAD
[1.2.0]: https://github.com/mgg-171093/mgg-pulse/releases/tag/v1.2.0
[1.0.0]: https://github.com/mgg-171093/mgg-pulse/releases/tag/v1.0.0
