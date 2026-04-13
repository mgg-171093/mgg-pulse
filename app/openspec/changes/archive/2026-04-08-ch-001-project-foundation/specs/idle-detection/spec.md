# Idle Detection Specification

## Purpose

Define cómo el sistema detecta el tiempo de inactividad real del usuario para alimentar el Rule Engine.

## Requirements

### Requirement: Real Idle Time Detection

El sistema MUST obtener el tiempo transcurrido desde la última entrada de usuario (mouse o teclado) usando la API Win32 `GetLastInputInfo`.

#### Scenario: User recently active

- GIVEN el usuario movió el mouse hace 5 segundos
- WHEN se consulta `IIdleDetector.GetIdleTime()`
- THEN el resultado es un `TimeSpan` de aproximadamente 5 segundos

#### Scenario: User idle for extended period

- GIVEN no hubo input de usuario en los últimos 10 minutos
- WHEN se consulta `IIdleDetector.GetIdleTime()`
- THEN el resultado es un `TimeSpan` >= 10 minutos

### Requirement: Idle Time Abstracted Behind Port

El sistema MUST acceder al idle time exclusivamente a través del port `IIdleDetector`. El Domain y Application MUST NOT referenciar Win32 directamente.

#### Scenario: Mock replaces real detector in tests

- GIVEN un test unitario que usa un `IIdleDetector` mockeado
- WHEN el mock retorna `TimeSpan.FromMinutes(5)`
- THEN la `IdleRule` usa ese valor sin acceder a Win32
