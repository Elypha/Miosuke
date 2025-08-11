// ECommons/Interop/WindowFunctions.cs
// --------------------------------
using System.Runtime.InteropServices;
using System.Threading;
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
    private static extern HWND FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);


    private static readonly ushort* FFXIVClassNamePtr;
    private static readonly Lock FFXIVClassNamePtrLock = new();
    static WindowFunctions()
    {
        var str = "FFXIVGAME\0";
        var size = str.Length * sizeof(char);
        var ptr = Marshal.AllocHGlobal(size);
        fixed (char* strPtr = str)
        {
            Buffer.MemoryCopy(strPtr, (void*)ptr, size, size);
        }
        FFXIVClassNamePtr = (ushort*)ptr;
    }

    public static void UnLoad()
    {
        Marshal.FreeHGlobal((IntPtr)FFXIVClassNamePtr);
    }


    public static bool TryFindGameWindow(out HWND hwnd)
    {
        hwnd = HWND.NULL;
        var prev = HWND.NULL;

        while (true)
        {
            prev = TerraFX.Interop.Windows.Windows.FindWindowEx(HWND.NULL, prev, FFXIVClassNamePtr, null);
            if (prev == HWND.NULL)
                break;

            uint pid;
            _ = TerraFX.Interop.Windows.Windows.GetWindowThreadProcessId(prev, &pid);
            if (pid == Environment.ProcessId)
            {
                hwnd = prev;
                break;
            }
        }

        return hwnd != HWND.NULL;
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
            TerraFX.Interop.Windows.Windows.SendMessage(hwnd, WM.WM_KEYDOWN, (WPARAM)keycode, (LPARAM)0);
            TerraFX.Interop.Windows.Windows.SendMessage(hwnd, WM.WM_KEYUP, (WPARAM)keycode, (LPARAM)0);
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
