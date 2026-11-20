using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using FluentAvalonia.UI.Controls;

namespace ExtraIsland.AuthorizeProvider;

[AuthorizeProviderInfo("extraIsland.authProviders.usbDrive", "U盘", "\uF3A2")]
public partial class UsbDriveAuthorizer : AuthorizeProviderControlBase<UsbDriveAuthorizerSettings> {
    const int ArrivalDebounceMs = 150;

    readonly UsbVolumeWatcher _watcher = new();
    readonly HashSet<string> _pendingArrivedLetters = new(StringComparer.OrdinalIgnoreCase);
    readonly SemaphoreSlim _operationLock = new(1, 1);

    CancellationTokenSource? _lifecycleCts;
    bool _isProcessingArrivals;
    bool _authorized;

    public UsbDriveAuthorizerViewModel ViewModel { get; } = new();

    public UsbDriveAuthorizer() {
        InitializeComponent();
        _watcher.VolumesArrived += OnVolumesArrived;
        _watcher.VolumesRemoved += OnVolumesRemoved;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);

        _lifecycleCts?.Cancel();
        _lifecycleCts?.Dispose();
        _lifecycleCts = new CancellationTokenSource();

        _authorized = false;
        _pendingArrivedLetters.Clear();
        _isProcessingArrivals = false;

        ViewModel.SyncFromSettings(IsEditingMode, Settings);
        if (!IsEditingMode && !ViewModel.HasBoundCredential) {
            ViewModel.ShowStatus(
                FAInfoBarSeverity.Error,
                "未配置验证U盘",
                "此U盘验证方式尚未绑定设备，请使用其他方式验证后重新配置。");
        }

        _watcher.Attach(TopLevel.GetTopLevel(this));
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        _watcher.Detach();
        _pendingArrivedLetters.Clear();
        _isProcessingArrivals = false;

        _lifecycleCts?.Cancel();
        _lifecycleCts?.Dispose();
        _lifecycleCts = null;

        base.OnDetachedFromVisualTree(e);
    }

    void OnVolumesArrived(object? sender, IReadOnlyList<string> letters) {
        if (_authorized || _lifecycleCts is null || _lifecycleCts.IsCancellationRequested) {
            return;
        }

        if (IsEditingMode && !ViewModel.IsWaitingForInsert) {
            return;
        }

        foreach (string letter in letters) {
            _pendingArrivedLetters.Add(letter);
        }

        if (!_isProcessingArrivals) {
            _isProcessingArrivals = true;
            _ = ProcessPendingArrivalsAsync(_lifecycleCts.Token);
        }
    }

    void OnVolumesRemoved(object? sender, IReadOnlyList<string> removedLetters) {
        if (!IsEditingMode || !ViewModel.ShowDrivePicker) {
            return;
        }

        HashSet<string> removedSet = new(removedLetters, StringComparer.OrdinalIgnoreCase);
        for (int i = ViewModel.AvailableDrives.Count - 1; i >= 0; i--) {
            UsbDriveInfo drive = ViewModel.AvailableDrives[i];
            if (!drive.DriveLetters.Any(removedSet.Contains)) {
                continue;
            }

            bool wasSelected = ReferenceEquals(ViewModel.SelectedDrive, drive);
            ViewModel.AvailableDrives.RemoveAt(i);
            if (wasSelected) {
                ViewModel.SelectedDrive = null;
            }
        }

        if (ViewModel.AvailableDrives.Count == 0) {
            ViewModel.ShowDrivePicker = false;
        } else {
            ViewModel.SelectedDrive ??= ViewModel.AvailableDrives[0];
        }
    }

    async Task ProcessPendingArrivalsAsync(CancellationToken ct) {
        try {
            while (_pendingArrivedLetters.Count > 0 && !_authorized && !ct.IsCancellationRequested) {
                await Task.Delay(ArrivalDebounceMs, ct);
                if (_authorized || ct.IsCancellationRequested) {
                    break;
                }

                string[] batch = _pendingArrivedLetters.ToArray();
                _pendingArrivedLetters.Clear();

                await _operationLock.WaitAsync(ct);
                try {
                    if (!_authorized &&
                        !ct.IsCancellationRequested &&
                        (!IsEditingMode || ViewModel.IsWaitingForInsert)) {
                        await HandleArrivedVolumesAsync(batch, ct);
                    }
                } finally {
                    _operationLock.Release();
                }
            }
        } catch (OperationCanceledException) {
            // Detached or cancelled cleanly.
        } catch (Exception ex) {
            if (!ct.IsCancellationRequested) {
                ViewModel.ShowStatus(FAInfoBarSeverity.Error, "处理U盘事件时出错", ex.Message);
            }
        } finally {
            if (!ct.IsCancellationRequested) {
                _isProcessingArrivals = false;
            }
        }
    }

    async Task HandleArrivedVolumesAsync(IReadOnlyList<string> letters, CancellationToken ct) {
        ViewModel.IsBusy = true;
        ViewModel.BusyMessage = "正在识别插入的U盘…";
        try {
            UsbDriveInfo? drive = await UsbDriveScanner.FindUsbDriveByLettersAsync(letters, ct);
            if (ct.IsCancellationRequested || drive is null) {
                return;
            }

            if (IsEditingMode) {
                await BindDriveCoreAsync(drive, ct);
                return;
            }

            if (Settings is null || string.IsNullOrWhiteSpace(Settings.PassHash)) {
                ViewModel.ShowStatus(
                    FAInfoBarSeverity.Error,
                    "未配置验证U盘",
                    "此U盘验证方式尚未绑定设备，请使用其他方式验证后重新配置。");
                return;
            }

            ViewModel.BusyMessage = $"正在验证 {drive.DisplayName}…";
            bool matched = await Task.Run(() => UsbCredentialHelper.VerifyDrive(drive, Settings), ct);
            if (ct.IsCancellationRequested) {
                return;
            }

            if (matched) {
                _authorized = true;
                ViewModel.ClearStatus();
                CompleteAuthorize();
            } else {
                ViewModel.ShowStatus(
                    FAInfoBarSeverity.Error,
                    "验证失败",
                    $"已检测到 {drive.DisplayName}，但与已绑定的验证U盘不匹配，请重试。");
            }
        } finally {
            if (!ct.IsCancellationRequested) {
                ViewModel.IsBusy = false;
                ViewModel.BusyMessage = string.Empty;
            }
        }
    }

    async Task BindDriveCoreAsync(UsbDriveInfo drive, CancellationToken ct) {
        if (Settings is null) {
            return;
        }

        ViewModel.BusyMessage = $"正在绑定 {drive.DisplayName}…";
        bool useFileMode = ViewModel.SelectedFileMode;

        UsbCredentialHelper.PreparedCredential prepared = await Task.Run(
            () => UsbCredentialHelper.PrepareBind(drive, useFileMode),
            ct);
        if (ct.IsCancellationRequested) {
            return;
        }

        if (!prepared.Success) {
            ViewModel.ShowStatus(
                FAInfoBarSeverity.Error,
                "绑定失败",
                prepared.ErrorMessage ?? "无法绑定该U盘。");
            return;
        }

        Settings.Salt = prepared.Salt;
        Settings.PassHash = prepared.PassHash;
        Settings.IsFileModeEnabled = prepared.IsFileModeEnabled;

        ViewModel.HasBoundCredential = true;
        ViewModel.BoundFileMode = Settings.IsFileModeEnabled;
        ViewModel.IsWaitingForInsert = false;
        ViewModel.ShowDrivePicker = false;
        ViewModel.AvailableDrives.Clear();
        ViewModel.SelectedDrive = null;

        string modeLabel = Settings.IsFileModeEnabled ? "密钥文件模式" : "硬件序列号模式";
        ViewModel.ShowStatus(
            FAInfoBarSeverity.Success,
            "绑定成功",
            $"已绑定 {drive.DisplayName}（{modeLabel}）。");
    }

    void WaitInsertButton_OnClick(object? sender, RoutedEventArgs e) {
        if (ViewModel.IsBusy) {
            return;
        }

        ViewModel.IsWaitingForInsert = !ViewModel.IsWaitingForInsert;
        ViewModel.ClearStatus();
        if (ViewModel.IsWaitingForInsert) {
            ViewModel.ShowDrivePicker = false;
            ViewModel.AvailableDrives.Clear();
            ViewModel.SelectedDrive = null;
        }
    }

    async void ScanExistingDrivesButton_OnClick(object? sender, RoutedEventArgs e) {
        if (_lifecycleCts is null || _lifecycleCts.IsCancellationRequested || ViewModel.IsBusy) {
            return;
        }

        CancellationToken ct = _lifecycleCts.Token;
        bool acquired = false;
        try {
            await _operationLock.WaitAsync(ct);
            acquired = true;
            if (ct.IsCancellationRequested) {
                return;
            }

            ViewModel.IsWaitingForInsert = false;
            ViewModel.ClearStatus();
            ViewModel.IsBusy = true;
            ViewModel.BusyMessage = "正在扫描已连接的U盘…";

            IReadOnlyList<UsbDriveInfo> drives = await UsbDriveScanner.ScanUsbDrivesAsync(ct);
            if (ct.IsCancellationRequested) {
                return;
            }

            if (drives.Count == 0) {
                ViewModel.ShowDrivePicker = false;
                ViewModel.AvailableDrives.Clear();
                ViewModel.SelectedDrive = null;
                ViewModel.ShowStatus(
                    FAInfoBarSeverity.Warning,
                    "未检测到U盘",
                    "未检测到已插入的U盘，请插入后重试。");
            } else if (drives.Count == 1) {
                await BindDriveCoreAsync(drives[0], ct);
            } else {
                ViewModel.AvailableDrives.Clear();
                foreach (UsbDriveInfo drive in drives) {
                    ViewModel.AvailableDrives.Add(drive);
                }
                ViewModel.SelectedDrive = drives[0];
                ViewModel.ShowDrivePicker = true;
                ViewModel.ShowStatus(
                    FAInfoBarSeverity.Informational,
                    "请选择U盘",
                    "检测到多个U盘，请在下方选择要绑定的设备。");
            }
        } catch (OperationCanceledException) {
            // Detached or cancelled cleanly.
        } catch (Exception ex) {
            if (!ct.IsCancellationRequested) {
                ViewModel.ShowStatus(FAInfoBarSeverity.Error, "扫描失败", ex.Message);
            }
        } finally {
            if (acquired) {
                _operationLock.Release();
            }
            if (!ct.IsCancellationRequested) {
                ViewModel.IsBusy = false;
                ViewModel.BusyMessage = string.Empty;
            }
        }
    }

    async void BindSelectedDriveButton_OnClick(object? sender, RoutedEventArgs e) {
        if (_lifecycleCts is null || _lifecycleCts.IsCancellationRequested || ViewModel.IsBusy) {
            return;
        }

        UsbDriveInfo? selectedDrive = ViewModel.SelectedDrive;
        if (selectedDrive is null) {
            return;
        }

        CancellationToken ct = _lifecycleCts.Token;
        bool acquired = false;
        try {
            await _operationLock.WaitAsync(ct);
            acquired = true;
            if (ct.IsCancellationRequested) {
                return;
            }

            ViewModel.IsBusy = true;
            ViewModel.ClearStatus();
            await BindDriveCoreAsync(selectedDrive, ct);
        } catch (OperationCanceledException) {
            // Detached or cancelled cleanly.
        } catch (Exception ex) {
            if (!ct.IsCancellationRequested) {
                ViewModel.ShowStatus(FAInfoBarSeverity.Error, "绑定失败", ex.Message);
            }
        } finally {
            if (acquired) {
                _operationLock.Release();
            }
            if (!ct.IsCancellationRequested) {
                ViewModel.IsBusy = false;
                ViewModel.BusyMessage = string.Empty;
            }
        }
    }

    async void VerifyButton_OnClick(object? sender, RoutedEventArgs e) {
        if (_authorized || _lifecycleCts is null || _lifecycleCts.IsCancellationRequested || ViewModel.IsBusy) {
            return;
        }

        if (Settings is null || string.IsNullOrWhiteSpace(Settings.PassHash)) {
            ViewModel.ShowStatus(
                FAInfoBarSeverity.Error,
                "未配置验证U盘",
                "此U盘验证方式尚未绑定设备，请使用其他方式验证后重新配置。");
            return;
        }

        CancellationToken ct = _lifecycleCts.Token;
        bool acquired = false;
        try {
            await _operationLock.WaitAsync(ct);
            acquired = true;
            if (_authorized || ct.IsCancellationRequested) {
                return;
            }

            ViewModel.IsBusy = true;
            ViewModel.BusyMessage = "正在扫描已连接的U盘…";
            ViewModel.ClearStatus();

            IReadOnlyList<UsbDriveInfo> drives = await UsbDriveScanner.ScanUsbDrivesAsync(ct);
            if (ct.IsCancellationRequested) {
                return;
            }

            if (drives.Count == 0) {
                ViewModel.ShowStatus(
                    FAInfoBarSeverity.Warning,
                    "未检测到U盘",
                    "未检测到已插入的U盘，请确认U盘已正确连接。");
                return;
            }

            ViewModel.BusyMessage = "正在验证U盘凭据…";
            bool matched = await Task.Run(() => drives.Any(d => UsbCredentialHelper.VerifyDrive(d, Settings)), ct);
            if (ct.IsCancellationRequested) {
                return;
            }

            if (matched) {
                _authorized = true;
                ViewModel.ClearStatus();
                CompleteAuthorize();
            } else {
                ViewModel.ShowStatus(
                    FAInfoBarSeverity.Error,
                    "验证失败",
                    "验证失败：当前已插入的U盘均不匹配。");
            }
        } catch (OperationCanceledException) {
            // Detached or cancelled cleanly.
        } catch (Exception ex) {
            if (!ct.IsCancellationRequested) {
                ViewModel.ShowStatus(FAInfoBarSeverity.Error, "扫描失败", ex.Message);
            }
        } finally {
            if (acquired) {
                _operationLock.Release();
            }
            if (!ct.IsCancellationRequested) {
                ViewModel.IsBusy = false;
                ViewModel.BusyMessage = string.Empty;
            }
        }
    }

    public override bool ValidateAuthorizeSettings() {
        if (Settings is null || string.IsNullOrWhiteSpace(Settings.PassHash)) {
            ViewModel.ShowStatus(
                FAInfoBarSeverity.Error,
                "尚未绑定U盘",
                "请先插入并绑定一个验证U盘后再保存。");
            return false;
        }

        if (ViewModel.SelectedFileMode != Settings.IsFileModeEnabled) {
            ViewModel.ShowStatus(
                FAInfoBarSeverity.Warning,
                "验证模式未同步",
                "您已切换验证模式，请重新绑定U盘后再保存，或切回原模式。");
            return false;
        }

        return true;
    }
}