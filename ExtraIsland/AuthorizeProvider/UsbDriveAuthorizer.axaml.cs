using Avalonia.Controls;
using System.IO;
using System.Windows;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;

namespace ExtraIsland.AuthorizeProvider;

[AuthorizeProviderInfo("extraIsland.authProviders.usbDrive","U盘","\uF3A2")]
public partial class UsbDriveAuthorizer : AuthorizeProviderControlBase<UsbDriveAuthorizerSettings> {

    readonly WindowsUtils.UsbDriveMonitor _monitor = new WindowsUtils.UsbDriveMonitor();
    public UsbDriveAuthorizer() {
        InitializeComponent();
        _monitor.UsbDriveInserted += OnUsbInserted;
    }

    bool EditLock { get; set; } = true;
    void OnUsbInserted(object? sender,string driveLetter) {
        Dispatcher.UIThread.Invoke(() => {
            if (IsEditingMode & EditLock) return;
            EditLock = true;
            Settings.OperationFinished = false;
            ToggleSwitch setter = (ModeSettingsControl.Footer as ToggleSwitch)!;
            bool isFileMode = setter.IsChecked!.Value;
            // TODO:多分区支持
            string path = $"{driveLetter}\\.verify.extraisland.nfo";
            WindowsUtils.UsbDriveInfo info = WindowsUtils.FindUsbDriveByLetter(driveLetter);
            if (info.SerialNumber == null & !isFileMode) {
                if (IsEditingMode) {
                    MessageBox.Show("此设备S/N异常,不能使用该设备!");
                    EditLock = false;
                } else {
                    Settings.OperationFinished = true;
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
                        MessageBox.Show("读取密钥文件时遇到错误");
                        return;
                    }
                } else {
                    Settings.PassHash = EiUtils.Sha256EncryptString(info.SerialNumber!);
                }
                Settings.Operating = false;
                Settings.OperationFinished = true;
                Settings.IsFileModeEnabled = isFileMode; 
            } else if (Settings.IsFileModeEnabled) {
                if (Settings.PassHash == GetCredentialHash(path)) {
                    CompleteAuthorize();
                } else {
                    Settings.OperationFinished = true;   
                }
            }
            else {
                if (Settings.PassHash == EiUtils.Sha256EncryptString(info.SerialNumber!)) {
                    CompleteAuthorize();
                } else {
                    Settings.OperationFinished = true;   
                }
            }
        });
    }

    static string? GetCredentialHash(string path) {
        return !File.Exists(path) ? null : EiUtils.Sha256EncryptString(File.ReadAllText(path));
    }

    void UsbDriveAuthorizer_OnUnloaded(object? sender,RoutedEventArgs routedEventArgs) {
        _monitor.UsbDriveInserted -= OnUsbInserted;
    }
    void SetCredential(object? sender,RoutedEventArgs routedEventArgs) {
        Settings.Operating = true;
        EditLock = false;
    }
    void VerifyButton_OnClick(object? sender,RoutedEventArgs routedEventArgs) {
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
        Settings.OperationFinished = true;
    }
    void UsbDriveAuthorizer_OnLoaded(object? sender,RoutedEventArgs routedEventArgs) {

    }
    
    public override bool ValidateAuthorizeSettings() {
        return true; //todo:implement this
    }
}