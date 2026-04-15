# Shell Navigation Specification

## Requirements

### Requirement: Lateral Shell Navigation

The system MUST present a left-side panel with Dashboard, Settings, and About. Dashboard SHALL contain only runtime status, start/stop, idle metrics, next action, and recent activity; settings and about/version actions MUST live on separate pages.

#### Scenario: Navigate through the shell

- GIVEN the shell is open
- WHEN the user selects Dashboard, Settings, or About from the left panel
- THEN the selected page is shown inside the same app shell
- AND the other sections remain available as separate entries
