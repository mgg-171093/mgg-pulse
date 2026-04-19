## Exploration: 2026-04-18-cicd-workflow-rethink

### Current State
The existing CI/CD system attempts an automated "push-to-main" continuous delivery model that is fundamentally flawed for this desktop app. 
1. **Hanging Tests**: The `MGG.Pulse.Tests.Unit` project contains ViewModels and Services interacting with WinUI 3 constructs. When run via `dotnet test` on headless Windows runners, these tests hang indefinitely waiting for UI threads or Dispatcher instances that never initialize.
2. **Brittle Dependencies**: `release.yml` uses Chocolatey (`choco install innosetup`) which frequently fails with HTTP 503 due to community repository rate limits and outages.
3. **Repository Mutation**: `release.yml` attempts to automatically bump version numbers in `Directory.Build.props` and update `latest.json`, then commit and push them directly to `main`. This bypasses branch protections, causes race conditions with other PRs, and leaves the repo in an inconsistent state if the pipeline fails midway.
4. **Brittle Meta-tests**: The test suite includes `GitHubActionsWorkflowTests.cs` which literally asserts the raw text of the YAML files using `IndexOf` and `Contains`. Any refactor of the CI immediately breaks the tests.

### Affected Areas
- `.github/workflows/ci.yml` — Currently hangs on `dotnet test`.
- `.github/workflows/release.yml` — Flaky `choco` steps and brittle `git push origin main` logic.
- `app/tests/MGG.Pulse.Tests.Unit/Infrastructure/Update/GitHubActionsWorkflowTests.cs` — The root cause of CI changes "breaking" unit tests.
- `app/tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj` — Needs a test split strategy.
- `app/src/MGG.Pulse.Application/Updates/CheckForUpdateUseCase.cs` — Currently fetches `latest.json` from the repo's `raw-main` content, which necessitates the CI committing back to the repo.

### Approaches

1. **Tag-Driven Stateless Releases (Recommended)**
   - **Trigger**: Developer manually pushes a tag `v1.2.3` (or creates a GitHub Release draft via UI).
   - **Flow**: Action checks out code, extracts version from the tag, builds using `dotnet publish -p:Version=1.2.3` (overriding `Directory.Build.props` locally), installs Inno Setup via a reliable native GitHub Action (or `winget`), creates the `.exe`, and uploads it alongside a dynamically generated `latest.json` to the GitHub Release Assets.
   - **Pros**: Clean repo history (no `[skip ci]` commits), completely stateless CI, no branch protection issues, reliable versioning.
   - **Cons**: Requires `CheckForUpdateUseCase` to pull `latest.json` from GitHub Release Assets (or the GitHub API) instead of the raw `main` branch.
   - **Effort**: Medium

2. **Fix-in-Place (Patching the existing flow)**
   - **Flow**: Keep the `push-to-main` model. Change `choco` to `winget`. Exclude UI tests. Keep `git push origin main`.
   - **Pros**: Requires zero changes to the app's `CheckForUpdateUseCase`.
   - **Cons**: Still mutates the repository, continues to be vulnerable to race conditions, requires Personal Access Tokens if branch protection is ever enabled.
   - **Effort**: Low

### Recommendation
**Replace entirely with Approach 1 (Tag-Driven Stateless Releases).**
The CI pipeline should absolutely not be committing `Directory.Build.props` and `latest.json` back to `main`. The `GitHubActionsWorkflowTests.cs` file should be permanently deleted because it tests configuration, not behavior, creating massive friction for infrastructure changes.

**Test Strategy:**
UI-dependent tests should be categorized. We should add a `<Trait>` attribute (or move them to a separate `MGG.Pulse.Tests.UI` project) and update `ci.yml` to run `dotnet test --filter "Category!=UI"`. This completely eliminates the headless hanging issue.

### Triggers, Jobs, and Sequencing
**ci.yml**:
- `on: [pull_request]`
- `job: test` -> `dotnet build` -> `dotnet test --filter "Category!=UI"`

**release.yml**:
- `on: push: tags: ['v*.*.*']`
- `job: publish` -> Extract version from `$GITHUB_REF_NAME` -> `dotnet publish -p:Version=$VERSION` -> Build Installer -> Upload to GitHub Releases.

### Risks
- Migrating away from the "raw main" `latest.json` means updating the updater logic. Existing installed clients looking at `raw...main...latest.json` won't find the *new* updates if we stop pushing to main. To mitigate this, we can manually leave a final `latest.json` on `main` that points to a gateway, OR the CD can upload to BOTH the release assets and we accept the repo mutation just for the JSON file (but drop it for `.props`).
- Non-goal: Headless UI test execution. It is too flaky to be worth the effort for this project.

### Ready for Proposal
Yes. The orchestrator can proceed with proposing the rewrite of the workflows to a tag-driven approach and the removal of the brittle YAML-parsing tests.