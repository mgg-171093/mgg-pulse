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
}
