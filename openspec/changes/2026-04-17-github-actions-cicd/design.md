# Design: GitHub Actions CI/CD

## Technical Approach

Two workflow files: `ci.yml` (develop push/PR — build + test) and `release.yml` (main push — build + package + release + update manifest). Both run on `windows-latest`. Version is the single source of truth in `Directory.Build.props`. The release workflow bumps patch, commits metadata, creates a GitHub Release, and updates `latest.json` on main — all automated with loop prevention via `[skip ci]`.

## Architecture Decisions

| Decision | Choice | Alternatives | Rationale |
|----------|--------|-------------|-----------|
| Workflow split | Two files: `ci.yml` + `release.yml` | Single file with conditionals | Cleaner triggers, easier to reason about, independent permissions |
| Runner OS | `windows-latest` | Ubuntu + cross-compile | WinUI 3 / Inno Setup require native Windows; no cross-compile path |
| .NET setup | `actions/setup-dotnet@v4` with `net8.0-windows10.0.19041.0` | Pre-installed SDK | Explicit version pinning; WinUI workload needs `dotnet workload install` |
| Inno Setup install | Chocolatey `choco install innosetup -y` | Manual download script | One-liner, cached by GHA, reliable |
| Semver bump | PowerShell script reads `Directory.Build.props`, bumps patch, writes back | Tag-based version, GitVersion | `Directory.Build.props` is already the source of truth; no new tooling |
| Loop prevention | Commit message includes `[skip ci]` + use `github.actor != 'github-actions[bot]'` condition | `paths-ignore`, separate bot token | Belt-and-suspenders; `[skip ci]` is the primary guard |
| Release creation | `softprops/action-gh-release@v2` | `gh release create` in script | Declarative, handles asset upload, widely used |
| latest.json update | Same release workflow: compute sha256, update JSON, commit to main with `[skip ci]` | Separate workflow triggered by release event | Simpler; single atomic pipeline |
| actionlint | Optional — `pre-commit` hook or manual, NOT in CI | CI step | Linting YAML in CI adds latency for minimal value on a solo project |
| Permissions | `contents: write` on release workflow only; CI is read-only | Broad permissions | Principle of least privilege |

## Data Flow

```
[develop push/PR] ──► ci.yml
                        ├── Setup .NET 8 + WinUI workload
                        ├── dotnet restore
                        ├── dotnet build --configuration Release
                        └── dotnet test (MGG.Pulse.Tests.Unit)

[main push] ──► release.yml  (guard: actor != bot, no [skip ci])
                 ├── Setup .NET 8 + WinUI workload
                 ├── Install Inno Setup (choco)
                 ├── Read version from Directory.Build.props
                 ├── Bump patch → write back to Directory.Build.props
                 ├── dotnet publish (win-x64, Release)
                 ├── ISCC → MGGPulse-Setup-{version}.exe
                 ├── Compute SHA-256 of installer
                 ├── Update app/build/latest.json (version, url, sha256)
                 ├── git commit Directory.Build.props + latest.json [skip ci]
                 ├── git push to main
                 ├── Create GitHub Release v{version}
                 └── Upload installer .exe as release asset
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `.github/workflows/ci.yml` | Create | CI workflow: build + test on develop push/PR |
| `.github/workflows/release.yml` | Create | Release workflow: build + package + release + manifest on main push |
| `app/build/build.ps1` | Modify | Refactor version-read logic into reusable function (optional, for CI reuse) |
| `ARCHITECTURE.md` | Modify | Add CI/CD section documenting the two-workflow model |

## Interfaces / Contracts

No new code interfaces. Workflow contracts:

**ci.yml triggers**: `push: branches: [develop]`, `pull_request: branches: [develop]`

**release.yml triggers**: `push: branches: [main]` with condition `github.actor != 'github-actions[bot]'`

**latest.json schema** (unchanged):
```json
{ "version": "x.y.z", "url": "https://...exe", "sha256": "hex", "notes": "..." }
```

**Version source of truth**: `app/Directory.Build.props` `<Version>` element — bumped by release workflow, never manually for releases.

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| CI | Build compiles, unit tests pass | `dotnet test` in ci.yml |
| Release | Installer produced, release created | Manual verification of first run; workflow logs |
| Workflow syntax | YAML validity | Optional local `actionlint` pre-commit |

## Migration / Rollout

No migration required. Workflows are additive — existing manual `build.ps1` continues to work locally. First release after merge will be the validation run.

## Open Questions

- [ ] Release notes: auto-generate from `[Unreleased]` section of CHANGELOG.md, or manual input? (Design assumes manual `notes` field in latest.json for now)
- [ ] Should the patch bump also update CHANGELOG.md `[Unreleased]` → `[x.y.z]` automatically? (Deferred — can add later)
