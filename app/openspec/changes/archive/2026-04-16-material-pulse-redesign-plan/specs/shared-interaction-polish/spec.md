# Shared Interaction Polish Specification

## Purpose

Define shared control polish for the Material Pulse redesign.

## Requirements

### Requirement: Consistent action control variants

The system MUST provide consistent visual variants for shared buttons, and icon usage SHALL follow a single rule set so icon-bearing actions remain recognizable and text-bearing actions remain legible. Icon-only actions MUST use dedicated icon-button treatment distinct from standard buttons.

#### Scenario: Button variants remain consistent

- GIVEN primary and supporting actions are shown together
- WHEN the controls are rendered
- THEN each action uses its assigned shared variant
- AND icon placement follows the shared rule set

#### Scenario: Icon-only actions remain distinct

- GIVEN an icon-only action is rendered
- WHEN the user compares it with a standard button
- THEN the icon button has dedicated visual treatment and clear hit-target affordance

### Requirement: Standardized shared surfaces and states

The system MUST standardize elevated and outlined surface treatments for shared controls and containers. Interactive controls MUST expose consistent hover, pressed, disabled, and focus states, SHALL use pointer/hand cursor for clickable affordances, and SHOULD provide subtle motion feedback.

#### Scenario: Shared controls use aligned states

- GIVEN multiple interactive controls are visible
- WHEN the user hovers, presses, focuses, or disables them
- THEN each control shows a consistent interaction-state model

#### Scenario: Shared surfaces use standard treatments

- GIVEN cards or similar surfaces are rendered
- WHEN the design system resolves their surface treatment
- THEN elevated and outlined surfaces use the approved shared treatments

### Requirement: Deferred glass premium direction

The system MUST NOT introduce Glass Material Premium visuals, blur layers, or partial glass styling as part of this change. That direction MAY be specified in a future change only.

#### Scenario: Current redesign excludes glass scope

- GIVEN the Material Pulse redesign for this change
- WHEN visual outcomes are reviewed
- THEN no Glass Material Premium treatment is required or partially shipped
