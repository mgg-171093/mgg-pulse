# Input Simulation Specification

## Purpose

Define el comportamiento de la simulación de input de usuario de manera segura y no intrusiva, usando Win32 SendInput.

## Requirements

### Requirement: Safe Mouse Simulation

El sistema MUST simular movimiento de mouse de no más de 2 píxeles en dirección aleatoria cuando el `InputType` es `Mouse` o `Combined`.

#### Scenario: Mouse movement within safe range

- GIVEN `InputType` es `Mouse` o `Combined`
- WHEN se ejecuta una acción de simulación
- THEN el mouse se mueve entre 1 y 2 píxeles en X o Y
- AND el movimiento es relativo (no absoluto) para no reposicionar el cursor

#### Scenario: Mouse does not move on keyboard-only

- GIVEN `InputType` es `Keyboard`
- WHEN se ejecuta una acción de simulación
- THEN NO se envía ningún evento de mouse

### Requirement: Safe Keyboard Simulation

El sistema MUST simular solo teclas no intrusivas (`Shift` o `Ctrl`) cuando el `InputType` es `Keyboard` o `Combined`.

#### Scenario: Non-intrusive key press

- GIVEN `InputType` es `Keyboard` o `Combined`
- WHEN se ejecuta una acción de simulación
- THEN se envía un key-down seguido de key-up de `VK_SHIFT` o `VK_CONTROL`
- AND la tecla simulada NO produce texto ni altera el estado de la aplicación activa

### Requirement: Simulation Does Not Interrupt User Activity

El sistema MUST NOT ejecutar input simulado si el usuario está activo (Intelligent mode).

#### Scenario: User typing — no simulation

- GIVEN modo `Intelligent` y el usuario escribió en los últimos 30 segundos
- WHEN el ciclo evalúa si ejecutar
- THEN la `IdleRule` bloquea la ejecución
- AND no se envía ningún evento de input
