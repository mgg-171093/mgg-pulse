using MGG.Pulse.UI.ViewModels;
using MGG.Pulse.UI.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace MGG.Pulse.UI.Views;

public sealed partial class LogsPage : Page
{
    public LogsViewModel ViewModel { get; }

    public LogsPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<LogsViewModel>();
        Loaded += LogsPage_Loaded;
    }

    private void LogsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        CursorHelper.ApplyHandCursorToInteractiveElements(this);
    }
}
