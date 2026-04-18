## Exploration: 2026-04-17-github-actions-cicd

### Current State
MGG Pulse relies on a manual build process documented in `build.ps1`. The app expects `Directory.Build.props` as the canonical source for its version. The WinUI compilation requires the `win-x64` target on Windows and is packed using an Inno Setup script (`pulse.iss`). Currently, the auto-updater polls `latest.json` hosted raw on the `main` branch. There are no automated CI/CD pipelines in `.github/workflows/` at the moment.

### Affected Areas
- `.github/workflows/ci.yml` — New CI workflow for pull requests and pushes on `develop`.
- `.github/workflows/release.yml` — New release workflow triggered on pushes to `main`.
- `app/Directory.Build.props` — Will act as the single source of truth for version metadata updated by the CI.
- `app/build/latest.json` — Will be overwritten during the release pipeline with the compiled `.exe` hash and pushed back to the repo.

### Approaches
1. **Hybrid Version Bump Script + Inno Setup Compilation (Inspired by mgg-packify)** — Create PowerShell scripts that read the commit log to determine SemVer, inject it into `Directory.Build.props`, execute `build.ps1`, retrieve the SHA-256 of the generated `.exe`, modify `latest.json`, create a GitHub Release, and finally push the metadata updates to the repo with `[skip ci]`.
   - Pros: Fully automates versioning and updates; aligns exactly with the MGG Packify model; prevents manual tampering.
   - Cons: Relies heavily on exact conventional commit messages.
   - Effort: Medium

### Recommendation
Adopt **Approach 1**. Splitting the workflows cleanly between `ci.yml` (build/test validation) and `release.yml` (automation, bump, release, and raw manifest sync) ensures no unverified code enters the updater loop. The `release.yml` must explicitly compute the SHA-256 after the Windows `iscc` compilation step before mutating `latest.json`.

### Risks
- **Windows Runner Restrictions:** Both WinUI compilation (`dotnet publish -r win-x64`) and Inno Setup strictly require a Windows environment (`runs-on: windows-latest`).
- **Permissions:** Releasing and pushing changes demands `permissions: contents: write` configured correctly on the repository to prevent 403 blocks.
- **Auto-Updater Blindness:** If `latest.json` is formatted improperly or the raw GitHub URL fails, installed client apps will crash during update checks. 

### Ready for Proposal
Yes. The repository structure is ready for the CI/CD files and the process translates perfectly from `mgg-packify` to a .NET ecosystem.