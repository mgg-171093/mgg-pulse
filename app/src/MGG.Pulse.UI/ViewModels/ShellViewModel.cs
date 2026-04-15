using CommunityToolkit.Mvvm.ComponentModel;

namespace MGG.Pulse.UI.ViewModels;

/// <summary>
/// ViewModel for the shell Navigation view.
/// Tracks the currently selected navigation item.
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private string _currentPageTag = "Dashboard";

    [ObservableProperty]
    private string _statusText = $"v{GetInstalledVersion()}";

    private static string GetInstalledVersion()
    {
        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        var ver = asm.GetName().Version ?? new Version(0, 1, 0);
        return $"{ver.Major}.{ver.Minor}.{ver.Build}";
    }
}
