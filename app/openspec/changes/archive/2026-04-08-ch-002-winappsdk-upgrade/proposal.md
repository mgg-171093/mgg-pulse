# Proposal: ch-002-winappsdk-upgrade — Windows App SDK 1.5 → 1.8 Upgrade

**Date**: 2026-04-08  
**Status**: Proposed

---

## Intent

Fix `System.DllNotFoundException: Unable to load DLL 'Microsoft.ui.xaml.dll'` at startup. The project references `Microsoft.WindowsAppSDK 1.5` but runtime 1.5 is not installed on the machine (only 1.6, 1.7, 1.8). Aligning the NuGet reference to the installed runtime resolves the crash with zero source code changes.

---

## Scope

### In Scope
- Upgrade `Microsoft.WindowsAppSDK` from `1.5.240607001` → `1.8.260317003` in `MGG.Pulse.UI.csproj`
- Upgrade `Microsoft.Windows.SDK.BuildTools` from `10.0.22621.756` → `10.0.28000.1721` in `MGG.Pulse.UI.csproj`

### Out of Scope
- Upgrading `CommunityToolkit.Mvvm` or `Microsoft.Extensions.DependencyInjection` (stable, no issue)
- Any source code changes to Domain, Application, Infrastructure, or UI layers
- MSIX packaging or installer changes
- CI/CD runtime installation automation

---

## Capabilities

### New Capabilities
None

### Modified Capabilities
None

> This is a pure dependency version bump. No spec-level behavior changes. No new or modified capabilities.

---

## Approach

**Option A — Pin to 1.8.6 (latest stable, already installed):**

Edit 2 lines in `src/MGG.Pulse.UI/MGG.Pulse.UI.csproj`:
```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260317003" />
<PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.28000.1721" />
```

Run `dotnet restore` to pull updated packages. All APIs used (DispatcherQueue, AppWindow, ResourceDictionary, BitmapImage, Storyboard) are verified stable and unchanged across 1.5 → 1.8.

---

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `src/MGG.Pulse.UI/MGG.Pulse.UI.csproj` | Modified | 2 version strings updated |
| `src/MGG.Pulse.Domain/` | None | No WindowsAppSDK reference |
| `src/MGG.Pulse.Application/` | None | No WindowsAppSDK reference |
| `src/MGG.Pulse.Infrastructure/` | None | No WindowsAppSDK reference |
| `tests/MGG.Pulse.Tests.Unit/` | None | No WindowsAppSDK reference |

---

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| NuGet restore downloads ~200 MB SDK metapackage | Low | Automatic on `dotnet restore`; one-time download |
| Future CI machine without 1.8 runtime | Low | Personal-use V1; same concern exists for any version |
| API breaking change in 1.5 → 1.8 | None | All used APIs verified stable across releases |

---

## Rollback Plan

Revert the 2 version strings in `MGG.Pulse.UI.csproj` to their original values:
- `Microsoft.WindowsAppSDK` → `1.5.240607001`
- `Microsoft.Windows.SDK.BuildTools` → `10.0.22621.756`

Run `dotnet restore`. App returns to pre-change state. (Note: runtime 1.5 must be manually installed for the reverted app to run.)

---

## Dependencies

- Windows App Runtime 1.8 already installed on the target machine (`8000.806.2252.0`) — no action required

---

## Success Criteria

- [ ] `dotnet restore` completes without error
- [ ] `dotnet build` completes without error or warning related to SDK version
- [ ] Application launches without `DllNotFoundException`
- [ ] All 32 existing unit tests continue to pass (`dotnet test`)
- [ ] No new source code changes required beyond the `.csproj` version bump
