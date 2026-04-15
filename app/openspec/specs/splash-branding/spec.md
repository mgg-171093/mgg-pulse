# Splash Branding Specification

## Requirements

### Requirement: Minimum Branded Splash Hold

The system MUST show a branded splash before the shell and SHALL keep it visible for at least 5 seconds while startup work completes. The splash MUST display the current app version.

#### Scenario: Fast startup still shows full splash

- GIVEN startup initialization completes in under 5 seconds
- WHEN the splash is shown
- THEN the splash remains visible until the 5-second minimum is reached
- AND the shell opens only after that minimum hold ends
