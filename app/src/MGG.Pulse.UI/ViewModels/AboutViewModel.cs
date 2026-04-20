using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MGG.Pulse.Application.Updates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace MGG.Pulse.UI.ViewModels;

/// <summary>
/// ViewModel for the About page.
/// Shows installed version and allows triggering a manual update check.
/// </summary>
public partial class AboutViewModel : ObservableObject
{
    private readonly CheckForUpdateUseCase _checkForUpdateUseCase;
    private readonly Func<UpdateCheckResult, Task<bool>> _tryApplyUpdateAsync;
    private UpdateCheckResult? _pendingUpdateResult;

    [ObservableProperty] private string _installedVersion;
    [ObservableProperty] private string _updateStatusMessage = string.Empty;
    [ObservableProperty] private bool _isCheckingForUpdate;
    [ObservableProperty] private bool _canInstallUpdate;

    public AboutViewModel(
        CheckForUpdateUseCase checkForUpdateUseCase,
        Func<UpdateCheckResult, Task<bool>>? tryApplyUpdateAsync = null)
    {
        _checkForUpdateUseCase = checkForUpdateUseCase;
        _tryApplyUpdateAsync = tryApplyUpdateAsync ?? App.TryApplyAvailableUpdateAsync;
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
                _pendingUpdateResult = null;
                CanInstallUpdate = false;
                UpdateStatusMessage = $"La verificación falló: {result.Error}";
                return;
            }

            if (result.Value?.UpdateAvailable == true && ApplyUpdateUseCase.CanApply(result.Value))
            {
                _pendingUpdateResult = result.Value;
                CanInstallUpdate = true;
                UpdateStatusMessage = $"Actualización disponible: v{result.Value.AvailableVersion}";
                UpdateStatusMessage = $"{UpdateStatusMessage}. Podés instalarla ahora.";
            }
            else if (result.Value?.UpdateAvailable == true)
            {
                _pendingUpdateResult = null;
                CanInstallUpdate = false;
                UpdateStatusMessage = $"Actualización disponible: v{result.Value.AvailableVersion}";
            }
            else
            {
                _pendingUpdateResult = null;
                CanInstallUpdate = false;
                UpdateStatusMessage = "Ya tenés la última versión.";
            }
        }
        catch (Exception ex)
        {
            _pendingUpdateResult = null;
            CanInstallUpdate = false;
            UpdateStatusMessage = $"La verificación falló: {ex.Message}";
        }
        finally
        {
            IsCheckingForUpdate = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanInstallUpdateNow))]
    private async Task InstallUpdateAsync()
    {
        if (_pendingUpdateResult is null)
        {
            CanInstallUpdate = false;
            UpdateStatusMessage = "No hay una actualización lista para instalar.";
            return;
        }

        UpdateStatusMessage = "Esperando confirmación para actualizar...";

        var started = await _tryApplyUpdateAsync(_pendingUpdateResult);
        if (started)
        {
            UpdateStatusMessage = "Iniciando actualización...";
            return;
        }

        UpdateStatusMessage = "Actualización pospuesta. Podés intentarlo de nuevo cuando quieras.";
    }

    private bool CanCheckForUpdate() => !IsCheckingForUpdate;
    private bool CanInstallUpdateNow() => CanInstallUpdate;

    public Visibility InstallButtonVisibility
        => CanInstallUpdate ? Visibility.Visible : Visibility.Collapsed;

    partial void OnIsCheckingForUpdateChanged(bool value)
        => CheckForUpdateCommand.NotifyCanExecuteChanged();

    partial void OnCanInstallUpdateChanged(bool value)
    {
        InstallUpdateCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(InstallButtonVisibility));
    }

    private static string GetInstalledVersion()
    {
        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        var ver = asm.GetName().Version ?? new Version(0, 1, 0);
        return $"{ver.Major}.{ver.Minor}.{ver.Build}";
    }
}
