# github-actions-ci Specification

## Purpose

Define hosted-runner validation that stays reliable for pull requests and branch validation.

## Requirements

### Requirement: Hosted-runner-safe validation

The CI workflow MUST complete on GitHub-hosted runners using restore, build, and only CI-safe automated tests. It MUST NOT require WinRT UI automation, tray interaction, installer execution, or other headless-unsafe checks.

#### Scenario: Pull request uses CI-safe suite

- GIVEN a pull request targets `develop` or `main`
- WHEN CI runs on a hosted Windows runner
- THEN restore and build SHALL execute from repository sources
- AND only CI-safe tests SHALL run

#### Scenario: Unsafe test remains local-only

- GIVEN a test depends on UI, WinRT desktop state, or installer side effects
- WHEN hosted CI is assembled
- THEN that test MUST be excluded from hosted validation
- AND CI SHALL still report status from the CI-safe suite

### Requirement: Test partitioning

The project SHALL support a split between CI-safe tests and local-only tests when a scenario cannot run reliably on hosted runners.

#### Scenario: New non-CI-safe coverage is added

- GIVEN a contributor adds a headless-unsafe test
- WHEN the workflow contract is updated
- THEN the test MUST be classified as local-only
- AND the CI-safe suite SHALL remain independently runnable in CI

### Requirement: Dependency minimization for validation

The CI workflow SHOULD avoid flaky external feeds when a GitHub-provided, runner-resident, or repository-contained option exists.

#### Scenario: Stable tooling path exists

- GIVEN a validation step needs auxiliary tooling
- WHEN a stable built-in or repository-pinned option exists
- THEN CI SHOULD use that option instead of a mutable external feed
