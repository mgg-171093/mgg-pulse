# Delta for splash-screen

## ADDED Requirements

### Requirement: Version Overlay

The splash screen MUST display the current application version during startup.

#### Scenario: Version is visible on splash

- GIVEN the application is starting
- WHEN the splash window is shown
- THEN the current installed version is visible on the splash surface

## MODIFIED Requirements

### Requirement: Transition to Main State

El sistema MUST cerrar el SplashWindow y transicionar al estado correcto cuando la inicialización termina, pero SHALL mantener el splash visible por un mínimo de 5 segundos desde que aparece.
(Previously: The splash closed as soon as initialization completed.)

#### Scenario: Transition to MainWindow (normal start)

- GIVEN "Start minimized" NO está habilitado
- WHEN la inicialización completa y el mínimo de 5 segundos ya transcurrió
- THEN el SplashWindow se cierra
- AND el MainWindow se muestra

#### Scenario: Transition to tray (start minimized)

- GIVEN "Start minimized" está habilitado
- WHEN la inicialización completa y el mínimo de 5 segundos ya transcurrió
- THEN el SplashWindow se cierra
- AND la app queda en background con solo el ícono en el tray visible

#### Scenario: Fast initialization waits for minimum duration

- GIVEN la inicialización termina antes de 5 segundos
- WHEN el splash sigue visible
- THEN la transición principal espera hasta completar el mínimo de 5 segundos
