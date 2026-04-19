# Delta for github-actions-ci

## MODIFIED Requirements

### Requirement: Hosted-runner-safe validation

The CI workflow MUST complete on GitHub-hosted runners using restore, build, and an explicitly named CI-safe test project whose discovery path does not load UI/WinRT-bound assemblies or types. It MUST NOT require WinRT UI automation, tray interaction, installer execution, or UI-bound test binaries during hosted validation.
(Previously: CI-safe tests were required, but the workflow did not require a discovery-safe project boundary.)

#### Scenario: Pull request uses CI-safe project

- GIVEN a pull request targets `develop` or `main`
- WHEN CI runs on a hosted Windows runner
- THEN restore and build SHALL execute from repository sources
- AND the test step SHALL target the CI-safe test project explicitly

#### Scenario: Discovery avoids UI-bound types

- GIVEN UI or WinRT-bound tests exist in the repository
- WHEN the hosted runner discovers tests for CI
- THEN discovery MUST NOT load those UI-bound tests or assemblies
- AND CI SHALL report status only from the CI-safe project

### Requirement: Test partitioning

The project SHALL support a split between a CI-safe test project and a separate local-only test project or equivalently isolated path when a scenario cannot run reliably on hosted runners. The CI-safe project MUST preserve meaningful automated coverage for Domain, Application, and Infrastructure-safe logic.
(Previously: The split allowed local-only classification but did not require an isolated path or preserved core-layer coverage.)

#### Scenario: New non-CI-safe coverage is added

- GIVEN a contributor adds a headless-unsafe test
- WHEN the workflow contract is updated
- THEN the test MUST be placed in the local-only project or isolated path
- AND the CI-safe suite SHALL remain independently runnable in CI

#### Scenario: Core logic remains covered

- GIVEN tests for Domain, Application, or Infrastructure-safe behavior exist
- WHEN the suite is split
- THEN those tests MUST remain in the CI-safe project
- AND hosted CI SHALL continue executing meaningful core-layer coverage
