using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Controls.CommonDialog;
using ExtraIsland.Shared;
using MaterialDesignThemes.Wpf;

namespace ExtraIsland.AuthorizeProvider;

[AuthorizeProviderInfo("extraIsland.authProviders.usbDrive","U盘",PackIconKind.Usb)]
public partial class UsbDriveAuthorizer {
    public static readonly DependencyProperty OperationFinishedProperty = DependencyProperty.Register(
        nameof(OperationFinished),typeof(bool),typeof(UsbDriveAuthorizer),new PropertyMetadata(default(bool)));

    public bool OperationFinished {
        get => (bool)GetValue(OperationFinishedProperty);
        set => SetValue(OperationFinishedProperty,value);
    }

    public static readonly DependencyProperty OperatingProperty = DependencyProperty.Register(
        nameof(Operating),typeof(bool),typeof(UsbDriveAuthorizer),new PropertyMetadata(default(bool)));

    public bool Operating {
        get => (bool)GetValue(OperatingProperty);
        set => SetValue(OperatingProperty,value);
    }

    readonly WindowsUtils.UsbDriveMonitor _monitor = new WindowsUtils.UsbDriveMonitor();
    public UsbDriveAuthorizer() {
        InitializeComponent();
        _monitor.UsbDriveInserted += OnUsbInserted;
    }

    bool EditLock { get; set; } = true;
    void OnUsbInserted(object? sender,string driveLetter) {
        if (IsEditingMode & EditLock) return;
        EditLock = true;
        OperationFinished = false;
        ToggleButton setter = (ModeSettingsControl.Switcher as ToggleButton)!;
        bool isFileMode = setter.IsChecked!.Value;
        // TODO:多分区支持
        string path = $"{driveLetter}\\.verify.extraisland.nfo";
        WindowsUtils.UsbDriveInfo info = WindowsUtils.FindUsbDriveByLetter(driveLetter);
        if (info.SerialNumber == null & !isFileMode) {
            if (IsEditingMode) {
                CommonDialog.ShowError("此设备S/N异常,不能使用该设备!");
                EditLock = false;
            } else {
                OperationFinished = true;
            }
            return;
        }
        if (IsEditingMode) {
            if (isFileMode) {
                if (!File.Exists(path)) {
                    File.WriteAllText(path, Guid.NewGuid().ToString());
                }
                string? hash = GetCredentialHash(path);
                if (hash != null) {
                    Settings.PassHash = hash;
                } else {
                    CommonDialog.ShowError("读取密钥文件时遇到错误");
                    return;
                }
            } else {
                Settings.PassHash = EiUtils.Sha256EncryptString(info.SerialNumber!);
            }
            Operating = false;
            OperationFinished = true;
            Settings.IsFileModeEnabled = isFileMode;   
        } else if (Settings.IsFileModeEnabled) {
            if (Settings.PassHash == GetCredentialHash(path)) {
                CompleteAuthorize();
            } else {
                OperationFinished = true;   
            }
        }
        else {
            if (Settings.PassHash == EiUtils.Sha256EncryptString(info.SerialNumber!)) {
                CompleteAuthorize();
            } else {
                OperationFinished = true;   
            }
        }
    }

    static string? GetCredentialHash(string path) {
        return !File.Exists(path) ? null : EiUtils.Sha256EncryptString(File.ReadAllText(path));
    }

    void UsbDriveAuthorizer_OnUnloaded(object sender,RoutedEventArgs e) {
        _monitor.UsbDriveInserted -= OnUsbInserted;
    }
    void SetCredential(object sender,RoutedEventArgs e) {
        Operating = true;
        EditLock = false;
    }
    void VerifyButton_OnClick(object sender,RoutedEventArgs e) {
        if (Settings.IsFileModeEnabled) {
            foreach (WindowsUtils.UsbDriveInfo info in WindowsUtils.ScanUsbDrive()) {
                if (info.DriveLetter == null) continue;
                if (info.DriveLetter.All(letter => Settings.PassHash != GetCredentialHash($"{letter}\\.verify.extraisland.nfo"))) continue;
                CompleteAuthorize();
                return;
            }
        } else if (WindowsUtils.ScanUsbDrive().Any(info => Settings.PassHash == EiUtils.Sha256EncryptString(info.SerialNumber!))) {
            CompleteAuthorize();
            return;
        }
        OperationFinished = true;
    }
    void UsbDriveAuthorizer_OnLoaded(object sender,RoutedEventArgs e) {

    }
}