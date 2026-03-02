using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ChrisKaczor.Wpf.Windows.FloatingStatusWindow;

public static partial class WindowManager
{
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private enum ChangeWindowMessageFilterExAction : uint
    {
        Allow = 1
    }

    [Flags]
    private enum SendMessageTimeoutFlags : uint
    {
        Normal = 0x0,
        Block = 0x1,
        AbortIfHung = 0x2,
        NoTimeoutIfNotHung = 0x8,
        ErrorOnExit = 0x20
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial void EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16, EntryPoint = "RegisterWindowMessageW")]
    private static partial int RegisterWindowMessage(string lpString);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowTextLengthW")]
    private static partial int GetWindowTextLength(IntPtr hWnd);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW", SetLastError = true)]
    private static partial IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageTimeoutW", SetLastError = true)]
    private static partial IntPtr SendMessageTimeout(
        IntPtr windowHandle,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        SendMessageTimeoutFlags flags,
        uint timeout,
        out IntPtr result);


    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial void ChangeWindowMessageFilterEx(IntPtr hWnd, int msg, ChangeWindowMessageFilterExAction action, IntPtr changeInfo);

    private const string WindowMessageIdentify = "FloatingStatusWindowLibrary_Identify";
    private const string WindowMessageSetLock = "FloatingStatusWindowLibrary_SetLock";
    private const string WindowMessageClose = "FloatingStatusWindowLibrary_Close";

    public static int IdentifyMessage { get; set; }
    public static int SetLockMessage { get; set; }
    public static int CloseMessage { get; set; }

    static WindowManager()
    {
        IdentifyMessage = RegisterWindowMessage(WindowMessageIdentify);
        SetLockMessage = RegisterWindowMessage(WindowMessageSetLock);
        CloseMessage = RegisterWindowMessage(WindowMessageClose);
    }

    private static readonly Lock WindowLocker = new();

    private static List<WindowInformation> _windowList;
    private static IntPtr _excludeHandle;

    public static void AllowMessagesThroughFilter(IntPtr hWnd)
    {
        ChangeWindowMessageFilterEx(hWnd, SetLockMessage, ChangeWindowMessageFilterExAction.Allow, IntPtr.Zero);
        ChangeWindowMessageFilterEx(hWnd, CloseMessage, ChangeWindowMessageFilterExAction.Allow, IntPtr.Zero);
    }

    public static List<WindowInformation> GetWindowList()
    {
        lock (WindowLocker)
        {
            _windowList = [];
            _excludeHandle = IntPtr.Zero;

            EnumWindows(EnumWindowProc, IntPtr.Zero);

            return _windowList;
        }
    }

    public static List<WindowInformation> GetWindowList(IntPtr excludeHandle)
    {
        lock (WindowLocker)
        {
            _windowList = [];
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
        if (hWnd == _excludeHandle)
            return true;

        SendMessageTimeout(hWnd, IdentifyMessage, IntPtr.Zero, IntPtr.Zero, SendMessageTimeoutFlags.Normal, 100, out var identifyResult);

        if (identifyResult == IdentifyMessage)
        {
            _windowList.Add(new WindowInformation(hWnd));
            return true;
        }

        var windowText = GetText(hWnd);

        if (windowText == "FloatingStatusWindow")
            _windowList.Add(new WindowInformation(hWnd));

        return true;
    }

    public static void SetLockOnAll(bool locked)
    {
        var lockState = locked ? 1 : 0;

        foreach (var w in GetWindowList())
            _ = SendMessage(w.Handle, SetLockMessage, lockState, IntPtr.Zero);
    }

    public static void CloseAll()
    {
        foreach (var w in GetWindowList())
            _ = SendMessage(w.Handle, CloseMessage, IntPtr.Zero, IntPtr.Zero);
    }
}