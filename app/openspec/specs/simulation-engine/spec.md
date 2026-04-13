# Simulation Engine Specification

## Purpose

Define el comportamiento del motor de simulación basado en reglas que decide cuándo y cómo ejecutar input simulado, dependiendo del modo de operación y el estado del sistema.

## Requirements

### Requirement: Rule-Based Execution Decision

El sistema MUST evaluar un conjunto de reglas antes de cada ciclo de simulación. La ejecución MUST ocurrir solo cuando todas las reglas relevantes al modo activo retornan `ShouldExecute = true`.

#### Scenario: Intelligent mode — user is idle

- GIVEN el modo es `Intelligent` y el idle time supera el threshold configurado
- WHEN el RuleEngine evalúa el ciclo
- THEN `RuleResult.ShouldExecute` es `true`
- AND `RuleResult.Reason` contiene "idle threshold exceeded"

#### Scenario: Intelligent mode — user is active

- GIVEN el modo es `Intelligent` y el idle time es menor al threshold
- WHEN el RuleEngine evalúa el ciclo
- THEN `RuleResult.ShouldExecute` es `false`
- AND la simulación NO se ejecuta

#### Scenario: Aggressive mode — always executes

- GIVEN el modo es `Aggressive`
- WHEN el RuleEngine evalúa el ciclo independientemente del idle time
- THEN `RuleResult.ShouldExecute` es `true`
- AND la `IdleRule` es bypassed

#### Scenario: Interval rule blocks premature execution

- GIVEN el tiempo desde la última simulación es menor al `MinSeconds` del `IntervalRange`
- WHEN el RuleEngine evalúa el ciclo
- THEN `RuleResult.ShouldExecute` es `false`
- AND `RuleResult.Reason` contiene "interval not elapsed"

### Requirement: Randomized Interval

El sistema SHOULD esperar un intervalo aleatorio entre `IntervalRange.MinSeconds` y `IntervalRange.MaxSeconds` entre ciclos de simulación cuando el rango min != max.

#### Scenario: Random interval within range

- GIVEN `IntervalRange(30, 60)` configurado
- WHEN el CycleOrchestrator calcula el próximo intervalo
- THEN el delay es un valor entre 30 y 60 segundos (inclusive)

#### Scenario: Fixed interval when min equals max

- GIVEN `IntervalRange(30, 30)` configurado
- WHEN el CycleOrchestrator calcula el próximo intervalo
- THEN el delay es exactamente 30 segundos

### Requirement: Cancellable Execution Loop

El sistema MUST detener el loop de simulación inmediatamente cuando se recibe una señal de cancelación.

#### Scenario: Cancellation during wait

- GIVEN la simulación está corriendo y esperando el siguiente intervalo
- WHEN se invoca `StopSimulationUseCase`
- THEN el loop se cancela sin ejecutar más acciones
- AND el `SimulationSession` registra el tiempo de fin

#### Scenario: Cancellation during rule evaluation

- GIVEN la simulación está en proceso de evaluar reglas
- WHEN el `CancellationToken` es cancelado
- THEN la evaluación se interrumpe y el loop termina limpiamente

### Requirement: Session Audit Trail

El sistema MUST registrar cada `SimulationAction` ejecutada dentro de la `SimulationSession` activa.

#### Scenario: Action recorded on execution

- GIVEN la simulación está activa y el RuleEngine permite ejecución
- WHEN se ejecuta una acción de input simulado
- THEN la acción se agrega a `SimulationSession.Actions`
- AND la acción contiene: tipo de input, timestamp, y la regla que la disparó
