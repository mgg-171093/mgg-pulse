# Apply Progress: ch-004-ui-wiring-and-combobox-fix

## Status: COMPLETE — All tasks implemented

**Mode**: Standard (no unit tests possible for MainViewModel in WinUI 3 UI layer — confirmed in design.md)
**Build**: ✅ 0 errors, 0 warnings
**Tests**: ✅ 32/32 passed

---

## Completed Tasks

### Phase 1: XAML Fix — MainPage.xaml
- [x] 1.1 Added `SelectedValuePath="Content"` to the Mode ComboBox (bound to `ViewModel.SelectedMode`)
- [x] 1.2 Added `SelectedValuePath="Content"` to the InputType ComboBox (bound to `ViewModel.SelectedInputType`)

### Phase 2: ViewModel — Constructor + Fields
- [x] 2.1 Added `private readonly CycleOrchestrator _orchestrator;` field
- [x] 2.2 Added four delegate fields: `_onActionExecuted`, `_logHandler`, `_onIdleTimeUpdated`, `_onNextScheduledUpdated`
- [x] 2.3 Updated constructor to accept `CycleOrchestrator orchestrator` and assign `_orchestrator = orchestrator`

### Phase 3: ViewModel — StartAsync Wiring
- [x] 3.1 Assigned all delegate fields before subscribing: `_onActionExecuted = UpdateLastAction`, `_logHandler = a => AddLogEntry(...)`, `_onIdleTimeUpdated = UpdateIdleTime`, `_onNextScheduledUpdated = UpdateNextScheduled`
- [x] 3.2 Subscribed all four delegates to `_orchestrator` events (including two handlers on `ActionExecuted`)
- [x] 3.3 Called `AddLogEntry("Simulation started.")` after subscribing events

### Phase 4: ViewModel — StopAsync Unsubscribe + Log
- [x] 4.1 Unsubscribed all four delegates with null-guard checks; nulled out all delegate fields
- [x] 4.2 Called `AddLogEntry("Simulation stopped.")` after unsubscribing

### Phase 5: DI Verification — App.xaml.cs
- [x] 5.1 Verified `App.xaml.cs`: `CycleOrchestrator` is `AddSingleton` (line 126), `MainViewModel` is `AddTransient` (line 131). DI auto-resolves the new parameter — **no changes required**.

### Phase 6: Build & Test Pass
- [x] 6.1 `dotnet build src/MGG.Pulse.UI/MGG.Pulse.UI.csproj` → Build succeeded, 0 errors, 0 warnings
- [x] 6.2 `dotnet test tests/MGG.Pulse.Tests.Unit` → Passed! Failed: 0, Passed: 32, Skipped: 0

---

## Files Changed

| File | Action | What Was Done |
|------|--------|---------------|
| `src/MGG.Pulse.UI/Views/MainPage.xaml` | Modified | Added `SelectedValuePath="Content"` to both ComboBox elements (lines 120, 134) |
| `src/MGG.Pulse.UI/ViewModels/MainViewModel.cs` | Modified | Added `using MGG.Pulse.Application.Orchestration;`, `_orchestrator` field + 4 delegate fields, updated constructor, wired events in `StartAsync`, unsubscribed in `StopAsync` |
| `src/MGG.Pulse.UI/App.xaml.cs` | No change | DI already correct — CycleOrchestrator Singleton, MainViewModel Transient |

---

## Implementation Notes

- `ActionExecuted` gets **two** separate subscriptions: `_onActionExecuted` (→ `UpdateLastAction`) and `_logHandler` (→ `AddLogEntry` with format `"{InputType} at {HH:mm:ss}"`). Both are stored as named fields to allow clean unsubscription.
- Delegate fields use the qualified type `Domain.ValueObjects.SimulationAction` (matching the existing `UpdateLastAction` method signature pattern in the file).
- LSP errors in UI project files are confirmed false positives — the language server lacks WinUI 3 packages. `dotnet build` is the authoritative source of truth.
- `StopAsync` nulls all delegate fields after unsubscription, preventing any stale reference retention across Start/Stop cycles.

---

## Deviations from Design

None — implementation matches design.md and tasks.md exactly.
