# MGG Pulse

> A configurable, rule-based user activity simulator for Windows.  
> Prevents session timeouts in remote environments (RDP, VDI, Citrix) by simulating minimal, non-intrusive input only when the user is genuinely idle.

---

## Quick Start

Requirements: **Windows 10 (1809+) or Windows 11**, **.NET 8 Desktop Runtime**, **Windows App SDK 1.5+**

```bash
# Build
dotnet build app/MGG.Pulse.slnx

# Run tests
dotnet test app/tests/MGG.Pulse.Tests.Unit
```

---

## Features

| Feature | Description |
|---|---|
| **Intelligent Mode** | Simulates input only when idle time exceeds threshold |
| **Aggressive Mode** | Always simulates, regardless of user activity |
| **Manual Mode** | Fixed-interval simulation |
| **Safe simulation** | Mouse moves 1–2px, keyboard uses Shift/Ctrl (non-disruptive) |
| **System Tray** | Runs silently, accessible via tray icon |
| **Dark/Light theme** | Material Design inspired, WinUI 3 native |
| **Persistent config** | Saves to `%AppData%\MGG\Pulse\config.json` |
| **Real-time logs** | Built-in log viewer with configurable verbosity |

---

## Architecture

```
MGG.Pulse.Domain          ← Entities, Value Objects, Ports (interfaces). Zero dependencies.
MGG.Pulse.Application     ← Use Cases, Rule Engine, Cycle Orchestrator
MGG.Pulse.Infrastructure  ← Win32 adapters, JSON config, file logger, tray service
MGG.Pulse.UI              ← WinUI 3 views, ViewModels (CommunityToolkit.Mvvm)
MGG.Pulse.Tests.Unit      ← xUnit + Moq — Domain and Application layer tests
```

**Dependency rule**: `UI → Application → Domain`.  
Infrastructure implements Domain ports. Domain has **zero** external dependencies.

---

## Repository Structure

```
mgg-pulse/
├── app/                          ← All source code
│   ├── src/
│   │   ├── MGG.Pulse.Domain/
│   │   ├── MGG.Pulse.Application/
│   │   ├── MGG.Pulse.Infrastructure/
│   │   └── MGG.Pulse.UI/
│   ├── tests/
│   │   └── MGG.Pulse.Tests.Unit/
│   ├── assets/branding/logo.png  ← Replace to update logo everywhere
│   ├── openspec/                 ← Spec-Driven Development artifacts
│   ├── .skills/                  ← Project-specific AI agent skills
│   ├── AGENTS.md                 ← AI agent instructions
│   └── README.md
├── .gitignore
└── README.md                     ← This file
```

---

## For Contributors / AI Agents

This project uses **Spec-Driven Development (SDD)** with `openspec` persistence.

- Agent instructions: [`app/AGENTS.md`](app/AGENTS.md)
- SDD config & specs: [`app/openspec/`](app/openspec/)
- Skill registry: [`app/.atl/skill-registry.md`](app/.atl/skill-registry.md)

---

## Logo

Replace `app/assets/branding/logo.png` (256×256 PNG) to update the logo in the splash screen and system tray. The build copies it automatically — no other changes needed.

---

## V1 Scope

Personal-use tool. Out of scope for V1:
- MSIX installer / code signing
- Auto-update
- UI automation tests
- Multi-profile support
