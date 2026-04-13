# MGG Pulse

A configurable, rule-based user activity simulator for Windows.  
Prevents session timeouts in remote environments (RDP, VDI, Citrix) by simulating minimal, non-intrusive input only when the user is genuinely idle.

---

## Features

- **Intelligent Mode** — simulates input only when idle time exceeds threshold
- **Aggressive Mode** — always simulate, regardless of user activity
- **Manual Mode** — simulate on fixed intervals
- **Safe simulation** — mouse moves 1–2px, keyboard uses Shift/Ctrl (non-disruptive)
- **System Tray** — runs silently in background, accessible from tray icon
- **Custom Splash Screen** — animated fade-in on startup
- **Dark/Light theme** — Material Design inspired, WinUI 3 native
- **Persistent config** — saves to `%AppData%\MGG\Pulse\config.json`
- **Real-time logs** — built-in log viewer with configurable verbosity

---

## Architecture

```
MGG.Pulse.Domain          ← Entities, Value Objects, Ports (interfaces). Zero dependencies.
MGG.Pulse.Application     ← Use Cases, Rule Engine, Cycle Orchestrator
MGG.Pulse.Infrastructure  ← Win32 adapters, JSON config, file logger, tray service
MGG.Pulse.UI              ← WinUI 3 views, ViewModels (CommunityToolkit.Mvvm)
MGG.Pulse.Tests.Unit      ← xUnit + Moq — Domain and Application tests
```

**Dependency rule**: `UI → Application → Domain`. Infrastructure implements Domain ports. Domain has zero external dependencies.

---

## Requirements

- Windows 10 (1809+) or Windows 11
- .NET 8 Desktop Runtime
- Windows App SDK 1.5+

---

## Build

```bash
dotnet build MGG.Pulse.sln
```

## Run Tests

```bash
dotnet test tests/MGG.Pulse.Tests.Unit
```

---

## Logo Replacement

The logo is defined in a single location:

```
assets/branding/logo.png
```

Replace that file (256×256 PNG recommended) to update the logo in:
- Splash screen
- System tray icon

The build pipeline copies it automatically — no other changes needed.

---

## Project Structure

```
mgg-pulse/
├── src/
│   ├── MGG.Pulse.Domain/
│   ├── MGG.Pulse.Application/
│   ├── MGG.Pulse.Infrastructure/
│   └── MGG.Pulse.UI/
├── tests/
│   └── MGG.Pulse.Tests.Unit/
├── assets/
│   └── branding/
│       └── logo.png          ← Replace to update logo everywhere
├── openspec/                 ← Spec-Driven Development artifacts
├── .skills/                  ← Project-specific agent skills
└── AGENTS.md                 ← AI agent instructions
```

---

## Config Location

```
%AppData%\MGG\Pulse\config.json
```

---

## V1 Scope

This is a personal-use tool. Out of scope for V1:

- MSIX installer / code signing
- Auto-update
- UI automation tests
- Multi-profile support
