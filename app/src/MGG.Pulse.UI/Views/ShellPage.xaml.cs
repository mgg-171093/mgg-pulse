using MGG.Pulse.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace MGG.Pulse.UI.Views;

public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; }

    public ShellPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ShellViewModel>();

        // Navigate to Dashboard by default
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item)
        {
            return;
        }

        var tag = item.Tag?.ToString();
        var pageType = tag switch
        {
            "Dashboard" => typeof(DashboardPage),
            "Settings"  => typeof(SettingsPage),
            "About"     => typeof(AboutPage),
            _           => typeof(DashboardPage)
        };

        ContentFrame.Navigate(pageType);
    }
}
