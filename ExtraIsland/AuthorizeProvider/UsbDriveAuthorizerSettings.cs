using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace ExtraIsland.AuthorizeProvider;

public class UsbDriveAuthorizerSettings: ObservableObject {
    bool _operating;

    [JsonIgnore]
    public bool Operating {
        get => _operating;
        set {
            _operating = value;
            OnPropertyChanged();
        }
    }
    
    bool _operationFinished;

    [JsonIgnore]
    public bool OperationFinished {
        get => _operationFinished;
        set {
            _operationFinished = value;
            OnPropertyChanged();
        }
    }

    public string PassHash { get; set; } = string.Empty;
    public bool IsFileModeEnabled { get; set; }
}