using System.Text.RegularExpressions;
using Avalonia.Data.Converters;
using ClassIsland.Core.Abstractions.Controls;
using ExtraIsland.ConfigHandlers;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

public partial class RhesisSettings : ComponentBase<RhesisConfig> {
    public RhesisSettings() {
        MainConfig = GlobalConstants.Handlers.MainConfig!.Data;
        InitializeComponent();
    }

    public MainConfigData MainConfig { get; set; } 
        
    [GeneratedRegex("[^0-9]+")]
    private static partial Regex NumberRegex();
    
    public List<RhesisDataSource> DataSources { get; } = [
        RhesisDataSource.All,
        RhesisDataSource.Hitokoto,
        RhesisDataSource.Jinrishici,
        RhesisDataSource.Saint,
        RhesisDataSource.SaintJinrishici
    ];
    
    public List<RhesisConfig.AttributesDisplayRule> AttributesRules { get; } = [
        RhesisConfig.AttributesDisplayRule.Sametime,
        RhesisConfig.AttributesDisplayRule.Separate
    ];
}

public class LimitIntToArgConverter : IValueConverter {
    public object Convert(object? value,Type targetType,object? parameter,
        System.Globalization.CultureInfo culture) {
        return (int)value! switch {
            0 => "",
            _ => $"max_length={((int)value).ToString()}&"
        };
    }

    public object ConvertBack(object? value,Type targetType,object? parameter,
        System.Globalization.CultureInfo culture) {
        return 0;
    }
}

