# Delta for Auto Updater

## MODIFIED Requirements

### Requirement: latest.json Contract and Polling

The system MUST consume a `latest.json` manifest containing at least `version`, `url`, and `sha256`. It MUST compare the installed version against that manifest at startup and every 4 hours thereafter. The startup check MUST retry transient fetch failures a bounded number of times before giving up, and SHALL keep the normal periodic cadence even when startup retries are exhausted.

(Previously: The system compared the installed version against the manifest at startup and every 4 hours thereafter without specifying startup retry behavior.)

#### Scenario: Startup retry finds a newer release

- GIVEN startup reaches the deferred update check
- AND the first manifest fetch attempt fails transiently
- WHEN a retry succeeds with a newer manifest version
- THEN the app marks an update as available without blocking shell startup

#### Scenario: Startup retries are exhausted

- GIVEN startup manifest fetches keep failing transiently
- WHEN the retry budget is exhausted
- THEN shell and tray startup continue without installer launch
- AND the next scheduled 4-hour check remains active

#### Scenario: Periodic check handles no-update or invalid manifest

- GIVEN the periodic 4-hour timer elapses
- WHEN the manifest is unchanged, older, or malformed
- THEN the installed version remains in use
- AND no installer launch occurs
