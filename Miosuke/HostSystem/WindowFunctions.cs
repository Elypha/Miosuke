// ECommons/Interop/WindowFunctions.cs
// --------------------------------
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace Miosuke.HostSystem;

public static unsafe class WindowFunctions
{
    public const int SW_MINIMIZE = 6;
    public const int SW_FORCEMINIMIZE = 11;
    public const int SW_HIDE = 0;
    public const int SW_SHOW = 5;
    public const int SW_SHOWNA = 8;


    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string? lpszWindow);


    public static bool TryFindGameWindow(out IntPtr hwnd)
    {
        hwnd = IntPtr.Zero;
        while (true)
        {
            hwnd = FindWindowEx(IntPtr.Zero, hwnd, "FFXIVGAME", null);
            if (hwnd == IntPtr.Zero) break;
            GetWindowThreadProcessId(hwnd, out var pid);
            if (pid == Environment.ProcessId) break;
        }
        return hwnd != IntPtr.Zero;
    }

    public static bool ApplicationIsActivated()
    {
        var activatedHandle = TerraFX.Interop.Windows.Windows.GetForegroundWindow();
        if (activatedHandle == HWND.NULL)
        {
            return false;
        }

        var procId = (uint)Environment.ProcessId;
        uint activeProcId;
        TerraFX.Interop.Windows.Windows.GetWindowThreadProcessId(activatedHandle, &activeProcId);

        return activeProcId == procId;
    }


    public static bool SendKeypress(int keycode)
    {
        if (TryFindGameWindow(out var hwnd))
        {
            TerraFX.Interop.Windows.Windows.SendMessage((HWND)hwnd, WM.WM_KEYDOWN, (WPARAM)keycode, (LPARAM)0);
            TerraFX.Interop.Windows.Windows.SendMessage((HWND)hwnd, WM.WM_KEYUP, (WPARAM)keycode, (LPARAM)0);
            return true;
        }
        return false;
    }


    public static bool? IsMinimised()
    {
        if (TryFindGameWindow(out var hwnd))
        {
            return IsIconic(hwnd);
        }
        return null;
    }

    public static bool IsMinimised(IntPtr hwnd)
    {
        return IsIconic(hwnd);
    }
}
