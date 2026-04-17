# Proposal: Updater Relaunch Hardening

## Intent

Three regressions make the auto-update flow unreliable in production:

1. Startup update-check fires once with a swallowed exception — transient network blips silently skip updates.
2. Inno Setup silent install uses `skipifsilent`, so post-install relaunch never happens.
3. App exits after splash when started minimized because `MainWindow` is created but never activated.

This patch fixes all three so the full "check → download → install → relaunch → start minimized" flow works end-to-end.

## Scope

### In Scope
- Add retry logic (with backoff) to `UpdateHostedService` startup check
- Remove `skipifsilent` from `pulse.iss` relaunch entry (or switch to unconditional `RunOnceId`)
- Ensure `MainWindow` is activated before the app reaches idle when `--minimized` is passed
- Regression test coverage for the relaunch and minimized-start paths

### Out of Scope
- Background periodic update checks (separate feature)
- Update UI / progress dialog changes
- Channel switching (stable/beta)

## Capabilities

### New Capabilities
- None

### Modified Capabilities
- `startup-update-check`: retry behavior added to the single-shot startup detection
- `post-install-relaunch`: installer relaunch entry must fire in silent mode
- `start-minimized`: window activation must survive the splash → main handoff

## Approach

**UpdateHostedService**: wrap the single `CheckForUpdate` call in a `Polly` retry policy (3 attempts, exponential backoff 2 s / 4 s / 8 s). Log each attempt and surface the final exception as a warning, not a silent swallow.

**pulse.iss**: change the `[Run]` relaunch entry from `Flags: skipifsilent nowait postinstall` to `Flags: nowait postinstall` so the app always relaunches after a silent install.

**App.xaml.cs**: call `MainWindow.Activate()` unconditionally during startup, then immediately minimize via `AppWindow.Hide()` / `Presenter` when the `--minimized` flag is present. This keeps the app process alive.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `app/src/MGG.Pulse.Infrastructure/Update/UpdateHostedService.cs` | Modified | Add Polly retry around startup check |
| `app/build/pulse.iss` | Modified | Remove `skipifsilent` from relaunch `[Run]` entry |
| `app/src/MGG.Pulse.UI/App.xaml.cs` | Modified | Activate window before conditional minimize |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Polly dependency not yet in Infrastructure project | Low | Already used elsewhere; verify csproj reference |
| Removing `skipifsilent` triggers visible window on silent install | Low | `nowait` + relaunch starts app normally with `--minimized` |
| `Activate()` + immediate hide causes flicker | Med | Use `OverlappedPresenter` in minimized state before `Activate()` |

## Rollback Plan

Revert the three file changes independently — each is self-contained. `pulse.iss` change is the most impactful; can be re-added via a hotfix installer build without a code change.

## Dependencies

- Polly v8 (confirm reference in `MGG.Pulse.Infrastructure.csproj`)

## Success Criteria

- [ ] App detects a new version on startup even when the first network call fails (verified in dev with a simulated 500)
- [ ] Silent install via `/VERYSILENT` relaunches the app automatically
- [ ] App started with `--minimized` is visible in the system tray and does not exit after splash
