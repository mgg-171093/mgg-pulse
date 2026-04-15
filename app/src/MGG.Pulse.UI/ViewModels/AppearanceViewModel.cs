using CommunityToolkit.Mvvm.ComponentModel;
using MGG.Pulse.UI.Services;

namespace MGG.Pulse.UI.ViewModels;

public partial class AppearanceViewModel : ObservableObject
{
    [ObservableProperty]
    private string _selectedTheme;

    public AppearanceViewModel()
    {
        _selectedTheme = ThemeService.GetSavedTheme();
    }

    public bool IsDarkTheme
    {
        get => string.Equals(SelectedTheme, "Dark", StringComparison.OrdinalIgnoreCase);
        set
        {
            if (value)
            {
                ChangeTheme("Dark");
            }
        }
    }

    public bool IsLightTheme
    {
        get => string.Equals(SelectedTheme, "Light", StringComparison.OrdinalIgnoreCase);
        set
        {
            if (value)
            {
                ChangeTheme("Light");
            }
        }
    }

    private void ChangeTheme(string nextTheme)
    {
        if (string.Equals(SelectedTheme, nextTheme, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SelectedTheme = nextTheme;
        ThemeService.SaveTheme(nextTheme);
        ThemeService.ApplyTheme(nextTheme);
        OnPropertyChanged(nameof(IsDarkTheme));
        OnPropertyChanged(nameof(IsLightTheme));
    }
}
