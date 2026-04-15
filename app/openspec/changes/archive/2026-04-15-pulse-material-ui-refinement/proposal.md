# Proposal: pulse-material-ui-refinement

## Intent

El diseño visual actual de MGG Pulse es funcional pero carece de coherencia visual. El Splash es pequeño, el shell no tiene barra de estado ni logo lateral, los logs viven dentro del Dashboard contaminando su responsabilidad, y no existe un control de apariencia (Light/Dark) para el usuario. Este cambio refina la UI aplicando Material Design guidelines sobre WinUI 3 sin librerías externas.

## Scope

### In Scope
- Redimensionar y rediseñar la Splash Window (≥600×750, logo grande, sin texto)
- Redimensionar la Main Window (≥800×600), aplicar `.ico` al taskbar/título
- Refinar ShellPage: logo en sidebar, status bar, corregir duplicado Settings, agregar nav items Appearance y Logs
- Extraer logs del DashboardPage a una nueva LogsPage independiente
- Crear AppearancePage con toggle Light/Dark persistido en `LocalSettings`
- Refinar temas (`DarkTheme.xaml` / `LightTheme.xaml`) con paleta Material Design (near-black dark, light-gray light, Green 500 primary, `CornerRadius="8"`)
- Aplicar `.ico` en `SystemTrayService` via `AppWindow.SetIcon()`

### Out of Scope
- Librerías externas de Material Design (WinUI Material, MaterialDesignInXaml)
- Animaciones tipo ripple / elevation shadows auténticas de Material
- Cambios en lógica de simulación o `SimulationConfig`
- Internacionalización / localización

## Capabilities

### New Capabilities
- `appearance-theme-toggle`: Controla la selección de tema Light/Dark desde la UI, persiste en `LocalSettings`, y lo aplica en runtime sin reiniciar.
- `logs-page`: Vista dedicada para el log de eventos del sistema, separada del Dashboard.

### Modified Capabilities
None

## Approach

Manual XAML styling sobre WinUI 3 con `ThemeDictionaries`. Se reutilizan los `ResourceDictionary` existentes (`DarkTheme.xaml`, `LightTheme.xaml`) y se refinan sus tokens de color. La persistencia del tema usa `Windows.Storage.ApplicationData.Current.LocalSettings`. La migración de logs mueve el binding de `LogText` desde `MainViewModel` (o `DashboardViewModel`) a un nuevo `LogsViewModel`, manteniendo el `ILoggerService` como fuente compartida.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `Windows/SplashWindow.xaml` | Modified | Resize ≥600×750, logo 400×450, eliminar texto |
| `Windows/MainWindow.xaml.cs` | Modified | Resize ≥800×600, `AppWindow.SetIcon()` |
| `Views/ShellPage.xaml` + `.cs` | Modified | Logo header, status bar, fix nav duplicado, añadir Appearance/Logs items |
| `Views/DashboardPage.xaml` | Modified | Remover área de logs |
| `ViewModels/MainViewModel.cs` | Modified | Remover lógica de log binding |
| `Views/LogsPage.xaml` | New | Vista de logs con binding a LogsViewModel |
| `ViewModels/LogsViewModel.cs` | New | ViewModel para logs, conecta con ILoggerService |
| `Views/AppearancePage.xaml` | New | UI de selección de tema |
| `ViewModels/AppearanceViewModel.cs` | New | Lógica de toggle + persistencia en LocalSettings |
| `Themes/DarkTheme.xaml` | Modified | Paleta Material, CornerRadius tokens |
| `Themes/LightTheme.xaml` | Modified | Paleta Material, CornerRadius tokens |
| `Infrastructure/Tray/SystemTrayService.cs` | Modified | AppWindow.SetIcon() con .ico |
| `Assets/` | Modified | Agregar `.ico` de branding |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| ILoggerService no puede pushear eventos a LogsViewModel cuando la página no está activa | Med | Usar un observable/event intermediario o mensaje via CommunityToolkit.Mvvm Messenger |
| AppWindow.SetIcon() requiere `.ico` físico presente en assets | Low | Verificar presencia del archivo antes de build; agregar al .csproj como content |
| ThemeDictionaries runtime swap puede dejar recursos sin resolver en WinUI 3 | Low | Probar swap en frío con restart de ResourceDictionary merged |

## Rollback Plan

Todos los cambios son aditivos o en archivos XAML/CS existentes sin tocar la capa de dominio. Revertir vía `git revert` del PR completo restaura el estado anterior sin efectos secundarios en persistencia o lógica de negocio.

## Dependencies

- `.ico` de branding debe existir o crearse antes del cambio en `SystemTrayService`
- `CommunityToolkit.Mvvm` (ya presente) — Messenger para comunicación cross-ViewModel de logs

## Success Criteria

- [ ] Splash muestra logo grande (≥400×450) sin texto, ventana ≥600×750
- [ ] Main Window arranca ≥800×600 con ícono visible en taskbar y título
- [ ] ShellPage muestra logo en sidebar, status bar funcional, sin Settings duplicado
- [ ] LogsPage muestra logs en tiempo real idénticos a los que mostraba el Dashboard
- [ ] AppearancePage permite cambiar Light/Dark y la preferencia persiste entre reinicios
- [ ] Temas visual con paleta Material Design aplicada (colores correctos, CornerRadius="8")
- [ ] Sin regresiones en lógica de simulación ni Settings/About
