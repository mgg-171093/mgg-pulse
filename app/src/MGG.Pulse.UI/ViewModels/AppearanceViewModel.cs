using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MGG.Pulse.Domain.Entities;
using MGG.Pulse.Domain.Ports;
using Microsoft.Windows.AppLifecycle;

namespace MGG.Pulse.UI.ViewModels;

public partial class AppearanceViewModel : ObservableObject
{
    private readonly IConfigRepository _configRepository;
    private readonly Action _restartAction;

    [ObservableProperty]
    private string _selectedTheme;

    [ObservableProperty]
    private bool _showRestartBanner;

    public AppearanceViewModel(IThemeService themeService, IConfigRepository configRepository, Action? restartAction = null)
    {
        _configRepository = configRepository;
        _restartAction = restartAction ?? (() => AppInstance.Restart(""));
        _selectedTheme = themeService.CurrentTheme;
        _showRestartBanner = false;
    }

    public bool IsDarkTheme
    {
        get => string.Equals(SelectedTheme, "Dark", StringComparison.OrdinalIgnoreCase);
        set
        {
            if (value)
            {
                _ = ApplyThemeSelectionAsync("Dark");
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
                _ = ApplyThemeSelectionAsync("Light");
            }
        }
    }

    public bool IsAutoTheme
    {
        get => string.Equals(SelectedTheme, "Auto", StringComparison.OrdinalIgnoreCase);
        set
        {
            if (value)
            {
                _ = ApplyThemeSelectionAsync("Auto");
            }
        }
    }

    public async Task ApplyThemeSelectionAsync(string nextTheme)
    {
        if (string.Equals(SelectedTheme, nextTheme, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SelectedTheme = nextTheme;

        var config = await LoadConfigOrDefaultAsync();
        config.UpdateAppearanceTheme(nextTheme);
        await _configRepository.SaveAsync(config);
        ShowRestartBanner = true;

        OnPropertyChanged(nameof(IsDarkTheme));
        OnPropertyChanged(nameof(IsLightTheme));
        OnPropertyChanged(nameof(IsAutoTheme));
    }

    [RelayCommand]
    private void Restart()
    {
        _restartAction();
    }

    private async Task<SimulationConfig> LoadConfigOrDefaultAsync()
    {
        try
        {
            return await _configRepository.LoadAsync();
        }
        catch
        {
            return SimulationConfig.Default;
        }
    }
}
