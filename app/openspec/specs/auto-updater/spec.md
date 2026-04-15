# Auto Updater Specification

## Requirements

### Requirement: latest.json Contract and Polling

The system MUST consume a `latest.json` manifest containing at least `version`, `url`, and `sha256`. It MUST compare the installed version against that manifest at startup and every 4 hours thereafter.

#### Scenario: Startup check finds a newer release

- GIVEN startup can reach `latest.json`
- WHEN the manifest version is newer than the installed version
- THEN the app marks an update as available
- AND the user can proceed through the update flow without blocking shell startup

#### Scenario: Periodic check handles no-update or invalid manifest

- GIVEN the periodic 4-hour timer elapses
- WHEN the manifest is unchanged, older, or malformed
- THEN the installed version remains in use
- AND no installer launch occurs
