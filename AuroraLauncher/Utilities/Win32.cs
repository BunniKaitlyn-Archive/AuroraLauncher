using System.Runtime.InteropServices;

public static class Win32
{
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool AllocConsole();

    public delegate bool HandlerRoutine(int dwCtrlType);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetConsoleCtrlHandler(HandlerRoutine HandlerRoutine, bool Add);
}
