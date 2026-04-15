# Installer Distribution Specification

## Requirements

### Requirement: Silent LocalAppData Installer Flow

The distribution pipeline MUST produce a setup executable that installs to `%LocalAppData%\MGG Pulse` by default and MUST support silent execution for updater-driven installs.

#### Scenario: Interactive install uses LocalAppData target

- GIVEN a user launches the setup executable manually
- WHEN the installer uses default settings
- THEN the app is installed under `%LocalAppData%\MGG Pulse`
- AND the install does not require elevation for that default path

#### Scenario: Updater launches silent install

- GIVEN an update package is ready
- WHEN the app starts the installer with silent arguments
- THEN the installer runs without manual prompts
- AND the app exits so the upgrade can complete
