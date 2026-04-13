using System.Text;

namespace MGG.Pulse.UI.Diagnostics;

/// <summary>
/// Static crash logger — writes to disk WITHOUT DI.
/// Safe to call from any point including before the DI container is built.
/// All failures are swallowed silently (the logger must never crash the app).
/// </summary>
internal static class CrashLogger
{
    private static readonly string CrashLogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MGG", "Pulse", "crash.log");

    public static void Write(Exception ex) =>
        Write($"[{ex.GetType().Name}] {ex.Message}\n{ex.StackTrace}");

    public static void Write(string message)
    {
        try
        {
            var dir = Path.GetDirectoryName(CrashLogPath)!;
            Directory.CreateDirectory(dir);
            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CRASH\n{message}\n{new string('-', 80)}\n";
            File.AppendAllText(CrashLogPath, entry, Encoding.UTF8);
        }
        catch { /* never throw from a crash logger */ }
    }
}
