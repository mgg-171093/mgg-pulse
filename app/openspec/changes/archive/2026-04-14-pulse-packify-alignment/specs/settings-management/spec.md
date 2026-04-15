# Settings Management Specification

## Requirements

### Requirement: Dedicated Settings Surface

The system MUST expose simulation settings on a dedicated Settings page. Changes SHALL persist through the existing configuration flow, and Dashboard MUST NOT duplicate those editing controls.

#### Scenario: User edits settings on the dedicated page

- GIVEN the user opens Settings
- WHEN the user changes mode, input type, or interval and saves
- THEN the updated values are persisted
- AND subsequent simulation cycles use the saved values
