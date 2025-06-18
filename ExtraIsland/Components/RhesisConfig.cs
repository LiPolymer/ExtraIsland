using System.Text.Json.Serialization;
using ClassIsland.Core.Controls;
using ClassIsland.Core.Controls.CommonDialog;
using ExtraIsland.Shared;
using MaterialDesignThemes.Wpf;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public class RhesisConfig {
    RhesisDataSource _dataSource = RhesisDataSource.SaintJinrishici;
    public RhesisDataSource DataSource { get => _dataSource;
        set {
            if (_dataSource == value) return;
            _dataSource = value;
            if (value == RhesisDataSource.Hitokoto | value == RhesisDataSource.All) { 
                new CommonDialogBuilder().AddAction("确认",PackIconKind.Check)
                    .SetPackIcon(PackIconKind.Alert)
                    .SetCaption("警告·数据源污染")
                    .SetContent("近期收到大量反馈 \"一言\" 源中存在部分不良内容\r\n请注意进行相应过滤设置\r\n我们概不为本组件其造成的一切影响负责")
                    .ShowDialog();
            }
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
    
    public bool IsHitokotoWarnConfirmed { get; set; }
}