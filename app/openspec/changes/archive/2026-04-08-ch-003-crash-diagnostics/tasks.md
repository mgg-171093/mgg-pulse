# Tasks: ch-003-crash-diagnostics

## Phase 1: Tests (RED — Strict TDD)

- [x] 1.1 Crear `tests/MGG.Pulse.Tests.Unit/ViewModels/MainViewModelStatusBrushTests.cs` — test `StatusIndicatorBrush.Color == #4CAF50` cuando `IsRunning = true`
- [x] 1.2 Agregar test `StatusIndicatorBrush.Color == #2A2E45` cuando `IsRunning = false`
- [x] 1.3 Agregar test `StatusIndicatorBrush` no es null al construir `MainViewModel`
- [x] 1.4 Verificar que los 3 tests compilan y **fallan** en rojo (GREEN no existe aún)

## Phase 2: UI — Foundation (CrashLogger)

- [x] 2.1 Crear directorio `src/MGG.Pulse.UI/Diagnostics/`
- [x] 2.2 Crear `src/MGG.Pulse.UI/Diagnostics/CrashLogger.cs` — static class, `Write(Exception)` y `Write(string)`, `File.AppendAllText` a `%AppData%\MGG\Pulse\crash.log`, falla silenciosamente en catch

## Phase 3: UI — Core Implementation

- [x] 3.1 Modificar `src/MGG.Pulse.UI/ViewModels/MainViewModel.cs` — agregar campo `_statusIndicatorBrush` inicializado con color `#2A2E45` y propiedad pública `StatusIndicatorBrush`
- [x] 3.2 Actualizar `OnIsRunningChanged(bool value)` en `MainViewModel.cs` — asignar nuevo `SolidColorBrush` según valor y llamar `OnPropertyChanged(nameof(StatusIndicatorBrush))`
- [x] 3.3 Modificar `src/MGG.Pulse.UI/App.xaml.cs` — registrar `this.UnhandledException += OnUnhandledException` en `App()` constructor **antes** de `InitializeComponent()`
- [x] 3.4 Agregar handler `OnUnhandledException` en `App.xaml.cs` — llama `CrashLogger.Write(e.Exception)`
- [x] 3.5 Envolver el body completo de `OnLaunched` en try/catch — catch llama `CrashLogger.Write(ex)` + `this.Exit()`

## Phase 4: UI — XAML Wiring

- [x] 4.1 Modificar `src/MGG.Pulse.UI/Views/MainPage.xaml` — reemplazar `<Ellipse.Fill><SolidColorBrush Color="{x:Bind ... Converter=BoolToColorConverter}"/></Ellipse.Fill>` por `<Ellipse Fill="{x:Bind ViewModel.StatusIndicatorBrush, Mode=OneWay}"/>`
- [x] 4.2 Remover la declaración de `BoolToColorConverter` de `Page.Resources` en `MainPage.xaml`
- [x] 4.3 Remover el `xmlns` o `using` de `BoolToColorConverter` si quedó en el archivo XAML

## Phase 5: Tests (GREEN)

- [x] 5.1 Ejecutar `dotnet test tests/MGG.Pulse.Tests.Unit` — los 3 tests de `MainViewModelStatusBrushTests` deben pasar
- [x] 5.2 Verificar que el resto de la suite sigue verde (no regresiones)

## Phase 6: Cleanup

- [x] 6.1 Verificar con grep que `BoolToColorConverter` no tiene más usages en el proyecto
- [x] 6.2 Eliminar `src/MGG.Pulse.UI/Converters/BoolToColorConverter.cs`
- [x] 6.3 Ejecutar `dotnet build` completo — verificar build limpio sin warnings de analyzer
