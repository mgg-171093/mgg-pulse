## Exploration: 2026-04-16-sidebar-exit-action

### Current State
Currently, `MGG Pulse` handles full application shutdown via `ExitApp()` in `App.xaml.cs`. This method is `private` and triggered by the tray service. Closing the `MainWindow` directly only hides it to the tray (MinimizeToTray behavior) via `OnMainWindowClosing`.
The sidebar is a `NavigationView` defined in `ShellPage.xaml`. It has `IsSettingsVisible="True"`, which automatically pins a "Configuración" item to the very bottom of the navigation pane. Any items added to `FooterMenuItems` normally appear *above* this built-in settings item.

### Affected Areas
- `app/src/MGG.Pulse.UI/App.xaml.cs` — Needs to expose `ExitApp()` (make it `internal` instead of `private`) so the UI can invoke it.
- `app/src/MGG.Pulse.UI/Views/ShellPage.xaml` — Needs a new `NavigationViewItem` for the "Salir" action. Also needs to manage the order of the footer items to ensure "Salir" is below "Configuración".
- `app/src/MGG.Pulse.UI/Views/ShellPage.xaml.cs` — Needs to handle the new action, prevent it from attempting navigation, and invoke `App.ExitApp()`.

### Approaches

1. **Manual Footer Items (Disable Built-in Settings)**
   Disable `IsSettingsVisible="False"` on the `NavigationView`. Manually add two `NavigationViewItem`s to `FooterMenuItems`: "Configuración" (Tag: "Settings") followed by "Salir" (Tag: "Exit").
   - Pros: Guarantees "Salir" is exactly below "Configuración". Allows re-using the existing `args.SelectedItem` tag switch for navigation. No command bindings required; handled cleanly in code-behind.
   - Cons: Requires redefining the Settings item manually in XAML.
   - Effort: Low

2. **Add to FooterMenuItems with Built-in Settings**
   Keep `IsSettingsVisible="True"` and simply add "Salir" to `FooterMenuItems`.
   - Pros: Less XAML to write.
   - Cons: "Salir" will appear *above* "Configuración", which violates the requirement to place it below.
   - Effort: Low

3. **ViewModel Command Binding via XAML `Tapped`**
   Add an `ICommand` to `ShellViewModel` and bind it via `x:Bind` on the "Salir" item's `Tapped` event.
   - Pros: More MVVM-pure for the action execution.
   - Cons: Redundant since `ShellPage.xaml.cs` already handles all navigation/selection events centrally. `NavigationView` selection behavior might conflict with a direct `Tapped` event if not handled carefully.
   - Effort: Low/Medium

### Recommendation
**Manual Footer Items (Disable Built-in Settings)** is the best approach. It strictly fulfills the requirement of placing "Salir" *below* "Configuración". By making `ExitApp()` internal in `App.xaml.cs`, we can intercept the "Exit" tag inside `NavView_SelectionChanged` in `ShellPage.xaml.cs` to execute the full application shutdown, avoiding empty page navigation or complex command routing.

### Risks
- WinUI 3 `NavigationView` selection visuals might linger on "Salir" for a split second before the app closes. This is negligible.
- If `ExitApp()` is modified in the future assuming a background context, it might break. However, `ExitApp()` currently marshals to the UI dispatcher safely, so this risk is mitigated.

### Ready for Proposal
Yes. The orchestrator can proceed to update the proposal/spec based on these findings (specifically noting that `ExitApp` is private, not public, and the `FooterMenuItems` behavior requires disabling `IsSettingsVisible`).
