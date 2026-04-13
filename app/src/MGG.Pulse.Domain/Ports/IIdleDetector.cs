namespace MGG.Pulse.Domain.Ports;

public interface IIdleDetector
{
    TimeSpan GetIdleTime();
}
