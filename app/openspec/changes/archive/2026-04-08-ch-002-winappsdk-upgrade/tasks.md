# Tasks: ch-002-winappsdk-upgrade

## Phase 1 — Dependency Bump

- [x] T-01: Bump Microsoft.WindowsAppSDK from 1.5.240607001 to 1.8.260317003 in MGG.Pulse.UI.csproj
- [x] T-02: Bump Microsoft.Windows.SDK.BuildTools from 10.0.22621.756 to 10.0.28000.1721 in MGG.Pulse.UI.csproj
- [x] T-03: dotnet restore — verify restore succeeds
- [x] T-04: dotnet build — verify 0 errors
- [x] T-05: dotnet test — verify 32/32 pass
