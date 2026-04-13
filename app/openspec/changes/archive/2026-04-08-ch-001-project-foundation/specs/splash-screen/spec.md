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

### Requirement: Transition to Main State

El sistema MUST cerrar el SplashWindow y transicionar al estado correcto cuando la inicialización termina.

#### Scenario: Transition to MainWindow (normal start)

- GIVEN "Start minimized" NO está habilitado
- WHEN la inicialización completa (progress = 100%)
- THEN el SplashWindow se cierra
- AND el MainWindow se muestra

#### Scenario: Transition to tray (start minimized)

- GIVEN "Start minimized" está habilitado
- WHEN la inicialización completa
- THEN el SplashWindow se cierra
- AND la app queda en background con solo el ícono en el tray visible

### Requirement: Centered on Primary Display

El sistema MUST mostrar el SplashWindow centrado en la pantalla primaria.

#### Scenario: Splash centered

- GIVEN cualquier resolución de pantalla
- WHEN el SplashWindow se muestra
- THEN está centrado horizontal y verticalmente en el área de trabajo de la pantalla primaria
