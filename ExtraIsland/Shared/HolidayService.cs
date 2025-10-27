using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ExtraIsland.Shared;

/// <summary>
/// 节假日服务，用于获取和判断节假日信息
/// </summary>
public static class HolidayService {
    private static readonly Dictionary<int, YearHolidayData> CachedHolidays = new();
    private static readonly SemaphoreSlim CacheLock = new(1, 1);
    
    /// <summary>
    /// 判断指定日期是否为节假日（包括双休日）
    /// </summary>
    /// <param name="date">要判断的日期</param>
    /// <returns>如果是节假日返回true，否则返回false</returns>
    public static async Task<bool> IsHolidayAsync(DateTime date) {
        // 首先检查是否为周末
        if (IsWeekend(date)) {
            return true;
        }
        
        // 获取该年份的节假日数据
        var yearData = await GetYearHolidayDataAsync(date.Year);
        if (yearData == null) {
            return false; // 如果无法获取数据，默认不是节假日
        }
        
        // 检查是否为节假日
        var dateKey = date.ToString("MM-dd");
        if (yearData.Holiday?.ContainsKey(dateKey) == true) {
            var holidayInfo = yearData.Holiday[dateKey];
            return holidayInfo.Holiday; // true为节假日，false为调休工作日
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取指定日期的节假日信息
    /// </summary>
    /// <param name="date">要查询的日期</param>
    /// <returns>节假日信息，如果不是节假日则返回null</returns>
    public static async Task<HolidayInfo?> GetHolidayInfoAsync(DateTime date) {
        if (IsWeekend(date)) {
            return new HolidayInfo {
                Holiday = true,
                Name = GetWeekendName(date),
                Wage = 1,
                Date = date.ToString("yyyy-MM-dd"),
                Rest = 0
            };
        }
        
        var yearData = await GetYearHolidayDataAsync(date.Year);
        if (yearData?.Holiday == null) {
            return null;
        }
        
        var dateKey = date.ToString("MM-dd");
        if (yearData.Holiday.ContainsKey(dateKey)) {
            var holidayInfo = yearData.Holiday[dateKey];
            if (holidayInfo.Holiday) {
                return holidayInfo;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 获取指定年份下一个节假日的信息
    /// </summary>
    /// <param name="fromDate">起始日期</param>
    /// <returns>下一个节假日信息</returns>
    public static async Task<(DateTime Date, string Name)?> GetNextHolidayAsync(DateTime fromDate) {
        // 检查从明天开始的未来一年内的日期
        for (int i = 1; i <= 365; i++) {
            var checkDate = fromDate.Date.AddDays(i);
            var holidayInfo = await GetHolidayInfoAsync(checkDate);
            
            if (holidayInfo != null) {
                return (checkDate, holidayInfo.Name);
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 获取指定年份的节假日数据
    /// </summary>
    /// <param name="year">年份</param>
    /// <returns>节假日数据</returns>
    private static async Task<YearHolidayData?> GetYearHolidayDataAsync(int year) {
        await CacheLock.WaitAsync();
        try {
            // 检查缓存
            if (CachedHolidays.TryGetValue(year, out var cached)) {
                return cached;
            }
            
            // 从API获取数据
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            try {
                var response = await httpClient.GetStringAsync($"https://timor.tech/api/holiday/year/{year}");
                var data = JsonSerializer.Deserialize<YearHolidayData>(response);
                
                if (data != null) {
                    CachedHolidays[year] = data;
                    return data;
                }
            }
            catch (Exception ex) {
                // 记录错误但不抛出异常，让程序继续运行
                try {
                    GlobalConstants.HostInterfaces.PluginLogger?.LogInformation($"获取节假日数据失败: {ex.Message}");
                }
                catch {
                    // 忽略日志记录错误
                }
            }
            
            return null;
        }
        finally {
            CacheLock.Release();
        }
    }
    
    /// <summary>
    /// 判断是否为周末
    /// </summary>
    /// <param name="date">日期</param>
    /// <returns>是否为周末</returns>
    private static bool IsWeekend(DateTime date) {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
    
    /// <summary>
    /// 获取周末名称
    /// </summary>
    /// <param name="date">日期</param>
    /// <returns>周末名称</returns>
    private static string GetWeekendName(DateTime date) {
        return date.DayOfWeek switch {
            DayOfWeek.Saturday => "周六",
            DayOfWeek.Sunday => "周日",
            _ => "周末"
        };
    }
}

/// <summary>
/// 年度节假日数据
/// </summary>
public class YearHolidayData {
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("holiday")]
    public Dictionary<string, HolidayInfo>? Holiday { get; set; }
}

/// <summary>
/// 节假日信息
/// </summary>
public class HolidayInfo {
    [JsonPropertyName("holiday")]
    public bool Holiday { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("wage")]
    public int Wage { get; set; }
    
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
    
    [JsonPropertyName("rest")]
    public int Rest { get; set; }
}