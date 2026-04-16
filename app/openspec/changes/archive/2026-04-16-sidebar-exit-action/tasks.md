# Tasks: Sidebar Exit Action

## Phase 1: UI Foundation (Sidebar Action)

- [x] 1.1 Modificar `app/src/MGG.Pulse.UI/Views/ShellPage.xaml` para agregar `NavigationView.FooterMenuItems` con `NavigationViewItem` **Salir** (`Tag="Exit"`, ícono de salida, copy en español).
- [x] 1.2 En `app/src/MGG.Pulse.UI/Views/ShellPage.xaml.cs`, registrar el item de footer para que use los mismos handlers de hover/focus/puntero que los ítems existentes y Configuración.

## Phase 2: Exit Wiring (UI Layer)

- [x] 2.1 Modificar `app/src/MGG.Pulse.UI/App.xaml.cs` para exponer `internal static void RequestExit()` que delegue en la instancia actual y reutilice `ExitApp()` sin duplicar lógica.
- [x] 2.2 Modificar `app/src/MGG.Pulse.UI/Views/ShellPage.xaml.cs` para interceptar `Tag == "Exit"` en `NavView_SelectionChanged`, cancelar navegación de contenido y llamar `App.RequestExit()`.
- [x] 2.3 En el mismo handler, preservar/restaurar la selección previa coherente del shell antes de cerrar (sin navegar a otra página), cumpliendo el escenario de “Salir fuera de navegación”.

## Phase 3: Verification (Spec Scenarios)

- [ ] 3.1 Verificación manual de `specs/system-tray/spec.md` (Scenario: **Sidebar exit reuses tray termination**): Salir ejecuta cierre completo equivalente a Exit del tray (sin residuo en tray).
- [ ] 3.2 Verificación manual de `specs/shell-navigation/spec.md` (Scenario: **Sidebar exit action stays outside page navigation**): no hay navegación intermedia y la selección se mantiene coherente hasta terminar.
- [ ] 3.3 Verificación manual visual en tema claro/oscuro: etiqueta **Salir** en español, cursor mano, estados hover/focus consistentes con el resto del sidebar.

## Phase 4: Regression & Closure

- [ ] 4.1 Ejecutar smoke manual del menú contextual del tray (`Show/Hide`, `Start/Stop`, `Exit`) para confirmar que el nuevo wiring no rompe comportamientos existentes.
- [ ] 4.2 Documentar en el PR/verify report que no se agregan unit tests por alcance WinUI-only (sin harness UI en `MGG.Pulse.Tests.Unit`) y dejar evidencia de checks manuales.
