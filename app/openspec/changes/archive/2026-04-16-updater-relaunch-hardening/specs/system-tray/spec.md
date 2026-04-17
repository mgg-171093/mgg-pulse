# Delta for System Tray

## MODIFIED Requirements

### Requirement: Tray Icon Always Visible When Running

El sistema MUST mostrar un ícono en la bandeja del sistema mientras la aplicación esté en ejecución, independientemente del estado de la ventana principal. Cuando "Start minimized" está habilitado, el sistema MUST mantener viva la app después de cerrar el splash, SHALL conservar la ventana principal creada pero oculta al usuario, y MUST NOT finalizar el proceso por quedar sin una ventana visible.

(Previously: El sistema exigía que la ventana principal no se mostrara al iniciar con "Start minimized" y que el ícono del tray fuera visible, pero no explicitaba que la app debía seguir viva después del splash.)

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
