using MGG.Pulse.Domain.Ports;
using System.Drawing;
using System.Windows.Forms;

namespace MGG.Pulse.Infrastructure.Tray;

public class SystemTrayService : ITrayService
{
    private NotifyIcon? _notifyIcon;
    private ContextMenuStrip? _menu;
    private ToolStripMenuItem? _startStopItem;
    private Thread? _staThread;
    private bool _isRunning;

    private Action? _onShow;
    private Action? _onStartStop;
    private Action? _onExit;

    public void Initialize(Action onShow, Action onStartStop, Action onExit)
    {
        _onShow = onShow;
        _onStartStop = onStartStop;
        _onExit = onExit;

        _staThread = new Thread(RunTrayThread)
        {
            IsBackground = true,
            Name = "TrayThread"
        };
        _staThread.SetApartmentState(ApartmentState.STA);
        _staThread.Start();
    }

    private void RunTrayThread()
    {
        Application.EnableVisualStyles();

        _notifyIcon = new NotifyIcon();
        _notifyIcon.Text = "MGG Pulse — Inactive";
        _notifyIcon.Visible = true;

        // Load icon from logo
        try
        {
            var logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png");
            if (File.Exists(logoPath))
            {
                using var bitmap = new Bitmap(logoPath);
                _notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
            }
            else
            {
                _notifyIcon.Icon = SystemIcons.Application;
            }
        }
        catch
        {
            _notifyIcon.Icon = SystemIcons.Application;
        }

        _notifyIcon.DoubleClick += (_, _) => _onShow?.Invoke();

        _menu = new ContextMenuStrip();

        var showItem = new ToolStripMenuItem("Show");
        showItem.Click += (_, _) => _onShow?.Invoke();
        _menu.Items.Add(showItem);

        _menu.Items.Add(new ToolStripSeparator());

        _startStopItem = new ToolStripMenuItem("Start");
        _startStopItem.Click += (_, _) => _onStartStop?.Invoke();
        _menu.Items.Add(_startStopItem);

        _menu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => _onExit?.Invoke();
        _menu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = _menu;

        Application.Run();
    }

    public void SetTooltip(string text)
    {
        if (_notifyIcon is null) return;
        InvokeOnTrayThread(() => _notifyIcon.Text = text[..Math.Min(text.Length, 63)]);
    }

    public void ShowNotification(string title, string message)
    {
        if (_notifyIcon is null) return;
        InvokeOnTrayThread(() => _notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info));
    }

    public void SetRunningState(bool isRunning)
    {
        _isRunning = isRunning;
        if (_notifyIcon is null || _startStopItem is null) return;

        InvokeOnTrayThread(() =>
        {
            _startStopItem!.Text = _isRunning ? "Stop" : "Start";
            _notifyIcon!.Text = _isRunning ? "MGG Pulse — Active" : "MGG Pulse — Inactive";
        });
    }

    public void Dispose()
    {
        InvokeOnTrayThread(() =>
        {
            if (_notifyIcon is not null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            Application.ExitThread();
        });
    }

    private void InvokeOnTrayThread(Action action)
    {
        if (_menu?.InvokeRequired == true)
            _menu.Invoke(action);
        else
            action();
    }
}
