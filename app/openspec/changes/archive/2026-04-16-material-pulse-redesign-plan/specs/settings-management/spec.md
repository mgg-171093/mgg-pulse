# Delta for Settings Management

## MODIFIED Requirements

### Requirement: Dedicated Settings Surface

The system MUST expose simulation settings on a dedicated Configuración page. Changes SHALL persist through the existing configuration flow, and Dashboard MUST NOT duplicate those editing controls. User-facing page labels SHOULD be Spanish for this redesign closure.

(Previously: The system only required a dedicated Settings page and did not define localized page labeling.)

#### Scenario: User edits settings on the dedicated page

- GIVEN the user opens Configuración
- WHEN the user changes mode, input type, or interval and saves
- THEN the updated values are persisted
- AND subsequent simulation cycles use the saved values

## ADDED Requirements

### Requirement: Appearance preference persistence

The system MUST persist Dark, Light, or Auto through the existing configuration flow and SHALL restore the saved appearance on the next launch. If Auto is saved, startup SHALL resolve the current system theme before themed UI is shown.

#### Scenario: Saved explicit appearance is restored

- GIVEN the user previously saved Dark or Light
- WHEN the application starts again
- THEN the saved appearance is restored automatically

#### Scenario: Saved Auto appearance follows the system

- GIVEN the user previously saved Auto
- AND the operating system currently prefers a specific theme
- WHEN the application starts again
- THEN the application resolves and applies that system theme before themed UI is shown
