using MGG.Pulse.Application.Orchestration;
using MGG.Pulse.Application.Rules;
using MGG.Pulse.Application.UseCases;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Infrastructure.Logging;
using MGG.Pulse.Infrastructure.Persistence;
using MGG.Pulse.Infrastructure.Tray;
using MGG.Pulse.Infrastructure.Win32;
using MGG.Pulse.UI.Diagnostics;
using MGG.Pulse.UI.ViewModels;
using MGG.Pulse.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace MGG.Pulse.UI;

public partial class App : Microsoft.UI.Xaml.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    private SplashWindow? _splashWindow;
    private MainWindow? _mainWindow;

    public App()
    {
        this.UnhandledException += (sender, args) =>
        {
            CrashLogger.Write(args.Exception);
            args.Handled = false; // let the process terminate — we already logged
        };
        InitializeComponent();
        Services = ConfigureServices();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            _splashWindow = new SplashWindow();
            _splashWindow.Activate();

            await InitializeServicesWithProgress();

            var config = await Services.GetRequiredService<IConfigRepository>().LoadAsync();

            // Always create MainWindow — it keeps the WinUI 3 message loop alive.
            // Only Activate() it if the user didn't choose StartMinimized.
            // Without at least one Window reference, the process exits immediately after Splash closes.
            _mainWindow = new MainWindow();
            if (!config.StartMinimized)
            {
                _mainWindow.Activate();
            }

            _splashWindow.Close();  // close after main window is created — no zero-window gap

            // Wire tray service
            var tray = Services.GetRequiredService<ITrayService>();
            tray.Initialize(
                onShow: ShowMainWindow,
                onStartStop: ToggleSimulation,
                onExit: ExitApp);
        }
        catch (Exception ex)
        {
            CrashLogger.Write(ex);
            Exit();
        }
    }

    private async Task InitializeServicesWithProgress()
    {
        if (_splashWindow is null) return;

        await _splashWindow.AdvanceProgressAsync(33, "Loading configuration...");
        _ = Services.GetRequiredService<IConfigRepository>();

        await _splashWindow.AdvanceProgressAsync(66, "Starting tray service...");
        _ = Services.GetRequiredService<ILoggerService>();

        await _splashWindow.AdvanceProgressAsync(100, "Ready.");
        await Task.Delay(400);
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
        {
            _mainWindow = new MainWindow();
        }
        _mainWindow.Activate();
    }

    private void ToggleSimulation()
    {
        // Delegate to ViewModel via the active window
        // This is called from the tray thread — keep it simple
    }

    private void ExitApp()
    {
        Services.GetRequiredService<ITrayService>().Dispose();
        _mainWindow?.Close();
        Exit();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Infrastructure
        services.AddSingleton<IIdleDetector, Win32IdleDetector>();
        services.AddSingleton<IInputSimulator, Win32InputSimulator>();
        services.AddSingleton<IConfigRepository, JsonConfigRepository>();
        services.AddSingleton<FileLoggerService>();
        services.AddSingleton<ILoggerService>(sp => sp.GetRequiredService<FileLoggerService>());
        services.AddSingleton<ITrayService, SystemTrayService>();

        // Application
        services.AddSingleton<RuleEngine>(sp => new RuleEngine(new IRule[]
        {
            new IdleRule(sp.GetRequiredService<IIdleDetector>()),
            new AggressiveModeRule(),
            new IntervalRule(30)
        }));
        services.AddSingleton<CycleOrchestrator>();
        services.AddTransient<StartSimulationUseCase>();
        services.AddTransient<StopSimulationUseCase>();

        // UI
        services.AddTransient<MainViewModel>();

        return services.BuildServiceProvider();
    }
}
