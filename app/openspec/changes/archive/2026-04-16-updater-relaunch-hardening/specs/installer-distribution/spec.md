# Delta for Installer Distribution

## MODIFIED Requirements

### Requirement: Silent LocalAppData Installer Flow

The distribution pipeline MUST produce a setup executable that installs to `%LocalAppData%\MGG Pulse` by default and MUST support silent execution for updater-driven installs. A successful updater-driven silent install MUST relaunch `MGG Pulse` automatically after the upgrade completes.

(Previously: The distribution pipeline required silent execution for updater-driven installs, but did not require automatic relaunch after success.)

#### Scenario: Interactive install uses LocalAppData target

- GIVEN a user launches the setup executable manually
- WHEN the installer uses default settings
- THEN the app is installed under `%LocalAppData%\MGG Pulse`
- AND the install does not require elevation for that default path

#### Scenario: Updater launches silent install

- GIVEN an update package is ready
- WHEN the app starts the installer with silent arguments for an updater-driven install
- THEN the installer runs without manual prompts
- AND `MGG Pulse` relaunches automatically after a successful install
