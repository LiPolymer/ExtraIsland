using System.Text.Json.Serialization;
using ClassIsland.Core.Controls;
using ClassIsland.Core.Controls.CommonDialog;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;
using MaterialDesignThemes.Wpf;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public class RhesisConfig : ObservableObject {
    public RhesisDataSource DataSource { get; set; } = RhesisDataSource.SaintJinrishici;

    public string IgnoreListString { get; set; } = string.Empty;
    
    public string HitokotoProp  { get; set; } = string.Empty;
    
    public DateTime LastUpdate { get; set; } = DateTime.Today;

    public int LengthLimitation { get; set; }
    
    [JsonIgnore]
    public string HitokotoLengthArgs {
        get {
            return LengthLimitation switch {
                0 => string.Empty,
                _ => $"max_length={LengthLimitation}&"
            };
        }
    }

    public TimeSpan UpdateTimeGap { get; set; } = TimeSpan.FromSeconds(30);

    [JsonIgnore]
    public double UpdateTimeGapSeconds {
        get => UpdateTimeGap.TotalSeconds;
        set => UpdateTimeGap = TimeSpan.FromSeconds(value);
    }
    
    public string SainticProp  { get; set; } = string.Empty;
    
    public bool IsAnimationEnabled { get; set; } = true;
    
    public bool IsSwapAnimationEnabled { get; set; }

    public bool IsAuthorShowEnabled { get; set; }
    public bool IsTitleShowEnabled { get; set; }

    int _attributesShowingInterval;
    public int AttributesShowingInterval {
        get => _attributesShowingInterval;
        set {
            if(_attributesShowingInterval == value) return;
            _attributesShowingInterval = value;
            OnPropertyChanged();
        }
    }
}