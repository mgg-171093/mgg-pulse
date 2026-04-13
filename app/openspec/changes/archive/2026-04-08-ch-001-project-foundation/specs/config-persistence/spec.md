# Config Persistence Specification

## Purpose

Define cómo se persiste y carga la configuración del usuario entre sesiones.

## Requirements

### Requirement: Load Configuration on Startup

El sistema MUST intentar cargar la configuración desde `%AppData%\MGG\Pulse\config.json` al iniciar. Si el archivo no existe, MUST usar valores por defecto.

#### Scenario: Config file exists

- GIVEN existe `%AppData%\MGG\Pulse\config.json` con configuración válida
- WHEN la aplicación inicia
- THEN la `SimulationConfig` se hidrata con los valores del archivo

#### Scenario: Config file missing — use defaults

- GIVEN no existe `%AppData%\MGG\Pulse\config.json`
- WHEN la aplicación inicia
- THEN se crea una `SimulationConfig` con valores por defecto: modo `Intelligent`, intervalo `(30, 60)`, input `Mouse`

#### Scenario: Config file corrupted

- GIVEN existe el archivo pero contiene JSON inválido
- WHEN la aplicación intenta cargar la config
- THEN se loguea el error y se usan los valores por defecto
- AND la app NO crashea

### Requirement: Save Configuration on Change

El sistema MUST persistir la `SimulationConfig` actualizada en `%AppData%\MGG\Pulse\config.json` cada vez que el usuario modifica una configuración.

#### Scenario: User changes mode

- GIVEN el usuario selecciona modo `Aggressive` en la UI
- WHEN se confirma el cambio
- THEN `config.json` se actualiza con `"mode": "Aggressive"`

### Requirement: Config Directory Auto-Creation

El sistema MUST crear el directorio `%AppData%\MGG\Pulse\` si no existe al intentar guardar.

#### Scenario: First run, directory missing

- GIVEN el directorio `%AppData%\MGG\Pulse\` no existe
- WHEN se guarda la configuración por primera vez
- THEN el directorio se crea automáticamente
- AND el archivo se guarda exitosamente
