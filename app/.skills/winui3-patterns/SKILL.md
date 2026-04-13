# Skill: WinUI 3 Patterns — MGG Pulse

## Trigger
Load this skill when writing XAML, ViewModels, or any WinUI 3 UI code.

---

## MVVM with CommunityToolkit.Mvvm

### ViewModel Base Pattern

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class MainViewModel : ObservableObject
{
    private readonly StartSimulationUseCase _startUseCase;
    private readonly StopSimulationUseCase _stopUseCase;
    private CancellationTokenSource? _cts;

    // Observable property — generates IsRunning property + OnIsRunningChanged
    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusText = "Inactive";

    [ObservableProperty]
    private string _lastAction = "—";

    [ObservableProperty]
    private TimeSpan _currentIdleTime;

    public MainViewModel(StartSimulationUseCase startUseCase, StopSimulationUseCase stopUseCase)
    {
        _startUseCase = startUseCase;
        _stopUseCase = stopUseCase;
    }

    // RelayCommand — generates StartCommand ICommand property
    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        IsRunning = true;
        StatusText = "Active";
        var result = await _startUseCase.ExecuteAsync(_config, _cts.Token);
        if (!result.IsSuccess) StatusText = $"Error: {result.Error}";
    }

    private bool CanStart() => !IsRunning;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private async Task StopAsync()
    {
        _cts?.Cancel();
        await _stopUseCase.ExecuteAsync();
        IsRunning = false;
        StatusText = "Inactive";
    }

    private bool CanStop() => IsRunning;
}
```

### Notify CanExecute on Property Change

```csharp
partial void OnIsRunningChanged(bool value)
{
    StartCommand.NotifyCanExecuteChanged();
    StopCommand.NotifyCanExecuteChanged();
}
```

---

## XAML Design System Application

### ResourceDictionary for Color Tokens

```xml
<!-- Themes/DarkTheme.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Color x:Key="BackgroundColor">#0F111A</Color>
    <Color x:Key="SurfaceColor">#1A1D2E</Color>
    <Color x:Key="SurfaceVariantColor">#23263A</Color>
    <Color x:Key="BorderColor">#2A2E45</Color>
    <Color x:Key="TextPrimaryColor">#FFFFFF</Color>
    <Color x:Key="TextSecondaryColor">#B0B3C0</Color>
    <Color x:Key="PrimaryColor">#4CAF50</Color>
    <Color x:Key="PrimaryHoverColor">#66BB6A</Color>
    <Color x:Key="PrimaryActiveColor">#388E3C</Color>

    <SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}"/>
    <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource SurfaceColor}"/>
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
</ResourceDictionary>
```

### Spacing — use Margin/Padding with the scale (4/8/16/24/32)

```xml
<!-- Correct -->
<StackPanel Spacing="16" Padding="24">
<Button Margin="0,8,0,0" Padding="16,8">

<!-- Wrong — avoid arbitrary values -->
<StackPanel Spacing="13" Padding="17">
```

### Border Radius — always 8px

```xml
<Border CornerRadius="8" Background="{StaticResource SurfaceBrush}" Padding="16">
<Button CornerRadius="8">
```

---

## Page / View Structure

```xml
<!-- MainPage.xaml -->
<Page x:Class="MGG.Pulse.UI.Views.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:vm="using:MGG.Pulse.UI.ViewModels"
      Background="{StaticResource BackgroundBrush}">

    <Page.DataContext>
        <!-- Resolved via DI in code-behind constructor -->
    </Page.DataContext>

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <!-- Content -->
        <!-- Footer / Controls -->
    </Grid>
</Page>
```

### Code-behind — only DI wiring, NO business logic

```csharp
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        // ViewModel injected via DI — no new MainViewModel() here
        DataContext = App.Services.GetRequiredService<MainViewModel>();
    }
}
```

---

## Bindings

```xml
<!-- One-way binding (display) -->
<TextBlock Text="{x:Bind ViewModel.StatusText, Mode=OneWay}"/>

<!-- Two-way binding (input controls) -->
<ToggleSwitch IsOn="{x:Bind ViewModel.IsRunning, Mode=TwoWay}"/>

<!-- Command binding -->
<Button Command="{x:Bind ViewModel.StartCommand}"
        Content="Start"
        Background="{StaticResource PrimaryBrush}"/>

<!-- Converter for visibility -->
<TextBlock Visibility="{x:Bind ViewModel.IsRunning, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}"/>
```

**Always use `x:Bind` over `Binding`** — compile-time checked, better performance.

---

## System Tray Integration

```csharp
// App.xaml.cs — initialize tray on startup
protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    // Don't create main window if starting minimized
    if (_config.StartMinimized)
    {
        _trayService.Initialize();
        return;
    }
    _mainWindow = new MainWindow();
    _mainWindow.Activate();
}
```

---

## Splash Screen Window

```csharp
// SplashWindow.xaml.cs
public sealed partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
        // No title bar, centered, fixed size
        AppWindow.SetPresenter(AppWindowPresenterKind.Default);
        AppWindow.Resize(new SizeInt32(400, 300));
        CenterOnScreen();
    }

    private void CenterOnScreen()
    {
        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary);
        var centerX = (displayArea.WorkArea.Width - 400) / 2;
        var centerY = (displayArea.WorkArea.Height - 300) / 2;
        AppWindow.Move(new PointInt32(centerX, centerY));
    }
}
```

---

## Common Pitfalls in WinUI 3

- **UI thread access**: always use `DispatcherQueue.TryEnqueue` when updating UI from background thread
  ```csharp
  _dispatcherQueue.TryEnqueue(() => StatusText = "Active");
  ```
- **x:Bind requires Mode=OneWay/TwoWay** for properties that change — default is OneTime
- **Page DataContext via DI**: set in constructor, never in XAML directly for DI-injected VMs
- **ObservableProperty naming**: `[ObservableProperty] private bool _isRunning` → generates `IsRunning` (strips leading underscore, capitalizes)
- **RelayCommand naming**: `[RelayCommand] private async Task StartAsync()` → generates `StartCommand`
