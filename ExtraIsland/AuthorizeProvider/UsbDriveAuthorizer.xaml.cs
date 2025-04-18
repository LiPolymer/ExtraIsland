using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ClassIsland.Core;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;
using MahApps.Metro.Controls;
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
        WindowsUtils.UsbDriveInfo info = WindowsUtils.FindUsbDriveByLetter(driveLetter);
        if (IsEditingMode) {
            Settings.PassHash = EiUtils.Sha256EncryptString(info.SerialNumber!);
            Operating = false;
            OperationFinished = true;
        } else if(Settings.PassHash == EiUtils.Sha256EncryptString(info.SerialNumber!)) {
            CompleteAuthorize();
        } else {
            OperationFinished = true;
        }
    }
    
    void UsbDriveAuthorizer_OnUnloaded(object sender,RoutedEventArgs e) {
        _monitor.UsbDriveInserted -= OnUsbInserted;
    }
    void ButtonBase_OnClick(object sender,RoutedEventArgs e) {
        Operating = true;
        EditLock = false;
    }
    void VerifyButton_OnClick(object sender,RoutedEventArgs e) {
        if (WindowsUtils.ScanUsbDrive().Any(info => Settings.PassHash == EiUtils.Sha256EncryptString(info.SerialNumber!))) {
            CompleteAuthorize();
            return;
        }
        OperationFinished = true;
    }
    void UsbDriveAuthorizer_OnLoaded(object sender,RoutedEventArgs e) {
        
    }
}