# Proposal: ch-003-crash-diagnostics

## Intent

La app crashea silenciosamente con exit `0xC000027B` al mostrar `MainWindow`.
La causa raíz es un `{x:Bind}` compilado sobre `SolidColorBrush.Color` usando `BoolToColorConverter` que retorna `Windows.UI.Color` boxed como `object` → `InvalidCastException` en el runtime XAML de WinUI 3.
El crash es silencioso porque `async void OnLaunched` no tiene try/catch ni `Application.UnhandledException`, y el logger vive en el DI container que nunca llega a escribir nada.

## Scope

### In Scope
- Corregir el binding XAML del Ellipse de status indicator (causa raíz del crash)
- Exponer `StatusIndicatorBrush` como `SolidColorBrush` en `MainViewModel` para binding directo sin converter
- Agregar `Application.UnhandledException` handler en `App()` constructor
- Wrappear el body de `OnLaunched` en try/catch con early log
- Implementar `CrashLogger` estático (sin DI) que escriba a `%AppData%\MGG\Pulse\crash.log` desde el primer instante

### Out of Scope
- Reescribir `BoolToColorConverter` para otros usos existentes (si los hay)
- Migrar `FileLoggerService` o cambiar el logger principal
- UI automation / E2E tests del flujo de startup
- Manejo de crashes en threads de background (fuera del UI thread)

## Capabilities

> Researched against `openspec/specs/`.

### New Capabilities
- `crash-diagnostics`: Infraestructura de diagnóstico de crashes — early crash logger estático + global unhandled exception handler en App.xaml.cs.

### Modified Capabilities
- `main-dashboard`: El Requirement "Status Display" cambia su implementación de binding — el indicador de estado usa `StatusIndicatorBrush` (property tipada en VM) en lugar del converter `BoolToColorConverter` sobre `SolidColorBrush.Color`.

## Approach

1. **Fix del binding (UI layer)**: reemplazar en `MainPage.xaml` el binding `<Ellipse.Fill><SolidColorBrush Color="{x:Bind ...Converter=BoolToColorConverter}"/>` por `<Ellipse Fill="{x:Bind ViewModel.StatusIndicatorBrush, Mode=OneWay}"/>`.
2. **ViewModel property**: agregar `SolidColorBrush StatusIndicatorBrush { get; }` en `MainViewModel`, actualizado en `OnPropertyChanged` de `IsRunning`. Retorna `new SolidColorBrush(Color.FromArgb(255, 76, 175, 80))` (verde primario) o `new SolidColorBrush(Colors.Transparent)` según estado.
3. **Early crash logger**: clase `CrashLogger` estática en `MGG.Pulse.UI` (o Infrastructure) con `Write(string message)` — directamente a `%AppData%\MGG\Pulse\crash.log`, sin DI, sin async.
4. **Global handler**: `Application.UnhandledException += OnUnhandledException` en el constructor de `App` — llama a `CrashLogger.Write` con tipo y mensaje de la excepción.
5. **try/catch en OnLaunched**: wrappear todo el body en try/catch, loggear al `CrashLogger` y re-throw.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `src/MGG.Pulse.UI/Views/MainPage.xaml` | Modified | Reemplazar binding anidado de SolidColorBrush.Color |
| `src/MGG.Pulse.UI/ViewModels/MainViewModel.cs` | Modified | Agregar `StatusIndicatorBrush` property |
| `src/MGG.Pulse.UI/App.xaml.cs` | Modified | Agregar `UnhandledException` handler + try/catch en `OnLaunched` |
| `src/MGG.Pulse.UI/Diagnostics/CrashLogger.cs` | New | Early static crash logger sin DI |
| `src/MGG.Pulse.UI/Converters/BoolToColorConverter.cs` | Removed | Ya no se usa — puede eliminarse o mantenerse sin uso |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| `MainViewModel` referencia tipo de UI (`SolidColorBrush`) — viola MVVM puro | Low | Aceptado en WinUI 3; alternativa sería `Windows.UI.Color` tipado, pero agrega complejidad innecesaria |
| `CrashLogger` en `MGG.Pulse.UI` duplica lógica con `FileLoggerService` | Low | `CrashLogger` es mínimo (~15 líneas) y escribe a un archivo diferente (`crash.log` vs `pulse-YYYY-MM-DD.log`) |
| Re-throw en `OnLaunched` catch puede no llegar al `UnhandledException` handler | Low | El handler global se registra en el constructor, antes que `OnLaunched`; el re-throw es solo por visibilidad |

## Rollback Plan

Todos los cambios están en la UI layer. Para revertir:
1. Restaurar el binding original en `MainPage.xaml` (reintroducir `<SolidColorBrush Color="{x:Bind ...}">`)
2. Eliminar `StatusIndicatorBrush` del ViewModel
3. Eliminar `CrashLogger.cs`
4. Eliminar el handler `UnhandledException` y try/catch del `App.xaml.cs`

No hay cambios de base de datos, config persistence ni Domain layer — el rollback es puramente de archivos UI.

## Dependencies

- Ninguna externa. Todos los tipos usados (`SolidColorBrush`, `Application.UnhandledException`, `File.AppendAllText`) ya están disponibles en el stack actual.

## Success Criteria

- [ ] La app arranca sin crash y muestra `MainWindow` correctamente
- [ ] El Ellipse de status indicator muestra verde cuando `IsRunning = true` y transparente/neutral cuando `IsRunning = false`
- [ ] Si ocurre un crash durante bootstrap, existe `%AppData%\MGG\Pulse\crash.log` con el stack trace
- [ ] `dotnet test MGG.Pulse.Tests.Unit` pasa sin regresiones
- [ ] No hay referencias a `BoolToColorConverter` activas en el binding del status indicator
