# Spec Delta: main-dashboard — ch-003-crash-diagnostics

## Delta Context

Este archivo contiene **únicamente el delta** respecto a `openspec/specs/main-dashboard/spec.md`.
Solo se modifica el Requirement "Status Display" con un constraint de implementación de binding.

## DELTA — Requirement: Status Display

### REQ-DELTA: Status indicator color binding MUST use strongly-typed SolidColorBrush

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
