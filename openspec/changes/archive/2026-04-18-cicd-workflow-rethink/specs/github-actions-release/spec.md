# github-actions-release Specification

## Purpose

Define dry-run release validation for pull requests and deterministic publishing for `main`.

## Requirements

### Requirement: Side-effect-free release validation

The release validation workflow for pull requests to `main` MUST validate release prerequisites without publishing assets, creating releases or tags, mutating committed files, or pushing commits.

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

### Requirement: Deterministic main publishing

The publish workflow on pushes to `main` MUST build a single release artifact from the already-committed main revision, compute checksum and download metadata from that exact artifact, and publish only after all pre-publish steps succeed. It MUST NOT leave `main` half-mutated when any publish step fails.

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

### Requirement: latest.json remains repository state

The system MUST keep `app/build/latest.json` committed on `main` and MUST NOT publish `latest.json` as a release asset.

#### Scenario: Manifest is updated after successful publish

- GIVEN release publication succeeds
- WHEN release metadata is finalized
- THEN `app/build/latest.json` SHALL be committed on `main` with the release version, URL, and SHA-256
- AND GitHub Release assets SHALL contain only distributable installer artifacts

### Requirement: Dependency minimization for publishing

The publish workflow SHOULD prefer GitHub-provided actions, repository-contained scripts, and pinned tooling paths over flaky external package feeds where possible.

#### Scenario: Packaging tool acquisition is chosen

- GIVEN release packaging needs non-default tooling
- WHEN a stable pinned or repository-contained path exists
- THEN the workflow SHOULD use that path before relying on a mutable external feed
