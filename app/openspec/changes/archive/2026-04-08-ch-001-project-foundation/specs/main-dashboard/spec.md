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

### Requirement: Mode Selection

El sistema MUST permitir al usuario seleccionar el modo de simulación (Intelligent / Aggressive / Manual) desde la UI.

#### Scenario: Mode change takes effect on next cycle

- GIVEN la simulación está activa en modo `Intelligent`
- WHEN el usuario selecciona `Aggressive`
- THEN el cambio se aplica en el próximo ciclo de evaluación
- AND la config se persiste automáticamente

### Requirement: Configuration Panel

El sistema MUST permitir configurar el tipo de input (Mouse / Keyboard / Combined) y el intervalo (fijo o rango aleatorio).

#### Scenario: Set input type

- GIVEN el usuario selecciona "Keyboard" en el panel de configuración
- WHEN se confirma
- THEN la simulación usará solo eventos de teclado

#### Scenario: Set random interval range

- GIVEN el usuario configura min=30s y max=60s
- WHEN se guarda la config
- THEN el CycleOrchestrator usará intervalos aleatorios entre 30 y 60 segundos

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

### Requirement: Stealth Options

El sistema MUST ofrecer opciones de inicio silencioso: "Start with Windows", "Start minimized", "Minimize to tray".

#### Scenario: Start with Windows persisted

- GIVEN el usuario habilita "Start with Windows"
- WHEN se guarda la configuración
- THEN se registra una entrada en el registro de Windows para inicio automático
