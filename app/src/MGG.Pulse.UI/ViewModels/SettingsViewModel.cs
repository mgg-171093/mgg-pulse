using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace MGG.Pulse.UI.ViewModels;

/// <summary>
/// ViewModel for the Settings page.
/// Exposes simulation configuration controls extracted from MainViewModel.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IConfigRepository _configRepository;
    private readonly IThemeService _themeService;
    private readonly MainViewModel? _mainViewModel;

    [ObservableProperty] private string _selectedMode = "Intelligent";
    [ObservableProperty] private string _selectedInputType = "Mouse";
    [ObservableProperty] private int _intervalMin = 30;
    [ObservableProperty] private int _intervalMax = 60;
    [ObservableProperty] private bool _startWithWindows;
    [ObservableProperty] private bool _startMinimized;
    [ObservableProperty] private bool _minimizeToTray = true;
    [ObservableProperty] private string _selectedAppearanceTheme = "Dark";

    [ObservableProperty] private string _saveStatusMessage = string.Empty;

    public SettingsViewModel(IConfigRepository configRepository, IThemeService themeService, MainViewModel mainViewModel)
    {
        _configRepository = configRepository;
        _themeService = themeService;
        _mainViewModel = mainViewModel;
        SelectedAppearanceTheme = _themeService.CurrentTheme;
        SyncFromMainViewModel();
    }

    // Test/support seam for persisted-theme flow without WinUI MainViewModel instantiation.
    public SettingsViewModel(IConfigRepository configRepository, IThemeService themeService)
    {
        _configRepository = configRepository;
        _themeService = themeService;
        _mainViewModel = null;
        SelectedAppearanceTheme = _themeService.CurrentTheme;
    }

    /// <summary>Pull current values from the shared MainViewModel config state.</summary>
    private void SyncFromMainViewModel()
    {
        if (_mainViewModel is null)
        {
            SelectedAppearanceTheme = _themeService.CurrentTheme;
            return;
        }

        SelectedMode      = _mainViewModel.SelectedMode;
        SelectedInputType = _mainViewModel.SelectedInputType;
        IntervalMin       = _mainViewModel.IntervalMin;
        IntervalMax       = _mainViewModel.IntervalMax;
        StartWithWindows  = _mainViewModel.StartWithWindows;
        StartMinimized    = _mainViewModel.StartMinimized;
        MinimizeToTray    = _mainViewModel.MinimizeToTray;
        SelectedAppearanceTheme = _themeService.CurrentTheme;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_mainViewModel is not null)
        {
            // Push values back to MainViewModel so running session picks them up
            _mainViewModel.SelectedMode      = SelectedMode;
            _mainViewModel.SelectedInputType = SelectedInputType;
            _mainViewModel.IntervalMin       = IntervalMin;
            _mainViewModel.IntervalMax       = IntervalMax;
            _mainViewModel.StartWithWindows  = StartWithWindows;
            _mainViewModel.StartMinimized    = StartMinimized;
            _mainViewModel.MinimizeToTray    = MinimizeToTray;
        }

        if (_mainViewModel is not null)
        {
            await _mainViewModel.SaveConfigCommand.ExecuteAsync(null);
        }

        var config = await _configRepository.LoadAsync();
        config.UpdateAppearanceTheme(SelectedAppearanceTheme);
        await _configRepository.SaveAsync(config);

        SaveStatusMessage = "Configuración guardada.";

        // Clear status after a short delay
        await Task.Delay(2000);
        SaveStatusMessage = string.Empty;
    }
}
