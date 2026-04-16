# Appearance Theme Switching Specification

## Purpose

Define appearance switching with save-and-restart semantics and theme-resource-based theming for Material Pulse.

## Requirements

### Requirement: Appearance selection with restart-to-apply

The system MUST expose Dark, Light, and Auto appearance options. Selecting an option SHALL persist the choice immediately but the visual change SHALL take effect on the next application restart. The UI MUST show a clear message indicating a restart is needed and MUST offer a `Reiniciar` action button that restarts the application.

#### Scenario: User switches appearance

- GIVEN the shell is open on the Appearance page
- WHEN the user selects a different appearance option
- THEN the choice is persisted immediately
- AND an informational message is shown: restart required to apply
- AND a `Reiniciar` button is displayed to restart the app

#### Scenario: User triggers restart from appearance UI

- GIVEN the user changed appearance and the restart message is visible
- WHEN the user clicks `Reiniciar`
- THEN the application restarts
- AND the new appearance is applied from startup

#### Scenario: Only approved appearance options are offered

- GIVEN the user opens the Appearance UI
- WHEN the options are shown
- THEN only Dark (`Oscuro`), Light (`Claro`), and Auto (`Automático`) are selectable

### Requirement: Theme resources and approved palette

The system MUST use `ThemeResource` bindings for theme-sensitive surfaces so the correct palette loads at startup. The Light theme MUST provide a complete light palette for body, sidebar, cards, dialogs, borders, and text. Both themes MUST keep Green 500 (`#4CAF50`) as the primary accent.

#### Scenario: Full light palette resolves for themed surfaces

- GIVEN Light appearance is active
- WHEN shared theme resources are resolved
- THEN body, sidebar, cards, dialogs, borders, and text use the light palette
- AND the primary accent remains Green 500
