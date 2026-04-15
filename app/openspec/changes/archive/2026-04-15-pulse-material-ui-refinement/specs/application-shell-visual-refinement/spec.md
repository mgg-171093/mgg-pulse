# application-shell-visual-refinement Specification

## Purpose

Define the branded shell, startup, and shared visual behavior for the refined MGG Pulse UI.

## Requirements

### Requirement: Branded startup and window chrome

The system MUST present a splash window of at least 600×750 with one prominent brand logo and no supporting text. The system MUST open the main window at at least 800×600 and SHALL show the branded application icon in window chrome, taskbar, and tray surfaces when the branding asset is available.

#### Scenario: Launch shows refined startup chrome

- GIVEN the application starts normally
- WHEN the splash and main window are created
- THEN the splash uses the minimum refined size with a large logo and no text
- AND the main window opens at or above the required minimum size

#### Scenario: Shell uses branded icon surfaces

- GIVEN the branded icon asset is available to the application
- WHEN the main window and tray surfaces are initialized
- THEN the branded icon is shown on the available shell surfaces

### Requirement: Shell navigation and status layout

The system MUST show a sidebar logo, a visible status bar, and one unique navigation entry for each destination exposed by the shell. The system SHALL expose Logs and Appearance as first-class destinations, and MUST NOT keep the operational log transcript inside Dashboard.

#### Scenario: Navigation is complete and non-duplicated

- GIVEN the user is on the shell
- WHEN navigation items are rendered
- THEN Appearance, Logs, Settings, and other existing destinations appear once each
- AND the status bar remains visible as shared shell chrome

#### Scenario: Dashboard remains focused on dashboard content

- GIVEN the user opens Dashboard after the refinement
- WHEN the page content is shown
- THEN operational logs are not displayed inside Dashboard

### Requirement: Material-aligned shared theme tokens

The system MUST provide shared theme resources that use a near-black dark surface, a light-gray light surface, a green primary accent, and 8px-equivalent corner radii across common shell surfaces.

#### Scenario: Shared tokens define the refined visual language

- GIVEN the application loads shared theme resources
- WHEN shell surfaces resolve colors and corner styling
- THEN the refined Material-aligned palette and corner radius values are available to those surfaces
