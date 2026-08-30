using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace ExtraIsland.Shared;

internal static class WindowStatusDetect {
    internal const int StatusMaximized = 0;
    internal const int StatusFullscreen = 1;

    internal static bool Check(int status) {
        return IsAnyWindowMatching(status);
    }

    internal static int Snapshot() {
        return (IsAnyWindowMatching(StatusMaximized) ? 1 : 0)
             | (IsAnyWindowMatching(StatusFullscreen) ? 2 : 0);
    }

    static bool IsAnyWindowMatching(int status) {
        bool matched = false;
        WindowStatusNative.EnumWindows((handle, _) => {
            if (IsWindowMatching(handle, status)) {
                matched = true;
                return false;
            }
            return true;
        }, IntPtr.Zero);
        return matched;
    }

    static bool IsWindowMatching(IntPtr handle, int status) {
        if (handle == IntPtr.Zero) return false;
        if (!WindowStatusNative.IsWindowVisible(handle)) return false;
        if (WindowStatusNative.IsWindowCloaked(handle)) return false;
        string className = WindowStatusNative.GetWindowClassName(handle);
        if (className is "WorkerW" or "Progman") return false;
        return status switch {
            StatusMaximized => WindowStatusNative.IsWindowMaximized(handle),
            StatusFullscreen => WindowStatusNative.IsWindowFullscreen(handle),
            _ => false
        };
    }
}

[SuppressMessage("Interoperability","SYSLIB1054:使用 “LibraryImportAttribute” 而不是 “DllImportAttribute” 在编译时生成 P/Invoke 封送代码")]
static class WindowStatusNative {
    const int DwmwaCloaked = 14;
    const uint MonitorDefaultToNearest = 2;

    internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    internal static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    internal static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern bool IsZoomed(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    internal static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll")]
    internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO info);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern int GetClassName(IntPtr hWnd, StringBuilder name, int maxCount);

    [DllImport("dwmapi.dll")]
    internal static extern int DwmGetWindowAttribute(IntPtr hwnd, int attribute, out int value, int size);

    internal static bool IsWindowMaximized(IntPtr handle) {
        return IsZoomed(handle);
    }

    internal static bool IsWindowCloaked(IntPtr handle) {
        return DwmGetWindowAttribute(handle, DwmwaCloaked, out int cloaked, sizeof(int)) == 0 && cloaked != 0;
    }

    internal static string GetWindowClassName(IntPtr handle) {
        StringBuilder buffer = new StringBuilder(256);
        return GetClassName(handle, buffer, buffer.Capacity) > 0 ? buffer.ToString() : string.Empty;
    }

    internal static bool IsWindowFullscreen(IntPtr handle) {
        if (!GetWindowRect(handle, out RECT rect)) return false;
        IntPtr monitor = MonitorFromWindow(handle, MonitorDefaultToNearest);
        if (monitor == IntPtr.Zero) return false;
        MONITORINFO info = new MONITORINFO {
            cbSize = Marshal.SizeOf<MONITORINFO>()
        };
        if (!GetMonitorInfo(monitor, ref info)) return false;
        return rect.Left <= info.RcMonitor.Left && rect.Top <= info.RcMonitor.Top
            && rect.Right >= info.RcMonitor.Right && rect.Bottom >= info.RcMonitor.Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MONITORINFO {
        public int cbSize;
        public RECT RcMonitor;
        public RECT RcWork;
        public uint dwFlags;
    }
}
