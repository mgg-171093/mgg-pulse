# Delta for About and Versioning

## MODIFIED Requirements

### Requirement: About Version Surface

The system MUST provide an About page that shows the installed application version and a manual Check for Updates action. That manual action MUST remain user-driven and SHALL report success or failure inline even when automatic startup retries and silent-install handoff are enabled elsewhere.

(Previously: The About page showed the installed version and a manual Check for Updates action, without guarding its behavior against new automatic updater flows.)

#### Scenario: About page shows current version

- GIVEN the user opens About
- WHEN the page loads
- THEN the installed version is displayed in a visible version field
- AND the Check for Updates button is available on the same page

#### Scenario: Manual check reuses updater flow

- GIVEN the user is on About
- WHEN the user selects Check for Updates
- THEN the app performs a one-shot version check using the same manifest rules as automatic checks
- AND the result is shown without restarting the app or forcing silent install
