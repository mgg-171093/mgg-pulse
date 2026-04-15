using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics;

namespace MGG.Pulse.UI.Windows;

public sealed partial class SplashWindow : Window
{
    /// <summary>Spec requirement: splash MUST be visible for at least 5 seconds.</summary>
    private const int MinimumDisplayMs = 5000;

    private readonly DateTimeOffset _shownAt;
    private bool _firstActivation = true;

    public SplashWindow()
    {
        InitializeComponent();
        _shownAt = DateTimeOffset.UtcNow;
        ConfigureWindow();
        Activated += OnFirstActivated;
    }

    private void ConfigureWindow()
    {
        var appWindow = AppWindow;
        appWindow.Resize(new SizeInt32(600, 750));
        appWindow.SetPresenter(AppWindowPresenterKind.Default);

        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        Title = "MGG Pulse";
        CenterOnScreen();
    }

    private void CenterOnScreen()
    {
        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary);
        var x = (displayArea.WorkArea.Width - 600) / 2;
        var y = (displayArea.WorkArea.Height - 750) / 2;
        AppWindow.Move(new PointInt32(x, y));
    }

    private async void OnFirstActivated(object sender, WindowActivatedEventArgs args)
    {
        if (!_firstActivation)
        {
            return;
        }

        _firstActivation = false;
        Activated -= OnFirstActivated;

        // Load logo
        try
        {
            var logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png");
            if (File.Exists(logoPath))
            {
                var bitmap = new BitmapImage(new Uri(logoPath));
                LogoImage.Source = bitmap;
            }
        }
        catch { /* no logo, continue */ }

        await Task.Delay(200);
    }

    /// <summary>
    /// Advances the progress bar. Should be called from App.xaml.cs during initialization.
    /// After all advances, call <see cref="WaitForMinimumHoldAsync"/> before closing.
    /// </summary>
    public async Task AdvanceProgressAsync(double progress, string statusMessage)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            InitProgress.Value = progress;
        });
        await Task.Delay(150);
    }

    /// <summary>
    /// Waits until the 5-second minimum display time has elapsed.
    /// Call this after all initialization is complete, before closing the splash.
    /// </summary>
    public async Task WaitForMinimumHoldAsync()
    {
        var elapsed = (DateTimeOffset.UtcNow - _shownAt).TotalMilliseconds;
        var remaining = MinimumDisplayMs - elapsed;
        if (remaining > 0)
        {
            await Task.Delay((int)remaining);
        }
    }

}
