# Delta for main-dashboard

## MODIFIED Requirements

### Requirement: Configuration Panel

The dashboard MUST remain focused on monitoring and runtime control only. Editable settings SHALL be managed on a dedicated Settings page, and the dashboard MUST NOT embed mode, input type, interval, or startup-option forms.
(Previously: The dashboard allowed users to edit input type and interval directly in the main UI.)

#### Scenario: Dashboard excludes settings forms

- GIVEN the user opens the dashboard
- WHEN the page renders
- THEN monitoring data and start or stop controls are visible
- AND editable mode, input type, interval, and startup-option controls are not shown there

## REMOVED Requirements

### Requirement: Mode Selection

(Reason: mode editing moves to the dedicated Settings page.)

### Requirement: Stealth Options

(Reason: startup and tray preferences belong to Settings, not the simplified dashboard.)
