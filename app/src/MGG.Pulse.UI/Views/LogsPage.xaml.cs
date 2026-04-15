using MGG.Pulse.UI.ViewModels;
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
    }
}
