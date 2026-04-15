using MGG.Pulse.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace MGG.Pulse.UI.Windows;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<MainViewModel>();
        ViewModel.HideWindowRequested += HideToTray;
        ConfigureWindow();
    }

    private void ConfigureWindow()
    {
        Title = "MGG Pulse";
        AppWindow.Resize(new SizeInt32(420, 640));
        AppWindow.SetPresenter(AppWindowPresenterKind.Default);

        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            AppWindow.TitleBar.ExtendsContentIntoTitleBar = false;
        }

        CenterOnScreen();
    }

    private void CenterOnScreen()
    {
        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary);
        var x = (displayArea.WorkArea.Width - 420) / 2;
        var y = (displayArea.WorkArea.Height - 640) / 2;
        AppWindow.Move(new PointInt32(x, y));
    }

    /// <summary>Hides window to tray without destroying it.</summary>
    public void HideToTray()
    {
        AppWindow.Hide();
    }
}
