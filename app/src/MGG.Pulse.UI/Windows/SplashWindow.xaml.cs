using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics;

namespace MGG.Pulse.UI.Windows;

public sealed partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
        ConfigureWindow();
        Activated += OnFirstActivated;
    }

    private bool _firstActivation = true;

    private void ConfigureWindow()
    {
        var appWindow = AppWindow;
        appWindow.Resize(new SizeInt32(400, 280));
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
        var x = (displayArea.WorkArea.Width - 400) / 2;
        var y = (displayArea.WorkArea.Height - 280) / 2;
        AppWindow.Move(new PointInt32(x, y));
    }

    private async void OnFirstActivated(object sender, WindowActivatedEventArgs args)
    {
        if (!_firstActivation) return;
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

        await AnimateSplashAsync();
    }

    private async Task AnimateSplashAsync()
    {
        // Fade in logo + text panel
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(800)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        Storyboard.SetTarget(fadeIn, LogoImage);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");
        storyboard.Children.Add(fadeIn);

        var textFade = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(800)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(textFade, TextPanel);
        Storyboard.SetTargetProperty(textFade, "Opacity");
        storyboard.Children.Add(textFade);

        storyboard.Begin();

        await Task.Delay(400);
    }

    public async Task AdvanceProgressAsync(double progress, string statusMessage)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            InitProgress.Value = progress;
            StatusText.Text = statusMessage;
        });
        await Task.Delay(150);
    }
}
