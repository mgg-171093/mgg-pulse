# Design: Sidebar Exit Action

## Technical Approach

Add a "Salir" footer item to the NavigationView sidebar that triggers the same `ExitApp()` path used by the tray "Exit" menu item. No new ports, no domain changes — purely UI-layer wiring.

## Architecture Decisions

| Decision | Choice | Alternatives | Rationale |
|----------|--------|-------------|-----------|
| Exit trigger location | `NavigationView.FooterMenuItems` | Separate button below NavView; PaneFooter template | FooterMenuItems is the idiomatic WinUI 3 pattern for non-navigable actions at the sidebar bottom. Keeps it visually consistent with existing nav items. |
| Exit mechanism | Call `App.ExitApp()` via internal accessor | New ICommand on ShellViewModel; Event aggregator | `ExitApp()` already handles update-service teardown, tray disposal, and dispatcher marshalling. Adding a ViewModel command would duplicate logic or require exposing App internals anyway. A simple `internal` accessor keeps it contained in the UI layer. |
| Selection handling | Intercept in `NavView_SelectionChanged`, cancel navigation, restore previous selection | Tapped event on the item directly | SelectionChanged is already the single routing point. Handling it there avoids duplicate event wiring. |

## Data Flow

```
User clicks "Salir" (FooterMenuItems)
    │
    ▼
ShellPage.NavView_SelectionChanged
    │  (detects Tag == "Exit", restores previous selection)
    │
    ▼
App.RequestExit()          ← new internal static method
    │
    ▼
App.ExitApp()              ← existing private method (unchanged logic)
    │
    ├─ _updateService.StopAsync()
    ├─ TrayService.Dispose()  (background thread)
    └─ _dispatcherQueue → MainWindow.Close() → Exit()
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `app/src/MGG.Pulse.UI/Views/ShellPage.xaml` | Modify | Add `NavigationView.FooterMenuItems` with a "Salir" item (Tag="Exit", Icon=Clear/Cancel) |
| `app/src/MGG.Pulse.UI/Views/ShellPage.xaml.cs` | Modify | In `NavView_SelectionChanged`: detect Tag "Exit", restore previous selection, call `App.RequestExit()` |
| `app/src/MGG.Pulse.UI/App.xaml.cs` | Modify | Add `internal static void RequestExit()` that forwards to the instance `ExitApp()` |

## Interfaces / Contracts

```csharp
// App.xaml.cs — new static accessor (no new types needed)
internal static void RequestExit()
{
    if (Current is App app)
    {
        app.ExitApp();
    }
}
```

No new interfaces, ports, or domain types required.

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Manual | Clicking "Salir" exits the app fully (no tray residue) | Smoke test |
| Manual | "Salir" does NOT navigate away from current page before exit | Visual check |
| Manual | Sidebar visual state: "Salir" hover/focus matches existing items | Visual check |

No unit tests — this is purely UI event wiring with no testable logic.

## Migration / Rollout

No migration required.

## Open Questions

None.
