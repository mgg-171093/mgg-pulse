# appearance-theme-toggle Specification

## Purpose

Define user-controlled Light/Dark theme selection for the application shell.

## Requirements

### Requirement: Runtime theme selection

The system MUST provide an Appearance page where the user can choose Light or Dark theme, and MUST apply the selected theme during the current session without requiring an application restart.

#### Scenario: User changes theme from the Appearance page

- GIVEN the user is on the Appearance page
- WHEN the user selects the other supported theme
- THEN the application theme changes during the current session
- AND the shell reflects the new theme without restart

#### Scenario: User reselects the active theme

- GIVEN one supported theme is already active
- WHEN the user selects that same theme again
- THEN the application remains stable and keeps the current theme applied

### Requirement: Persisted theme preference

The system MUST persist the user theme preference in local application settings and SHALL restore that preference on the next launch. If no valid preference exists, the system SHALL use the application default theme.

#### Scenario: Stored preference is restored on next launch

- GIVEN the user previously selected a supported theme
- WHEN the application starts again
- THEN the previously selected theme is restored automatically

#### Scenario: Missing or invalid preference falls back safely

- GIVEN no valid saved theme preference exists
- WHEN the application starts
- THEN the application uses its default theme
