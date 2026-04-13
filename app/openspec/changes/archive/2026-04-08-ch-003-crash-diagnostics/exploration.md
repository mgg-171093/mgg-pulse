# Exploration: ch-003-crash-diagnostics

**Date**: 2026-04-08  
**Change**: ch-003-crash-diagnostics  
**Problem**: `System.InvalidCastException` en `MGG.Pulse.UI.dll` al mostrar `MainWindow` después del SplashScreen. Exit code `0xC000027B` (unhandled exception en thread de UI). Sin logs porque la app muere antes de inicializar el logger.

---

## Current State

### Flujo de inicio

1. `App()` constructor → `InitializeComponent()` → `ConfigureServices()` → DI container listo
2. `OnLaunched(args)` → `SplashWindow` → `InitializeServicesWithProgress()` → `LoadAsync()` config → `MainWindow` → tray
3. `MainWindow` constructor → `InitializeComponent()` → `ConfigureWindow()` → instancia `MainPage` (dentro del XAML)
4. `MainPage` constructor → `InitializeComponent()` → DI resolve `MainViewModel` → `InitializeAsync()`

### Estado actual de cada archivo investigado

#### `App.xaml.cs`
- `OnLaunched` es `async void` — **CRÍTICO**: cualquier excepción que escape de aquí mata el proceso silenciosamente (no hay `AppDomain.UnhandledException`, no hay `TaskScheduler.UnobservedTaskException`, no hay `Application.UnhandledException`)
- **No hay try/catch en OnLaunched**
- **No hay ningún unhandled exception handler global registrado**
- DI wiring correcto, no hay circular deps visibles
- `FileLoggerService` se registra como singleton pero NO se instancia early — solo existe en el DI container

#### `App.xaml`
- Carga `XamlControlsResources` + `DarkTheme.xaml` → correcto
- Si `DarkTheme.xaml` fallara al parsear, **la excepción ocurriría en el constructor de App**, no en OnLaunched

#### `MainWindow.xaml` / `MainWindow.xaml.cs`
- El XAML solo tiene `<views:MainPage/>` — instancia MainPage directamente como contenido
- `ConfigureWindow()` usa `AppWindowTitleBar.IsCustomizationSupported()` — no puede lanzar (es una comprobación estática)
- `AppWindow.Resize`, `AppWindow.Move` — seguros
- `InitializeComponent()` instancia `MainPage` → **aquí es donde vive el riesgo real**

#### `MainPage.xaml`
- Usa los siguientes `StaticResource`:
  - `BackgroundBrush` ✅ definido en DarkTheme.xaml
  - `TextPrimaryBrush` ✅ definido
  - `TextSecondaryBrush` ✅ definido
  - `PrimaryBrush` ✅ definido
  - `CardStyle` ✅ definido
  - `PrimaryButtonStyle` ✅ definido
  - `SurfaceVariantBrush` ✅ definido
  - `BoolToVisibilityConverter` ✅ declarado en `Page.Resources`
  - `BoolToColorConverter` ✅ declarado en `Page.Resources`
- **Todos los StaticResources están presentes** — esta causa queda descartada

#### `MainPage.xaml.cs`
- Constructor llama `App.Services.GetRequiredService<MainViewModel>()` — puede lanzar si DI falla
- Luego `_ = ViewModel.InitializeAsync()` — fire-and-forget (`_ =`): si lanza, la excepción queda unobserved pero **no mata el proceso en .NET 8** (solo dispara `UnobservedTaskException`)
- **La resolución DI de `MainViewModel` sí puede lanzar** si alguna dependencia no está registrada

#### `MainViewModel.cs`
- Constructor recibe `StartSimulationUseCase`, `StopSimulationUseCase`, `IConfigRepository`, `ITrayService`
- **PROBLEMA ENCONTRADO**: `MainViewModel` está registrado como `AddTransient<MainViewModel>()`. Sus dependencias son:
  - `StartSimulationUseCase` → `AddTransient` ✅
  - `StopSimulationUseCase` → `AddTransient` ✅
  - `IConfigRepository` → `AddSingleton<JsonConfigRepository>()` ✅
  - `ITrayService` → `AddSingleton<SystemTrayService>()` ✅
- DI resolvería sin problema — no hay deps no registradas
- `LoadConfigToProperties()` hace `_config.Mode.ToString()` y `_config.InputType.ToString()` → safe si `_config` es `SimulationConfig.Default`

#### `BoolToColorConverter.cs`
- Retorna `Windows.UI.Color` (struct)
- El binding en XAML es: `<SolidColorBrush Color="{x:Bind ViewModel.IsRunning, Converter={StaticResource BoolToColorConverter}}"/>`
- `SolidColorBrush.Color` espera `Windows.UI.Color`
- El converter retorna `object` (boxed `Color`) — **PROBLEMA POTENCIAL**: en WinUI 3, `{x:Bind}` con conversión de tipo puede fallar con `InvalidCastException` si el runtime intenta hacer cast de `object` a `Color` y la fuente boxeada no es exactamente el tipo esperado por el property setter del XAML runtime
- Esto es **exactamente el patrón que produce `System.InvalidCastException` en WinUI 3**

#### `FileLoggerService.cs`
- Escribe a `%AppData%\MGG\Pulse\logs\pulse-YYYY-MM-DD.log`
- Inicialización lazy (se crea el directorio al primer `LogAsync`)
- **No hay early logger**: el logger solo existe dentro del DI container, que no escribe nada hasta que se llame `LogAsync`

---

## Affected Areas

- `src/MGG.Pulse.UI/App.xaml.cs` — ausencia de exception handlers globales y try/catch en `OnLaunched`
- `src/MGG.Pulse.UI/Views/MainPage.xaml` — binding `BoolToColorConverter` sobre `{x:Bind}` con `SolidColorBrush.Color`
- `src/MGG.Pulse.UI/Converters/BoolToColorConverter.cs` — retorna `Windows.UI.Color` vía `object`, problemático en WinUI 3 {x:Bind}
- `src/MGG.Pulse.Infrastructure/Logging/FileLoggerService.cs` — no hay crash log de bootstrap

---

## Root Cause Analysis

### Causa Raíz #1 — CRÍTICA: `BoolToColorConverter` + `{x:Bind}` + `SolidColorBrush.Color` (probabilidad: MUY ALTA)

```xaml
<SolidColorBrush Color="{x:Bind ViewModel.IsRunning, Mode=OneWay, 
                          Converter={StaticResource BoolToColorConverter}}"/>
```

El converter retorna `Windows.UI.Color` (value type) boxed como `object`. El XAML runtime de WinUI 3 al intentar asignar el valor al property `Color` del `SolidColorBrush` realiza un cast interno. 

**El problema específico**: `SolidColorBrush.Color` es de tipo `Windows.UI.Color` pero cuando se usa dentro de un elemento markup de `Ellipse.Fill` (un XAML markup extension), el runtime XAML puede intentar hacer el cast como si fuera un `Color` de `Microsoft.UI` en lugar de `Windows.UI`, o bien el boxing/unboxing del valor retornado del converter falla cuando el runtime genera código `x:Bind` compilado.

En WinUI 3 con `{x:Bind}` (compiled bindings), el tipo esperado por el property setter **se verifica en tiempo de compilación parcialmente, pero el cast del valor devuelto por el converter ocurre en runtime**. Si hay un type mismatch entre el `Color` que retorna el converter y el `Color` que espera el setter, se lanza `System.InvalidCastException`.

**Adicionalmente**: La estructura `<Ellipse.Fill><SolidColorBrush Color="{x:Bind ...}"/></Ellipse.Fill>` es inusual. Normalmente se bindea directamente la brush, no el `Color` de una brush anidada. Esta construcción puede ser incompatible con `{x:Bind}`.

### Causa Raíz #2 — CRÍTICA: `async void OnLaunched` sin try/catch global (probabilidad: ALTA)

```csharp
protected override async void OnLaunched(LaunchActivatedEventArgs args)
{
    // Sin try/catch
    // Sin Application.UnhandledException handler
    // Sin AppDomain.UnhandledException handler
}
```

Cualquier excepción que escape de `OnLaunched` — incluyendo la de causa #1 — mata el proceso con `0xC000027B` sin dejar rastro. **Este no es el origen del crash, pero sí explica por qué no hay logs y por qué el exit code es ese**.

### Causa Raíz #3 — SECUNDARIA: No hay early crash logger (probabilidad: CERTEZA como causa de "sin logs")

`FileLoggerService` vive en el DI container y nunca escribe nada durante el bootstrap. Si el crash ocurre antes de que `InitializeAsync()` complete, no hay ningún archivo de log. Se necesita un crash logger que escriba a `%AppData%\MGG\Pulse\crash.log` **antes de la DI**.

---

## Approaches

### Approach 1 — Fix directo del binding + global exception handler (RECOMENDADO)
Corregir el binding XAML problemático Y agregar infrastructure de diagnóstico.

**Cambios concretos:**
1. En `MainPage.xaml`: cambiar el binding de `BoolToColorConverter` para bindear directamente a un `Brush` en el ViewModel en lugar de hacer `Color="{x:Bind ... Converter=...}"` dentro de un `SolidColorBrush` anidado
2. En `MainViewModel.cs`: agregar property `StatusIndicatorBrush` que retorne la brush correcta directamente
3. En `App.xaml.cs`: agregar `Application.UnhandledException` handler y wrappear `OnLaunched` en try/catch con early file logger
4. Early crash logger: método estático que escriba a `%AppData%\MGG\Pulse\crash.log` sin DI

- **Pros**: Fix preciso de la causa raíz + diagnóstico para futuros crashes
- **Cons**: Toca XAML, ViewModel y App.xaml.cs
- **Effort**: Medium

### Approach 2 — Solo agregar diagnostics sin corregir el binding
Agregar exception handler global para capturar el error, ver el mensaje exacto, y decidir después.

- **Pros**: Bajo riesgo, clarifica si la causa raíz es efectivamente el converter
- **Cons**: La app sigue crasheando — solo obtenemos el log del error
- **Effort**: Low

### Approach 3 — Reemplazar BoolToColorConverter con ViewModel property + eliminar el binding anidado
Eliminar el converter completamente y usar una property `SolidColorBrush StatusIndicatorColor` en el ViewModel.

- **Pros**: Clean, elimina la causa raíz de forma definitiva, más testeable
- **Cons**: Requiere cambios en ViewModel (retorna un tipo de UI layer desde VM — violación leve de MVVM purismo, pero aceptable para WinUI 3)
- **Effort**: Medium

---

## Recommendation

**Approach 1 + 3 combinados** — se implementan en este orden:

1. **Early crash logger** (estático, sin DI) en `App.xaml.cs` que escriba a `crash.log` al primer instante
2. **`Application.UnhandledException` handler** para capturar y loggear cualquier excepción en el UI thread
3. **try/catch en `OnLaunched`** con log al crash file
4. **Fix del binding**: reemplazar `<Ellipse.Fill><SolidColorBrush Color="{x:Bind ViewModel.IsRunning, Converter=...}"/>` por una property `StatusIndicatorBrush` (tipo `SolidColorBrush`) en el ViewModel, y bindear directamente `Fill="{x:Bind ViewModel.StatusIndicatorBrush, Mode=OneWay}"`

El Approach 2 (solo diagnostics) NO es suficiente porque la app seguiría crasheando para el usuario final.

---

## Risks

- **WinUI 3 + ViewModel retornando SolidColorBrush**: el ViewModel estaría retornando un tipo de `Microsoft.UI.Xaml.Media` — esto es una dependencia de UI en el ViewModel. Es pragmático para WinUI 3 pero viola MVVM estricto. Alternativa pura: usar `{x:Bind}` compilado sin converter, con una property de tipo `Color` correctamente tipada (no `object`). Se puede resolver haciendo que el converter retorne `Color` fuertemente tipado — ver nota abajo.
- **Converter tipado**: si `BoolToColorConverter.Convert()` retorna `Color` (value type) directamente con tipo de retorno `object`, el boxing puede causar el cast failure. La solución limpia es crear un converter específico de `IValueConverter` que retorne el tipo exacto esperado, o evitar `{x:Bind}` sobre un property de tipo value-type dentro de markup anidado.
- **`async void OnLaunched`**: es el patrón estándar de WinUI 3 (Microsoft lo documenta así) — no se puede cambiar la firma, solo se puede wrappear el body.
- **Early logger acoplado a Infrastructure**: el crash logger en App.xaml.cs podría duplicar lógica con `FileLoggerService`. Solución: crear método estático `CrashLogger.Write(string)` en Infrastructure o directamente en UI como static helper.

---

## Additional Findings

### StaticResources — TODOS PRESENTES ✅
Verificación completa contra `DarkTheme.xaml`:
| Resource usado en MainPage.xaml | ¿Definido? |
|---|---|
| `BackgroundBrush` | ✅ |
| `TextPrimaryBrush` | ✅ |
| `TextSecondaryBrush` | ✅ |
| `PrimaryBrush` | ✅ |
| `CardStyle` | ✅ |
| `PrimaryButtonStyle` | ✅ |
| `SurfaceVariantBrush` | ✅ |

No hay `XamlParseException` por StaticResource faltante. Esta causa queda **descartada**.

### DI Resolution — CORRECTO ✅
`MainViewModel` tiene todas sus dependencias registradas en `ConfigureServices()`. No hay circular deps ni servicios faltantes. Esta causa queda **descartada**.

### `InitializeAsync` fire-and-forget — NO ES EL CRASH ✅
`_ = ViewModel.InitializeAsync()` — en .NET 8 las `UnobservedTaskException` NO matan el proceso por defecto. Esta causa queda **descartada como fuente del exit 0xC000027B**.

---

## Ready for Proposal

**Sí** — la causa raíz está identificada con alta confianza:

1. **Causa primaria**: binding `{x:Bind}` sobre `SolidColorBrush.Color` con converter que retorna `Windows.UI.Color` boxed-as-object → `InvalidCastException` en WinUI 3 compiled bindings
2. **Causa amplificadora**: `async void OnLaunched` sin exception handler global → crash silencioso, sin logs

**Scope del ch-003**:
- Fix del binding XAML problemático (Ellipse status indicator)
- Global exception handler en `App.xaml.cs` (`Application.UnhandledException`)
- Early crash logger (`crash.log`) independiente de DI
- try/catch en `OnLaunched`

La propuesta puede avanzar directamente.
