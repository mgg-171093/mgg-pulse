# Tasks — 2026-04-16-updater-relaunch-hardening

> Derived during apply because `tasks.md` artifact was missing. Batch was inferred from verified proposal/spec/design scope.

## Phase 1: Updater relaunch hardening (minimal implementation batch)

- [x] 1.1 Add startup retry behavior to `UpdateHostedService` for transient failures while keeping periodic checks single-attempt.
- [x] 1.2 Add/adjust unit tests for startup retry success/exhaustion and periodic non-retry behavior.
- [x] 1.3 Ensure silent installer launcher uses `/SILENT /RESTARTAPPLICATION` and add unit coverage for launch arguments.
- [x] 1.4 Ensure Inno Setup `[Run]` entry relaunches app in silent installs (remove `skipifsilent`).
- [x] 1.5 Ensure start-minimized path activates then hides `MainWindow` to keep process alive after splash.
- [x] 1.6 Add regression test asserting activate+hide startup-minimized sequence in `App.xaml.cs`.
- [x] 1.7 Preserve manual update-check behavior (existing About/manual flow test remains passing).
