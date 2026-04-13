# Tasks: MGG Pulse — Project Foundation

## Phase 1: Solución y Domain (fundación sin dependencias)

- [ ] 1.1 Crear `MGG.Pulse.sln` y estructura de directorios `src/` y `tests/`
- [ ] 1.2 Crear `src/MGG.Pulse.Domain/MGG.Pulse.Domain.csproj` (net8.0, sin NuGet refs)
- [ ] 1.3 Crear enums: `SimulationMode.cs`, `InputType.cs`, `LogLevel.cs` en `Domain/Enums/`
- [ ] 1.4 Crear value objects: `IntervalRange.cs`, `SimulationAction.cs` en `Domain/ValueObjects/`
- [ ] 1.5 Crear entities: `SimulationConfig.cs`, `SimulationState.cs`, `SimulationSession.cs` en `Domain/Entities/`
- [ ] 1.6 Crear ports: `IInputSimulator.cs`, `IIdleDetector.cs`, `ILoggerService.cs`, `IConfigRepository.cs`, `ITrayService.cs` en `Domain/Ports/`

## Phase 2: Application Layer

- [ ] 2.1 Crear `src/MGG.Pulse.Application/MGG.Pulse.Application.csproj` (referencia Domain)
- [ ] 2.2 Crear `Application/Common/Result.cs` — record `Result<T>` con Ok/Fail
- [ ] 2.3 Crear `Application/Rules/IRule.cs` y `SimulationContext.cs`
- [ ] 2.4 Crear `Application/Rules/RuleResult.cs` — record con ShouldExecute, Reason, Priority
- [ ] 2.5 Crear `Application/Rules/IdleRule.cs` — bloquea si idle < threshold
- [ ] 2.6 Crear `Application/Rules/AggressiveModeRule.cs` — permite siempre en Aggressive mode
- [ ] 2.7 Crear `Application/Rules/IntervalRule.cs` — bloquea si no pasó el intervalo mínimo
- [ ] 2.8 Crear `Application/Rules/RuleEngine.cs` — evalúa colección de IRule, retorna RuleResult final
- [ ] 2.9 Crear `Application/Orchestration/CycleOrchestrator.cs` — loop con delay randomizado + evaluate + execute
- [ ] 2.10 Crear `Application/UseCases/StartSimulationUseCase.cs` — retorna `Result<SimulationSession>`
- [ ] 2.11 Crear `Application/UseCases/StopSimulationUseCase.cs` — cancela CancellationToken, retorna `Result<bool>`

## Phase 3: Infrastructure Layer

- [ ] 3.1 Crear `src/MGG.Pulse.Infrastructure/MGG.Pulse.Infrastructure.csproj` (referencia Domain, agrega System.Windows.Forms)
- [ ] 3.2 Crear `Infrastructure/Win32/Win32IdleDetector.cs` — P/Invoke `GetLastInputInfo`
- [ ] 3.3 Crear `Infrastructure/Win32/Win32InputSimulator.cs` — P/Invoke `SendInput` (mouse 1-2px, Shift/Ctrl)
- [ ] 3.4 Crear `Infrastructure/Persistence/JsonConfigRepository.cs` — load/save `%AppData%\MGG\Pulse\config.json`
- [ ] 3.5 Crear `Infrastructure/Logging/FileLoggerService.cs` — log rotativo en `%AppData%\MGG\Pulse\logs\`
- [ ] 3.6 Crear `Infrastructure/Tray/SystemTrayService.cs` — `NotifyIcon` en thread STA dedicado con menú contextual

## Phase 4: UI Layer

- [ ] 4.1 Crear `src/MGG.Pulse.UI/MGG.Pulse.UI.csproj` (WinUI 3, Windows App SDK 1.5, CommunityToolkit.Mvvm, referencia Application + Infrastructure)
- [ ] 4.2 Crear `UI/Themes/DarkTheme.xaml` y `LightTheme.xaml` con color tokens
- [ ] 4.3 Crear `UI/ViewModels/MainViewModel.cs` — ObservableObject, properties, RelayCommands
- [ ] 4.4 Crear `UI/Windows/SplashWindow.xaml` + code-behind — ventana sin chrome, logo fade-in, progress bar
- [ ] 4.5 Crear `UI/Windows/MainWindow.xaml` + code-behind — ventana principal 400×600
- [ ] 4.6 Crear `UI/Views/MainPage.xaml` + code-behind — Dashboard, controles, config, logs
- [ ] 4.7 Crear `App.xaml` + `App.xaml.cs` — Composition Root, DI container, startup flow (Splash → Main/Tray)
- [ ] 4.8 Configurar build para copiar `assets/branding/logo.png` a output (Content, CopyAlways)

## Phase 5: Tests Unitarios

- [ ] 5.1 Crear `tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj` (xUnit, Moq, referencia Domain + Application)
- [ ] 5.2 Tests `Domain/ValueObjects/IntervalRangeTests.cs` — invariants, equality, argumentos inválidos
- [ ] 5.3 Tests `Application/Rules/IdleRuleTests.cs` — todos los scenarios del spec
- [ ] 5.4 Tests `Application/Rules/AggressiveModeRuleTests.cs` — bypass en Aggressive mode
- [ ] 5.5 Tests `Application/Rules/IntervalRuleTests.cs` — bloqueo por intervalo no elapsed
- [ ] 5.6 Tests `Application/Rules/RuleEngineTests.cs` — combinaciones de reglas, prioridades
- [ ] 5.7 Tests `Application/UseCases/StartSimulationUseCaseTests.cs` — success, cancellation
- [ ] 5.8 Tests `Application/UseCases/StopSimulationUseCaseTests.cs` — stop limpio
- [ ] 5.9 Ejecutar `dotnet test` y verificar que todos los tests pasan en verde
