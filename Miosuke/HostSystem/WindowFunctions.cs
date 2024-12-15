// ECommons/Interop/WindowFunctions.cs
// --------------------------------
using Miosuke.Messages;
using PInvoke;
using System;
using System.Runtime.InteropServices;

//using System.Windows.Forms;
using static PInvoke.User32;

namespace Miosuke.HostSystem;

public static partial class WindowFunctions
{
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool IsIconic(IntPtr hWnd);


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

    /// <summary>Returns true if the current application has focus, false otherwise</summary>
    public static bool ApplicationIsActivated()
    {
        var activatedHandle = GetForegroundWindow();
        if (activatedHandle == IntPtr.Zero)
        {
            return false;       // No window is currently activated
        }

        var procId = Environment.ProcessId;
        GetWindowThreadProcessId(activatedHandle, out var activeProcId);

        return activeProcId == procId;
    }

    public static bool SendKeypress(int keycode)
    {
        if (TryFindGameWindow(out var hwnd))
        {
            User32.SendMessage(hwnd, WindowMessage.WM_KEYDOWN, (IntPtr)keycode, (IntPtr)0);
            User32.SendMessage(hwnd, WindowMessage.WM_KEYUP, (IntPtr)keycode, (IntPtr)0);
            return true;
        }
        return false;
    }

    /*public static bool SendKeypress(Keys key)
    {
        return SendKeypress((int)key);
    }*/

    public static bool? IsMinimised()
    {
        if (TryFindGameWindow(out var hwnd))
        {
            return IsIconic(hwnd);
        }
        return null;
    }
}
