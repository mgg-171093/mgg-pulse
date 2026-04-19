# Delta for github-actions-release

## MODIFIED Requirements

### Requirement: Side-effect-free release validation

The release validation workflow for pull requests to `main` MUST validate release prerequisites without publishing assets, creating releases or tags, mutating committed files, or pushing commits. When automated tests run in this workflow, the test step MUST target the CI-safe test project explicitly and MUST NOT rely on UI/WinRT-bound test discovery.
(Previously: Release validation was dry-run only, but did not require an explicit CI-safe test target.)

#### Scenario: Pull request readiness is dry-run only

- GIVEN a pull request targets `main`
- WHEN release validation runs
- THEN packaging prerequisites and release metadata inputs SHALL be checked in dry-run form
- AND no release, tag, or repository mutation SHALL occur

#### Scenario: Dry-run validation fails safely

- GIVEN release-readiness validation fails
- WHEN the workflow stops
- THEN the pull request MUST be blocked from passing release validation
- AND `main` SHALL remain unchanged

#### Scenario: Release validation uses CI-safe project

- GIVEN release-readiness validation executes tests
- WHEN the hosted runner reaches the test step
- THEN it MUST invoke the CI-safe test project explicitly
- AND UI/WinRT-bound tests SHALL remain outside hosted discovery

### Requirement: Deterministic main publishing

The publish workflow on pushes to `main` MUST build a single release artifact from the already-committed main revision, compute checksum and download metadata from that exact artifact, and publish only after all pre-publish steps succeed. It MUST use the CI-safe test project explicitly for hosted automated tests and MUST NOT leave `main` half-mutated when any publish step fails.
(Previously: Deterministic publishing required pre-publish success, but did not require an explicit CI-safe test target.)

#### Scenario: Publish succeeds from one authoritative artifact

- GIVEN a push to `main` contains releaseable version metadata
- WHEN publishing completes successfully
- THEN the release asset, checksum, and manifest SHALL reference the same built artifact
- AND repository mutation SHALL occur only after publish prerequisites succeed

#### Scenario: Publish fails before repository mutation

- GIVEN artifact creation, checksum generation, or release creation fails
- WHEN the publish workflow aborts
- THEN no new commit SHALL be pushed back to `main`
- AND previously committed repository state SHALL remain authoritative

#### Scenario: Main publish uses CI-safe project

- GIVEN the publish workflow runs on a hosted runner
- WHEN pre-publish tests execute
- THEN the workflow MUST target the CI-safe test project explicitly
- AND only CI-safe coverage SHALL gate publishing
