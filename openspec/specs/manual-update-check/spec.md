# Manual Update Check Specification

## Purpose

Definir cómo el flujo manual de `Acerca de` expone y aplica actualizaciones detectadas.

## Requirements

### Requirement: Manual Update Apply Entry Point

The system MUST provide a user-initiated way to start installation from `Acerca de` after an applicable update is detected.

#### Scenario: Offer apply action after manual detection

- GIVEN the user runs a manual update check from `Acerca de`
- WHEN an applicable update is detected
- THEN the UI MUST indicate that an update is available
- AND the UI MUST provide a control to start installation from that screen

#### Scenario: No apply action without applicable update

- GIVEN the user runs a manual update check from `Acerca de`
- WHEN no applicable update is detected
- THEN the UI MUST NOT offer the install action

### Requirement: Shared Update Apply Path

The system MUST route manual installs and startup-confirmed installs through the same update-application path used by the existing updater/install flow.

#### Scenario: Manual apply reuses existing install flow

- GIVEN `Acerca de` has detected an applicable update
- WHEN the user starts installation from that screen
- THEN the app MUST invoke the same update-application path as the existing updater/install flow
- AND a successful handoff MUST follow the same post-launch exit behavior as that flow
