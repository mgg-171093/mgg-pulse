using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MGG.Pulse.Application.Updates;
using Microsoft.Extensions.DependencyInjection;

namespace MGG.Pulse.UI.ViewModels;

/// <summary>
/// ViewModel for the About page.
/// Shows installed version and allows triggering a manual update check.
/// </summary>
public partial class AboutViewModel : ObservableObject
{
    private readonly CheckForUpdateUseCase _checkForUpdateUseCase;

    [ObservableProperty] private string _installedVersion;
    [ObservableProperty] private string _updateStatusMessage = string.Empty;
    [ObservableProperty] private bool _isCheckingForUpdate;

    public AboutViewModel(CheckForUpdateUseCase checkForUpdateUseCase)
    {
        _checkForUpdateUseCase = checkForUpdateUseCase;
        _installedVersion = GetInstalledVersion();
    }

    [RelayCommand(CanExecute = nameof(CanCheckForUpdate))]
    private async Task CheckForUpdateAsync()
    {
        IsCheckingForUpdate = true;
        UpdateStatusMessage = "Buscando actualizaciones...";

        try
        {
            var result = await _checkForUpdateUseCase.ExecuteAsync(CancellationToken.None);

            if (!result.IsSuccess)
            {
                UpdateStatusMessage = $"La verificación falló: {result.Error}";
                return;
            }

            if (result.Value?.UpdateAvailable == true)
            {
                UpdateStatusMessage = $"Actualización disponible: v{result.Value.AvailableVersion}";
            }
            else
            {
                UpdateStatusMessage = "Ya tenés la última versión.";
            }
        }
        finally
        {
            IsCheckingForUpdate = false;
        }
    }

    private bool CanCheckForUpdate() => !IsCheckingForUpdate;

    partial void OnIsCheckingForUpdateChanged(bool value)
        => CheckForUpdateCommand.NotifyCanExecuteChanged();

    private static string GetInstalledVersion()
    {
        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        var ver = asm.GetName().Version ?? new Version(0, 1, 0);
        return $"{ver.Major}.{ver.Minor}.{ver.Build}";
    }
}
