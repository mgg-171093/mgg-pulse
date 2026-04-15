namespace MGG.Pulse.Domain.Ports;

public interface IIdleDetector
{
    public TimeSpan GetIdleTime();
}
