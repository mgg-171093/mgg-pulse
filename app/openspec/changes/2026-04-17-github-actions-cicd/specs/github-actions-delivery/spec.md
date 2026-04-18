# GitHub Actions Delivery Specification

## Purpose

Define repository automation that validates changes safely and publishes desktop releases only from eligible `main` branch pushes.

## Requirements

### Requirement: Develop and Pull Request CI Validation

The repository MUST run CI validation on pushes to `develop` and on pull requests targeting `develop` or `main` when workflow, build, test, or release-related files change. Validation MUST restore dependencies, execute automated tests, and validate workflow syntax. CI validation MUST NOT publish release artifacts or mutate repository metadata.

#### Scenario: Develop push runs validation

- GIVEN a commit is pushed to `develop`
- WHEN the CI workflow starts
- THEN dependencies are restored, automated tests run, and workflow definitions are validated
- AND no tag, release, version commit, or manifest publication occurs

#### Scenario: Relevant pull request runs safe validation

- GIVEN a pull request targets `develop` or `main`
- WHEN the changed files affect workflow, build, test, or release automation
- THEN the repository runs the same validation gates
- AND the pull request run remains non-publishing and metadata-safe

### Requirement: Main Pull Requests Validate Release Readiness

Pull requests targeting `main` MUST execute release-readiness validation that exercises the release pipeline logic far enough to prove readiness. These runs MUST NOT publish installers, MUST NOT create tags or releases, and MUST NOT commit version or `latest.json` changes to the repository.

#### Scenario: Main PR checks release readiness without side effects

- GIVEN a pull request targets `main`
- WHEN release-readiness validation is executed
- THEN release prerequisites, tests, build steps, and workflow syntax are validated
- AND no artifact publication or repository mutation occurs

### Requirement: Main Pushes Perform Real Continuous Delivery

A release-eligible push to `main` MUST execute real continuous delivery. The workflow MUST bump the application version, run automated tests, build the installer, compute its SHA-256 hash, update `app/build/latest.json` on `main`, create the release tag and GitHub release, and publish the installer artifact. The `latest.json` stored on `main` SHALL remain the updater source of truth via the raw file URL, and the workflow MUST NOT substitute a release-asset manifest model.

#### Scenario: Main push publishes a release

- GIVEN a release-eligible commit is pushed to `main`
- WHEN the delivery workflow succeeds
- THEN version metadata, installer artifact, SHA-256 hash, `latest.json`, tag, and GitHub release are all published consistently
- AND `latest.json` on `main` references the published installer using the raw-main manifest model

#### Scenario: Delivery failure stops publication

- GIVEN a release-eligible commit is pushed to `main`
- WHEN tests, build, hashing, or release publication fails
- THEN the workflow reports failure before completing the remaining publish steps
- AND no partial release is treated as the new latest version

### Requirement: Automation Loop Prevention

The workflow MUST prevent infinite reruns caused by automation-authored commits or tags while still allowing normal human-driven delivery from `main`.

#### Scenario: Bot-authored metadata commit does not recurse

- GIVEN the workflow creates a commit to update version or `latest.json`
- WHEN that bot-authored event reaches `main`
- THEN the delivery workflow is skipped or short-circuited for that automation event
- AND no recursive publish loop begins
