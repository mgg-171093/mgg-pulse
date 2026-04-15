using MGG.Pulse.UI.Services;
using Windows.Storage;
using Xunit;

namespace MGG.Pulse.Tests.Unit.UI.Services;

public class ThemeServiceTests
{
    [Fact]
    public void GetSavedTheme_WhenPreferenceMissing_ReturnsDarkDefault()
    {
        ThemeService.SaveTheme("Unknown");

        var theme = ThemeService.GetSavedTheme();

        Assert.Equal("Dark", theme);
    }

    [Theory]
    [InlineData("Dark")]
    [InlineData("Light")]
    public void SaveTheme_WhenSupportedValue_PersistsPreference(string expectedTheme)
    {
        ThemeService.SaveTheme(expectedTheme);

        var theme = ThemeService.GetSavedTheme();
        Assert.Equal(expectedTheme, theme);
    }

    [Fact]
    public void GetSavedTheme_WhenSavedValueIsInvalid_FallsBackToDark()
    {
        ThemeService.SaveTheme("Sepia");

        var theme = ThemeService.GetSavedTheme();

        Assert.Equal("Dark", theme);
    }
}
