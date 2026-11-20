using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.AuthorizeProvider;

public class UsbDriveAuthorizerSettings : ObservableObject {
    string _passHash = string.Empty;
    string _salt = string.Empty;
    bool _isFileModeEnabled;

    public string PassHash {
        get => _passHash;
        set => SetProperty(ref _passHash, value);
    }

    public string Salt {
        get => _salt;
        set => SetProperty(ref _salt, value);
    }

    public bool IsFileModeEnabled {
        get => _isFileModeEnabled;
        set => SetProperty(ref _isFileModeEnabled, value);
    }
}