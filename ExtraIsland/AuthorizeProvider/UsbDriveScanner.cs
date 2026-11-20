using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace ExtraIsland.AuthorizeProvider;

public sealed record UsbDriveInfo(
    string? SerialNumber,
    IReadOnlyList<string> DriveLetters,
    string DisplayName,
    int? DeviceNumber);

[SuppressMessage("Interoperability", "SYSLIB1054:使用 “LibraryImportAttribute” 而不是 “DllImportAttribute” 在编译时生成 P/Invoke 封送代码")]
public static class UsbDriveScanner {
    const uint FileShareReadWrite = 0x00000003u;
    const uint OpenExisting = 3u;
    const uint IoctlStorageGetDeviceNumber = 0x002D1080u;
    const uint IoctlStorageQueryProperty = 0x002D1400u;
    const int FileDeviceDisk = 7;
    const int BusTypeUsb = 7;
    const int MaxDescriptorBufferSize = 65536;

    static readonly int[] ArrivalRetryDelaysMs = [0, 150, 250, 400, 600, 800];

    static readonly HashSet<string> InvalidSerialPlaceholders = new(StringComparer.OrdinalIgnoreCase) {
        "none",
        "null",
        "n/a",
        "unknown",
        "default",
        "to be filled by o.e.m.",
        "0000"
    };

    public static IReadOnlyList<UsbDriveInfo> ScanUsbDrives() {
        List<VolumeProbeResult> usbVolumes = [];

        DriveInfo[] drives;
        try {
            drives = DriveInfo.GetDrives();
        } catch {
            return [];
        }

        foreach (DriveInfo drive in drives) {
            DriveType driveType;
            try {
                driveType = drive.DriveType;
            } catch {
                continue;
            }

            if (driveType is not (DriveType.Removable or DriveType.Fixed)) {
                continue;
            }

            string? normalizedLetter = NormalizeDriveLetter(drive.Name);
            if (normalizedLetter is null) {
                continue;
            }

            VolumeProbeResult? probed = ProbeUsbVolume(normalizedLetter);
            if (probed is not null) {
                usbVolumes.Add(probed);
            }
        }

        return GroupVolumes(usbVolumes);
    }

    public static Task<IReadOnlyList<UsbDriveInfo>> ScanUsbDrivesAsync(CancellationToken cancellationToken = default) {
        return Task.Run(ScanUsbDrives, cancellationToken);
    }

    public static async Task<UsbDriveInfo?> FindUsbDriveByLettersAsync(
        IReadOnlyList<string> driveLetters,
        CancellationToken cancellationToken = default) {
        HashSet<string> targetLetters = new(StringComparer.OrdinalIgnoreCase);
        foreach (string letter in driveLetters) {
            string? normalized = NormalizeDriveLetter(letter);
            if (normalized is not null) {
                targetLetters.Add(normalized);
            }
        }

        if (targetLetters.Count == 0) {
            return null;
        }

        foreach (int delayMs in ArrivalRetryDelaysMs) {
            if (delayMs > 0) {
                await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
            } else {
                cancellationToken.ThrowIfCancellationRequested();
            }

            IReadOnlyList<UsbDriveInfo> usbDrives = await ScanUsbDrivesAsync(cancellationToken).ConfigureAwait(false);
            foreach (UsbDriveInfo usbDrive in usbDrives) {
                foreach (string letter in usbDrive.DriveLetters) {
                    if (targetLetters.Contains(letter)) {
                        return usbDrive;
                    }
                }
            }
        }

        return null;
    }

    internal static string? NormalizeDriveLetter(string? driveLetter) {
        if (string.IsNullOrWhiteSpace(driveLetter)) {
            return null;
        }

        string trimmed = driveLetter.Trim();
        char ch = char.ToUpperInvariant(trimmed[0]);
        if (ch is < 'A' or > 'Z') {
            return null;
        }

        if (trimmed.Length == 1) {
            return $"{ch}:";
        }

        if (trimmed[1] != ':') {
            return null;
        }

        if (trimmed.Length == 2 || (trimmed.Length == 3 && trimmed[2] is '\\' or '/')) {
            return $"{ch}:";
        }

        return null;
    }

    internal static string? NormalizeAndValidateSerialNumber(string? raw) {
        if (string.IsNullOrWhiteSpace(raw)) {
            return null;
        }

        string cleaned = raw.Trim().Trim('\0').Trim();
        if (cleaned.Length < 4) {
            return null;
        }

        char first = cleaned[0];
        bool allSame = true;
        foreach (char c in cleaned) {
            if (c is < '\x20' or > '\x7E') {
                return null;
            }
            if (c != first) {
                allSame = false;
            }
        }

        if (allSame || InvalidSerialPlaceholders.Contains(cleaned)) {
            return null;
        }

        return cleaned;
    }

    static VolumeProbeResult? ProbeUsbVolume(string normalizedLetter) {
        string volumePath = $@"\\.\{normalizedLetter}";
        using SafeFileHandle volumeHandle = CreateFileW(
            volumePath,
            0,
            FileShareReadWrite,
            IntPtr.Zero,
            OpenExisting,
            0,
            IntPtr.Zero);

        if (volumeHandle.IsInvalid) {
            return null;
        }

        int? deviceNumber = TryGetDeviceNumber(volumeHandle);
        bool hasVolumeDescriptor = TryQueryStorageDescriptor(volumeHandle, out StorageDescriptor volumeDescriptor);

        if (hasVolumeDescriptor && volumeDescriptor.BusType != BusTypeUsb) {
            return null;
        }

        StorageDescriptor descriptor = volumeDescriptor;
        bool isUsb = hasVolumeDescriptor && volumeDescriptor.BusType == BusTypeUsb;

        bool needsPhysicalQuery = !hasVolumeDescriptor ||
                                  string.IsNullOrWhiteSpace(descriptor.SerialNumber) ||
                                  (string.IsNullOrWhiteSpace(descriptor.VendorId) &&
                                   string.IsNullOrWhiteSpace(descriptor.ProductId));

        if (needsPhysicalQuery && deviceNumber.HasValue) {
            string physicalPath = $@"\\.\PhysicalDrive{deviceNumber.Value}";
            using SafeFileHandle physicalHandle = CreateFileW(
                physicalPath,
                0,
                FileShareReadWrite,
                IntPtr.Zero,
                OpenExisting,
                0,
                IntPtr.Zero);

            if (!physicalHandle.IsInvalid &&
                TryQueryStorageDescriptor(physicalHandle, out StorageDescriptor physicalDescriptor) &&
                physicalDescriptor.BusType == BusTypeUsb) {
                isUsb = true;
                descriptor = new StorageDescriptor(
                    BusTypeUsb,
                    FirstNonEmpty(descriptor.VendorId, physicalDescriptor.VendorId),
                    FirstNonEmpty(descriptor.ProductId, physicalDescriptor.ProductId),
                    FirstNonEmpty(descriptor.SerialNumber, physicalDescriptor.SerialNumber));
            }
        }

        if (!isUsb) {
            return null;
        }

        return new VolumeProbeResult(
            normalizedLetter,
            deviceNumber,
            NormalizeAndValidateSerialNumber(descriptor.SerialNumber),
            descriptor.VendorId,
            descriptor.ProductId);
    }

    static IReadOnlyList<UsbDriveInfo> GroupVolumes(List<VolumeProbeResult> usbVolumes) {
        List<UsbDriveInfo> result = [];
        Dictionary<int, List<VolumeProbeResult>> byDeviceNumber = [];
        List<VolumeProbeResult> ungrouped = [];

        foreach (VolumeProbeResult vol in usbVolumes) {
            if (vol.DeviceNumber.HasValue) {
                if (!byDeviceNumber.TryGetValue(vol.DeviceNumber.Value, out List<VolumeProbeResult>? list)) {
                    list = [];
                    byDeviceNumber[vol.DeviceNumber.Value] = list;
                }
                list.Add(vol);
            } else {
                ungrouped.Add(vol);
            }
        }

        foreach ((int deviceNumber, List<VolumeProbeResult> group) in byDeviceNumber.OrderBy(kv => kv.Key)) {
            result.Add(BuildDriveInfo(deviceNumber, group));
        }

        foreach (VolumeProbeResult single in ungrouped.OrderBy(v => v.DriveLetter, StringComparer.OrdinalIgnoreCase)) {
            result.Add(BuildDriveInfo(null, [single]));
        }

        return result;
    }

    static UsbDriveInfo BuildDriveInfo(int? deviceNumber, List<VolumeProbeResult> volumes) {
        List<string> letters = volumes
            .Select(v => v.DriveLetter)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(l => l, StringComparer.OrdinalIgnoreCase)
            .ToList();

        string? serial = volumes.Select(v => v.SerialNumber).FirstOrDefault(s => !string.IsNullOrEmpty(s));
        string? vendor = volumes.Select(v => v.VendorId).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
        string? product = volumes.Select(v => v.ProductId).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

        string deviceName = BuildDeviceName(vendor, product);
        string displayName = $"{deviceName} ({string.Join(", ", letters)})";
        return new UsbDriveInfo(serial, letters, displayName, deviceNumber);
    }

    static string BuildDeviceName(string? vendor, string? product) {
        string v = CollapseWhitespace(vendor);
        string p = CollapseWhitespace(product);

        if (v.Length > 0 && p.Length > 0) {
            return p.StartsWith(v, StringComparison.OrdinalIgnoreCase) ? p : $"{v} {p}";
        }

        if (p.Length > 0) {
            return p;
        }

        return v.Length > 0 ? v : "USB 存储器";
    }

    static string CollapseWhitespace(string? value) {
        if (string.IsNullOrWhiteSpace(value)) {
            return string.Empty;
        }

        return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    static string? FirstNonEmpty(string? primary, string? fallback) {
        return !string.IsNullOrWhiteSpace(primary) ? primary : fallback;
    }

    static int? TryGetDeviceNumber(SafeFileHandle handle) {
        uint dummyIn = 0;
        byte[] buffer = new byte[12];
        if (!DeviceIoControl(
                handle,
                IoctlStorageGetDeviceNumber,
                ref dummyIn,
                0,
                buffer,
                buffer.Length,
                out int bytesReturned,
                IntPtr.Zero) || bytesReturned < 8) {
            return null;
        }

        int deviceType = BitConverter.ToInt32(buffer, 0);
        if (deviceType != FileDeviceDisk) {
            return null;
        }

        return BitConverter.ToInt32(buffer, 4);
    }

    static bool TryQueryStorageDescriptor(SafeFileHandle handle, out StorageDescriptor descriptor) {
        descriptor = default;

        StoragePropertyQuery query = new StoragePropertyQuery {
            PropertyId = 0, // StorageDeviceProperty
            QueryType = 0,  // PropertyStandardQuery
            AdditionalParameters = 0
        };
        int querySize = Marshal.SizeOf<StoragePropertyQuery>();
        byte[] buffer = new byte[1024];

        if (!DeviceIoControl(
                handle,
                IoctlStorageQueryProperty,
                ref query,
                querySize,
                buffer,
                buffer.Length,
                out int bytesReturned,
                IntPtr.Zero)) {
            return false;
        }

        if (bytesReturned >= 8) {
            uint requiredSize = BitConverter.ToUInt32(buffer, 4);
            if (requiredSize > buffer.Length && requiredSize <= MaxDescriptorBufferSize) {
                buffer = new byte[requiredSize];
                if (!DeviceIoControl(
                        handle,
                        IoctlStorageQueryProperty,
                        ref query,
                        querySize,
                        buffer,
                        buffer.Length,
                        out bytesReturned,
                        IntPtr.Zero)) {
                    return false;
                }
            }
        }

        if (bytesReturned < 36) {
            return false;
        }

        int busType = BitConverter.ToInt32(buffer, 28);
        string? vendorId = ReadDescriptorString(buffer, bytesReturned, BitConverter.ToUInt32(buffer, 12));
        string? productId = ReadDescriptorString(buffer, bytesReturned, BitConverter.ToUInt32(buffer, 16));
        string? serialNumber = ReadDescriptorString(buffer, bytesReturned, BitConverter.ToUInt32(buffer, 24));

        descriptor = new StorageDescriptor(busType, vendorId, productId, serialNumber);
        return true;
    }

    static string? ReadDescriptorString(byte[] buffer, int bytesReturned, uint offset) {
        if (offset == 0 || offset >= (uint)bytesReturned) {
            return null;
        }

        int start = (int)offset;
        int end = Array.IndexOf(buffer, (byte)0, start, bytesReturned - start);
        if (end < 0) {
            end = bytesReturned;
        }

        if (end <= start) {
            return null;
        }

        string value = Encoding.ASCII.GetString(buffer, start, end - start).Trim();
        return value.Length == 0 ? null : value;
    }

    readonly record struct StorageDescriptor(
        int BusType,
        string? VendorId,
        string? ProductId,
        string? SerialNumber);

    sealed record VolumeProbeResult(
        string DriveLetter,
        int? DeviceNumber,
        string? SerialNumber,
        string? VendorId,
        string? ProductId);

    [StructLayout(LayoutKind.Sequential)]
    struct StoragePropertyQuery {
        public int PropertyId;
        public int QueryType;
        public byte AdditionalParameters;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern SafeFileHandle CreateFileW(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        uint dwIoControlCode,
        ref uint lpInBuffer,
        int nInBufferSize,
        [Out] byte[] lpOutBuffer,
        int nOutBufferSize,
        out int lpBytesReturned,
        IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        uint dwIoControlCode,
        ref StoragePropertyQuery lpInBuffer,
        int nInBufferSize,
        [Out] byte[] lpOutBuffer,
        int nOutBufferSize,
        out int lpBytesReturned,
        IntPtr lpOverlapped);
}