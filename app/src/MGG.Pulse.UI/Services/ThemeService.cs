using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using MGG.Pulse.Domain.Ports;

namespace MGG.Pulse.UI.Services;

public class ThemeService : IThemeService
{
    private const string ThemePreferenceKey = "AppTheme";
    private const string DarkTheme = "Dark";
    private const string LightTheme = "Light";
    private const string AutoTheme = "Auto";
    private const string DarkThemeSource = "ms-appx:///Themes/DarkTheme.xaml";
    private const string LightThemeSource = "ms-appx:///Themes/LightTheme.xaml";

    private static readonly HashSet<string> ThemeDictionarySources =
    [
        DarkThemeSource,
        LightThemeSource
    ];

    private static readonly string[] _requiredThemeResourceKeys =
    [
        "BackgroundColor", "BackgroundBrush",
        "SurfaceColor", "SurfaceBrush",
        "SurfaceRaisedColor", "SurfaceRaisedBrush",
        "SurfaceVariantColor", "SurfaceVariantBrush",
        "SurfaceAccentColor", "SurfaceAccentBrush",
        "BorderColor", "BorderBrush",
        "TextPrimaryColor", "TextPrimaryBrush",
        "TextSecondaryColor", "TextSecondaryBrush",
        "TextDisabledColor", "TextDisabledBrush",
        "PrimaryColor", "PrimaryBrush",
        "PrimaryHoverColor", "PrimaryHoverBrush",
        "PrimaryActiveColor", "PrimaryActiveBrush",
        "PrimaryContainerColor", "PrimaryContainerBrush",
        "PrimarySubtleColor", "PrimarySubtleBrush",
        "FocusRingColor", "FocusRingBrush",
        "SidebarSurfaceColor", "SidebarSurfaceBrush",
        "SidebarHoverColor", "SidebarHoverBrush",
        "SidebarSelectedColor", "SidebarSelectedBrush"
    ];

    private static string _fallbackTheme = DarkTheme;
    private readonly Func<string> _systemThemeResolver;

    public string CurrentTheme { get; private set; } = DarkTheme;

    public static IReadOnlyCollection<string> RequiredThemeResourceKeys => _requiredThemeResourceKeys;

    public ThemeService()
        : this(ResolveSystemTheme)
    {
    }

    public ThemeService(Func<string> systemThemeResolver)
    {
        _systemThemeResolver = systemThemeResolver ?? throw new ArgumentNullException(nameof(systemThemeResolver));
    }

    public void ApplyTheme(string themeName)
    {
        var normalizedTheme = Normalize(themeName);
        CurrentTheme = normalizedTheme;

        var effectiveTheme = ResolveEffectiveTheme(normalizedTheme);

        var app = global::Microsoft.UI.Xaml.Application.Current;
        if (app is null)
        {
            return;
        }

        var themeUri = new Uri(ResolveThemeDictionarySource(effectiveTheme));

        var themeDictionary = new global::Microsoft.UI.Xaml.ResourceDictionary
        {
            Source = themeUri
        };

        var activeThemeIndex = -1;
        for (var index = 0; index < app.Resources.MergedDictionaries.Count; index++)
        {
            var source = app.Resources.MergedDictionaries[index].Source?.ToString();
            if (source is not null && ThemeDictionarySources.Contains(source))
            {
                activeThemeIndex = index;
                break;
            }
        }

        if (activeThemeIndex >= 0)
        {
            app.Resources.MergedDictionaries[activeThemeIndex] = themeDictionary;
        }
        else
        {
            app.Resources.MergedDictionaries.Add(themeDictionary);
        }

    }

    public string ResolveEffectiveTheme(string appearance)
    {
        var normalized = Normalize(appearance);
        if (!string.Equals(normalized, AutoTheme, StringComparison.Ordinal))
        {
            return normalized;
        }

        try
        {
            return Normalize(_systemThemeResolver());
        }
        catch
        {
            return DarkTheme;
        }
    }

    public static string ResolveThemeDictionarySource(string? themeName)
    {
        var normalizedTheme = Normalize(themeName);
        return normalizedTheme == LightTheme ? LightThemeSource : DarkThemeSource;
    }

    public string GetSavedTheme()
    {
        try
        {
            var value = ApplicationData.Current.LocalSettings.Values[ThemePreferenceKey]?.ToString();
            var normalized = Normalize(value);
            _fallbackTheme = normalized;
            return normalized;
        }
        catch (InvalidOperationException)
        {
            return _fallbackTheme;
        }
    }

    public void SaveTheme(string themeName)
    {
        var normalized = Normalize(themeName);
        _fallbackTheme = normalized;
        CurrentTheme = normalized;

        try
        {
            ApplicationData.Current.LocalSettings.Values[ThemePreferenceKey] = normalized;
        }
        catch (InvalidOperationException)
        {
            // Unit-test and non-packaged host fallback: keep in-memory preference.
        }
    }

    private static string Normalize(string? themeName)
    {
        if (string.Equals(themeName, AutoTheme, StringComparison.OrdinalIgnoreCase))
        {
            return AutoTheme;
        }

        return string.Equals(themeName, LightTheme, StringComparison.OrdinalIgnoreCase)
            ? LightTheme
            : DarkTheme;
    }

    private static bool IsDark(Color color)
    {
        var luminance = ((0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B)) / 255d;
        return luminance < 0.5d;
    }

    private static string ResolveSystemTheme()
    {
        var backgroundColor = new UISettings().GetColorValue(UIColorType.Background);
        return IsDark(backgroundColor) ? DarkTheme : LightTheme;
    }

}
