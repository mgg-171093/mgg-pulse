# Exploration: ch-002-winappsdk-upgrade — Windows App SDK 1.5 → 1.8 Upgrade

**Date**: 2026-04-08  
**Status**: Complete

---

## Current State

The project uses `Microsoft.WindowsAppSDK 1.5.240607001` referenced exclusively in the UI project.
The runtime installed on this machine is 1.6, 1.7, and 1.8 — runtime 1.5 is NOT installed.
This causes a `System.DllNotFoundException: Unable to load DLL 'Microsoft.ui.xaml.dll'` at startup because the framework-dependent app cannot find the matching runtime DLL.

### Current package versions in `MGG.Pulse.UI.csproj`

| Package | Current Version |
|---|---|
| `Microsoft.WindowsAppSDK` | `1.5.240607001` |
| `Microsoft.Windows.SDK.BuildTools` | `10.0.22621.756` |
| `CommunityToolkit.Mvvm` | `8.2.2` |
| `Microsoft.Extensions.DependencyInjection` | `8.0.0` |

### Latest stable versions available on NuGet (as of 2026-04-08)

| Package | Latest Stable |
|---|---|
| `Microsoft.WindowsAppSDK` | **1.8.260317003** (1.8.6, released 2026-03-18) |
| `Microsoft.Windows.SDK.BuildTools` | **10.0.28000.1721** |

### Installed runtimes on this machine

```
Microsoft.WindowsAppRuntime.1.6   6000.519.329.0
Microsoft.WindowsAppRuntime.1.7   7000.785.2325.0  (multiple versions)
Microsoft.WindowsAppRuntime.1.8   8000.806.2252.0  (latest: 1.8.6)
```

---

## Affected Areas

| File | Reason |
|---|---|
| `src/MGG.Pulse.UI/MGG.Pulse.UI.csproj` | **Only file that needs to change** — holds both `Microsoft.WindowsAppSDK` and `Microsoft.Windows.SDK.BuildTools` references |
| `src/MGG.Pulse.Infrastructure/MGG.Pulse.Infrastructure.csproj` | No WindowsAppSDK reference — **not affected** |
| `src/MGG.Pulse.Domain/MGG.Pulse.Domain.csproj` | No WindowsAppSDK reference — **not affected** |
| `src/MGG.Pulse.Application/MGG.Pulse.Application.csproj` | No WindowsAppSDK reference — **not affected** |
| `tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj` | No WindowsAppSDK reference — **not affected** |

### Architecture constraint verification ✅

The hexagonal architecture dependency rule is intact:
- Domain: zero NuGet refs, zero project refs ✅
- Application: only references Domain ✅
- Infrastructure: only references Domain + `System.Text.Json` ✅
- Tests: only references Domain + Application ✅
- UI: references Application + Infrastructure (composition root) ✅

---

## API Compatibility Analysis (1.5 → 1.8)

The UI layer uses the following Windows App SDK APIs. All have been reviewed for breaking changes:

### `DispatcherQueue` (Microsoft.UI.Dispatching)

**Used in**:
- `MainViewModel.cs:61` — `public Microsoft.UI.Dispatching.DispatcherQueue? DispatcherQueue { get; set; }`
- `MainViewModel.cs:93,134,144,154,163` — `DispatcherQueue?.TryEnqueue(() => ...)`
- `SplashWindow.xaml.cs:102` — `DispatcherQueue.TryEnqueue(() => ...)`
- `App.g.i.cs` (generated) — `DispatcherQueueSynchronizationContext` bootstrap

**Breaking changes 1.5 → 1.8**: **None.** `DispatcherQueue.TryEnqueue` and `DispatcherQueueSynchronizationContext` APIs are stable and unchanged across 1.x releases.

### `AppWindow` / `AppWindowTitleBar` / `DisplayArea` (Microsoft.UI.Windowing)

**Used in**:
- `MainWindow.xaml.cs:18–34` — `AppWindow.Resize`, `AppWindow.SetPresenter`, `AppWindowTitleBar.IsCustomizationSupported()`, `AppWindow.TitleBar.ExtendsContentIntoTitleBar`, `DisplayArea.GetFromWindowId`, `AppWindow.Move`
- `SplashWindow.xaml.cs:24–44` — same APIs + `AppWindow.TitleBar.ButtonBackgroundColor`, `ButtonInactiveBackgroundColor`

**Breaking changes 1.5 → 1.8**: **None for these APIs.** 1.8.0 _added_ new experimental `AppWindow` placement APIs (`GetCurrentPlacement`, `SaveCurrentPlacement`, etc.) and `DisplayArea.GetMetricsFromWindowId`, but all existing APIs (`Resize`, `SetPresenter`, `Move`, `TitleBar`, `GetFromWindowId`) remain unchanged and fully compatible.

**Note**: `AppWindowTitleBar.IsCustomizationSupported()` is called as a guard before accessing TitleBar properties — this pattern remains valid in 1.8.

### `ResourceDictionary` / XAML themes (Microsoft.UI.Xaml)

**Used in**:
- `App.xaml` — `<ResourceDictionary.MergedDictionaries>` with `DarkTheme.xaml` and `LightTheme.xaml`
- `Themes/DarkTheme.xaml`, `Themes/LightTheme.xaml` — custom color resource dictionaries

**Breaking changes 1.5 → 1.8**: **None.** `ResourceDictionary.MergedDictionaries` XAML usage is unchanged. The 1.8 release notes list only additive controls (SplitMenuFlyoutItem, new AI APIs, Storage Pickers) and bug fixes to existing XAML controls — none affect the ResourceDictionary pattern used here.

### XAML Animation / Storyboard (Microsoft.UI.Xaml.Media.Animation)

**Used in**:
- `SplashWindow.xaml.cs:70–95` — `DoubleAnimation`, `Storyboard`, `CubicEase`

**Breaking changes**: **None.** Animation APIs are stable since 1.0.

### `BitmapImage` (Microsoft.UI.Xaml.Media.Imaging)

**Used in**:
- `SplashWindow.xaml.cs:59` — `new BitmapImage(new Uri(logoPath))`

**Breaking changes**: **None.**

---

## Breaking Changes Summary: 1.5 → 1.8

After reviewing the 1.6, 1.7, and 1.8 release notes, **there are zero breaking changes affecting the APIs used in MGG Pulse**. The changes in 1.8 are:

1. **NuGet metapackage refactor** (1.8.0): The SDK package is now a metapackage pointing to component packages. This is transparent to consumers — the `Microsoft.WindowsAppSDK` package reference works exactly the same way.
2. **PackageManagement capability** (1.8.0): Required for AppContainer-packaged apps. MGG Pulse uses `WindowsPackageType=None` (unpackaged) — **not affected**.
3. **BuildTools.MSIX refactored to standalone package** (1.8.0): Only relevant if using MSIX publishing. MGG Pulse is unpackaged — **not affected**.
4. **New additive APIs only**: All 1.8.x releases add new AI, Storage Pickers, and ML APIs — purely additive, no removals or renames.

---

## Microsoft.Windows.SDK.BuildTools — Version Upgrade Assessment

**Current**: `10.0.22621.756`  
**Latest stable**: `10.0.28000.1721`

**Should it be upgraded?** Yes. The BuildTools package provides Windows SDK headers and tools consumed during the build. Upgrading alongside the SDK is the recommended approach per Microsoft guidance. The version schema (`10.0.NNNNN.BBBB`) tracks Windows SDK releases, not Windows App SDK releases — they are versioned independently but should be kept current.

**Risk**: Low. BuildTools upgrades are non-breaking for framework-dependent WinUI apps — they only affect build tooling, not runtime behavior.

---

## Approaches

### Option A — Pin to 1.8.6 (latest stable) + upgrade BuildTools ✅ RECOMMENDED

Bump both:
```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260317003" />
<PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.28000.1721" />
```

- **Pros**: Matches the runtime already installed (1.8.6 = `8000.806.2252.0`), gets all 1.8.x bug fixes including XAML and windowing fixes, aligns with current machine state perfectly, future-proof until 2.0
- **Cons**: Slightly larger SDK download on first restore; 1.8 is newer than the originally shipped version (low risk)
- **Effort**: Very Low — 2-line edit in a single `.csproj`

### Option B — Target any installed runtime (1.6 or 1.7)

Bump to `1.6.x` or `1.7.x` to match an older installed runtime.

- **Pros**: Even smaller diff from 1.5
- **Cons**: Not the latest stable — still needs a NuGet restore, doesn't leverage the best-available runtime (1.8.6), creates unnecessary version sprawl, and the machine may not retain 1.6/1.7 after Windows updates
- **Effort**: Same

### Option C — Install 1.5 runtime separately (without changing NuGet)

Install the missing 1.5 runtime installer manually on the machine.

- **Pros**: Zero code changes
- **Cons**: Runtime 1.5 is end of life (released June 2024), requires installing an outdated runtime manually, does not solve the problem for future developers or CI machines, completely wrong direction for a maintained project
- **Effort**: Medium (manual machine setup, non-repeatable)

---

## Recommendation

**Use Option A: upgrade `Microsoft.WindowsAppSDK` to `1.8.260317003` and `Microsoft.Windows.SDK.BuildTools` to `10.0.28000.1721`** in `src/MGG.Pulse.UI/MGG.Pulse.UI.csproj`.

This is a zero-risk, minimal-change fix. The upgrade path 1.5 → 1.8 has no breaking changes for any API currently used. The change is isolated to two version strings in a single file. No source code modifications are required.

---

## Risks

- **Low**: NuGet restore downloads updated packages (~200 MB for the SDK metapackage). Handled automatically by `dotnet restore`.
- **Low**: If a CI/CD pipeline later targets a machine without 1.8 runtime, it needs the runtime installed. But for a personal-use V1 desktop app, this is acceptable — and the same issue exists for all versions.
- **None**: No API breaking changes. No source code changes required. No architectural layer impact.

---

## Files to Change

```
src/MGG.Pulse.UI/MGG.Pulse.UI.csproj
  Line 18: "1.5.240607001"  →  "1.8.260317003"
  Line 19: "10.0.22621.756" →  "10.0.28000.1721"
```

---

## Ready for Proposal

**Yes.** Scope is crystal clear: 2-line change in 1 file, zero source code impact, zero architectural impact. The proposal and task breakdown will be trivial.
