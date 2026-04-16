# Splash Screen Specification

## Purpose

Define el comportamiento de la ventana de inicio animada que se muestra mientras la app inicializa sus servicios.

## Requirements

### Requirement: Animated Logo Display

El sistema MUST mostrar una ventana de splash con el logo de la app animado con fade-in al iniciar.

#### Scenario: Logo fade-in on launch

- GIVEN la aplicación se está iniciando
- WHEN el SplashWindow se muestra
- THEN el logo aparece con una animación de fade-in (opacidad 0 → 1) en 800ms

#### Scenario: No title bar or chrome

- GIVEN el SplashWindow está visible
- THEN la ventana NO tiene título, botones de minimizar/maximizar/cerrar ni bordes del sistema

### Requirement: Progress Indication

El sistema MUST mostrar una barra de progreso que avanza mientras se inicializan los servicios en background.

#### Scenario: Progress advances during init

- GIVEN el SplashWindow está visible
- WHEN los servicios se inicializan secuencialmente (config → detector → tray)
- THEN la barra de progreso avanza de 0% a 100%

### Requirement: Version Overlay

The splash screen MUST display the current application version during startup.

#### Scenario: Version is visible on splash

- GIVEN the application is starting
- WHEN the splash window is shown
- THEN the current installed version is visible on the splash surface

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

### Requirement: Centered on Primary Display

El sistema MUST mostrar el SplashWindow centrado en la pantalla primaria.

#### Scenario: Splash centered

- GIVEN cualquier resolución de pantalla
- WHEN el SplashWindow se muestra
- THEN está centrado horizontal y verticalmente en el área de trabajo de la pantalla primaria

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
