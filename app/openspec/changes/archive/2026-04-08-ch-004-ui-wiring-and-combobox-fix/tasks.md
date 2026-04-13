# Tasks: UI Wiring and ComboBox Fix

## Phase 1: XAML Fix — MainPage.xaml

- [x] 1.1 Add `SelectedValuePath="Content"` to the Mode `ComboBox` in `src/MGG.Pulse.UI/Views/MainPage.xaml` (the one bound to `ViewModel.SelectedMode`)
- [x] 1.2 Add `SelectedValuePath="Content"` to the InputType `ComboBox` in `src/MGG.Pulse.UI/Views/MainPage.xaml` (the one bound to `ViewModel.SelectedInputType`)

## Phase 2: ViewModel — Constructor + Fields

- [x] 2.1 Add private field `private CycleOrchestrator _orchestrator;` to `src/MGG.Pulse.UI/ViewModels/MainViewModel.cs`
- [x] 2.2 Add private delegate fields for unsubscription: `private Action<SimulationAction>? _onActionExecuted;` and `private Action<SimulationAction>? _logHandler;` and `private Action<TimeSpan>? _onIdleTimeUpdated;` and `private Action<DateTime>? _onNextScheduledUpdated;` to `MainViewModel.cs`
- [x] 2.3 Update `MainViewModel` constructor to accept `CycleOrchestrator orchestrator` and assign `_orchestrator = orchestrator;`

## Phase 3: ViewModel — StartAsync Wiring

- [x] 3.1 In `StartAsync()`, instantiate and assign all delegate fields: `_onActionExecuted = UpdateLastAction;`, `_logHandler = a => AddLogEntry($"{a.InputType} at {a.ExecutedAt:HH:mm:ss}");`, `_onIdleTimeUpdated = UpdateIdleTime;`, `_onNextScheduledUpdated = UpdateNextScheduled;`
- [x] 3.2 In `StartAsync()`, subscribe all delegates to `_orchestrator` events: `ActionExecuted += _onActionExecuted;`, `ActionExecuted += _logHandler;`, `IdleTimeUpdated += _onIdleTimeUpdated;`, `NextScheduledUpdated += _onNextScheduledUpdated;`
- [x] 3.3 In `StartAsync()`, call `AddLogEntry("Simulation started")` after subscribing events

## Phase 4: ViewModel — StopAsync Unsubscribe + Log

- [x] 4.1 In `StopAsync()`, unsubscribe all four delegates from `_orchestrator` events (`ActionExecuted -= _onActionExecuted;`, `ActionExecuted -= _logHandler;`, `IdleTimeUpdated -= _onIdleTimeUpdated;`, `NextScheduledUpdated -= _onNextScheduledUpdated;`) and null them out
- [x] 4.2 In `StopAsync()`, call `AddLogEntry("Simulation stopped")` after unsubscribing events

## Phase 5: DI Verification — App.xaml.cs

- [x] 5.1 Verify in `src/MGG.Pulse.UI/App.xaml.cs` that `CycleOrchestrator` is registered as singleton and `MainViewModel` as transient — confirm DI resolves `CycleOrchestrator` automatically into the updated constructor with no factory override needed; update registration only if auto-resolution fails

## Phase 6: Build & Test Pass

- [x] 6.1 Run `dotnet build` from solution root; confirm zero errors in `MGG.Pulse.UI` and all other projects
- [x] 6.2 Run `dotnet test` from solution root; confirm all `Tests.Unit` tests pass (no regressions)
