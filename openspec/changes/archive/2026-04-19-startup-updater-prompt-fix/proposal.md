# Proposal: Startup Updater Prompt Fix

## Intent

El flujo de auto-actualización al inicio tiene dos problemas opuestos que se combinan para una mala UX:

1. **Startup path** (`OnUpdateAvailable` en `App.xaml.cs`): descarga e instala silenciosamente sin pedir permiso al usuario. Peligroso e intrusivo.
2. **Manual path** (`AboutViewModel`): detecta la actualización y muestra un mensaje, pero nunca ofrece instalarla. Incompleto.

La fix: ambos caminos deben converger en un único diálogo de confirmación (`ContentDialog`) que pregunte al usuario antes de aplicar la actualización.

## Scope

### In Scope
- Crear un diálogo de confirmación reutilizable (`UpdatePromptDialog` o via `ContentDialog` inline) en la capa UI
- Hacer que el handler de startup (`OnUpdateAvailable` en `App.xaml.cs`) muestre el diálogo antes de aplicar
- Hacer que `AboutViewModel` ofrezca el mismo camino de instalación cuando detecta una versión disponible
- La lógica de apply (`ApplyUpdateUseCase`) permanece sin cambios

### Out of Scope
- Cambios a `UpdateHostedService`, `CheckForUpdateUseCase` o `ApplyUpdateUseCase`
- Progreso de descarga en tiempo real (future enhancement)
- Diferenciación entre actualizaciones críticas y opcionales
- Retry logic del startup check (ya implementada)

## Capabilities

### New Capabilities
- None

### Modified Capabilities
- `startup-update-check`: startup handler MUST prompt user before applying; no silent installs
- `manual-update-check`: About page MUST offer "Instalar ahora" button when update is available

## Approach

**`App.xaml.cs` — `OnUpdateAvailable`**: reemplazar la llamada directa a `ApplyUpdateUseCase` por un `ContentDialog` de confirmación. Si el usuario acepta → apply + exit. Si cancela → mostrar notificación de tray (comportamiento actual de fallback).

**`AboutViewModel`**: cuando `result.Value.UpdateAvailable == true` y `ApplyUpdateUseCase.CanApply(result)`, habilitar un segundo comando `InstallUpdateAsync` que llame a `ApplyUpdateUseCase` y luego señalice a `App` para exit via un event/callback. El mensaje de estado pasa a ser secundario.

**Reutilización**: el `ContentDialog` puede vivir como método helper en `App.xaml.cs` (`ShowUpdatePromptAsync`) o como clase separada. Ambos caminos lo invocan.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `app/src/MGG.Pulse.UI/App.xaml.cs` | Modified | `OnUpdateAvailable`: prompt antes de apply |
| `app/src/MGG.Pulse.UI/ViewModels/AboutViewModel.cs` | Modified | Agregar `InstallUpdateCommand` cuando update disponible |
| `app/src/MGG.Pulse.UI/Views/AboutPage.xaml` | Modified | Botón "Instalar ahora" ligado al nuevo comando |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| `ContentDialog` desde `App.xaml.cs` requiere `XamlRoot` | Med | Usar `_mainWindow.Content.XamlRoot`; guardar referencia en `OnLaunched` |
| Usuario cancela el diálogo en startup → update se pierde hasta el próximo ciclo | Low | Aceptable; el check periódico de 4 h lo vuelve a ofrecer |
| `AboutViewModel` necesita acceso a `ApplyUpdateUseCase` y callback de exit | Low | Ambos disponibles via DI; el callback de exit ya existe como `App.RequestExit()` |

## Rollback Plan

Los cambios están acotados a tres archivos UI. Revertir cualquiera de ellos es independiente. El comportamiento actual (silent apply) se restaura eliminando el `await ShowUpdatePromptAsync(...)` y dejando la llamada directa a `applyUseCase`.

## Dependencies

- Ninguna nueva. `ApplyUpdateUseCase` y `ContentDialog` ya están disponibles.

## Success Criteria

- [ ] Al detectar una actualización en startup, aparece un diálogo preguntando al usuario antes de instalar
- [ ] Si el usuario cancela, la app continúa normalmente y muestra una notificación de tray
- [ ] Desde `Acerca de`, cuando hay una versión nueva disponible, aparece un botón "Instalar ahora"
- [ ] Ambos caminos usan el mismo `ApplyUpdateUseCase` sin duplicar lógica
