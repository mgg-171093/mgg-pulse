using MGG.Pulse.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace MGG.Pulse.UI.Views;

public sealed partial class AppearancePage : Page
{
    public AppearanceViewModel ViewModel { get; }

    public AppearancePage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<AppearanceViewModel>();
    }
}
