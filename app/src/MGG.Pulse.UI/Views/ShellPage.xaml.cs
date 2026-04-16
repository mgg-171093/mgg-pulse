using MGG.Pulse.UI.ViewModels;
using MGG.Pulse.UI.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace MGG.Pulse.UI.Views;

public sealed partial class ShellPage : Page
{
    private static readonly SolidColorBrush _sidebarHoverBrush = new();
    private static readonly SolidColorBrush _sidebarSelectedBrush = new();
    private static readonly SolidColorBrush _focusRingBrush = new();

    public ShellViewModel ViewModel { get; }

    public ShellPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ShellViewModel>();
        Loaded += ShellPage_Loaded;
        ActualThemeChanged += ShellPage_ActualThemeChanged;
        NavView.IsSettingsVisible = true;
        if (NavView.SettingsItem is NavigationViewItem settingsItem)
        {
            settingsItem.Content = "Configuración";
            settingsItem.GotFocus += NavItem_GotFocus;
            settingsItem.LostFocus += NavItem_LostFocus;
            settingsItem.PointerEntered += NavItem_PointerEntered;
            settingsItem.PointerExited += NavItem_PointerExited;
        }

        // Navigate to Dashboard by default
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (args.SelectedItem is not NavigationViewItem item)
        {
            return;
        }

        var tag = item.Tag?.ToString();
        var pageType = tag switch
        {
            "Dashboard" => typeof(DashboardPage),
            "Settings"  => typeof(SettingsPage),
            "Appearance" => typeof(AppearancePage),
            "Logs" => typeof(LogsPage),
            "About"     => typeof(AboutPage),
            _           => typeof(DashboardPage)
        };

        ContentFrame.Navigate(pageType);

        foreach (var menuItem in NavView.MenuItems.OfType<NavigationViewItem>())
        {
            menuItem.Background = ReferenceEquals(menuItem, item)
                ? _sidebarSelectedBrush
                : new SolidColorBrush(global::Microsoft.UI.Colors.Transparent);
            menuItem.BorderBrush = new SolidColorBrush(global::Microsoft.UI.Colors.Transparent);
            menuItem.BorderThickness = new Thickness(0);
        }
    }

    private void NavItem_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not NavigationViewItem item || ReferenceEquals(item, NavView.SelectedItem))
        {
            return;
        }

        item.Background = _sidebarHoverBrush;
    }

    private void NavItem_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not NavigationViewItem item)
        {
            return;
        }

        item.Background = ReferenceEquals(item, NavView.SelectedItem)
            ? _sidebarSelectedBrush
            : new SolidColorBrush(global::Microsoft.UI.Colors.Transparent);
    }

    private void NavItem_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not NavigationViewItem item)
        {
            return;
        }

        item.BorderBrush = _focusRingBrush;
        item.BorderThickness = new Thickness(1);
    }

    private void NavItem_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not NavigationViewItem item)
        {
            return;
        }

        item.BorderBrush = new SolidColorBrush(global::Microsoft.UI.Colors.Transparent);
        item.BorderThickness = new Thickness(0);
    }

    private void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        CursorHelper.ApplyHandCursorToInteractiveElements(this);
        RehydrateThemeBrushes();
    }

    private void ShellPage_ActualThemeChanged(FrameworkElement sender, object args)
    {
        RehydrateThemeBrushes();
    }

    private static void RehydrateThemeBrushes()
    {
        _sidebarHoverBrush.Color = ((SolidColorBrush)global::Microsoft.UI.Xaml.Application.Current.Resources["SidebarHoverBrush"]).Color;
        _sidebarSelectedBrush.Color = ((SolidColorBrush)global::Microsoft.UI.Xaml.Application.Current.Resources["SidebarSelectedBrush"]).Color;
        _focusRingBrush.Color = ((SolidColorBrush)global::Microsoft.UI.Xaml.Application.Current.Resources["FocusRingBrush"]).Color;
    }
}
