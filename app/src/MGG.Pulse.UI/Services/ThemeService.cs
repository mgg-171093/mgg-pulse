using Windows.Storage;

namespace MGG.Pulse.UI.Services;

public static class ThemeService
{
    private const string ThemePreferenceKey = "AppTheme";
    private const string DarkTheme = "Dark";
    private const string LightTheme = "Light";

    private static string _fallbackTheme = DarkTheme;

    public static void ApplyTheme(string themeName)
    {
        var app = global::Microsoft.UI.Xaml.Application.Current;
        if (app is null)
        {
            return;
        }

        var normalizedTheme = Normalize(themeName);
        var themeUri = normalizedTheme == LightTheme
            ? new Uri("ms-appx:///Themes/LightTheme.xaml")
            : new Uri("ms-appx:///Themes/DarkTheme.xaml");

        var themeDictionary = new global::Microsoft.UI.Xaml.ResourceDictionary
        {
            Source = themeUri
        };

        if (app.Resources.MergedDictionaries.Count >= 2)
        {
            app.Resources.MergedDictionaries[1] = themeDictionary;
            return;
        }

        app.Resources.MergedDictionaries.Add(themeDictionary);
    }

    public static string GetSavedTheme()
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

    public static void SaveTheme(string themeName)
    {
        var normalized = Normalize(themeName);
        _fallbackTheme = normalized;

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
        return string.Equals(themeName, LightTheme, StringComparison.OrdinalIgnoreCase)
            ? LightTheme
            : DarkTheme;
    }
}
