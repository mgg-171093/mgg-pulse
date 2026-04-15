# Main Dashboard Specification

## Purpose

Define el comportamiento de la interfaz principal que permite al usuario controlar y monitorear la simulación.

## Requirements

### Requirement: Status Display

El sistema MUST mostrar en todo momento el estado actual de la simulación (Active / Inactive), el idle time actual, la última acción simulada, y el próximo tiempo de ejecución estimado.

#### Scenario: Status reflects running simulation

- GIVEN la simulación está activa
- WHEN el usuario observa el dashboard
- THEN el indicador de estado muestra "Active" con color verde (#4CAF50)
- AND el idle time se actualiza en tiempo real cada segundo

#### Scenario: Status reflects stopped simulation

- GIVEN la simulación está detenida
- WHEN el usuario observa el dashboard
- THEN el indicador de estado muestra "Inactive" con color neutral
- AND "Last action" muestra "—" si no hubo acciones

### Requirement: Status Indicator Color Binding (ch-003)

El status indicator (Ellipse en el header) MUST obtener su color mediante binding directo a una
property de tipo `Microsoft.UI.Xaml.Media.SolidColorBrush` en el ViewModel.

**Constraint**: NO se DEBE usar un converter (`IValueConverter`) que retorne un value type
(`Windows.UI.Color`) boxeado como `object` en un binding `{x:Bind}` compilado sobre
`SolidColorBrush.Color`, ya que produce `InvalidCastException` en WinUI 3 runtime.

#### Scenario: Status indicator color via strongly-typed ViewModel property

- GIVEN el ViewModel expone `StatusIndicatorBrush` de tipo `SolidColorBrush`
- WHEN el XAML hace `Fill="{x:Bind ViewModel.StatusIndicatorBrush, Mode=OneWay}"`
- THEN el binding compila sin converter
- AND no ocurre `InvalidCastException` en runtime
- AND el Ellipse muestra color primario (#4CAF50) cuando `IsRunning = true`
- AND el Ellipse muestra color neutro (#2A2E45) cuando `IsRunning = false`

#### Scenario: SolidColorBrush actualizado en cambio de IsRunning

- GIVEN `IsRunning` cambia de `false` a `true`
- WHEN `OnIsRunningChanged` es invocado por CommunityToolkit.Mvvm
- THEN `StatusIndicatorBrush` es actualizado a `new SolidColorBrush(Color.FromArgb(255, 76, 175, 80))`
- AND `OnPropertyChanged(nameof(StatusIndicatorBrush))` es notificado

- GIVEN `IsRunning` cambia de `true` a `false`
- WHEN `OnIsRunningChanged` es invocado
- THEN `StatusIndicatorBrush` es actualizado a `new SolidColorBrush(Color.FromArgb(255, 42, 46, 69))`

### Requirement: Start / Stop Control

El sistema MUST proporcionar un botón primario para iniciar y detener la simulación.

#### Scenario: Start simulation

- GIVEN la simulación está detenida
- WHEN el usuario presiona "Start"
- THEN la simulación inicia
- AND el botón cambia a "Stop"
- AND el estado cambia a "Active"

#### Scenario: Stop simulation

- GIVEN la simulación está activa
- WHEN el usuario presiona "Stop"
- THEN la simulación se detiene limpiamente
- AND el botón cambia a "Start"

### Requirement: Configuration Panel

The dashboard MUST remain focused on monitoring and runtime control only. Editable settings SHALL be managed on a dedicated Settings page, and the dashboard MUST NOT embed mode, input type, interval, or startup-option forms.
(Previously: The dashboard allowed users to edit input type and interval directly in the main UI.)

#### Scenario: Dashboard excludes settings forms

- GIVEN the user opens the dashboard
- WHEN the page renders
- THEN monitoring data and start or stop controls are visible
- AND editable mode, input type, interval, and startup-option controls are not shown there

### Requirement: Real-time Log Viewer

El sistema MUST mostrar un panel de logs en tiempo real con los eventos de simulación. El nivel de verbosidad MUST ser configurable (Minimal / Normal / Verbose).

#### Scenario: Log entry on simulation action

- GIVEN el nivel de log es `Normal` o `Verbose`
- WHEN se ejecuta una acción simulada
- THEN aparece una nueva entrada en el log viewer con timestamp y descripción de la acción

#### Scenario: Minimal log suppresses routine entries

- GIVEN el nivel de log es `Minimal`
- WHEN se ejecuta una acción simulada de rutina
- THEN NO se agrega entrada al log viewer
- AND solo errores o cambios de estado se loguean

---

### Requirement: ComboBox SelectedValue Resolution (ch-004)

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

### Requirement: Real-time IdleTime Updates (ch-004)

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

### Requirement: Real-time LastAction Updates (ch-004)

`LastActionText` MUST update to describe the most recently simulated action whenever `CycleOrchestrator.ActionExecuted` fires.

Format: `"{InputType} at {HH:mm:ss}"` — e.g. `"Mouse at 14:32:05"`.

#### Scenario: LastAction label reflects executed action

- GIVEN the simulation is running
- WHEN `CycleOrchestrator` fires `ActionExecuted` with a `SimulationAction`
- THEN `MainViewModel.UpdateLastAction(SimulationAction)` is invoked
- AND `LastActionText` shows the input type and timestamp of the action

---

### Requirement: Real-time NextScheduled Updates (ch-004)

`NextScheduledText` MUST update to show time remaining until the next scheduled action whenever `CycleOrchestrator.NextScheduledUpdated` fires.

Format: `"in {N}s"` — e.g. `"in 45s"`.

#### Scenario: NextScheduled label reflects upcoming action time

- GIVEN the simulation is running
- WHEN `CycleOrchestrator` fires `NextScheduledUpdated` with a `DateTime`
- THEN `MainViewModel.UpdateNextScheduled(DateTime)` is invoked
- AND `NextScheduledText` shows the seconds remaining until that time

---

### Requirement: Activity Log Live Entries (ch-004)

`LogText` MUST receive new entries for simulation start, stop, and each executed action.

| Event | Log entry content |
|-------|-------------------|
| Start | Simulation started message |
| Stop | Simulation stopped message |
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

### Requirement: Event Subscription Lifecycle (ch-004)

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
