# Spec: crash-diagnostics (NEW)

## Purpose

Define los requisitos de la infraestructura de diagnóstico de crashes de MGG Pulse.
Esta capability garantiza que cualquier excepción no manejada durante el bootstrap o en runtime
quede registrada en disco antes de que el proceso muera, sin depender del DI container.

## Requirements

### Requirement: REQ-01 — Global Unhandled Exception Capture

El sistema MUST registrar un handler de `Application.UnhandledException` en el constructor de `App`
**antes** de `InitializeComponent()` para capturar excepciones XAML de parse y excepciones de runtime
en el UI thread.

#### Scenario: Exception occurs after InitializeComponent

- GIVEN la app ya inicializó los componentes XAML
- WHEN ocurre una excepción no manejada en el UI thread
- THEN `OnUnhandledException` es invocado
- AND `CrashLogger.Write` escribe el tipo, mensaje y stack trace a `crash.log`
- AND la excepción queda marcada como `Handled = false` (el proceso puede terminar)

#### Scenario: Exception occurs during XAML parse

- GIVEN `Application.UnhandledException` fue registrado antes de `InitializeComponent()`
- WHEN el parser XAML lanza una excepción en el startup
- THEN el handler es invocado antes del cierre del proceso
- AND `crash.log` contiene la información de la excepción

---

### Requirement: REQ-02 — Bootstrap Crash Logging

El body completo de `OnLaunched` MUST estar envuelto en un bloque try/catch.
En caso de excepción: se MUST loggear a `crash.log` y llamar a `Exit()`.

#### Scenario: Exception during splash window initialization

- GIVEN `OnLaunched` inicia la secuencia de bootstrap (SplashWindow, DI, MainWindow)
- WHEN una excepción es lanzada en cualquier punto del body
- THEN el catch la pasa a `CrashLogger.Write(ex)`
- AND llama a `this.Exit()` para terminar el proceso limpiamente
- AND NO re-throw (evitar doble-handler con UnhandledException)

#### Scenario: DI container fails to build

- GIVEN `ConfigureServices()` lanza una excepción
- WHEN el constructor de `App` la propaga
- THEN no hay handler DI disponible
- AND `CrashLogger` MUST poder operar sin DI (no accede a `Services`)

---

### Requirement: REQ-03 — Early Static Crash Logger

El sistema MUST proveer una clase `CrashLogger` estática en `MGG.Pulse.UI`
que escriba a `%AppData%\MGG\Pulse\crash.log` sin depender de DI ni de `FileLoggerService`.

#### Scenario: Write crash entry with Exception

- GIVEN se llama `CrashLogger.Write(Exception ex)`
- WHEN el método ejecuta
- THEN escribe una entrada con formato `[YYYY-MM-DD HH:mm:ss UTC] [CRASH] ExceptionType: Message\nStackTrace`
- AND usa `File.AppendAllText` (síncrono — no async)
- AND crea el directorio `%AppData%\MGG\Pulse\` si no existe

#### Scenario: Write crash entry with plain message

- GIVEN se llama `CrashLogger.Write(string message)`
- WHEN el método ejecuta
- THEN escribe una entrada con formato `[YYYY-MM-DD HH:mm:ss UTC] [CRASH] message`

#### Scenario: CrashLogger fails silently

- GIVEN el directorio de AppData no es accesible (permisos, disco lleno)
- WHEN `CrashLogger.Write` lanza excepción internamente
- THEN la excepción es swallowed (try/catch interno)
- AND el proceso NO crashea a causa del logger
