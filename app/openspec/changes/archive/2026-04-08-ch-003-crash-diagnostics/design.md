# Design: ch-003-crash-diagnostics

## Technical Approach

Corrección de crash silencioso en WinUI 3 mediante dos intervenciones paralelas en la capa UI:
(1) eliminar el binding anidado con converter sobre value type boxeado, reemplazándolo con
binding directo a `SolidColorBrush` en ViewModel; (2) agregar infraestructura de diagnóstico
temprana (static `CrashLogger` + `Application.UnhandledException`) que opera antes y después
de que el DI container exista. Cambios 100% en `MGG.Pulse.UI` — ninguna capa inferior se toca.

## Architecture Decisions

| # | Decision | Choice | Alternatives | Rationale |
|---|----------|--------|--------------|-----------|
| 1 | Tipo de `StatusIndicatorBrush` | `Microsoft.UI.Xaml.Media.SolidColorBrush` en ViewModel | `Windows.UI.Color` + converter; Resource Dictionary dinámico | Elimina la causa raíz del crash. WinUI 3 `{x:Bind}` compilado valida tipos en compile-time: `SolidColorBrush` es asignable a `Fill` directamente sin boxing. MVVM puro diría "no references to UI types in VM" — pragmáticamente aceptado porque es WinUI 3, no hay alternativa sin complejidad innecesaria. |
| 2 | Ubicación de `CrashLogger` | `MGG.Pulse.UI/Diagnostics/CrashLogger.cs` — static class | `Infrastructure.Logging` (nuevo adapter); `FileLoggerService` extendido | Debe operar ANTES de que el DI container esté construido (incluso si `ConfigureServices()` falla). Estar en Infrastructure requeriría instanciar sin DI de todos modos. Static en UI layer es explícito sobre su propósito: diagnóstico de startup de la app. ~15 líneas, no duplica lógica de negocio. |
| 3 | Timing del handler `UnhandledException` | Registrado en `App()` antes de `InitializeComponent()` | Después de `InitializeComponent()`; en `OnLaunched` | Registrar antes de `InitializeComponent()` cubre errores de parse XAML (`App.xaml`). Si se registra después, esos errores pasan desapercibidos. |
| 4 | Manejo en `OnLaunched` catch | `CrashLogger.Write(ex)` + `this.Exit()` — NO re-throw | Re-throw; solo log | Re-throw desde `async void` no llega al handler global en WinUI 3 (diferente pump). `Exit()` asegura terminación controlada. El handler `UnhandledException` cubre otras rutas; `OnLaunched` tiene su propio catch por ser `async void`. |
| 5 | `BoolToColorConverter` | Eliminar el archivo | Mantener sin uso; refactorizar a retornar `SolidColorBrush` | No hay otros usages confirmados. Mantenerlo sin uso es deuda técnica. Eliminarlo es la decisión más limpia. |

## Data Flow

### Startup con crash (nuevo comportamiento)

```
App() constructor
  ├── this.UnhandledException += OnUnhandledException   ← antes de InitializeComponent
  ├── InitializeComponent()
  └── Services = ConfigureServices()

OnLaunched(args)                   ← async void
  └── try
        ├── new SplashWindow()
        ├── await InitializeServicesWithProgress()
        ├── await IConfigRepository.LoadAsync()
        ├── new MainWindow() / Activate()
        └── ITrayService.Initialize(...)
      catch(Exception ex)
        ├── CrashLogger.Write(ex)  ← escribe a crash.log síncrono
        └── this.Exit()

UnhandledException handler (runtime exceptions fuera de OnLaunched)
  └── CrashLogger.Write(e.Exception)
```

### StatusIndicatorBrush data flow

```
IsRunning (ObservableProperty)
  └── OnIsRunningChanged(bool value)
        ├── StartCommand.NotifyCanExecuteChanged()
        ├── StopCommand.NotifyCanExecuteChanged()
        ├── OnPropertyChanged(nameof(IsNotRunning))
        └── _statusIndicatorBrush = value
              ? new SolidColorBrush(Color.FromArgb(255,76,175,80))   // #4CAF50
              : new SolidColorBrush(Color.FromArgb(255,42,46,69))    // #2A2E45
            OnPropertyChanged(nameof(StatusIndicatorBrush))

MainPage.xaml
  └── <Ellipse Fill="{x:Bind ViewModel.StatusIndicatorBrush, Mode=OneWay}"/>
       ↑ compiled binding — tipo SolidColorBrush → Fill: sin boxing, sin converter
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `src/MGG.Pulse.UI/Diagnostics/CrashLogger.cs` | **Create** | Static class — `Write(Exception)` y `Write(string)`, `File.AppendAllText` a `%AppData%\MGG\Pulse\crash.log` |
| `src/MGG.Pulse.UI/App.xaml.cs` | **Modify** | Registrar `UnhandledException` en constructor antes de `InitializeComponent()`; envolver body de `OnLaunched` en try/catch |
| `src/MGG.Pulse.UI/ViewModels/MainViewModel.cs` | **Modify** | Agregar `SolidColorBrush StatusIndicatorBrush` (campo `_statusIndicatorBrush`); actualizar en `OnIsRunningChanged` |
| `src/MGG.Pulse.UI/Views/MainPage.xaml` | **Modify** | Reemplazar `<Ellipse.Fill><SolidColorBrush Color="{x:Bind ...Converter=BoolToColorConverter}"/>` por `<Ellipse Fill="{x:Bind ViewModel.StatusIndicatorBrush, Mode=OneWay}"/>`; remover `BoolToColorConverter` de `Page.Resources` |
| `src/MGG.Pulse.UI/Converters/BoolToColorConverter.cs` | **Delete** | No tiene más usos activos |

## Interfaces / Contracts

```csharp
// src/MGG.Pulse.UI/Diagnostics/CrashLogger.cs
namespace MGG.Pulse.UI.Diagnostics;

internal static class CrashLogger
{
    // Path: %AppData%\MGG\Pulse\crash.log
    public static void Write(Exception ex);
    public static void Write(string message);
    // Falla silenciosamente si el filesystem no está disponible
}
```

```csharp
// En MainViewModel — nuevo miembro público (backing field privado)
private SolidColorBrush _statusIndicatorBrush = new SolidColorBrush(Color.FromArgb(255,42,46,69));
public SolidColorBrush StatusIndicatorBrush => _statusIndicatorBrush;
// Actualizado + notificado en OnIsRunningChanged(bool value)
```

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Unit — `CrashLogger` | Escribe archivo con formato correcto; falla silenciosamente si path no accesible | No es factible testear escritura real sin filesystem mock; `CrashLogger` es tan simple (~15 líneas) que el testing se valida manualmente. Sin test unitario. |
| Unit — `MainViewModel` | `StatusIndicatorBrush` cambia color al cambiar `IsRunning` | xUnit + instancia real de VM con mocks de `StartSimulationUseCase`, `StopSimulationUseCase`, `IConfigRepository`, `ITrayService`. Verificar que `StatusIndicatorBrush.Color` == `Color.FromArgb(255,76,175,80)` cuando `IsRunning = true`. |
| Unit — `App` | No es testeable (constructor WinUI 3) | Sin test. Validación manual al correr la app. |

## Migration / Rollout

No migration required. Cambio aditivo (nuevo archivo `CrashLogger.cs`) + modificaciones de archivos UI.
El `crash.log` se crea on-demand; si no existe no hay problema.
Para rollback: ver `proposal.md` — reversión es puramente de archivos UI, sin cambios de config/DB/Domain.

## Open Questions

- [ ] ¿Hay otros usages de `BoolToColorConverter` fuera de `MainPage.xaml`? (búsqueda visual rápida confirmó solo uno, pero verificar en apply)
