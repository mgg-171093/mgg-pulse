namespace MGG.Pulse.Domain.Ports;

public interface ITrayService
{
    public void Initialize(Action onShow, Action onStartStop, Action onExit);
    public void SetTooltip(string text);
    public void ShowNotification(string title, string message);
    public void SetRunningState(bool isRunning);
    public void Dispose();
}
