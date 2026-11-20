using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;
using FluentAvalonia.UI.Controls;

namespace ExtraIsland.AuthorizeProvider;

public class UsbDriveAuthorizerViewModel : ObservableObject {
    bool _isEditingMode;
    bool _selectedFileMode;
    bool _boundFileMode;
    bool _hasBoundCredential;
    bool _isWaitingForInsert;
    bool _isBusy;
    string _busyMessage = string.Empty;
    bool _statusIsOpen;
    string _statusTitle = string.Empty;
    string _statusMessage = string.Empty;
    FAInfoBarSeverity _statusSeverity = FAInfoBarSeverity.Informational;
    UsbDriveInfo? _selectedDrive;
    bool _showDrivePicker;

    public bool IsEditingMode {
        get => _isEditingMode;
        set => SetProperty(ref _isEditingMode, value);
    }

    public bool SelectedFileMode {
        get => _selectedFileMode;
        set {
            if (SetProperty(ref _selectedFileMode, value)) {
                OnPropertyChanged(nameof(IsModeMismatch));
            }
        }
    }

    public bool BoundFileMode {
        get => _boundFileMode;
        set {
            if (SetProperty(ref _boundFileMode, value)) {
                OnPropertyChanged(nameof(IsModeMismatch));
                OnPropertyChanged(nameof(VerifyModeHint));
                OnPropertyChanged(nameof(BoundStatusText));
            }
        }
    }

    public bool HasBoundCredential {
        get => _hasBoundCredential;
        set {
            if (SetProperty(ref _hasBoundCredential, value)) {
                OnPropertyChanged(nameof(IsModeMismatch));
                OnPropertyChanged(nameof(WaitInsertButtonText));
                OnPropertyChanged(nameof(BoundStatusText));
            }
        }
    }

    public bool IsWaitingForInsert {
        get => _isWaitingForInsert;
        set {
            if (SetProperty(ref _isWaitingForInsert, value)) {
                OnPropertyChanged(nameof(WaitInsertButtonText));
                OnPropertyChanged(nameof(ShowWaitingBanner));
            }
        }
    }

    public bool IsBusy {
        get => _isBusy;
        set {
            if (SetProperty(ref _isBusy, value)) {
                OnPropertyChanged(nameof(CanBindSelectedDrive));
                OnPropertyChanged(nameof(ShowWaitingBanner));
            }
        }
    }

    public string BusyMessage {
        get => _busyMessage;
        set => SetProperty(ref _busyMessage, value);
    }

    public bool StatusIsOpen {
        get => _statusIsOpen;
        set => SetProperty(ref _statusIsOpen, value);
    }

    public string StatusTitle {
        get => _statusTitle;
        set => SetProperty(ref _statusTitle, value);
    }

    public string StatusMessage {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public FAInfoBarSeverity StatusSeverity {
        get => _statusSeverity;
        set => SetProperty(ref _statusSeverity, value);
    }

    public ObservableCollection<UsbDriveInfo> AvailableDrives { get; } = [];

    public UsbDriveInfo? SelectedDrive {
        get => _selectedDrive;
        set {
            if (SetProperty(ref _selectedDrive, value)) {
                OnPropertyChanged(nameof(CanBindSelectedDrive));
            }
        }
    }

    public bool ShowDrivePicker {
        get => _showDrivePicker;
        set => SetProperty(ref _showDrivePicker, value);
    }

    public bool IsModeMismatch => HasBoundCredential && SelectedFileMode != BoundFileMode;

    public string WaitInsertButtonText =>
        IsWaitingForInsert
            ? "取消等待插入"
            : HasBoundCredential
                ? "重新绑定（等待插入）"
                : "等待插入U盘";

    public string VerifyModeHint =>
        BoundFileMode ? "当前验证模式：密钥文件模式" : "当前验证模式：硬件序列号模式";

    public string BoundStatusText =>
        HasBoundCredential
            ? $"已绑定验证U盘（{(BoundFileMode ? "密钥文件模式" : "硬件序列号模式")}）"
            : "尚未绑定验证U盘";

    public bool CanBindSelectedDrive => !IsBusy && SelectedDrive is not null;

    public bool ShowWaitingBanner => IsWaitingForInsert && !IsBusy;

    public void ShowStatus(FAInfoBarSeverity severity, string title, string message) {
        StatusSeverity = severity;
        StatusTitle = title;
        StatusMessage = message;
        StatusIsOpen = true;
    }

    public void ClearStatus() {
        StatusIsOpen = false;
        StatusTitle = string.Empty;
        StatusMessage = string.Empty;
    }

    public void SyncFromSettings(bool isEditingMode, UsbDriveAuthorizerSettings? settings) {
        IsEditingMode = isEditingMode;
        bool hasBound = settings is not null && !string.IsNullOrWhiteSpace(settings.PassHash);
        bool isFileMode = settings?.IsFileModeEnabled ?? false;

        HasBoundCredential = hasBound;
        BoundFileMode = isFileMode;
        SelectedFileMode = isFileMode;
        IsWaitingForInsert = false;
        IsBusy = false;
        BusyMessage = string.Empty;
        AvailableDrives.Clear();
        SelectedDrive = null;
        ShowDrivePicker = false;
        ClearStatus();
    }
}