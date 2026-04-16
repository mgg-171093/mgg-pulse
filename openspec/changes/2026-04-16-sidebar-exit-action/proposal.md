# Proposal: Sidebar Exit Action

## Intent

Users currently must right-click the system tray to exit MGG Pulse. There is no in-shell path to close the app, which forces users to hunt for the tray icon. Adding an "Salir" action to the sidebar's footer area exposes the same `ExitApp()` flow directly from the navigation shell.

## Scope

### In Scope
- Add a `NavigationViewItem` ("Salir") to `NavigationView.FooterMenuItems`, below the built-in Configuración item
- Wire the item's `Tapped` event to invoke `App.ExitApp()` via a command on `ShellViewModel`
- Prevent the item from being selected as a navigation target (no page navigation)
- Apply existing hover/focus/pointer visual styles (consistent with other nav items)

### Out of Scope
- Confirmation dialog before exit
- Keyboard shortcut (Alt+F4 already works)
- Changes to tray exit behavior
- Any change outside `MGG.Pulse.UI`

## Capabilities

### New Capabilities
- `sidebar-exit-action`: Sidebar footer item that exits the application

### Modified Capabilities
- None

## Approach

`NavigationView.FooterMenuItems` in WinUI 3 renders items below the built-in settings item — the correct anchor point.  
`ShellViewModel` gains an `IRelayCommand ExitCommand` (CommunityToolkit.Mvvm) that calls `Application.Current` cast to `App` and invokes `ExitApp()`.  
`ShellPage.xaml` binds the footer item's `Tapped` to the command via `x:Bind`. The item's `Tag` is set to a sentinel value and the `SelectionChanged` handler already in `ShellPage.xaml.cs` skips navigation when the tag is unrecognized — no extra guard needed beyond verifying this.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `app/src/MGG.Pulse.UI/Views/ShellPage.xaml` | Modified | Add FooterMenuItems entry |
| `app/src/MGG.Pulse.UI/Views/ShellPage.xaml.cs` | Modified | Wire pointer events for the footer item |
| `app/src/MGG.Pulse.UI/ViewModels/ShellViewModel.cs` | Modified | Add `ExitCommand` |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Footer item triggers navigation and leaves blank content frame | Low | Verify SelectionChanged guard; add explicit tag exclusion if needed |
| `App` cast fails at runtime | Low | `Application.Current` is always `App` in this codebase; no null risk |

## Rollback Plan

Revert the three file changes. No data migration, no schema change, no service impact.

## Dependencies

- None — `ExitApp()` is already public on `App`

## Success Criteria

- [ ] "Salir" item appears in the sidebar below Configuración in both dark and light themes
- [ ] Clicking "Salir" exits the process (same behavior as tray Exit)
- [ ] No unintended page navigation occurs on click
- [ ] Existing nav items and selection state are unaffected
