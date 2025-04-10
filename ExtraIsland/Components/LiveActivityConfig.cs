using System.Text.Json.Serialization;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public class LiveActivityConfig {
    public event Action? OnMarginChanged;
    bool _isLeftNegativeMargin;
    public bool IsLeftNegativeMargin {
        get => _isLeftNegativeMargin; 
        set { 
            if (_isLeftNegativeMargin == value) return;
            _isLeftNegativeMargin = value;
            OnMarginChanged?.Invoke();
        }
    }
    bool _isRightNegativeMargin;
    public bool IsRightNegativeMargin {
        get => _isRightNegativeMargin; 
        set {
            if (_isRightNegativeMargin == value) return;
            _isRightNegativeMargin = value;
            OnMarginChanged?.Invoke();
        }
    }

    public string IgnoreListString { get; set; } = "Program Manager";

    [JsonIgnore]
    public string[] IgnoreList {
        get {
            try {
                return IgnoreListString.Split("\r\n");
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return [];
            }
        }
    }

    public bool IsAnimationEnabled { get; set; } = true;
    
    public event Action? OnLyricsChanged;
    bool _isLyricsEnabled;
    public bool IsLyricsEnabled {
        get => _isLyricsEnabled;
        set {
            if (_isLyricsEnabled == value) return;
            _isLyricsEnabled = value;
            OnLyricsChanged?.Invoke();
        }
    }
    
    public bool IsSleepyUploaderEnabled { get; set; }
    public string SleepyUrl { get; set; } = string.Empty;
    public string SleepySecret { get; set; } = string.Empty;
    public string SleepyPattern { get; set; } = "{0}";
    public string SleepyDeviceId { get; set; } = "0";
    public string SleepyDevice { get; set; } = "ExtraIsland Sleepy Interface";
}