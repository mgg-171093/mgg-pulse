# Tasks: GitHub Actions CI/CD

> `tasks.md` was missing for this change. This file defines the minimal dependency-ready apply batch derived from proposal/spec/design.

## Phase 1: Workflow automation foundation

- [x] 1.1 Create `.github/workflows/ci.yml` for PR/develop validation (restore, build, test, workflow syntax checks) with least-privilege permissions.
- [x] 1.2 Create `.github/workflows/release.yml` with PR-to-main release-readiness validation and push-to-main real release automation on Windows runners.
- [x] 1.3 Implement release metadata automation (patch semver bump in `app/Directory.Build.props`, installer hash + URL, update `app/build/latest.json`, create GitHub release).
- [x] 1.4 Prevent workflow recursion from bot-authored metadata commits.
- [x] 1.5 Add focused unit-level workflow contract tests under `app/tests/MGG.Pulse.Tests.Unit`.
- [x] 1.6 Update docs minimally so CI/CD model and raw-main `latest.json` contract are explicit.

## Phase 2: Verify blocker remediation

- [x] 2.1 Make release metadata publication atomic: defer any push to `main` until tests/build/release creation and `latest.json` update complete successfully.
- [x] 2.2 Add workflow contract coverage asserting failure-ordering/no-early-push behavior in release workflow definition.
