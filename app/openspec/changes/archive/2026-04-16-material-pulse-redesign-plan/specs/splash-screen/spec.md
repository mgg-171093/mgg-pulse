# Delta for Splash Screen

## ADDED Requirements

### Requirement: Launch appearance synchronization

The splash screen MUST resolve the persisted appearance before first render. If the persisted appearance is Auto, the splash SHALL use the current system theme. The initial shell appearance MUST match the splash without an intermediate flash of a different theme.

#### Scenario: Persisted explicit appearance is applied before splash render

- GIVEN the user previously saved Dark or Light
- WHEN the application launches
- THEN the splash is first shown with that saved appearance
- AND the initial shell uses the same appearance

#### Scenario: Auto appearance follows the system on launch

- GIVEN the user previously saved Auto
- AND the operating system currently prefers a specific theme
- WHEN the application launches
- THEN the splash resolves that system theme before first render
- AND the initial shell matches the same resolved theme
