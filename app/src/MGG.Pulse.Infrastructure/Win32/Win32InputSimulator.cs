using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;
using System.Runtime.InteropServices;

namespace MGG.Pulse.Infrastructure.Win32;

public class Win32InputSimulator : IInputSimulator
{
    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;
    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const ushort VK_SHIFT = 0x10;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUT
    {
        [FieldOffset(0)] public uint type;
        [FieldOffset(4)] public MOUSEINPUT mi;
        [FieldOffset(4)] public KEYBDINPUT ki;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    private readonly Random _random = new();

    public Task ExecuteAsync(InputType inputType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        switch (inputType)
        {
            case InputType.Mouse:
                SimulateMouse();
                break;
            case InputType.Keyboard:
                SimulateKeyboard();
                break;
            case InputType.Combined:
                SimulateMouse();
                SimulateKeyboard();
                break;
        }

        return Task.CompletedTask;
    }

    private void SimulateMouse()
    {
        // Move 1-2 pixels in a random direction (relative)
        var dx = _random.Next(0, 2) == 0 ? 1 : -1;
        var dy = _random.Next(0, 2) == 0 ? 1 : -1;

        var input = new INPUT
        {
            type = INPUT_MOUSE,
            mi = new MOUSEINPUT
            {
                dx = dx,
                dy = dy,
                dwFlags = MOUSEEVENTF_MOVE
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    private void SimulateKeyboard()
    {
        var keyDown = new INPUT
        {
            type = INPUT_KEYBOARD,
            ki = new KEYBDINPUT { wVk = VK_SHIFT }
        };
        var keyUp = new INPUT
        {
            type = INPUT_KEYBOARD,
            ki = new KEYBDINPUT { wVk = VK_SHIFT, dwFlags = KEYEVENTF_KEYUP }
        };

        SendInput(2, new[] { keyDown, keyUp }, Marshal.SizeOf<INPUT>());
    }
}
