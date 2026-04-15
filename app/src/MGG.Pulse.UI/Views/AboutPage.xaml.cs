using MGG.Pulse.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace MGG.Pulse.UI.Views;

public sealed partial class AboutPage : Page
{
    public AboutViewModel ViewModel { get; }

    public AboutPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<AboutViewModel>();
    }
}
