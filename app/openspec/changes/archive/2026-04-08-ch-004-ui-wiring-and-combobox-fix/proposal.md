# Proposal: UI Wiring and ComboBox Fix

## Intent

Two silent bugs prevent the UI from ever reflecting simulation state:

1. **ComboBox crash** — `SelectedValue` with inline `ComboBoxItem` children returns the item object, not the string `Content`. WinUI 3 type-mismatch crashes the binding silently; Mode and InputType selectors don't bind.
2. **Event wiring absent** — `CycleOrchestrator` fires `ActionExecuted`, `IdleTimeUpdated`, and `NextScheduledUpdated` but nobody subscribes. `MainViewModel` has all the receivers; they are just never connected. Result: IdleTime, LastAction, NextAction, and ActivityLog stay frozen at startup values.

Both bugs existed since the initial UI scaffold. This change makes the dashboard actually live.

## Scope

### In Scope
- Add `SelectedValuePath="Content"` to both ComboBoxes (`SimulationMode` and `InputType`) in `MainPage.xaml`
- Inject `CycleOrchestrator` into `MainViewModel`; subscribe events in `StartAsync`, unsubscribe in `StopAsync`
- Register the updated `MainViewModel` constructor in `App.xaml.cs` DI composition root

### Out of Scope
- Changes to `CycleOrchestrator` internals or event signatures
- New logging infrastructure
- Any Domain or Application layer code changes
- Automated UI / E2E tests (out of scope per V1 constraints)

## Capabilities

### New Capabilities
- None

### Modified Capabilities
- `main-dashboard`: Add requirements for live event binding (IdleTime, LastAction, NextScheduled, ActivityLog update in real time) and correct ComboBox value resolution via `SelectedValuePath`.

## Approach

**ComboBox fix**: single-line XAML attribute addition — `SelectedValuePath="Content"` — on both `<ComboBox>` elements. No C# changes needed for this fix.

**Event wiring**: `MainViewModel.StartAsync()` subscribes to the three `CycleOrchestrator` events before calling the use case. `StopAsync()` unsubscribes. `CycleOrchestrator` is constructor-injected (it lives in Application layer; MainViewModel is UI layer — direction is valid per the dependency rule). `App.xaml.cs` provides `CycleOrchestrator` as a singleton so the same instance flows to both `StartSimulationUseCase` and `MainViewModel`.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `src/MGG.Pulse.UI/Views/MainPage.xaml` | Modified | Add `SelectedValuePath="Content"` to 2 ComboBoxes |
| `src/MGG.Pulse.UI/ViewModels/MainViewModel.cs` | Modified | Inject `CycleOrchestrator`; subscribe/unsubscribe events |
| `src/MGG.Pulse.UI/App.xaml.cs` | Modified | Register `CycleOrchestrator` singleton; update `MainViewModel` DI registration |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Double-subscription if `StartAsync` called twice | Low | Guard with `IsRunning` check already present in ViewModel |
| Memory leak if `StopAsync` never called | Low | Unsubscribe in `StopAsync`; app lifecycle is single-window |

## Rollback Plan

Revert the three files to their pre-change state. No schema or data migration involved — config.json is unaffected. Git revert of the single commit is sufficient.

## Dependencies

- `CycleOrchestrator` must already be registered in DI (it is, via `StartSimulationUseCase`)

## Success Criteria

- [ ] Selecting "Aggressive" mode in the ComboBox updates `ViewModel.SelectedMode` without crash
- [ ] Selecting "Keyboard" input type updates `ViewModel.SelectedInputType` without crash
- [ ] IdleTime label updates every second while simulation is running
- [ ] LastAction label shows the most recent simulated action description
- [ ] NextScheduled label shows the upcoming scheduled time
- [ ] ActivityLog panel receives new entries on each simulated action
- [ ] All existing unit tests pass (`dotnet test MGG.Pulse.Tests.Unit`)
