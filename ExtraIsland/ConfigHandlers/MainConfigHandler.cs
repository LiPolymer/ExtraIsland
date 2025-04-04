using System.ComponentModel;
using System.Drawing;
using System.IO;
using ClassIsland.Shared.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;

namespace ExtraIsland.ConfigHandlers;

public class MainConfigHandler {
    readonly string _configPath;
    public MainConfigData Data { get; set; }

    public MainConfigHandler() {
        _configPath = Path.Combine(GlobalConstants.PluginConfigFolder!,"Main.json");
        Data = new MainConfigData();

        InitializeConfig();
        SubscribeToChanges();
    }

    void InitializeConfig() {
        if (string.IsNullOrEmpty(GlobalConstants.PluginConfigFolder)) {
            throw new InvalidOperationException("配置文件夹路径未设置");
        }

        if (!File.Exists(_configPath)) {
            Save();
            return;
        }

        try {
            Data = ConfigureFileHelper.LoadConfig<MainConfigData>(_configPath);
        }
        catch (Exception ex) {
            Console.WriteLine($"[ExIsLand][Tracer][MainCfgHandler] 加载配置文件失败: {ex.Message}");
            File.Delete(_configPath);
            Save();
        }
    }

    void SubscribeToChanges() {
        Data.PropertyChanged += OnPropertyChanged;
        Data.TinyFeatures.JuniorGuide.PropertyChanged += OnPropertyChanged;
        Data.Dock.PropertyChanged += OnPropertyChanged;
    }

    void OnPropertyChanged(object? sender,PropertyChangedEventArgs e) {
        Save();
    }

    void Save() {
        try {
            ConfigureFileHelper.SaveConfig(_configPath,Data);
        }
        catch (Exception ex) {
            Console.WriteLine($"[ExIsLand][Tracer][MainCfgHandler] 写入配置文件失败: {ex.Message}");
            throw;
        }
    }
}

public class MainConfigData : ObservableObject {
    public event Action? RestartPropertyChanged;
    
    bool _isLifeModeActivated;
    public bool IsLifeModeActivated {
        get => _isLifeModeActivated;
        set {
            if (value == _isLifeModeActivated) return;
            _isLifeModeActivated = value;
            OnPropertyChanged();
            RestartPropertyChanged?.Invoke();
        }
    }
    
    bool _isExperimentalModeActivated;
    public bool IsExperimentalModeActivated {
        get => _isExperimentalModeActivated;
        set {
            if (value == _isExperimentalModeActivated) return;
            _isExperimentalModeActivated = value;
            OnPropertyChanged();
            //RestartPropertyChanged?.Invoke();
        }
    }
    
    bool _isTelemetryActivated = true;
    public bool IsTelemetryActivated {
        get => _isTelemetryActivated;
        set {
            if (value == _isTelemetryActivated) return;
            _isTelemetryActivated = value;
            OnPropertyChanged();
            RestartPropertyChanged?.Invoke();
        }
    }
    
    public DockConfig Dock { get; set; } = new DockConfig(); 
    public class DockConfig : ObservableObject {
        bool _enabled;
        public bool Enabled {
            get => _enabled;
            set {
                if (value == _enabled) return;
                _enabled = value;
                OnPropertyChanged();
            }
        }
        
        Color _color = Color.FromArgb(0x00,0x00,0xff,0xff);
        public Color Color {
            get => _color;
            set {
                if (value.Equals(_color)) return;
                _color = value;
                OnPropertyChanged();
            }
        }

        MainWindowHandler.AccentHelper.AccentState _accentState = MainWindowHandler.AccentHelper.AccentState.AccentEnableBlurbehind;
        public MainWindowHandler.AccentHelper.AccentState AccentState {
            get => _accentState;
            set {
                if (value == _accentState) return;
                _accentState = value;
                OnPropertyChanged();
                GlobalConstants.Handlers.MainWindow?.UpdateAccent(value);
            }
        }
    }
    
    public TinyFeaturesConfig TinyFeatures { get; set; } = new TinyFeaturesConfig();

    public class TinyFeaturesConfig : ObservableObject {
        public JuniorGuideConfig JuniorGuide { get; set; } = new JuniorGuideConfig();

        public class JuniorGuideConfig : ObservableObject {
            bool _enabled;
            public bool Enabled {
                get => _enabled;
                set {
                    if (value == _enabled) return;
                    _enabled = value;
                    OnPropertyChanged();
                }
            }

            string _header = "欢迎 · 电子白板使用导引";
            public string Header {
                get => _header;
                set {
                    if (value == _header) return;
                    _header = value;
                    OnPropertyChanged();
                }
            }

            int _keepTime = 60000;
            public int KeepTime {
                get => _keepTime;
                set {
                    if (value == _keepTime) return;
                    _keepTime = value;
                    OnPropertyChanged();
                }
            }

            string _content = "# 一体机交接指南\r\n"
                              + "这是一个示列\r\n"
                              + "*支持Markdown*\r\n"
                              + "> 请按需修改\r\n"
                              + "> awa!";
            public string Content {
                get => _content;
                set {
                    if (value == _content) return;
                    _content = value;
                    OnPropertyChanged();
                }
            }

            string _link = "https://docs.classisland.tech/";
            public string Link {
                get => _link;
                set {
                    if (value == _link) return;
                    _link = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}