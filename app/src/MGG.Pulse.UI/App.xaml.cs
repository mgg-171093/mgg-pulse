using MGG.Pulse.Application.Abstractions;
using MGG.Pulse.Application.Orchestration;
using MGG.Pulse.Application.Rules;
using MGG.Pulse.Application.Updates;
using MGG.Pulse.Application.UseCases;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Infrastructure.Logging;
using MGG.Pulse.Infrastructure.Persistence;
using MGG.Pulse.Infrastructure.Tray;
using MGG.Pulse.Infrastructure.Update;
using MGG.Pulse.Infrastructure.Win32;
using MGG.Pulse.UI.Diagnostics;
using MGG.Pulse.UI.Services;
using MGG.Pulse.UI.ViewModels;
using MGG.Pulse.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace MGG.Pulse.UI;

public partial class App : Microsoft.UI.Xaml.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    private SplashWindow? _splashWindow;
    private MainWindow? _mainWindow;
    private DispatcherQueue? _dispatcherQueue;
    private bool _isExiting;
    private UpdateHostedService? _updateService;

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
        // Capture WinUI 3 dispatcher FIRST — must be on this thread
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        try
        {
            var config = await Services.GetRequiredService<IConfigRepository>().LoadAsync();
            var themeService = Services.GetRequiredService<IThemeService>();
            themeService.ApplyTheme(config.AppearanceTheme);
            var resolvedTheme = themeService.ResolveEffectiveTheme(config.AppearanceTheme);

            _splashWindow = new SplashWindow(resolvedTheme);
            ApplyThemeToRootElements(resolvedTheme);
            _splashWindow.Activate();

            await InitializeServicesWithProgress();

            // Always create MainWindow — it keeps the WinUI 3 message loop alive.
            // Only Activate() it if the user didn't choose StartMinimized.
            // Without at least one Window reference, the process exits immediately after Splash closes.
            _mainWindow = new MainWindow();
            ApplyThemeToRootElements(resolvedTheme);

            // Give ViewModel the dispatcher so it can enqueue UI updates
            _mainWindow.ViewModel.DispatcherQueue = _dispatcherQueue;

            // Intercept window close → hide to tray (unless we're really exiting)
            _mainWindow.AppWindow.Closing += OnMainWindowClosing;

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

            // Start background update checker (non-blocking)
            _updateService = Services.GetRequiredService<UpdateHostedService>();
            _updateService.UpdateAvailable += OnUpdateAvailable;
            _ = _updateService.StartAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            CrashLogger.Write(ex);
            Exit();
        }
    }

    internal static void ApplyThemeToRootElements(string resolvedTheme)
    {
        if (Current is App app)
        {
            app.ApplyThemeToOpenWindows(resolvedTheme);
        }
    }

    private void ApplyThemeToOpenWindows(string resolvedTheme)
    {
        var requestedTheme = ToElementTheme(resolvedTheme);
        ApplyThemeToWindowRoot(_splashWindow, requestedTheme);
        ApplyThemeToWindowRoot(_mainWindow, requestedTheme);
    }

    private static void ApplyThemeToWindowRoot(Window? window, ElementTheme requestedTheme)
    {
        if (window?.Content is FrameworkElement root)
        {
            root.RequestedTheme = requestedTheme;
        }
    }

    private static ElementTheme ToElementTheme(string resolvedTheme)
    {
        return string.Equals(resolvedTheme, "Light", StringComparison.OrdinalIgnoreCase)
            ? ElementTheme.Light
            : ElementTheme.Dark;
    }

    private void OnMainWindowClosing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        var vm = _mainWindow?.ViewModel;
        if (!_isExiting && vm is not null && vm.MinimizeToTray)
        {
            args.Cancel = true;         // prevent real close
            _mainWindow?.AppWindow.Hide();
        }
    }

    private async Task InitializeServicesWithProgress()
    {
        if (_splashWindow is null)
        {
            return;
        }

        await _splashWindow.AdvanceProgressAsync(33, "Loading configuration...");
        _ = Services.GetRequiredService<IConfigRepository>();

        await _splashWindow.AdvanceProgressAsync(66, "Starting tray service...");
        _ = Services.GetRequiredService<ILoggerService>();

        await _splashWindow.AdvanceProgressAsync(100, "Ready.");

        // Spec: splash MUST be visible for at least 5 seconds
        await _splashWindow.WaitForMinimumHoldAsync();
    }

    /// <summary>Called from tray thread — marshals to WinUI 3 dispatcher.</summary>
    private void ShowMainWindow()
    {
        _dispatcherQueue?.TryEnqueue(() =>
        {
            if (_mainWindow is null)
            {
                return;
            }

            _mainWindow.AppWindow.Show();
            _mainWindow.Activate();
        });
    }

    /// <summary>Called from tray thread — marshals toggle to WinUI 3 dispatcher.</summary>
    private void ToggleSimulation()
    {
        _dispatcherQueue?.TryEnqueue(() =>
        {
            var vm = _mainWindow?.ViewModel;
            if (vm is null)
            {
                return;
            }

            if (vm.IsRunning)
            {
                _ = vm.StopCommand.ExecuteAsync(null);
            }
            else
            {
                _ = vm.StartCommand.ExecuteAsync(null);
            }
        });
    }

    /// <summary>Called from tray thread — marshals exit sequence to WinUI 3 dispatcher.</summary>
    private void ExitApp()
    {
        _isExiting = true;

        // Stop update service before exit
        _ = _updateService?.StopAsync(CancellationToken.None);

        // Dispose tray on background thread — it has its own STA loop
        Task.Run(() => Services.GetRequiredService<ITrayService>().Dispose());

        _dispatcherQueue?.TryEnqueue(() =>
        {
            _mainWindow?.Close();
            Exit();
        });
    }

    /// <summary>
    /// Fired from UpdateHostedService (background thread) — marshals to WinUI 3 dispatcher.
    /// When the update result carries a download URL and SHA-256, downloads and launches the
    /// installer silently, then exits the app to hand off to Inno Setup.
    /// Otherwise falls back to showing a tray notification only.
    /// </summary>
    private void OnUpdateAvailable(UpdateCheckResult result)
    {
        _dispatcherQueue?.TryEnqueue(async () =>
        {
            if (ApplyUpdateUseCase.CanApply(result))
            {
                var applyUseCase = Services.GetRequiredService<ApplyUpdateUseCase>();
                var applyResult  = await applyUseCase.ExecuteAsync(
                    result.DownloadUrl!,
                    result.Sha256!,
                    CancellationToken.None);

                if (applyResult.IsSuccess)
                {
                    // Installer is running — exit so Inno Setup can replace the executable.
                    ExitApp();
                    return;
                }
            }

            // Fallback: no download data, or download/verify failed → show balloon notification.
            var tray = Services.GetService<ITrayService>();
            tray?.ShowNotification(
                "Update Available",
                $"MGG Pulse {result.AvailableVersion} is ready. Restart to install.");
        });
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Infrastructure
        services.AddSingleton<IIdleDetector, Win32IdleDetector>();
        services.AddSingleton<IInputSimulator, Win32InputSimulator>();
        services.AddSingleton<IConfigRepository, JsonConfigRepository>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<IThemeService>(sp => sp.GetRequiredService<ThemeService>());
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

        // Auto-updater
        var installedVersion = GetInstalledVersion();
        const string manifestUrl = "https://raw.githubusercontent.com/mgg-171093/mgg-pulse/main/app/build/latest.json";

        services.AddSingleton<HttpClient>();
        services.AddSingleton<IUpdateService>(sp =>
            new GithubReleaseUpdateService(sp.GetRequiredService<HttpClient>(), manifestUrl));
        services.AddSingleton<ITimeProvider, SystemTimeProvider>();
        services.AddSingleton<CheckForUpdateUseCase>(sp =>
            new CheckForUpdateUseCase(sp.GetRequiredService<IUpdateService>(), installedVersion));
        services.AddSingleton<UpdateHostedService>(sp =>
            new UpdateHostedService(
                sp.GetRequiredService<CheckForUpdateUseCase>(),
                sp.GetRequiredService<ITimeProvider>()));
        services.AddSingleton<IFileDownloader>(sp =>
            new HttpFileDownloader(sp.GetRequiredService<HttpClient>()));
        services.AddSingleton<IInstallerLauncher, InnoSetupInstallerLauncher>();
        services.AddTransient<ApplyUpdateUseCase>();

        // UI — singleton so App and all pages share the same instances
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<LogsViewModel>();
        services.AddSingleton<AppearanceViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<AboutViewModel>();

        return services.BuildServiceProvider();
    }

    /// <summary>Reads the assembly version, e.g. "1.0.0".</summary>
    private static string GetInstalledVersion()
    {
        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        var ver = asm.GetName().Version ?? new Version(0, 1, 0);
        return $"{ver.Major}.{ver.Minor}.{ver.Build}";
    }
}
