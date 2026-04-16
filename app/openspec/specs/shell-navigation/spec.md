# Shell Navigation Specification

## Requirements

### Requirement: Lateral Shell Navigation

The system MUST present a left-side panel with Dashboard, Configuración, About, and an Appearance entry or menu for theme switching. Dashboard SHALL contain only runtime status, start/stop, idle metrics, next action, and recent activity; settings and about/version actions MUST live on separate pages. The shell MUST keep only the built-in/localized Configuración entry and MUST NOT show duplicate settings destinations. The sidebar MUST remain visually distinct from the page body, and navigation items MUST expose clear hover, selected, and keyboard focus states. Clickable navigation affordances SHALL use a pointer/hand cursor and SHOULD provide subtle motion or state feedback without distracting from content. User-facing navigation labels SHOULD be Spanish for this redesign closure.

#### Scenario: Navigate through the shell

- GIVEN the shell is open
- WHEN the user selects Dashboard, Configuración, or About from the left panel
- THEN the selected page is shown inside the same app shell
- AND Appearance remains available as a separate shell action

#### Scenario: Duplicated settings entry is removed

- GIVEN the shell navigation is rendered
- WHEN the user reviews the available destinations
- THEN only one localized Configuración entry is shown
- AND no redundant Settings destination is present

#### Scenario: Navigation states communicate interactivity

- GIVEN the user hovers, focuses, or selects a navigation item
- WHEN the navigation state changes
- THEN the sidebar remains visually separated from the content area
- AND the item shows distinct state treatment with pointer cursor and non-distracting feedback

### Requirement: Shell window icon visibility

The system MUST show the provided application icon in the window chrome and taskbar when a valid `.ico` asset is correctly wired.

#### Scenario: Valid wired icon is visible in shell surfaces

- GIVEN a valid `.ico` asset is provided and correctly wired to the application window
- WHEN the shell window opens
- THEN the window chrome shows the application icon
- AND the taskbar shows the same application icon
