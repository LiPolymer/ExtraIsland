using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace ExtraIsland.Shared;

[SuppressMessage("Interoperability","SYSLIB1054:使用 “LibraryImportAttribute” 而不是 “DllImportAttribute” 在编译时生成 P/Invoke 封送代码")]
public static class WindowsUtils {
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll",CharSet = CharSet.Unicode)]
    static extern int GetWindowText(IntPtr hWnd,StringBuilder text,int count);

    [DllImport("user32.dll",SetLastError = true)]
    // ReSharper disable once IdentifierTypo
    static extern uint GetWindowThreadProcessId(IntPtr hWnd,out int lpdwProcessId);

    /// 检测当前系统前台窗口是否属于本进程
    public static bool IsOurWindowInForeground() {
        IntPtr foregroundWindow = GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero) return false;
        GetWindowThreadProcessId(foregroundWindow,out int processId);
        return processId == Environment.ProcessId;
    }

    /// 获取当前前台窗口标题
    public static string? GetActiveWindowTitle() {
        const int nChars = 256;
        StringBuilder buffer = new StringBuilder(nChars);
        IntPtr handle = GetForegroundWindow();
        return GetWindowText(handle,buffer,nChars) > 0 ? buffer.ToString() : null;
    }
}