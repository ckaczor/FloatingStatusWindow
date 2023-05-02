using ChrisKaczor.Wpf.Windows;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ChrisKaczor.Wpf.FloatingStatusWindow;

public static partial class WindowManager
{
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial void EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16, EntryPoint = "RegisterWindowMessageW")]
    private static partial uint RegisterWindowMessage(string lpString);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowTextLengthW")]
    private static partial int GetWindowTextLength(IntPtr hWnd);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial void SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private const string WindowMessageSetLock = "FloatingStatusWindowLibrary_SetLock";
    private const string WindowMessageClose = "FloatingStatusWindowLibrary_Close";

    public static uint SetLockMessage { get; set; }
    public static uint CloseMessage { get; set; }

    static WindowManager()
    {
        SetLockMessage = RegisterWindowMessage(WindowMessageSetLock);
        CloseMessage = RegisterWindowMessage(WindowMessageClose);
    }

    private static readonly object WindowLocker = new();

    private static List<WindowInformation> _windowList;
    private static IntPtr _excludeHandle;

    public static List<WindowInformation> GetWindowList()
    {
        lock (WindowLocker)
        {
            _windowList = new List<WindowInformation>();
            _excludeHandle = IntPtr.Zero;

            EnumWindows(EnumWindowProc, IntPtr.Zero);

            return _windowList;
        }
    }

    public static List<WindowInformation> GetWindowList(IntPtr excludeHandle)
    {
        lock (WindowLocker)
        {
            _windowList = new List<WindowInformation>();
            _excludeHandle = excludeHandle;

            EnumWindows(EnumWindowProc, IntPtr.Zero);

            return _windowList;
        }
    }

    private static string GetText(IntPtr hWnd)
    {
        // Allocate correct string length first
        var length = GetWindowTextLength(hWnd);
        var sb = new StringBuilder(length + 1);
        _ = GetWindowText(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private static bool EnumWindowProc(IntPtr hWnd, IntPtr lParam)
    {
        var windowText = GetText(hWnd);

        if (windowText == "FloatingStatusWindow" && hWnd != _excludeHandle)
            _windowList.Add(new WindowInformation(hWnd));

        return true;
    }

    public static void SetLockOnAll(bool locked)
    {
        var lockState = locked ? 1 : 0;

        foreach (var w in GetWindowList())
            SendMessage(w.Handle, SetLockMessage, lockState, IntPtr.Zero);
    }

    public static void CloseAll()
    {
        foreach (var w in GetWindowList())
            SendMessage(w.Handle, CloseMessage, IntPtr.Zero, IntPtr.Zero);
    }
}