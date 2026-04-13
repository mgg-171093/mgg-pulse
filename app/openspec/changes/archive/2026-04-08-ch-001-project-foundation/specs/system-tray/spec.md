# System Tray Specification

## Purpose

Define el comportamiento del ícono en la bandeja del sistema que permite operar la app desde background.

## Requirements

### Requirement: Tray Icon Always Visible When Running

El sistema MUST mostrar un ícono en la bandeja del sistema mientras la aplicación esté en ejecución, independientemente del estado de la ventana principal.

#### Scenario: App minimized to tray

- GIVEN la opción "Minimize to tray" está habilitada
- WHEN el usuario minimiza la ventana principal
- THEN la ventana desaparece de la taskbar
- AND el ícono permanece visible en la bandeja del sistema

#### Scenario: App started minimized

- GIVEN la opción "Start minimized" está habilitada
- WHEN la aplicación inicia
- THEN la ventana principal NO se muestra
- AND el ícono del tray está visible inmediatamente

### Requirement: Tray Context Menu

El sistema MUST mostrar un menú contextual al hacer click derecho sobre el ícono del tray con las opciones: Show/Hide, Start/Stop Simulation, y Exit.

#### Scenario: Show window from tray

- GIVEN la ventana principal está oculta
- WHEN el usuario hace click derecho → "Show"
- THEN la ventana principal se muestra y recibe el foco

#### Scenario: Start simulation from tray

- GIVEN la simulación está detenida
- WHEN el usuario hace click derecho → "Start"
- THEN la simulación inicia como si se hubiera presionado el botón en la UI

#### Scenario: Exit from tray

- GIVEN la app está corriendo
- WHEN el usuario hace click derecho → "Exit"
- THEN la simulación se detiene, el ícono se remueve y la app cierra

### Requirement: Tray Tooltip Reflects State

El sistema MUST actualizar el tooltip del ícono del tray para reflejar el estado actual (Active / Inactive).

#### Scenario: Tooltip when active

- GIVEN la simulación está corriendo
- WHEN el usuario pasa el mouse sobre el ícono del tray
- THEN el tooltip muestra "MGG Pulse — Active"

#### Scenario: Tooltip when inactive

- GIVEN la simulación está detenida
- WHEN el usuario pasa el mouse sobre el ícono del tray
- THEN el tooltip muestra "MGG Pulse — Inactive"
