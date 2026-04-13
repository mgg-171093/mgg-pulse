# Changelog

All notable changes to MGG Pulse are documented here.  
Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)  
Versioning: [Semantic Versioning](https://semver.org/spec/v2.0.0.html)

---

## [Unreleased]

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

[Unreleased]: https://github.com/manugomezf/mgg-pulse/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/manugomezf/mgg-pulse/releases/tag/v1.0.0
