# Proposal: Material Pulse Redesign Plan

## Intent

MGG Pulse lacks the premium, tactile feel its positioning demands. The sidebar merges visually into the body, controls have no consistent pointer affordances, hover/active states are flat, and there is no light theme or Appearance toggle. This planning change establishes the phased redesign direction — Material Pulse — so subsequent implementation changes can be approved and executed with a clear, shared contract.

## Scope

### In Scope
- Phase 1 — **Design System & Tokens**: New Light/Dark Resource Dictionaries, full token mapping, Appearance menu in shell for theme switching.
- Phase 2 — **Shell & Navigation**: Sidebar as a distinct elevated surface using `Sidebar*` tokens; visual separation from content frame.
- Phase 3 — **Component Polish**: Richer hover/active/focus states on Buttons, Cards, and interactive controls; consistent hand/pointer cursors; micro-motion.
- Persist selected theme (Light/Dark) via `IConfigRepository` / `SettingsViewModel`.
- Green 500 (`#4CAF50`) remains primary accent in both palettes.
- Light palette as defined in exploration (19 tokens).
- Existing dark palette remains baseline dark direction.

### Out of Scope
- Glass Material Premium (deferred — future Stage 2).
- New pages or features beyond theme infrastructure.
- MSIX packaging, auto-update, E2E automation.

## Capabilities

### New Capabilities
- `appearance-theme-switching`: User-facing Appearance menu (Light/Dark) with persistence via settings.

### Modified Capabilities
- `shell-navigation`: Sidebar elevation, surface distinction, Appearance menu entry.
- `settings-management`: Theme preference storage and retrieval.

## Approach

Phased UI Redesign (recommended by exploration):
1. Establish token foundation (Resource Dictionaries) and theme-switching infrastructure first — prevents per-component inconsistency.
2. Apply shell/sidebar changes once tokens exist — guarantees sidebar uses the right surface tokens.
3. Polish controls last — richer states built on top of a stable token layer.

Theme switching logic in `SettingsViewModel` / a `IThemeService` port; UI re-applies the root `RequestedTheme` at runtime. Hand cursor applied via global implicit style or attached behavior on `ButtonBase`.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `app/src/MGG.Pulse.UI/App.xaml` | Modified | Root theme resource dictionary merge |
| `app/src/MGG.Pulse.UI/Styles/` | New | LightTheme.xaml, DarkTheme.xaml token dictionaries |
| `app/src/MGG.Pulse.UI/Views/ShellPage.xaml` | Modified | Sidebar elevation, Appearance menu |
| `app/src/MGG.Pulse.UI/ViewModels/SettingsViewModel.cs` | Modified | Theme preference property + persistence |
| `app/src/MGG.Pulse.UI/Controls/` | Modified | Button, Card styles — hover/active/focus/cursor |
| `app/src/MGG.Pulse.Domain/` | None | No domain changes |
| `app/src/MGG.Pulse.Infrastructure/` | Minor | Config schema: add `AppearanceTheme` field |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| WinUI 3 `NavigationView` template complexity — overriding sidebar styles is verbose | Med | Prefer implicit styles + token overrides over full re-template where possible |
| Hand cursor not applied globally — some controls missed | Med | Audit via global `ButtonBase` implicit style; add to style checklist |
| Theme switch flicker on startup (race between resource load and window render) | Low | Apply `RequestedTheme` in `App.xaml.cs` before first window activation |
| MVVM boundary blurred — theme logic leaking into code-behind | Low | Encapsulate in `IThemeService`; ViewModel calls service, view reacts via binding |

## Rollback Plan

This is a planning-only change — no code is modified. If the plan is rejected:
- Delete or archive this change folder.
- Revert `exploration.md` to draft status.

When implementation begins (future changes), each phase ships independently. Rolling back a phase means reverting its Resource Dictionary merge in `App.xaml` and restoring the prior style files — no domain or infrastructure changes are required for Phases 1–3.

## Dependencies

- Exploration: `app/openspec/changes/2026-04-16-material-pulse-redesign-plan/exploration.md`
- Archived prior attempt: `app/openspec/changes/archive/2026-04-15-pulse-material-ui-refinement/` (reference for prior specs on `appearance-theme-toggle` and `application-shell-visual-refinement`)

## Success Criteria

- [ ] Light and Dark Resource Dictionaries exist and all tokens resolve without fallback.
- [ ] Appearance menu allows toggling Light/Dark; preference persists across restarts.
- [ ] Sidebar is visually distinct from the content frame in both themes.
- [ ] All interactive controls show hand/pointer cursor on hover.
- [ ] No regressions in existing functional behavior (idle detection, simulation engine, tray).
- [ ] Glass Material Premium remains explicitly deferred with no partial artifacts merged.
