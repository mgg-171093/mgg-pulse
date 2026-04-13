# Skill Registry — MGG Pulse

Generated: 2026-04-08

---

## Project Conventions

| File | Description |
|---|---|
| `AGENTS.md` | Main agent instructions — architecture, conventions, domain model, skills to load |
| `openspec/config.yaml` | SDD config — stack context, testing capabilities, workflow rules |

---

## Project Skills (`.skills/`)

| Skill | Trigger | Path |
|---|---|---|
| `csharp-conventions` | Writing any C# code in this project | `.skills/csharp-conventions/SKILL.md` |
| `winui3-patterns` | Writing XAML, ViewModels, WinUI 3 UI code | `.skills/winui3-patterns/SKILL.md` |
| `xunit-testing` | Writing tests in MGG.Pulse.Tests.Unit | `.skills/xunit-testing/SKILL.md` |

---

## User Skills (auto-loaded by trigger)

| Skill | Trigger | Source |
|---|---|---|
| `branch-pr` | Creating a pull request / opening a PR | User global skills |
| `go-testing` | Writing Go tests, Bubbletea TUI testing | User global skills |
| `issue-creation` | Creating a GitHub issue, reporting a bug | User global skills |
| `judgment-day` | "judgment day", adversarial review | User global skills |
| `sdd-apply` | Implementing tasks from a change | User global skills |
| `sdd-archive` | Archiving a completed change | User global skills |
| `sdd-design` | Writing technical design for a change | User global skills |
| `sdd-explore` | Exploring/investigating a feature | User global skills |
| `sdd-init` | Initializing SDD in a project | User global skills |
| `sdd-propose` | Creating a change proposal | User global skills |
| `sdd-spec` | Writing specs for a change | User global skills |
| `sdd-tasks` | Breaking down a change into tasks | User global skills |
| `sdd-verify` | Verifying implementation against specs | User global skills |
| `skill-creator` | Creating a new AI skill | User global skills |
| `skill-registry` | Updating the skill registry | User global skills |

---

## Load Order

When writing code for this project, ALWAYS load in this order:
1. `csharp-conventions` — naming, layer rules, DI, error handling
2. `winui3-patterns` (if UI code) — XAML, MVVM, bindings
3. `xunit-testing` (if writing tests) — naming, Moq, AAA pattern

Multiple skills can be active simultaneously.
