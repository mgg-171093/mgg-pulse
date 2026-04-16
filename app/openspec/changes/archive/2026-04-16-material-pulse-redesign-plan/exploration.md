## Exploration: Material Pulse Redesign Plan

### Current State
MGG Pulse currently has a functional UI in WinUI 3, but lacks the tactile and premium feel intended for the application. The sidebar merges visually with the main body, reducing the navigational distinction. Clickable elements lack consistent hand/pointer cursors, and the application currently only supports a baseline dark palette without a complementary light theme or an Appearance menu to switch them.

### Affected Areas
- `app/src/MGG.Pulse.UI/App.xaml` & Theme Resource Dictionaries — Will need new resource dictionaries for Light/Dark themes and token mappings.
- `app/src/MGG.Pulse.UI/Views/ShellPage.xaml` — The main navigation shell. Needs visual separation (elevation/color) from the content frame, and an Appearance menu for theme switching.
- `app/src/MGG.Pulse.UI/Controls/*` — All custom or overridden WinUI controls (Buttons, Cards). They will require updated styles for richer hover states, elevation, motion, and cursor changes (`ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand)`).
- `app/src/MGG.Pulse.UI/ViewModels/SettingsViewModel.cs` (or equivalent) — To support the Appearance menu and theme switching logic (Dark/Light).

### Design Tokens (Light Theme Palette)
A new Light theme complementing the existing dark palette must be integrated. Primary accent remains Green 500 (`#4CAF50`).
- Background: `#F4F7F5`
- Surface: `#FFFFFF`
- SurfaceRaised: `#F8FBF8`
- SurfaceVariant: `#E8F0EA`
- SurfaceAccent: `#DCE9DF`
- Border: `#CBD8CE`
- TextPrimary: `#152018`
- TextSecondary: `#5B6A60`
- TextDisabled: `#97A39A`
- Primary: `#4CAF50`
- PrimaryHover: `#43A047`
- PrimaryActive: `#2E7D32`
- PrimaryContainer: `#DDF3DF`
- PrimarySubtle: `#EDF8EE`
- FocusRing: `#7BC67E`
- SidebarSurface: `#F7FAF7`
- SidebarHover: `#E7F3E8`
- SidebarSelected: `#D9EFDB`

### Approaches
1. **Phased UI Redesign (Recommended)**
   - Phase 1: **Design System & Tokens (Foundation).** Implement the Resource Dictionaries for Light/Dark themes, including the Light theme palette above. Map these tokens to WinUI resources. Add the Appearance menu to the Shell.
   - Phase 2: **Shell & Navigation.** Redesign the `ShellPage` to make the sidebar a distinct, premium navigation surface. Apply `Sidebar*` tokens.
   - Phase 3: **Component Polish.** Update Buttons, Cards, and other controls to include richer hover states, icons, motion, and consistent hand/pointer cursors.
   - Pros: Minimizes regression risk, establishes a solid token foundation first.
   - Cons: Takes slightly longer end-to-end.
   - Effort: Medium/High

2. **All-at-once Replacement**
   - Replace all styles and update the shell concurrently.
   - Pros: Faster overall delivery if successful.
   - Cons: High risk of visual regressions or missed states across the app. Harder to review.
   - Effort: High

### Recommendation
**Phased UI Redesign**. Establishing the token foundation and theme switching infrastructure first ensures that subsequent component and page-level updates are consistent and robust. 

*Note: The "Glass Material Premium" direction is explicitly deferred as a future Stage 2 / Phase B and is out of scope for this change.*

### Risks
- WinUI 3 styling complexities: Overriding default WinUI 3 control templates (like `NavigationView` or `Button`) to add custom hover animations and cursors can be verbose and prone to breaking default behaviors.
- Ensuring the hand cursor is consistently applied across all standard WinUI interactive elements might require a global implicit style or behavior attachment.
- MVVM separation must be maintained while dealing with UI/Theme changes; logic for saving the theme preference should reside in a service/ViewModel.

### Ready for Proposal
Yes. The exploration clearly defines the scope, the specific tokens required for the Light theme, the architectural approach (Phased), and the distinction between current and future ("Glass") scopes.