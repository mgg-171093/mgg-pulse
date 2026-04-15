# About and Versioning Specification

## Requirements

### Requirement: About Version Surface

The system MUST provide an About page that shows the installed application version and a manual Check for Updates action.

#### Scenario: About page shows current version

- GIVEN the user opens About
- WHEN the page loads
- THEN the installed version is displayed in a visible version field
- AND the Check for Updates button is available on the same page

#### Scenario: Manual check reuses updater flow

- GIVEN the user is on About
- WHEN the user selects Check for Updates
- THEN the app performs the same version check used at startup and on the 4-hour cadence
- AND the result is shown without restarting the app
