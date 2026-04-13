# Design: UI Wiring and ComboBox Fix

## Technical Approach

Two isolated fixes applied in a single change:

1. **ComboBox crash** — add `SelectedValuePath="Content"` to both ComboBox elements in `MainPage.xaml`. WinUI 3 without this attribute resolves `SelectedValue` to the `ComboBoxItem` wrapper object, not its string `Content`, causing a type mismatch when bound to `string` ViewModel properties.

2. **Event bridge** — inject `CycleOrchestrator` into `MainViewModel` constructor and subscribe to its three events (`ActionExecuted`, `IdleTimeUpdated`, `NextScheduledUpdated`) inside `StartAsync`, unsubscribing in `StopAsync`. The ViewModel already has all handler methods implemented — only the wiring is missing.

No new types, no new interfaces, no layer boundary violations.

## Architecture Decisions

| # | Decision | Choice | Rejected | Rationale |
|---|----------|--------|----------|-----------|
| D1 | ComboBox value resolution | `SelectedValuePath="Content"` | Switch to `ItemsSource` with string list | One-attribute fix; keeps inline items which are static/known at design time. ItemsSource approach adds ViewModel properties and an `ObservableCollection<string>` for no benefit. |
| D2 | CycleOrchestrator injection point | Inject into `MainViewModel` constructor | Add `Configure(onAction, onIdle, onNext)` to `StartSimulationUseCase` | VM is in UI layer → Application layer dependency is valid per the project's dependency rule. UseCase callback pattern would couple Application layer to UI concerns. Simpler, consistent with existing DI pattern. |
| D3 | Subscription lifetime | Subscribe in `StartAsync`, unsubscribe in `StopAsync` | Subscribe once in constructor | Avoids accumulating stale handlers across multiple Start/Stop cycles. `_cts` is recreated each `StartAsync` — event handlers must match that lifecycle. |

## Data Flow

After fix, the runtime signal path is:

```
CycleOrchestrator.RunAsync()
  │
  ├─ IdleTimeUpdated?.Invoke(idleTime)      ──→ MainViewModel.UpdateIdleTime()
  │                                               └─ DispatcherQueue.TryEnqueue()
  │                                                    └─ IdleTimeText = "42s"   ──→ UI
  │
  ├─ NextScheduledUpdated?.Invoke(nextAt)   ──→ MainViewModel.UpdateNextScheduled()
  │                                               └─ DispatcherQueue.TryEnqueue()
  │                                                    └─ NextScheduledText = "in 18s" ──→ UI
  │
  └─ ActionExecuted?.Invoke(action)         ──→ MainViewModel.UpdateLastAction()
                                                 MainViewModel.AddLogEntry()
                                                  └─ DispatcherQueue.TryEnqueue()
                                                       └─ LastActionText / LogText ──→ UI
```

Events are fired from the orchestrator's background `Task` — all handlers marshal back to the UI thread via `DispatcherQueue.TryEnqueue`, which is already the established pattern in the ViewModel.

## File Changes

| File | Action | What changes |
|------|--------|--------------|
| `src/MGG.Pulse.UI/Views/MainPage.xaml` | Modify | Add `SelectedValuePath="Content"` to Mode and InputType ComboBox elements |
| `src/MGG.Pulse.UI/ViewModels/MainViewModel.cs` | Modify | Add `CycleOrchestrator` constructor parameter; subscribe in `StartAsync`; unsubscribe in `StopAsync` |
| `src/MGG.Pulse.UI/App.xaml.cs` | No change | `CycleOrchestrator` already registered as `Singleton`; `MainViewModel` already `Transient` — DI resolves automatically |

## Interfaces / Contracts

No new interfaces. Existing event signatures consumed as-is:

```csharp
// CycleOrchestrator (Application layer) — read-only from ViewModel's perspective
public event Action<SimulationAction>? ActionExecuted;
public event Action<TimeSpan>?         IdleTimeUpdated;
public event Action<DateTime>?         NextScheduledUpdated;
```

Subscription pattern in `MainViewModel`:

```csharp
// StartAsync
_orchestrator.ActionExecuted     += UpdateLastAction;
_orchestrator.ActionExecuted     += a => AddLogEntry($"{a.InputType} at {a.ExecutedAt:HH:mm:ss}");
_orchestrator.IdleTimeUpdated    += UpdateIdleTime;
_orchestrator.NextScheduledUpdated += UpdateNextScheduled;

// StopAsync
_orchestrator.ActionExecuted     -= UpdateLastAction;
_orchestrator.ActionExecuted     -= ... // need named local or lambda field
_orchestrator.IdleTimeUpdated    -= UpdateIdleTime;
_orchestrator.NextScheduledUpdated -= UpdateNextScheduled;
```

> **Implementation note**: the `AddLogEntry` subscription requires a stored delegate field (not an inline lambda) so it can be unsubscribed by reference. Declare `private Action<SimulationAction>? _logHandler` and assign it once in `StartAsync`.

## Testing Strategy

| Layer | What to test | Approach |
|-------|-------------|----------|
| Unit (`Tests.Unit`) | `UpdateIdleTime` formats TimeSpan correctly (seconds vs minutes) | Instantiate VM with mocked deps; call method directly; assert `IdleTimeText` |
| Unit | `UpdateNextScheduled` shows "now" when `diff ≤ 0` | Same; pass `DateTime.UtcNow.AddSeconds(-1)` |
| Unit | `UpdateLastAction` formats `LastActionText` with InputType and time | Same |
| Unit | Subscribe/unsubscribe cycle: after `StopAsync`, orchestrator event fires no ViewModel update | Raise event manually after stop; assert property unchanged |
| Manual | ComboBox retains selection across Start/Stop | Launch app, change mode, start, stop — verify selection persists |
| Manual | Status card fields update while running | Start simulation, observe IdleTimeText, LastActionText, NextScheduledText update live |

## Open Questions

- None. Design is complete and unblocked.
