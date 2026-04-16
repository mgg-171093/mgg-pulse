# Delta for Shell Navigation

## MODIFIED Requirements

### Requirement: Lateral Shell Navigation

The system MUST present a left-side panel with Dashboard, Configuración, About, and an Appearance entry or menu for theme switching. Dashboard SHALL contain only runtime status, start/stop, idle metrics, next action, and recent activity; settings and about/version actions MUST live on separate pages. The shell MUST keep only the built-in/localized Configuración entry and MUST NOT show duplicate settings destinations. The sidebar MUST also expose a clear **Salir** action below Configuración. That action MUST use Spanish labeling, MUST behave as an application command instead of a page destination, and MUST preserve coherent shell selection until termination completes. The sidebar MUST remain visually distinct from the page body, and navigation items MUST expose clear hover, selected, and keyboard focus states. Clickable navigation affordances SHALL use a pointer/hand cursor and SHOULD provide subtle motion or state feedback without distracting from content. User-facing navigation labels SHOULD be Spanish for this redesign closure.

(Previously: The shell did not require a dedicated sidebar exit action or define how a non-navigation exit command should preserve coherent shell state.)

#### Scenario: Navigate through the shell

- GIVEN the shell is open
- WHEN the user selects Dashboard, Configuración, or About from the left panel
- THEN the selected page is shown inside the same app shell
- AND Appearance and Salir remain available as separate shell actions

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

#### Scenario: Sidebar exit action stays outside page navigation

- GIVEN any shell page is currently selected
- WHEN the user activates Salir from the sidebar
- THEN the shell does not navigate to a new content page
- AND the current navigation selection remains coherent until the app closes
