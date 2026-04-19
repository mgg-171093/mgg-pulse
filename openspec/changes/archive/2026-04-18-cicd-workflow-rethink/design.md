# Design: CI/CD Workflow Rethink

## Technical Approach

Replace both current workflows from scratch. The existing `ci.yml` and `release.yml` work structurally but accumulated inline PowerShell that's hard to debug and test. The rethink extracts logic into reusable `.ps1` scripts, simplifies YAML to orchestration-only, and makes the release pipeline atomic and idempotent.

## Architecture Decisions

| Decision | Choice | Alternative | Rationale |
|----------|--------|-------------|-----------|
| Replace vs patch | Replace from scratch | Patch existing | Current workflows embed 60+ lines of inline PS; refactoring in-place is harder than a clean rewrite with the same proven structure |
| Script extraction | `.github/scripts/*.ps1` helpers | Keep inline | Testable locally, debuggable with `pwsh -File`, reduces YAML to `run: .github/scripts/X.ps1` |
| Workflow count | Keep 2 files: `ci.yml` + `release.yml` | Single file with conditionals | Clean trigger separation; single-file conditionals are harder to read and debug |
| CI test filtering | `dotnet test --filter "Category!=Integration"` + `[Trait("Category","Integration")]` | Separate test projects | Trait-based filtering is .NET-native, no project restructuring needed; CI runs only unit tests, integration tests can be added later without workflow changes |
| Release atomicity | Script-level gate: build+hash+release+manifest in one script with early-exit on failure | Step-level `if: success()` | A single script controls the entire release transaction — if any step fails, nothing is committed or pushed; cleaner than relying on YAML `if` chains |
| Version bump | Extracted `bump-version.ps1` returning JSON output | Inline XML manipulation | Reusable locally; can be tested independently; same logic, just extracted |
| Loop prevention | `[skip ci]` in commit message + `github.actor != 'github-actions[bot]'` | `paths-ignore` on props/json | Belt-and-suspenders; `paths-ignore` can't distinguish bot vs human commits |
| Contract tests | Update existing `GitHubActionsWorkflowTests.cs` to match new structure | Delete tests | Tests catch workflow regressions; update assertions to match new step names |
| Inno Setup caching | `actions/cache` for choco Inno Setup install | Install every run | Saves ~45s per release run; cache key on choco package version |

## Data Flow

```
Push to main (non-bot, no [skip ci])
  │
  ├─ release.yml ──→ bump-version.ps1 ──→ new version string
  │                                           │
  │         dotnet restore ◄──────────────────┘
  │         dotnet build
  │         dotnet test --filter "Category!=Integration"
  │                  │
  │         build-release.ps1 ──→ build.ps1 ──→ installer .exe
  │                  │
  │         publish-release.ps1:
  │           1. Compute SHA-256
  │           2. gh release create + upload
  │           3. Update latest.json
  │           4. git add props + latest.json
  │           5. git commit [skip ci] [skip release]
  │           6. git push
  │
  └─ (nothing pushed to repo until step 6 succeeds)
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `.github/workflows/ci.yml` | Replace | Simplified: checkout, setup, restore, build, test (with trait filter) |
| `.github/workflows/release.yml` | Replace | Orchestration-only YAML calling extracted scripts |
| `.github/scripts/bump-version.ps1` | Create | Reads `Directory.Build.props`, bumps patch, writes back, outputs version+tag |
| `.github/scripts/publish-release.ps1` | Create | Atomic: compute hash, create GH release, update `latest.json`, commit+push metadata |
| `app/tests/.../GitHubActionsWorkflowTests.cs` | Modify | Update assertions to match new step names and script references |

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Contract | Workflow YAML structure, step ordering, script references | `GitHubActionsWorkflowTests.cs` — string assertions on YAML content |
| Script unit | `bump-version.ps1` output format | Pester test or manual `pwsh -File` validation |
| Smoke | Full release pipeline | First real push to `main` after implementation; verify release + `latest.json` update |

## Migration / Rollout

Single PR replacing both workflows + adding scripts. No phased rollout needed — workflows only run on their respective triggers. First merge to `main` is the live smoke test.

## Open Questions

- [ ] Should we add Pester tests for the `.ps1` scripts, or are the C# contract tests + manual validation sufficient for V1?
