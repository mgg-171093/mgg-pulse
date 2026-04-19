# Design: CI Core Test Split

## Technical Approach

Split the single `MGG.Pulse.Tests.Unit` project into two: a CI-safe project targeting `net8.0-windows` (no WinUI/WinRT) that runs Domain, Application, and Infrastructure tests, and a local-only project keeping the existing `net8.0-windows10.0.19041.0` TFM + UI reference for ViewModel/Service tests. CI workflow runs only the core project; developers run both via solution.

## Architecture Decisions

| Decision | Choice | Alternatives | Rationale |
|----------|--------|-------------|-----------|
| Split strategy | Two physical test projects | Single project with multi-TFM; trait-based filtering | Filtering failed — xUnit discovers ALL types at assembly load, pulling WinUI DLLs that hang on CI. Physical split is the only reliable boundary. |
| Core project TFM | `net8.0-windows` (no Windows SDK version suffix) | `net8.0` | Infrastructure uses `System.Windows.Forms` (Win32 interop). `net8.0-windows` compiles on CI without WinUI SDK. |
| Core project name | `MGG.Pulse.Tests.Core` | `Tests.CI`, `Tests.Unit.Core` | "Core" signals platform-independent tests; avoids confusion with "CI" which implies environment, not content. |
| Local project name | `MGG.Pulse.Tests.UI` | Keep `Tests.Unit` | Rename clarifies purpose. Old name disappears; no backward compat concern. |
| RuntimeIdentifier in core | Omit (`<RuntimeIdentifier>` removed) | Keep `win-x64` | Omitting RID lets `dotnet test` run without platform lock — critical for CI hosted runners. |

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `app/tests/MGG.Pulse.Tests.Core/MGG.Pulse.Tests.Core.csproj` | Create | TFM `net8.0-windows`, refs Domain+Application+Infrastructure only, no `RuntimeIdentifier` |
| `app/tests/MGG.Pulse.Tests.UI/MGG.Pulse.Tests.UI.csproj` | Create | Clone of current csproj (TFM `net8.0-windows10.0.19041.0`, refs all including UI) |
| `app/tests/MGG.Pulse.Tests.Unit/` | Delete | Replaced by the two new projects |
| `app/MGG.Pulse.slnx` | Modify | Replace `Tests.Unit` entry with `Tests.Core` + `Tests.UI` |
| `.github/workflows/ci.yml` | Modify | Point test step to `Tests.Core` project, remove `--filter` |
| `app/tests/MGG.Pulse.Tests.Core/Domain/**` | Move | All `Domain/` test files from old project |
| `app/tests/MGG.Pulse.Tests.Core/Application/**` | Move | All `Application/` test files |
| `app/tests/MGG.Pulse.Tests.Core/Infrastructure/**` | Move | All `Infrastructure/` test files |
| `app/tests/MGG.Pulse.Tests.UI/UI/**` | Move | All `UI/` test files (ViewModels, Services) |

## Data Flow

```
CI (GitHub Actions)                    Local (developer)
       │                                      │
       ▼                                      ▼
  Tests.Core.csproj                    Both projects via slnx
  ┌─────────────────┐                  ┌─────────────────────┐
  │ Domain tests    │                  │ Tests.Core + Tests.UI│
  │ Application     │                  └─────────────────────┘
  │ Infrastructure  │
  └────────┬────────┘
           │ refs
   Domain, Application, Infrastructure
   (no WinUI, no WinRT)
```

## Interfaces / Contracts

No new interfaces. Existing test code moves unchanged — only `using` namespace prefixes change from `MGG.Pulse.Tests.Unit.*` to `MGG.Pulse.Tests.Core.*` / `MGG.Pulse.Tests.UI.*`.

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Validation | Both projects build and tests pass | `dotnet test` each project locally |
| CI | Core tests run without hang | Push to develop, verify workflow completes |
| Regression | No test lost in migration | Count test methods before/after split |

## Migration Strategy

1. Create both new project dirs and csproj files
2. Move test files by directory (Domain/, Application/, Infrastructure/ → Core; UI/ → UI)
3. Update namespaces via find-replace (`Tests.Unit` → `Tests.Core` / `Tests.UI`)
4. Update slnx and ci.yml
5. Delete old `Tests.Unit` directory
6. Run `dotnet build app/MGG.Pulse.slnx` + `dotnet test` both projects locally
7. Push — CI runs only Tests.Core

## Open Questions

- [ ] None — design is self-contained.
