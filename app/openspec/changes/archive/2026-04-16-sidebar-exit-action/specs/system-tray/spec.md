# Delta for System Tray

## MODIFIED Requirements

### Requirement: Tray Context Menu

El sistema MUST mostrar un menú contextual al hacer click derecho sobre el ícono del tray con las opciones: Show/Hide, Start/Stop Simulation, y Exit. El comando Exit MUST ejecutar el cierre completo de la aplicación: detener la simulación, remover el ícono del tray y finalizar el proceso. La acción **Salir** del sidebar MUST invocar ese mismo cierre completo y MUST NOT ocultar ni minimizar la app al tray.

(Previously: Exit solo estaba definido para el menú contextual del tray y no exigía compartir la misma semántica de cierre con una acción del sidebar.)

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
