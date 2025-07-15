using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public class RhesisConfig : ObservableObject {
    RhesisDataSource _dataSource = RhesisDataSource.SaintJinrishici;
    public RhesisDataSource DataSource { 
        get => _dataSource;
        set {
            if (_dataSource == value) return;
            _dataSource = value;
            OnPropertyChanged();
        } 
    }

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

    int _attributesShowingInterval = 3;
    public int AttributesShowingInterval {
        get => _attributesShowingInterval;
        set {
            if(_attributesShowingInterval == value) return;
            _attributesShowingInterval = value;
            OnPropertyChanged();
        }
    }

    AttributesDisplayRule _attributesRule = AttributesDisplayRule.Sametime;
    public AttributesDisplayRule AttributesRule {
        get => _attributesRule;
        set {
            if (value == _attributesRule) return;
            _attributesRule = value;
            OnPropertyChanged();
        }
    }

    public enum AttributesDisplayRule {
        [Description("同时展示")]
        Sametime,
        [Description("分开展示")]
        Separate
    }
}