using MGG.Pulse.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace MGG.Pulse.UI.Views;

public sealed partial class DashboardPage : Page
{
    public MainViewModel ViewModel { get; }

    public DashboardPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<MainViewModel>();
    }
}
