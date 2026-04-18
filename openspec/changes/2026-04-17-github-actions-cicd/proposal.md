# Proposal: GitHub Actions CI/CD

## Intent

MGG Pulse currently requires a fully manual release workflow: the developer runs `build.ps1` locally, generates the installer, computes SHA-256, updates `latest.json`, commits to main, and creates a GitHub Release by hand. There is also no automated gate on pull requests or the `develop` branch — broken builds can be merged silently. This change introduces two GitHub Actions workflows: one for continuous integration (PR + develop) and one for automated release publishing triggered on pushes to main.

## Scope

### In Scope
- CI workflow: builds and tests on every PR and every push to `develop`
- Release workflow: builds installer + computes SHA-256 + creates GitHub Release + updates `latest.json` on every push to `main`
- Preserve the `latest.json`-on-main model (auto-updater fetches raw from main)
- Automate current manual close-and-publish steps (no more hand-crafted SHA, no manual release draft)

### Out of Scope
- Code signing / MSIX packaging
- Publishing to Windows Store
- Auto-bump of version (version remains driven by `Directory.Build.props`)
- Matrix builds / multi-runtime targets beyond win-x64
- Deployment environments or staging gates

## Capabilities

### New Capabilities
- `github-actions-ci`: PR and develop-branch validation workflow (build + test)
- `github-actions-release`: Main-branch publish workflow (installer build, SHA-256, GitHub Release, latest.json commit)

### Modified Capabilities
- None

## Approach

Two workflow files under `.github/workflows/`:

1. **`ci.yml`** — triggers on `pull_request` (any branch → any branch) and `push` to `develop`.  
   Steps: `dotnet restore` → `dotnet build` → `dotnet test`. Uses `windows-latest` runner (required for WinUI 3 / Win32 APIs). No artifact upload needed.

2. **`release.yml`** — triggers on `push` to `main`.  
   Steps: read version from `Directory.Build.props` → `dotnet publish` → run Inno Setup via `choco install innosetup` → compute SHA-256 → create GitHub Release with `gh release create` → update `build/latest.json` → commit + push `latest.json` back to main (bot commit, skips CI with `[skip ci]`).

Runner: `windows-latest` (required; no Linux runner can build WinUI 3 / call win-x64 native toolchain).  
Secrets needed: `GITHUB_TOKEN` (built-in, already available).

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `.github/workflows/ci.yml` | New | PR + develop CI |
| `.github/workflows/release.yml` | New | Main-branch publish pipeline |
| `app/build/latest.json` | Modified (automated) | Written by release workflow on every main push |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Inno Setup not available on `windows-latest` | Low | Install via Chocolatey in workflow |
| `latest.json` bot commit triggers release workflow loop | Med | Add `[skip ci]` to commit message; release workflow filters by commit author or tag |
| WinUI 3 requires specific Windows SDK version | Low | Pin `windows-latest`; add `windowsSdkPackageVersion` to `dotnet publish` args if needed |
| SHA-256 mismatch if artifact re-built | Low | SHA computed from the exact artifact uploaded to the release |

## Rollback Plan

Delete or disable `.github/workflows/release.yml`. The `latest.json` is in version control — revert the last bot commit to restore the previous release pointer. CI workflow can be disabled without affecting main.

## Dependencies

- GitHub Actions `windows-latest` runner
- Chocolatey available on runner (for Inno Setup install)
- `GITHUB_TOKEN` with `contents: write` permission (default for public repos)

## Success Criteria

- [ ] A PR to `develop` triggers CI and fails if `dotnet build` or `dotnet test` fails
- [ ] A push to `main` automatically creates a GitHub Release named `v{version}`
- [ ] `build/latest.json` on `main` is updated by the pipeline with correct SHA-256 and URL
- [ ] The auto-updater in a running MGG Pulse instance detects the new release via the updated `latest.json`
- [ ] No manual steps remain in the release process beyond bumping `<Version>` in `Directory.Build.props` and merging to `main`
