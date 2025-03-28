using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

public partial class RhesisSettings
{
    public RhesisSettings()
    {
        InitializeComponent();
    }

    [GeneratedRegex("[^0-9]+")]
    private static partial Regex NumberRegex();
    void TextBoxNumberCheck(object sender, TextCompositionEventArgs e)
    {
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

    private void CheckboxChanged(object sender, RoutedEventArgs e)
    {
        // Recalculate weights when checkboxes change
        RecalculateWeights();
    }

    private void RecalculateWeights()
    {
        // Count enabled sources
        int enabledCount = 0;
        if (Settings.EnableHitokoto) enabledCount++;
        if (Settings.EnableJinrishici) enabledCount++;
        if (Settings.EnableSaint) enabledCount++;
        if (Settings.EnableOnlineTxt) enabledCount++;

        if (enabledCount == 0)
        {
            // No sources enabled, set default values
            Settings.HitokotoWeight = 25;
            Settings.JinrishiciWeight = 25;
            Settings.SaintWeight = 25;
            Settings.OnlineTxtWeight = 25;
            return;
        }

        // Calculate even distribution
        int evenWeight = 100 / enabledCount;

        // Assign weights
        if (Settings.EnableHitokoto) Settings.HitokotoWeight = evenWeight;
        if (Settings.EnableJinrishici) Settings.JinrishiciWeight = evenWeight;
        if (Settings.EnableSaint) Settings.SaintWeight = evenWeight;
        if (Settings.EnableOnlineTxt) Settings.OnlineTxtWeight = evenWeight;

        // Adjust for remainders
        int remainder = 100 - (evenWeight * enabledCount);
        if (remainder > 0)
        {
            if (Settings.EnableHitokoto)
            {
                Settings.HitokotoWeight += remainder;
                remainder = 0;
            }
            else if (Settings.EnableJinrishici)
            {
                Settings.JinrishiciWeight += remainder;
                remainder = 0;
            }
            else if (Settings.EnableSaint)
            {
                Settings.SaintWeight += remainder;
                remainder = 0;
            }
            else if (Settings.EnableOnlineTxt)
            {
                Settings.OnlineTxtWeight += remainder;
            }
        }
    }

    // Add method to show prefetch status
    private void ShowPrefetchStatus(object sender, RoutedEventArgs e)
    {
        var counts = RhesisHandler.PrefetchManager.GetQuoteCounts();
        var totalCount = RhesisHandler.PrefetchManager.GetTotalQuoteCount();

        string message = $"预请求状态：\n" +
                         $"一言：{counts[RhesisDataSource.Hitokoto]} 条\n" +
                         $"今日诗词：{counts[RhesisDataSource.Jinrishici]} 条\n" +
                         $"诏预：{counts[RhesisDataSource.Saint]} 条\n" +
                         $"在线TXT：{counts[RhesisDataSource.OnlineTxt]} 条\n" +
                         $"总计：{totalCount} 条";

        MessageBox.Show(message, "预请求状态", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

public class LimitIntToArgConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        return (int)value! switch
        {
            0 => "",
            _ => $"max_length={((int)value).ToString()}&"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        return 0;
    }
}
