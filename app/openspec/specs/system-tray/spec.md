# System Tray Specification

## Purpose

Define el comportamiento del ícono en la bandeja del sistema que permite operar la app desde background.

## Requirements

### Requirement: Tray Icon Always Visible When Running

El sistema MUST mostrar un ícono en la bandeja del sistema mientras la aplicación esté en ejecución, independientemente del estado de la ventana principal. Cuando "Start minimized" está habilitado, el sistema MUST mantener viva la app después de cerrar el splash, SHALL conservar la ventana principal creada pero oculta al usuario, y MUST NOT finalizar el proceso por quedar sin una ventana visible.

#### Scenario: App minimized to tray

- GIVEN la opción "Minimize to tray" está habilitada
- WHEN el usuario minimiza la ventana principal
- THEN la ventana desaparece de la taskbar
- AND el ícono permanece visible en la bandeja del sistema

#### Scenario: App started minimized

- GIVEN la opción "Start minimized" está habilitada
- AND el splash completó la inicialización
- WHEN la aplicación cierra el splash
- THEN la ventana principal permanece oculta
- AND el proceso sigue vivo con el ícono del tray visible

### Requirement: Tray Context Menu

El sistema MUST mostrar un menú contextual al hacer click derecho sobre el ícono del tray con las opciones: Show/Hide, Start/Stop Simulation, y Exit. El comando Exit MUST ejecutar el cierre completo de la aplicación: detener la simulación, remover el ícono del tray y finalizar el proceso. La acción **Salir** del sidebar MUST invocar ese mismo cierre completo y MUST NOT ocultar ni minimizar la app al tray.

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
- AND la app no queda oculta ni minimizada al tray

#### Scenario: Sidebar exit reuses tray termination

- GIVEN la app está corriendo desde cualquier página del shell
- WHEN el usuario activa "Salir" desde el sidebar
- THEN ocurre el mismo cierre completo que con "Exit" del tray
- AND la aplicación no navega a una nueva página antes de finalizar

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
