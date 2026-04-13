# Delta for main-dashboard

## ADDED Requirements

### Requirement: ComboBox SelectedValue Resolution

The Mode and InputType ComboBoxes MUST use `SelectedValuePath="Content"` so that their bound ViewModel properties receive a `string` value rather than a `ComboBoxItem` object.

| ComboBox | Bound Property | Expected value type |
|----------|---------------|---------------------|
| SimulationMode | `SelectedMode` | `string` (e.g. `"Aggressive"`) |
| InputType | `SelectedInputType` | `string` (e.g. `"Keyboard"`) |

#### Scenario: Mode selection produces string value

- GIVEN the Mode ComboBox has `SelectedValuePath="Content"`
- WHEN the user selects "Aggressive"
- THEN `ViewModel.SelectedMode` equals the string `"Aggressive"`
- AND no binding exception is thrown

#### Scenario: ComboBox restores selection on load

- GIVEN `ViewModel.SelectedMode` is `"Intelligent"` at startup
- WHEN the dashboard is displayed
- THEN the Mode ComboBox shows "Intelligent" as the selected item

#### Scenario: InputType selection produces string value

- GIVEN the InputType ComboBox has `SelectedValuePath="Content"`
- WHEN the user selects "Keyboard"
- THEN `ViewModel.SelectedInputType` equals the string `"Keyboard"`
- AND no binding exception is thrown

---

### Requirement: Real-time IdleTime Updates

While simulation is running, `IdleTimeText` MUST update every cycle to reflect the current system idle duration, sourced from the `CycleOrchestrator.IdleTimeUpdated` event.

#### Scenario: IdleTime label updates during active simulation

- GIVEN the simulation is running
- WHEN `CycleOrchestrator` fires `IdleTimeUpdated` with a `TimeSpan` value
- THEN `MainViewModel.UpdateIdleTime(TimeSpan)` is invoked
- AND `IdleTimeText` reflects the new idle duration

#### Scenario: IdleTime label does not update when stopped

- GIVEN the simulation is stopped
- WHEN the system becomes idle
- THEN `IdleTimeText` MUST NOT change (no event subscription active)

---

### Requirement: Real-time LastAction Updates

`LastActionText` MUST update to describe the most recently simulated action whenever `CycleOrchestrator.ActionExecuted` fires.

Format: `"{InputType} at {HH:mm:ss}"` — e.g. `"Mouse at 14:32:05"`.

#### Scenario: LastAction label reflects executed action

- GIVEN the simulation is running
- WHEN `CycleOrchestrator` fires `ActionExecuted` with a `SimulationAction`
- THEN `MainViewModel.UpdateLastAction(SimulationAction)` is invoked
- AND `LastActionText` shows the input type and timestamp of the action

---

### Requirement: Real-time NextScheduled Updates

`NextScheduledText` MUST update to show time remaining until the next scheduled action whenever `CycleOrchestrator.NextScheduledUpdated` fires.

Format: `"in {N}s"` — e.g. `"in 45s"`.

#### Scenario: NextScheduled label reflects upcoming action time

- GIVEN the simulation is running
- WHEN `CycleOrchestrator` fires `NextScheduledUpdated` with a `DateTime`
- THEN `MainViewModel.UpdateNextScheduled(DateTime)` is invoked
- AND `NextScheduledText` shows the seconds remaining until that time

---

### Requirement: Activity Log Live Entries

`LogText` MUST receive new entries for simulation start, stop, and each executed action.

| Event | Log entry content |
|-------|-------------------|
| Start | Simulation started message with timestamp |
| Stop | Simulation stopped message with timestamp |
| Action executed | Action type and timestamp |

#### Scenario: Log entry on simulated action

- GIVEN the simulation is running and `ActionExecuted` fires
- WHEN `UpdateLastAction` processes the event
- THEN a new entry is appended to `LogText` describing the action type

#### Scenario: Log entry on simulation start

- GIVEN `StartAsync` is called
- WHEN simulation begins
- THEN a start entry is appended to `LogText`

#### Scenario: Log entry on simulation stop

- GIVEN `StopAsync` is called
- WHEN simulation ends
- THEN a stop entry is appended to `LogText`

---

### Requirement: Event Subscription Lifecycle

`MainViewModel` MUST subscribe to `CycleOrchestrator` events in `StartAsync` and MUST unsubscribe all of them in `StopAsync` to prevent memory leaks and stale updates.

| Phase | Action |
|-------|--------|
| `StartAsync` | Subscribe `IdleTimeUpdated`, `ActionExecuted`, `NextScheduledUpdated` |
| `StopAsync` | Unsubscribe all three events |

#### Scenario: Events subscribed on start

- GIVEN `MainViewModel.StartAsync()` is called
- WHEN the use case begins execution
- THEN `CycleOrchestrator.IdleTimeUpdated`, `ActionExecuted`, and `NextScheduledUpdated` are all subscribed

#### Scenario: Events unsubscribed on stop

- GIVEN the simulation is running with all three events subscribed
- WHEN `MainViewModel.StopAsync()` is called
- THEN all three subscriptions to `CycleOrchestrator` are removed
- AND no further UI updates occur after stop
