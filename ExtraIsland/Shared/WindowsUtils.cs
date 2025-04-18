using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Interop;
using Microsoft.Extensions.Logging;

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

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
        #pragma warning disable CA1806
        GetWindowThreadProcessId(foregroundWindow,out int processId);
        #pragma warning restore CA1806
        return processId == Environment.ProcessId;
    }

    /// 获取当前前台窗口标题
    public static string? GetActiveWindowTitle() {
        const int nChars = 256;
        StringBuilder buffer = new StringBuilder(nChars);
        IntPtr handle = GetForegroundWindow();
        return GetWindowText(handle,buffer,nChars) > 0 ? buffer.ToString() : null;
    }

    public class UsbDriveMonitor : IDisposable {
        HwndSource? _hwndSource;
        IntPtr _notificationHandle;

        public event EventHandler<string>? UsbDriveInserted;

        public UsbDriveMonitor() {
            CreateWindowHandle();
            RegisterForDeviceNotifications();
        }

        void CreateWindowHandle() {
            HwndSourceParameters parameters = new HwndSourceParameters("UsbDriveMonitorWindow") {
                Width = 0,
                Height = 0,
                WindowStyle = 0,
                ExtendedWindowStyle = 0,
            };

            _hwndSource = new HwndSource(parameters);
            _hwndSource.AddHook(WndProc);
        }

        void RegisterForDeviceNotifications() {
            DevBroadcastDeviceinterface dbh = new DevBroadcastDeviceinterface {
                dbcc_size = Marshal.SizeOf(typeof(DevBroadcastDeviceinterface)),
                dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE,
                dbcc_classguid = GUID_DEVINTERFACE_VOLUME
            };

            IntPtr buffer = Marshal.AllocHGlobal(dbh.dbcc_size);
            Marshal.StructureToPtr(dbh,buffer,true);

            _notificationHandle = RegisterDeviceNotification(
                _hwndSource!.Handle,
                buffer,
                DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        IntPtr WndProc(IntPtr hwnd,int msg,IntPtr wParam,IntPtr lParam,ref bool handled) {
            if (msg != WM_DEVICECHANGE) return IntPtr.Zero;
            switch (wParam) {
                case DBT_DEVICEARRIVAL:
                    int deviceType = Marshal.ReadInt32(lParam,4);
                    if (deviceType == DBT_DEVTYP_VOLUME) {
                        DevBroadcastVolume volumeInfo = (DevBroadcastVolume)Marshal.PtrToStructure(
                            lParam,typeof(DevBroadcastVolume))!;
                        string driveLetter = GetDriveLetter(volumeInfo.dbcv_unitmask);
                        UsbDriveInserted?.Invoke(this,driveLetter);
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        static string GetDriveLetter(uint unitMask) {
            for (int i = 0; i < 26; i++) {
                if ((unitMask & (1 << i)) != 0) {
                    return $"{Convert.ToChar('A' + i)}:";
                }
            }
            return string.Empty;
        }

        public void Dispose() {
            if (_notificationHandle != IntPtr.Zero) {
                UnregisterDeviceNotification(_notificationHandle);
                _notificationHandle = IntPtr.Zero;
            }

            _hwndSource?.Dispose();
            GC.SuppressFinalize(this);
        }

        #region Win32 Interop

        const int WM_DEVICECHANGE = 0x219;
        const int DBT_DEVICEARRIVAL = 0x8000;
        const int DBT_DEVTYP_VOLUME = 0x00000002;
        const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;
        const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        static readonly Guid GUID_DEVINTERFACE_VOLUME = new Guid("53F5630D-B6BF-11D0-94F2-00A0C91EFB8B");

        [DllImport("user32.dll",CharSet = CharSet.Auto,SetLastError = true)]
        static extern IntPtr RegisterDeviceNotification(
            IntPtr hRecipient,
            IntPtr NotificationFilter,
            int Flags);

        [DllImport("user32.dll",SetLastError = true)]
        [return:MarshalAs(UnmanagedType.Bool)]
        static extern bool UnregisterDeviceNotification(IntPtr Handle);

        [StructLayout(LayoutKind.Sequential)]
        struct DevBroadcastDeviceinterface {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValArray,SizeConst = 255)]
            public char[] dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DevBroadcastVolume {
            public int dbcv_size;
            public int dbcv_devicetype;
            public int dbcv_reserved;
            public uint dbcv_unitmask;
            public ushort dbcv_flags;
        }

        #endregion

    }

    public class UsbDriveInfo {
        public string? SerialNumber;
        public List<string>? DriveLetter;
    }

    public static List<UsbDriveInfo> ScanUsbDrive() {
        List<UsbDriveInfo> driveInfos = [];
        ManualResetEventSlim completionEvent = new ManualResetEventSlim();
        Exception? threadException = null;

        // 创建专用 STA 线程执行 WMI 操作
        Thread staThread = new Thread(() => {
            ManagementClass driveClass = new ManagementClass("Win32_DiskDrive");
            // ReSharper disable PossibleInvalidCastExceptionInForeachLoop
            foreach (ManagementObject drive in driveClass.GetInstances()) {
                if (drive!["InterfaceType"]?.ToString() != "USB") continue;
                try {
                    // 获取关联的分区
                    List<string> driveLetters = [];
                    foreach (ManagementObject partition in drive.GetRelated("Win32_DiskPartition")) {
                        // 获取关联的逻辑磁盘
                        foreach (ManagementObject disk in partition.GetRelated("Win32_LogicalDisk")) {
                            driveLetters.Add(disk["DeviceID"].ToString()!);
                        }
                    }

                    // 获取设备序列号
                    string pnpID = drive["PNPDeviceID"]?.ToString() ?? "";
                    string[] info = pnpID.Split('&');
                    string serial = (info.Length > 3
                        ? info[3].Split('\\').LastOrDefault()
                        : null)!;

                    // 显示结果
                    driveInfos.Add(new UsbDriveInfo {
                        SerialNumber = serial,
                        DriveLetter = driveLetters
                    });
                }
                catch (Exception ex) {
                    GlobalConstants.HostInterfaces.PluginLogger?.LogError(ex,"扫描USB设备时遇到问题");
                }
                finally {
                    drive.Dispose();
                }
            }
            completionEvent.Set();
            // ReSharper restore PossibleInvalidCastExceptionInForeachLoop
        });

        // 配置 STA 线程
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.IsBackground = true;
        staThread.Start();
        completionEvent.Wait();

        // 处理线程异常
        if (threadException == null) return driveInfos;
        GlobalConstants.HostInterfaces.PluginLogger?.LogError(threadException,"扫描USB设备时遇到问题");
        return [];
    }

    public static UsbDriveInfo FindUsbDriveByLetter(string driveLetter) {
        foreach (UsbDriveInfo info in ScanUsbDrive()) {
            if (info.DriveLetter == null) continue;
            if (info.DriveLetter.Contains(driveLetter)) return info;
        }
        return new UsbDriveInfo();
    }

    public static void DetectUsb() {
        List<UsbDriveInfo> driveInfos = ScanUsbDrive();
        foreach (UsbDriveInfo driveInfo in driveInfos) {
            switch (driveInfo.DriveLetter!.Count) {
                case <= 0:
                    MessageBox.Show($"找到U盘但未分配盘符\n序列号: {driveInfo.SerialNumber}");
                    break;
                default:
                    MessageBox.Show($"U盘序列号: {driveInfo.SerialNumber}\n盘符: {string.Join(", ",driveInfo.DriveLetter)}");
                    break;
            }
        }
    }
}