using MGG.Pulse.UI.ViewModels;
using MGG.Pulse.UI.Helpers;
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
        Loaded += AppearancePage_Loaded;
    }

    private void AppearancePage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        CursorHelper.ApplyHandCursorToInteractiveElements(this);
    }
}
