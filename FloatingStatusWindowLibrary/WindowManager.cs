using Common.Native;
using Common.Wpf.Windows;
using System;
using System.Collections.Generic;

namespace FloatingStatusWindowLibrary
{
    public static class WindowManager
    {
        private const string WindowMessageSetLock = "FloatingStatusWindowLibrary_SetLock";
        private const string WindowMessageClose = "FloatingStatusWindowLibrary_Close";

        public static uint SetLockMessage { get; set; }
        public static uint CloseMessage { get; set; }

        static WindowManager()
        {
            SetLockMessage = Functions.User32.RegisterWindowMessage(WindowMessageSetLock);
            CloseMessage = Functions.User32.RegisterWindowMessage(WindowMessageClose);
        }

        private static readonly object WindowLocker = new object();

        private static List<WindowInformation> _windowList;
        private static IntPtr _excludeHandle;

        public static List<WindowInformation> GetWindowList()
        {
            lock (WindowLocker)
            {
                _windowList = new List<WindowInformation>();
                _excludeHandle = IntPtr.Zero;

                Functions.User32.EnumWindows(EnumWindowProc, IntPtr.Zero);

                return _windowList;
            }
        }

        public static List<WindowInformation> GetWindowList(IntPtr excludeHandle)
        {
            lock (WindowLocker)
            {
                _windowList = new List<WindowInformation>();
                _excludeHandle = excludeHandle;

                Functions.User32.EnumWindows(EnumWindowProc, IntPtr.Zero);

                return _windowList;
            }
        }

        private static bool EnumWindowProc(IntPtr hWnd, IntPtr lParam)
        {
            var windowText = Functions.Window.GetText(hWnd);

            if (windowText == "FloatingStatusWindow" && hWnd != _excludeHandle)
                _windowList.Add(new WindowInformation(hWnd));

            return true;
        }

        public static void SetLockOnAll(bool locked)
        {
            var lockState = locked ? (IntPtr) 1 : (IntPtr) 0;

            foreach (var w in GetWindowList())
                Functions.User32.SendMessage(w.Handle, SetLockMessage, lockState, IntPtr.Zero);
        }

        public static void CloseAll()
        {
            foreach (var w in GetWindowList())
                Functions.User32.SendMessage(w.Handle, CloseMessage, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
