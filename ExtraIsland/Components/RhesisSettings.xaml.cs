using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

public partial class RhesisSettings {
    public RhesisSettings() {
        InitializeComponent();
    }

    [GeneratedRegex("[^0-9]+")]
    private static partial Regex NumberRegex();
    void TextBoxNumberCheck(object sender,TextCompositionEventArgs e) {
        Regex re = NumberRegex();
        e.Handled = re.IsMatch(e.Text) & e.Text.Length != 0;
    }
    
    public List<RhesisDataSource> DataSources { get; } = [
        RhesisDataSource.All,
        RhesisDataSource.Hitokoto,
        RhesisDataSource.Jinrishici,
        RhesisDataSource.Saint,
        RhesisDataSource.SaintJinrishici,
        RhesisDataSource.OnlineTxt
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
