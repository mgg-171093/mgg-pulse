# Startup Update Check Specification

## Purpose

Definir cómo MGG Pulse maneja una actualización detectada durante el inicio.

## Requirements

### Requirement: Startup Update Confirmation

The system MUST request explicit user confirmation before applying a startup-detected update when that update can be installed.

#### Scenario: Prompt before startup apply

- GIVEN startup detects an update that can be applied
- WHEN the app can safely present a confirmation dialog
- THEN the app MUST show a prompt before starting installation
- AND the prompt MUST offer `Actualizar` and `Cancelar`

#### Scenario: Cancel defers installation

- GIVEN the startup confirmation prompt is visible
- WHEN the user chooses `Cancelar` or dismisses the prompt
- THEN the app MUST continue without starting installation
- AND the same update MAY be offered again on a later manual check or next app launch

### Requirement: Safe Startup Dialog Degradation

The system MUST degrade safely when startup cannot present the confirmation dialog.

#### Scenario: No safe dialog host at startup

- GIVEN startup detects an update that can be applied
- AND no safe dialog host is available
- WHEN the app evaluates whether to show the prompt
- THEN the app MUST NOT crash
- AND the app MUST NOT start installation during that startup cycle
