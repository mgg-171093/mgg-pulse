# Proposal: MGG Pulse — Project Foundation

## Intent

Crear desde cero la solución .NET 8 completa para MGG Pulse: estructura de proyectos, Domain puro, Application con Rule Engine, Infrastructure con adaptadores Win32, UI con WinUI 3 + System Tray, y suite de tests unitarios. Este es el change fundacional — sin él no existe el producto.

## Scope

### In Scope
- Solución `.sln` con 5 proyectos correctamente referenciados
- `MGG.Pulse.Domain` — Entities, Value Objects, Enums, Ports (sin dependencias externas)
- `MGG.Pulse.Application` — Use Cases, IRule, RuleEngine, CycleOrchestrator, Result<T>
- `MGG.Pulse.Infrastructure` — Win32InputSimulator, Win32IdleDetector, JsonConfigRepository, FileLoggerService, SystemTrayService (WinForms NotifyIcon en thread STA)
- `MGG.Pulse.UI` — SplashWindow (animada), MainWindow, MainPage, MainViewModel, Design System (tokens de color, dark/light)
- `MGG.Pulse.Tests.Unit` — tests xUnit + Moq para Domain y Application
- Config persistence: `%AppData%\MGG\Pulse\config.json`
- Logo único en `assets/branding/logo.png` referenciado desde Splash y Tray

### Out of Scope
- MSIX installer / code signing
- Auto-update
- UI automation / E2E tests
- Multi-profile support
- Light theme (tokens definidos pero dark es el default activo)

## Capabilities

### New Capabilities
- `simulation-engine`: Rule-based engine que evalúa idle time, modo e intervalos para decidir si simular input
- `input-simulation`: Simulación segura de input Win32 (mouse 1-2px, teclado Shift/Ctrl)
- `idle-detection`: Detección de tiempo idle real del usuario via GetLastInputInfo
- `config-persistence`: Carga y guardado de configuración en JSON (%AppData%)
- `system-tray`: Ícono en bandeja del sistema con menú contextual y notificaciones
- `splash-screen`: Ventana de inicio animada con logo y barra de progreso
- `main-dashboard`: Dashboard principal con estado, controles, configuración y logs en tiempo real

### Modified Capabilities
None — proyecto nuevo, todo es nuevo.

## Approach

Construcción bottom-up respetando la regla de dependencias:
1. **Domain** → sin referencias externas, define contratos (ports)
2. **Application** → solo referencia Domain, implementa lógica de negocio
3. **Infrastructure** → implementa ports del Domain con Win32 y file system
4. **UI** → referencia Application, DI wiring en App.xaml.cs
5. **Tests.Unit** → referencia Domain + Application, mockea Infrastructure

System Tray usa WinForms `NotifyIcon` en thread STA dedicado — más maduro y estable que la API nativa de WinUI 3 para este caso.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `src/MGG.Pulse.Domain/` | New | Entities, VOs, Enums, Ports |
| `src/MGG.Pulse.Application/` | New | Use Cases, Rules, Orchestrator |
| `src/MGG.Pulse.Infrastructure/` | New | Adaptadores Win32, JSON, Tray |
| `src/MGG.Pulse.UI/` | New | WinUI 3 App, Windows, Views, VMs |
| `tests/MGG.Pulse.Tests.Unit/` | New | Suite de tests unitarios |
| `MGG.Pulse.sln` | New | Solución .NET con referencias entre proyectos |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| WinForms + WinUI 3 en mismo proceso | Medium | Thread STA dedicado para NotifyIcon, comunicación via DispatcherQueue |
| SendInput bloqueado por UAC/antivirus | Low | V1 es personal, no-elevated; documentar si ocurre |
| CommunityToolkit source generators | Low | Verificar que los proyectos sean `partial class` correctamente |
| AppWindow API requiere Win SDK 1.3+ | Low | Fijar versión mínima en .csproj |

## Rollback Plan

Proyecto nuevo — no hay estado previo. Si el change falla, se elimina la carpeta `src/` y `tests/` y se vuelve al estado actual (solo archivos de configuración del proyecto).

## Dependencies

- Windows App SDK 1.5+
- .NET 8 Desktop Runtime
- NuGet: `Microsoft.WindowsAppSDK`, `CommunityToolkit.Mvvm`, `Microsoft.Extensions.DependencyInjection`, `System.Text.Json`, `xunit`, `Moq`

## Success Criteria

- [ ] `dotnet build MGG.Pulse.sln` compila sin errores ni warnings
- [ ] `dotnet test tests/MGG.Pulse.Tests.Unit` pasa todos los tests en verde
- [ ] App inicia, muestra SplashScreen animado y luego MainWindow
- [ ] Start/Stop funciona: la simulación corre y se detiene correctamente
- [ ] Ícono aparece en System Tray con menú contextual funcional
- [ ] Config se persiste en `%AppData%\MGG\Pulse\config.json`
- [ ] Domain project tiene zero referencias externas (verificable en .csproj)
