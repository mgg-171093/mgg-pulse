using MGG.Pulse.Domain.Ports;
using System.Runtime.InteropServices;

namespace MGG.Pulse.Infrastructure.Win32;

public class Win32IdleDetector : IIdleDetector
{
    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    public TimeSpan GetIdleTime()
    {
        var lastInput = new LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>() };
        if (!GetLastInputInfo(ref lastInput))
        {
            return TimeSpan.Zero;
        }

        var idleMilliseconds = (uint)Environment.TickCount - lastInput.dwTime;
        return TimeSpan.FromMilliseconds(idleMilliseconds);
    }
}
