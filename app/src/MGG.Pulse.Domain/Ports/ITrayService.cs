namespace MGG.Pulse.Domain.Ports;

public interface ITrayService
{
    void Initialize(Action onShow, Action onStartStop, Action onExit);
    void SetTooltip(string text);
    void ShowNotification(string title, string message);
    void SetRunningState(bool isRunning);
    void Dispose();
}
