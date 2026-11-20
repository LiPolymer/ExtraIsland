using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Threading;

namespace ExtraIsland.AuthorizeProvider;

public sealed class UsbVolumeWatcher {
    const uint WmDeviceChange = 0x0219u;
    const nuint DbtDeviceArrival = 0x8000u;
    const nuint DbtDeviceRemoveComplete = 0x8004u;
    const int DbtDevTypVolume = 2;
    const ushort DbtfNet = 0x0002;

    TopLevel? _attachedTopLevel;

    public event EventHandler<IReadOnlyList<string>>? VolumesArrived;
    public event EventHandler<IReadOnlyList<string>>? VolumesRemoved;

    public void Attach(TopLevel? topLevel) {
        if (topLevel is null) {
            Detach();
            return;
        }

        if (ReferenceEquals(_attachedTopLevel, topLevel)) {
            return;
        }

        Detach();
        _attachedTopLevel = topLevel;
        Win32Properties.AddWndProcHookCallback(topLevel, WndProcHook);
    }

    public void Detach() {
        if (_attachedTopLevel is null) {
            return;
        }

        Win32Properties.RemoveWndProcHookCallback(_attachedTopLevel, WndProcHook);
        _attachedTopLevel = null;
    }

    internal static IReadOnlyList<string> ExtractDriveLetters(uint unitMask) {
        if (unitMask == 0) {
            return [];
        }

        List<string> letters = [];
        for (int i = 0; i < 26; i++) {
            if ((unitMask & (1u << i)) != 0) {
                letters.Add($"{(char)('A' + i)}:");
            }
        }
        return letters;
    }

    IntPtr WndProcHook(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
        try {
            if (msg != WmDeviceChange || lParam == IntPtr.Zero) {
                return IntPtr.Zero;
            }

            nuint eventType = (nuint)(nint)wParam;
            if (eventType is not (DbtDeviceArrival or DbtDeviceRemoveComplete)) {
                return IntPtr.Zero;
            }

            int size = Marshal.ReadInt32(lParam, 0);
            int deviceType = Marshal.ReadInt32(lParam, 4);
            if (deviceType != DbtDevTypVolume || size < Marshal.SizeOf<DevBroadcastVolume>()) {
                return IntPtr.Zero;
            }

            DevBroadcastVolume volume = Marshal.PtrToStructure<DevBroadcastVolume>(lParam);
            if ((volume.dbcv_flags & DbtfNet) != 0) {
                return IntPtr.Zero;
            }

            IReadOnlyList<string> letters = ExtractDriveLetters(volume.dbcv_unitmask);
            if (letters.Count == 0) {
                return IntPtr.Zero;
            }

            if (eventType == DbtDeviceArrival) {
                Dispatcher.UIThread.Post(() => VolumesArrived?.Invoke(this, letters));
            } else {
                Dispatcher.UIThread.Post(() => VolumesRemoved?.Invoke(this, letters));
            }
        } catch {
            // Never let unexpected Win32 message payloads destabilize Avalonia's message pump.
        }

        return IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable InconsistentNaming
    struct DevBroadcastVolume {
        public int dbcv_size;
        public int dbcv_devicetype;
        public int dbcv_reserved;
        public uint dbcv_unitmask;
        public ushort dbcv_flags;
    }
    // ReSharper restore InconsistentNaming
}