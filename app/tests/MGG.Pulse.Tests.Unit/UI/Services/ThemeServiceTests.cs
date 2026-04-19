using System.Xml.Linq;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.UI.Services;
using Xunit;

namespace MGG.Pulse.Tests.Unit.UI.Services;

public class ThemeServiceTests
{
    [Fact]
    public void ThemeService_ImplementsIThemeService_WithDarkCurrentThemeByDefault()
    {
        IThemeService service = new ThemeService();

        Assert.Equal("Dark", service.CurrentTheme);
    }

    // CI NOTE: This test is local-only (WinRT/UI-bound) and excluded in CI via Category!=Integration.
    [Trait("Category", "Integration")]
    [Theory(Skip = "Accesses Microsoft.UI.Xaml.Application.Current which can hang on headless CI runners.")]
    [InlineData("Light", "Light")]
    [InlineData("Dark", "Dark")]
    [InlineData("invalid", "Dark")]
    [InlineData("Auto", "Auto")]
    public void ApplyTheme_UpdatesCurrentTheme_WithNormalizedValue(string inputTheme, string expectedTheme)
    {
        IThemeService service = new ThemeService(() => "Dark");

        service.ApplyTheme(inputTheme);

        Assert.Equal(expectedTheme, service.CurrentTheme);
    }

    [Theory]
    [InlineData("Dark", "Dark")]
    [InlineData("Light", "Light")]
    [InlineData("invalid", "Dark")]
    public void ResolveEffectiveTheme_ReturnsExpectedResolvedTheme(string inputTheme, string expectedTheme)
    {
        var service = new ThemeService();

        var resolved = service.ResolveEffectiveTheme(inputTheme);

        Assert.Equal(expectedTheme, resolved);
    }

    [Fact]
    public void ResolveEffectiveTheme_WhenAutoSelected_UsesSystemThemeResolver()
    {
        var darkResolverService = new ThemeService(() => "Dark");
        var lightResolverService = new ThemeService(() => "Light");

        Assert.Equal("Dark", darkResolverService.ResolveEffectiveTheme("Auto"));
        Assert.Equal("Light", lightResolverService.ResolveEffectiveTheme("Auto"));
    }

    [Fact]
    public void AppCompositionRoot_RegistersIThemeServicePort()
    {
        var appCode = File.ReadAllText(ResolveUiFilePath("App.xaml.cs"));

        Assert.Contains("services.AddSingleton<ThemeService>();", appCode, StringComparison.Ordinal);
        Assert.Contains("services.AddSingleton<IThemeService>(sp => sp.GetRequiredService<ThemeService>());", appCode, StringComparison.Ordinal);
    }

    [Fact]
    public void OnLaunched_AppliesPersistedAppearanceThemeBeforeWindowActivation()
    {
        var appCode = File.ReadAllText(ResolveUiFilePath("App.xaml.cs"));

        var applyCall = appCode.IndexOf("themeService.ApplyTheme(config.AppearanceTheme);", StringComparison.Ordinal);
        var splashCtor = appCode.IndexOf("_splashWindow = new SplashWindow(resolvedTheme);", StringComparison.Ordinal);
        var activateCall = appCode.IndexOf("_mainWindow.Activate();", StringComparison.Ordinal);

        Assert.True(applyCall >= 0, "Expected startup theme apply call in OnLaunched.");
        Assert.True(splashCtor >= 0, "Expected SplashWindow to receive resolved theme before activation.");
        Assert.True(activateCall >= 0, "Expected main window activation call in OnLaunched.");
        Assert.True(applyCall < splashCtor, "Theme must be applied before splash window creation.");
        Assert.True(splashCtor < activateCall, "Splash should resolve with active theme before main window activation.");
    }

    [Fact]
    public void OnLaunched_WhenStartMinimized_ActivatesThenHidesMainWindow()
    {
        var appCode = File.ReadAllText(ResolveUiFilePath("App.xaml.cs"));

        Assert.Contains("if (config.StartMinimized)", appCode, StringComparison.Ordinal);
        Assert.Contains("_mainWindow.Activate();", appCode, StringComparison.Ordinal);
        Assert.Contains("_mainWindow.AppWindow.Hide();", appCode, StringComparison.Ordinal);

        var minimizedIf = appCode.IndexOf("if (config.StartMinimized)", StringComparison.Ordinal);
        var activateCall = appCode.IndexOf("_mainWindow.Activate();", minimizedIf, StringComparison.Ordinal);
        var hideCall = appCode.IndexOf("_mainWindow.AppWindow.Hide();", minimizedIf, StringComparison.Ordinal);

        Assert.True(minimizedIf >= 0, "Expected minimized startup branch in OnLaunched.");
        Assert.True(activateCall > minimizedIf, "MainWindow must activate inside minimized startup branch.");
        Assert.True(hideCall > activateCall, "MainWindow must hide immediately after activation in minimized startup branch.");
    }

    [Fact]
    public void OnLaunched_LoadsConfigAsynchronouslyWithoutBlockingGetResult()
    {
        var appCode = File.ReadAllText(ResolveUiFilePath("App.xaml.cs"));

        Assert.Contains("var config = await Services.GetRequiredService<IConfigRepository>().LoadAsync();", appCode, StringComparison.Ordinal);
        Assert.DoesNotContain("LoadAsync().GetAwaiter().GetResult()", appCode, StringComparison.Ordinal);
    }

    [Fact]
    public void OnLaunched_AppliesThemeToWindowRootsInsteadOfMutatingApplicationRequestedTheme()
    {
        var appCode = File.ReadAllText(ResolveUiFilePath("App.xaml.cs"));
        var themeServiceCode = File.ReadAllText(ResolveUiFilePath("Services", "ThemeService.cs"));

        Assert.Contains("ApplyThemeToRootElements(resolvedTheme);", appCode, StringComparison.Ordinal);
        Assert.Contains("root.RequestedTheme = requestedTheme;", appCode, StringComparison.Ordinal);
        Assert.DoesNotContain("app.RequestedTheme =", themeServiceCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellPage_UsesSidebarSurfaceTokenForPaneSeparation()
    {
        var shellXaml = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml"));

        Assert.Contains("Background=\"{ThemeResource SidebarSurfaceBrush}\"", shellXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellPage_DefinesExplicitSidebarHoverSelectedFocusStateHooks()
    {
        var shellXaml = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml"));
        var shellCodeBehind = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml.cs"));

        Assert.Contains("PointerEntered=\"NavItem_PointerEntered\"", shellXaml, StringComparison.Ordinal);
        Assert.Contains("PointerExited=\"NavItem_PointerExited\"", shellXaml, StringComparison.Ordinal);
        Assert.Contains("GotFocus=\"NavItem_GotFocus\"", shellXaml, StringComparison.Ordinal);
        Assert.Contains("LostFocus=\"NavItem_LostFocus\"", shellXaml, StringComparison.Ordinal);

        Assert.Contains("SidebarHoverBrush", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("SidebarSelectedBrush", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("FocusRingBrush", shellCodeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void RequiredThemeResourceKeys_ContainsColorAndBrushKeysForMaterialPulseTokens()
    {
        var requiredKeys = ThemeService.RequiredThemeResourceKeys;

        Assert.Contains("SurfaceRaisedColor", requiredKeys);
        Assert.Contains("SurfaceRaisedBrush", requiredKeys);
        Assert.Contains("SurfaceAccentColor", requiredKeys);
        Assert.Contains("SurfaceAccentBrush", requiredKeys);
        Assert.Contains("PrimaryContainerColor", requiredKeys);
        Assert.Contains("PrimaryContainerBrush", requiredKeys);
        Assert.Contains("PrimarySubtleColor", requiredKeys);
        Assert.Contains("PrimarySubtleBrush", requiredKeys);
        Assert.Contains("FocusRingColor", requiredKeys);
        Assert.Contains("FocusRingBrush", requiredKeys);
        Assert.Contains("SidebarSurfaceColor", requiredKeys);
        Assert.Contains("SidebarSurfaceBrush", requiredKeys);
        Assert.Contains("SidebarHoverColor", requiredKeys);
        Assert.Contains("SidebarHoverBrush", requiredKeys);
        Assert.Contains("SidebarSelectedColor", requiredKeys);
        Assert.Contains("SidebarSelectedBrush", requiredKeys);
    }

    [Fact]
    public void ThemeDictionaries_ContainAllRequiredTokenKeysInLightAndDarkThemes()
    {
        var requiredKeys = ThemeService.RequiredThemeResourceKeys;
        var darkThemeKeys = GetThemeKeys("DarkTheme.xaml");
        var lightThemeKeys = GetThemeKeys("LightTheme.xaml");

        Assert.All(requiredKeys, key => Assert.Contains(key, darkThemeKeys));
        Assert.All(requiredKeys, key => Assert.Contains(key, lightThemeKeys));
    }

    [Fact]
    public void SharedStylesDictionary_ContainsBaselineSharedResources()
    {
        var sharedStyleKeys = GetXamlKeys("src", "MGG.Pulse.UI", "Themes", "SharedStyles.xaml");

        Assert.Contains("StateLayerHoverOpacity", sharedStyleKeys);
        Assert.Contains("StateLayerPressedOpacity", sharedStyleKeys);
        Assert.Contains("StateLayerFocusOpacity", sharedStyleKeys);
        Assert.Contains("BaseButtonStyle", sharedStyleKeys);
        Assert.Contains("BaseCardStyle", sharedStyleKeys);
    }

    [Fact]
    public void SharedStylesDictionary_ContainsPhase3ButtonAndCardPolishStyles()
    {
        var sharedStyleKeys = GetXamlKeys("src", "MGG.Pulse.UI", "Themes", "SharedStyles.xaml");

        Assert.Contains("PrimaryButtonStyle", sharedStyleKeys);
        Assert.Contains("SecondaryButtonStyle", sharedStyleKeys);
        Assert.Contains("IconButtonStyle", sharedStyleKeys);
        Assert.Contains("CardStyle", sharedStyleKeys);
        Assert.Contains("FocusedCardStyle", sharedStyleKeys);
    }

    [Fact]
    public void SharedStylesDictionary_DefinesImplicitHandCursorForInteractiveControls()
    {
        var sharedStyles = File.ReadAllText(ResolveUiFilePath("Themes", "SharedStyles.xaml"));

        Assert.Contains("TargetType=\"Button\"", sharedStyles, StringComparison.Ordinal);
        Assert.Contains("ProtectedCursor", sharedStyles, StringComparison.Ordinal);
        Assert.Contains("InputSystemCursorShape.Hand", sharedStyles, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellHostedPages_UseSharedCardAndButtonStyles()
    {
        var dashboard = File.ReadAllText(ResolveUiFilePath("Views", "DashboardPage.xaml"));
        var settings = File.ReadAllText(ResolveUiFilePath("Views", "SettingsPage.xaml"));
        var appearance = File.ReadAllText(ResolveUiFilePath("Views", "AppearancePage.xaml"));
        var logs = File.ReadAllText(ResolveUiFilePath("Views", "LogsPage.xaml"));
        var about = File.ReadAllText(ResolveUiFilePath("Views", "AboutPage.xaml"));

        Assert.Contains("Style=\"{StaticResource CardStyle}\"", dashboard, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource CardStyle}\"", settings, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource CardStyle}\"", appearance, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource CardStyle}\"", logs, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource CardStyle}\"", about, StringComparison.Ordinal);

        Assert.Contains("Style=\"{StaticResource PrimaryButtonStyle}\"", dashboard, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource PrimaryButtonStyle}\"", settings, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource SecondaryButtonStyle}\"", about, StringComparison.Ordinal);

        var shell = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml"));
        Assert.Contains("NavigationViewItem", shell, StringComparison.Ordinal);
        Assert.Contains("GotFocus=\"NavItem_GotFocus\"", shell, StringComparison.Ordinal);
        Assert.Contains("PointerEntered=\"NavItem_PointerEntered\"", shell, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellNavigation_UsesSingleLocalizedSettingsEntryAndSpanishLabels()
    {
        var shell = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml"));

        Assert.DoesNotContain("Content=\"Settings\"", shell, StringComparison.Ordinal);
        Assert.Contains("IsSettingsVisible=\"False\"", shell, StringComparison.Ordinal);
        Assert.Contains("Content=\"Configuración\"", shell, StringComparison.Ordinal);
        Assert.Contains("Content=\"Dashboard\"", shell, StringComparison.Ordinal);
        Assert.Contains("Content=\"Apariencia\"", shell, StringComparison.Ordinal);
        Assert.Contains("Content=\"Registros\"", shell, StringComparison.Ordinal);
        Assert.Contains("Content=\"Acerca de\"", shell, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellSelectionChanged_HandlesBuiltInSettingsItem()
    {
        var shellCodeBehind = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml.cs"));

        Assert.Contains("string.Equals(tag, \"Settings\", StringComparison.Ordinal)", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("ContentFrame.Navigate(typeof(SettingsPage));", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("_lastNavigableSelection = item;", shellCodeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellNavigation_DefinesSidebarFooterExitActionInSpanish()
    {
        var shell = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml"));

        Assert.Contains("<NavigationView.FooterMenuItems>", shell, StringComparison.Ordinal);
        Assert.Contains("Content=\"Salir\"", shell, StringComparison.Ordinal);
        Assert.Contains("Tag=\"Exit\"", shell, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellSelectionChanged_InterceptsExitTagAndRequestsAppShutdown()
    {
        var shellCodeBehind = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml.cs"));

        Assert.Contains("if (string.Equals(tag, \"Exit\", StringComparison.Ordinal))", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("RestorePreviousSelection(sender);", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("App.RequestExit();", shellCodeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellSelectionChanged_RestoresPreviousSelectionWhenExitIsActivated()
    {
        var shellCodeBehind = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml.cs"));

        Assert.Contains("private object? _lastNavigableSelection;", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("private void RestorePreviousSelection(NavigationView sender)", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("sender.SelectedItem = _lastNavigableSelection;", shellCodeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellSelectionChanged_SuppressesNavigationDuringExitSelectionRestore()
    {
        var shellCodeBehind = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml.cs"));

        Assert.Contains("private bool _isRestoringSelection;", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("if (_isRestoringSelection)", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("_isRestoringSelection = true;", shellCodeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellFooterExitItem_UsesSameHoverFocusPointerHandlersAsNavigationItems()
    {
        var shellCodeBehind = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml.cs"));

        Assert.Contains("foreach (var footerItem in NavView.FooterMenuItems.OfType<NavigationViewItem>())", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("footerItem.GotFocus += NavItem_GotFocus;", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("footerItem.PointerEntered += NavItem_PointerEntered;", shellCodeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void App_ExposesRequestExitStaticAccessorThatReusesExitApp()
    {
        var appCode = File.ReadAllText(ResolveUiFilePath("App.xaml.cs"));

        Assert.Contains("internal static void RequestExit()", appCode, StringComparison.Ordinal);
        Assert.Contains("if (Current is App app)", appCode, StringComparison.Ordinal);
        Assert.Contains("app.ExitApp();", appCode, StringComparison.Ordinal);
    }

    [Fact]
    public void Pages_UseThemeResourceForThemeSensitiveBrushes()
    {
        var shell = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml"));
        var dashboard = File.ReadAllText(ResolveUiFilePath("Views", "DashboardPage.xaml"));
        var settings = File.ReadAllText(ResolveUiFilePath("Views", "SettingsPage.xaml"));
        var appearance = File.ReadAllText(ResolveUiFilePath("Views", "AppearancePage.xaml"));
        var logs = File.ReadAllText(ResolveUiFilePath("Views", "LogsPage.xaml"));
        var about = File.ReadAllText(ResolveUiFilePath("Views", "AboutPage.xaml"));
        var sharedStyles = File.ReadAllText(ResolveUiFilePath("Themes", "SharedStyles.xaml"));

        Assert.DoesNotContain("{StaticResource BackgroundBrush}", shell, StringComparison.Ordinal);
        Assert.Contains("{ThemeResource BackgroundBrush}", shell, StringComparison.Ordinal);
        Assert.Contains("{ThemeResource SidebarSurfaceBrush}", shell, StringComparison.Ordinal);
        Assert.DoesNotContain("{StaticResource BackgroundBrush}", dashboard, StringComparison.Ordinal);
        Assert.DoesNotContain("{StaticResource BackgroundBrush}", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("{StaticResource BackgroundBrush}", appearance, StringComparison.Ordinal);
        Assert.DoesNotContain("{StaticResource BackgroundBrush}", logs, StringComparison.Ordinal);
        Assert.DoesNotContain("{StaticResource BackgroundBrush}", about, StringComparison.Ordinal);
        Assert.Contains("{ThemeResource BorderBrush}", shell, StringComparison.Ordinal);
        Assert.DoesNotContain("Property=\"BorderBrush\" Value=\"{StaticResource BorderBrush}\"", sharedStyles, StringComparison.Ordinal);
    }

    [Fact]
    public void AppearancePage_ExposesOnlyDarkLightAutoSpanishOptions()
    {
        var appearance = File.ReadAllText(ResolveUiFilePath("Views", "AppearancePage.xaml"));

        Assert.Contains("Content=\"Oscuro\"", appearance, StringComparison.Ordinal);
        Assert.Contains("Content=\"Claro\"", appearance, StringComparison.Ordinal);
        Assert.Contains("Content=\"Automático\"", appearance, StringComparison.Ordinal);
        Assert.Contains("El cambio de apariencia se aplicará al reiniciar la aplicación.", appearance, StringComparison.Ordinal);
        Assert.Contains("Content=\"Reiniciar\"", appearance, StringComparison.Ordinal);
        Assert.Contains("Severity=\"Informational\"", appearance, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"Dark\"", appearance, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"Light\"", appearance, StringComparison.Ordinal);
    }

    [Fact]
    public void AppearanceViewModel_UsesRestartBasedFlowWithoutImmediateApply()
    {
        var appearanceViewModel = File.ReadAllText(ResolveUiFilePath("ViewModels", "AppearanceViewModel.cs"));

        Assert.Contains("private bool _showRestartBanner;", appearanceViewModel, StringComparison.Ordinal);
        Assert.Contains("[RelayCommand]", appearanceViewModel, StringComparison.Ordinal);
        Assert.Contains("private void Restart()", appearanceViewModel, StringComparison.Ordinal);
        Assert.Contains("AppInstance.Restart", appearanceViewModel, StringComparison.Ordinal);
        Assert.DoesNotContain("_themeService.ApplyTheme", appearanceViewModel, StringComparison.Ordinal);
        Assert.Contains("ShowRestartBanner = true;", appearanceViewModel, StringComparison.Ordinal);
    }

    [Fact]
    public void SettingsViewModel_DoesNotApplyThemeImmediatelyWhenSavingAppearance()
    {
        var settingsViewModel = File.ReadAllText(ResolveUiFilePath("ViewModels", "SettingsViewModel.cs"));

        Assert.DoesNotContain("_themeService.ApplyTheme", settingsViewModel, StringComparison.Ordinal);
        Assert.Contains("config.UpdateAppearanceTheme(SelectedAppearanceTheme);", settingsViewModel, StringComparison.Ordinal);
    }

    [Fact]
    public void InfrastructureTrayService_UsesBrandedIconIcoPath()
    {
        var trayService = File.ReadAllText(ResolveInfraFilePath("Tray", "SystemTrayService.cs"));

        Assert.Contains("Path.Combine(AppContext.BaseDirectory, \"Assets\", \"icon.ico\")", trayService, StringComparison.Ordinal);
        Assert.Contains("new Icon(iconPath)", trayService, StringComparison.Ordinal);
    }

    [Fact]
    public void SplashWindow_IsThemeAwareAndWiresWindowIcon()
    {
        var splashCodeBehind = File.ReadAllText(ResolveUiFilePath("Windows", "SplashWindow.xaml.cs"));
        var splashXaml = File.ReadAllText(ResolveUiFilePath("Windows", "SplashWindow.xaml"));

        Assert.Contains("public SplashWindow(string resolvedTheme)", splashCodeBehind, StringComparison.Ordinal);
        Assert.Contains("RequestedTheme =", splashCodeBehind, StringComparison.Ordinal);
        Assert.Contains("AppWindow.SetIcon", splashCodeBehind, StringComparison.Ordinal);
        Assert.Contains("{ThemeResource BackgroundBrush}", splashXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void WindowIcon_IsGeneratedFromCanonicalBrandingSource()
    {
        var iconGenerator = File.ReadAllText(ResolveProjectFilePath("tools", "gen-icon.ps1"));

        Assert.Contains("assets\\branding\\icon-app.png", iconGenerator, StringComparison.Ordinal);
        Assert.Contains("assets\\branding\\icon.ico", iconGenerator, StringComparison.Ordinal);
        Assert.Contains("generate_icon.py", iconGenerator, StringComparison.Ordinal);
        Assert.Contains("Pillow", iconGenerator, StringComparison.Ordinal);
    }

    [Fact]
    public void IconGeneratorPythonScript_BuildsStrictMultiResolutionIcoFrames()
    {
        var pythonGenerator = File.ReadAllText(ResolveProjectFilePath("tools", "generate_icon.py"));

        Assert.Contains("from PIL import Image", pythonGenerator, StringComparison.Ordinal);
        Assert.Contains("SIZES", pythonGenerator, StringComparison.Ordinal);
        Assert.Contains("(16, 16)", pythonGenerator, StringComparison.Ordinal);
        Assert.Contains("(32, 32)", pythonGenerator, StringComparison.Ordinal);
        Assert.Contains("(48, 48)", pythonGenerator, StringComparison.Ordinal);
        Assert.Contains("(256, 256)", pythonGenerator, StringComparison.Ordinal);
        Assert.Contains("struct.pack(\"<HHH\", 0, 1, num_images)", pythonGenerator, StringComparison.Ordinal);
        Assert.Contains("ICO frames:", pythonGenerator, StringComparison.Ordinal);
    }

    [Fact]
    public void UiProject_DeclaresApplicationIconFromBrandingAsset()
    {
        var uiProject = File.ReadAllText(ResolveUiFilePath("MGG.Pulse.UI.csproj"));

        Assert.Contains("<ApplicationIcon>..\\..\\assets\\branding\\icon.ico</ApplicationIcon>", uiProject, StringComparison.Ordinal);
    }

    [Fact]
    public void DashboardPage_UsesSharedPrimarySecondaryActionHierarchy()
    {
        var dashboard = File.ReadAllText(ResolveUiFilePath("Views", "DashboardPage.xaml"));

        Assert.Contains("Content=\"Iniciar\"", dashboard, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource PrimaryButtonStyle}\"", dashboard, StringComparison.Ordinal);
        Assert.Contains("Content=\"Detener\"", dashboard, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource SecondaryButtonStyle}\"", dashboard, StringComparison.Ordinal);
        Assert.DoesNotContain("Background=\"#CC3333\"", dashboard, StringComparison.Ordinal);
    }

    [Fact]
    public void SettingsAndAppearancePages_ExposeClearSectionLabels()
    {
        var settings = File.ReadAllText(ResolveUiFilePath("Views", "SettingsPage.xaml"));
        var appearance = File.ReadAllText(ResolveUiFilePath("Views", "AppearancePage.xaml"));

        Assert.Contains("Text=\"Configuración\"", settings, StringComparison.Ordinal);
        Assert.Contains("Text=\"Comportamiento de simulación\"", settings, StringComparison.Ordinal);
        Assert.Contains("Text=\"Apariencia\"", appearance, StringComparison.Ordinal);
        Assert.Contains("Text=\"Preferencias de apariencia\"", appearance, StringComparison.Ordinal);
        Assert.Contains("Text=\"El cambio de apariencia se aplicará al reiniciar la aplicación.\"", appearance, StringComparison.Ordinal);
    }

    [Fact]
    public void SettingsPage_LocalizesVisibleModeAndInputOptionsWhileKeepingEnumTagsStable()
    {
        var settings = File.ReadAllText(ResolveUiFilePath("Views", "SettingsPage.xaml"));

        Assert.Contains("<ComboBoxItem Content=\"Inteligente\" Tag=\"Intelligent\" />", settings, StringComparison.Ordinal);
        Assert.Contains("<ComboBoxItem Content=\"Agresivo\" Tag=\"Aggressive\" />", settings, StringComparison.Ordinal);
        Assert.Contains("<ComboBoxItem Content=\"Manual\" Tag=\"Manual\" />", settings, StringComparison.Ordinal);

        Assert.Contains("<ComboBoxItem Content=\"Ratón\" Tag=\"Mouse\" />", settings, StringComparison.Ordinal);
        Assert.Contains("<ComboBoxItem Content=\"Teclado\" Tag=\"Keyboard\" />", settings, StringComparison.Ordinal);
        Assert.Contains("<ComboBoxItem Content=\"Combinado\" Tag=\"Combined\" />", settings, StringComparison.Ordinal);

        Assert.DoesNotContain("<ComboBoxItem Content=\"Intelligent\" />", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("<ComboBoxItem Content=\"Aggressive\" />", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("<ComboBoxItem Content=\"Mouse\" />", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("<ComboBoxItem Content=\"Keyboard\" />", settings, StringComparison.Ordinal);
        Assert.DoesNotContain("<ComboBoxItem Content=\"Combined\" />", settings, StringComparison.Ordinal);
    }

    [Fact]
    public void SettingsPage_LocalizesIntervalPlaceholders()
    {
        var settings = File.ReadAllText(ResolveUiFilePath("Views", "SettingsPage.xaml"));

        Assert.Contains("PlaceholderText=\"Mín\"", settings, StringComparison.Ordinal);
        Assert.Contains("PlaceholderText=\"Máx\"", settings, StringComparison.Ordinal);
    }

    [Fact]
    public void MainViewModel_LocalizesRuntimeStatusAndSchedulingCopy()
    {
        var mainViewModel = File.ReadAllText(ResolveUiFilePath("ViewModels", "MainViewModel.cs"));

        Assert.Contains("private string _statusText = \"Inactivo\";", mainViewModel, StringComparison.Ordinal);
        Assert.Contains("StatusText = \"Activo\";", mainViewModel, StringComparison.Ordinal);
        Assert.Contains("StatusText = \"Inactivo\";", mainViewModel, StringComparison.Ordinal);
        Assert.Contains("_trayService.SetTooltip(\"MGG Pulse — Activo\");", mainViewModel, StringComparison.Ordinal);
        Assert.Contains("_trayService.SetTooltip(\"MGG Pulse — Inactivo\");", mainViewModel, StringComparison.Ordinal);
        Assert.Contains("NextScheduledText = diff.TotalSeconds > 0 ? $\"en {(int)diff.TotalSeconds}s\" : \"ahora\";", mainViewModel, StringComparison.Ordinal);
    }

    [Fact]
    public void MainViewModel_LocalizesLastActionInputTypeLabels()
    {
        var mainViewModel = File.ReadAllText(ResolveUiFilePath("ViewModels", "MainViewModel.cs"));

        Assert.Contains("FormatInputTypeLabel", mainViewModel, StringComparison.Ordinal);
        Assert.Contains("InputType.Mouse => \"Ratón\"", mainViewModel, StringComparison.Ordinal);
        Assert.Contains("InputType.Keyboard => \"Teclado\"", mainViewModel, StringComparison.Ordinal);
        Assert.Contains("InputType.Combined => \"Combinado\"", mainViewModel, StringComparison.Ordinal);
        Assert.Contains("LastActionText = $\"{FormatInputTypeLabel(action.InputType)} a las {action.ExecutedAt:HH:mm:ss}\";", mainViewModel, StringComparison.Ordinal);
    }

    [Fact]
    public void AboutViewModel_LocalizesUpdateMessagesToSpanish()
    {
        var aboutViewModel = File.ReadAllText(ResolveUiFilePath("ViewModels", "AboutViewModel.cs"));

        Assert.Contains("UpdateStatusMessage = \"Buscando actualizaciones...\";", aboutViewModel, StringComparison.Ordinal);
        Assert.Contains("UpdateStatusMessage = $\"La verificación falló: {result.Error}\";", aboutViewModel, StringComparison.Ordinal);
        Assert.Contains("UpdateStatusMessage = $\"Actualización disponible: v{result.Value.AvailableVersion}\";", aboutViewModel, StringComparison.Ordinal);
        Assert.Contains("UpdateStatusMessage = \"Ya tenés la última versión.\";", aboutViewModel, StringComparison.Ordinal);
    }

    [Fact]
    public void CursorHelper_DefinesRuntimeHandCursorWithProtectedCursor()
    {
        var cursorHelper = File.ReadAllText(ResolveUiFilePath("Helpers", "CursorHelper.cs"));

        Assert.Contains("InputSystemCursorShape.Hand", cursorHelper, StringComparison.Ordinal);
        Assert.Contains("ProtectedCursor", cursorHelper, StringComparison.Ordinal);
        Assert.Contains("ApplyHandCursorToInteractiveElements", cursorHelper, StringComparison.Ordinal);
    }

    [Fact]
    public void InteractivePages_InvokeRuntimeCursorHelperOnLoad()
    {
        var shellCodeBehind = File.ReadAllText(ResolveUiFilePath("Views", "ShellPage.xaml.cs"));
        var dashboardCodeBehind = File.ReadAllText(ResolveUiFilePath("Views", "DashboardPage.xaml.cs"));
        var settingsCodeBehind = File.ReadAllText(ResolveUiFilePath("Views", "SettingsPage.xaml.cs"));
        var appearanceCodeBehind = File.ReadAllText(ResolveUiFilePath("Views", "AppearancePage.xaml.cs"));
        var logsCodeBehind = File.ReadAllText(ResolveUiFilePath("Views", "LogsPage.xaml.cs"));
        var aboutCodeBehind = File.ReadAllText(ResolveUiFilePath("Views", "AboutPage.xaml.cs"));

        Assert.Contains("CursorHelper.ApplyHandCursorToInteractiveElements(this);", shellCodeBehind, StringComparison.Ordinal);
        Assert.Contains("CursorHelper.ApplyHandCursorToInteractiveElements(this);", dashboardCodeBehind, StringComparison.Ordinal);
        Assert.Contains("CursorHelper.ApplyHandCursorToInteractiveElements(this);", settingsCodeBehind, StringComparison.Ordinal);
        Assert.Contains("CursorHelper.ApplyHandCursorToInteractiveElements(this);", appearanceCodeBehind, StringComparison.Ordinal);
        Assert.Contains("CursorHelper.ApplyHandCursorToInteractiveElements(this);", logsCodeBehind, StringComparison.Ordinal);
        Assert.Contains("CursorHelper.ApplyHandCursorToInteractiveElements(this);", aboutCodeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void LogsAndAboutPages_UseConsistentRefinedHeadingsAndActions()
    {
        var logs = File.ReadAllText(ResolveUiFilePath("Views", "LogsPage.xaml"));
        var about = File.ReadAllText(ResolveUiFilePath("Views", "AboutPage.xaml"));

        Assert.Contains("Text=\"Registros en tiempo de ejecución\"", logs, StringComparison.Ordinal);
        Assert.Contains("Text=\"Acerca de MGG Pulse\"", about, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource SecondaryButtonStyle}\"", about, StringComparison.Ordinal);
    }

    [Fact]
    public void AppResources_MergedDictionaries_IncludeSharedStylesAndThemeDictionary()
    {
        var mergedDictionarySources = GetMergedDictionarySourcesFromAppXaml();

        Assert.Contains("Themes/SharedStyles.xaml", mergedDictionarySources);
        Assert.Contains("Themes/DarkTheme.xaml", mergedDictionarySources);
    }

    [Theory]
    [InlineData("Light", "ms-appx:///Themes/LightTheme.xaml")]
    [InlineData("Dark", "ms-appx:///Themes/DarkTheme.xaml")]
    [InlineData("invalid", "ms-appx:///Themes/DarkTheme.xaml")]
    public void ResolveThemeDictionarySource_ReturnsExpectedThemePath(string inputTheme, string expectedPath)
    {
        var path = ThemeService.ResolveThemeDictionarySource(inputTheme);

        Assert.Equal(expectedPath, path);
    }

    // CI NOTE: This test is local-only (WinRT storage-bound) and excluded in CI via Category!=Integration.
    [Trait("Category", "Integration")]
    [Fact(Skip = "Requires WinRT ApplicationData storage and can hang on headless CI runners.")]
    public void GetSavedTheme_WhenPreferenceMissing_ReturnsDarkDefault()
    {
        var service = new ThemeService();
        service.SaveTheme("Unknown");

        var theme = service.GetSavedTheme();

        Assert.Equal("Dark", theme);
    }

    // CI NOTE: This test is local-only (WinRT storage-bound) and excluded in CI via Category!=Integration.
    [Trait("Category", "Integration")]
    [Theory(Skip = "Requires WinRT ApplicationData storage and can hang on headless CI runners.")]
    [InlineData("Dark")]
    [InlineData("Light")]
    public void SaveTheme_WhenSupportedValue_PersistsPreference(string expectedTheme)
    {
        var service = new ThemeService();
        service.SaveTheme(expectedTheme);

        var theme = service.GetSavedTheme();
        Assert.Equal(expectedTheme, theme);
    }

    // CI NOTE: This test is local-only (WinRT storage-bound) and excluded in CI via Category!=Integration.
    [Trait("Category", "Integration")]
    [Fact(Skip = "Requires WinRT ApplicationData storage and can hang on headless CI runners.")]
    public void GetSavedTheme_WhenSavedValueIsInvalid_FallsBackToDark()
    {
        var service = new ThemeService();
        service.SaveTheme("Sepia");

        var theme = service.GetSavedTheme();

        Assert.Equal("Dark", theme);
    }

    private static HashSet<string> GetThemeKeys(string themeFileName)
    {
        var themePath = ResolveUiFilePath("Themes", themeFileName);
        var xaml = XDocument.Load(themePath);
        var xNamespace = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");

        return xaml.Descendants()
            .Select(element => element.Attribute(xNamespace + "Key")?.Value)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);
    }

    private static HashSet<string> GetXamlKeys(params string[] segments)
    {
        var filePath = ResolveUiFilePath(segments);
        var xaml = XDocument.Load(filePath);
        var xNamespace = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");

        return xaml.Descendants()
            .Select(element => element.Attribute(xNamespace + "Key")?.Value)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);
    }

    private static HashSet<string> GetMergedDictionarySourcesFromAppXaml()
    {
        var appXamlPath = ResolveUiFilePath("App.xaml");
        var xaml = XDocument.Load(appXamlPath);

        return xaml.Descendants()
            .Where(element => element.Name.LocalName == "ResourceDictionary")
            .Select(element => element.Attribute("Source")?.Value)
            .Where(source => !string.IsNullOrWhiteSpace(source))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string ResolveUiFilePath(params string[] relativeSegments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, relativeSegments[0] == "src"
                ? Path.Combine(relativeSegments)
                : Path.Combine(["src", "MGG.Pulse.UI", .. relativeSegments]));

            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Unable to locate file '{Path.Combine(relativeSegments)}' from '{AppContext.BaseDirectory}'.");
    }

    private static string ResolveProjectFilePath(params string[] relativeSegments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, Path.Combine(relativeSegments));
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Unable to locate project file '{Path.Combine(relativeSegments)}' from '{AppContext.BaseDirectory}'.");
    }

    private static string ResolveInfraFilePath(params string[] relativeSegments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, Path.Combine(["src", "MGG.Pulse.Infrastructure", .. relativeSegments]));
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Unable to locate infra file '{Path.Combine(relativeSegments)}' from '{AppContext.BaseDirectory}'.");
    }
}
