using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ExtraIsland.Shared;

public interface IHolidayService {
    /// <summary>
    /// 判断指定日期是否为节假日（包括双休日）
    /// </summary>
    Task<bool> IsHolidayAsync(DateTime date);

    /// <summary>
    /// 获取指定日期的节假日信息
    /// </summary>
    Task<HolidayInfo?> GetHolidayInfoAsync(DateTime date);

    /// <summary>
    /// 获取指定日期之后的下一个节假日的信息
    /// </summary>
    Task<(DateTime Date,string Name)?> GetNextHolidayAsync(DateTime fromDate);
}

/// <summary>
/// 节假日服务,用于获取和判断节假日信息
/// </summary>
public class HolidayService : IHolidayService {
    static readonly HttpClient HttpClient = new() {
        Timeout = TimeSpan.FromSeconds(10)
    };
    readonly ILogger<HolidayService> _logger;
    readonly Dictionary<int,YearHolidayData> _cachedHolidays = new();
    readonly SemaphoreSlim _cacheLock = new(1,1);

    public HolidayService(ILogger<HolidayService> logger) {
        _logger = logger;
    }

    public async Task<bool> IsHolidayAsync(DateTime date) {
        // 首先检查是否为周末
        if (IsWeekend(date)) {
            return true;
        }

        // 获取该年份的节假日数据
        YearHolidayData? yearData = await GetYearHolidayDataAsync(date.Year);
        if (yearData == null) {
            return false; // 如果无法获取数据，默认不是节假日
        }

        // 检查是否为节假日
        string dateKey = date.ToString("MM-dd");
        return yearData.Holiday?.TryGetValue(dateKey,out HolidayInfo? holidayInfo) is true
               && holidayInfo.Holiday; // true为节假日，false为调休工作日
    }

    public async Task<HolidayInfo?> GetHolidayInfoAsync(DateTime date) {
        if (IsWeekend(date)) {
            return new HolidayInfo {
                Holiday = true,
                Name = GetWeekendName(date),
                Wage = 1,
                Date = date.ToString("yyyy-MM-dd"),
                Rest = 0
            };
        }

        YearHolidayData? yearData = await GetYearHolidayDataAsync(date.Year);
        if (yearData?.Holiday == null) {
            return null;
        }

        string dateKey = date.ToString("MM-dd");
        if (!yearData.Holiday.TryGetValue(dateKey,out HolidayInfo? holidayInfo)) return null;
        return holidayInfo.Holiday ? holidayInfo : null;
    }

    public async Task<(DateTime Date,string Name)?> GetNextHolidayAsync(DateTime fromDate) {
        // 检查从明天开始的未来一年内的日期
        for (int i = 1; i <= 365; i++) {
            DateTime checkDate = fromDate.Date.AddDays(i);
            HolidayInfo? holidayInfo = await GetHolidayInfoAsync(checkDate);

            if (holidayInfo != null) {
                return (checkDate,holidayInfo.Name);
            }
        }

        return null;
    }

    /// <summary>
    /// 获取指定年份的节假日数据
    /// </summary>
    async Task<YearHolidayData?> GetYearHolidayDataAsync(int year) {
        await _cacheLock.WaitAsync();
        try {
            // 检查缓存
            if (_cachedHolidays.TryGetValue(year,out YearHolidayData? cached)) {
                return cached;
            }

            try {
                string response = await HttpClient.GetStringAsync($"https://timor.tech/api/holiday/year/{year}");
                YearHolidayData? data = JsonSerializer.Deserialize<YearHolidayData>(response);

                if (data != null) {
                    _cachedHolidays[year] = data;
                    return data;
                }
            } catch (Exception ex) {
                // 记录错误但不抛出异常，让程序继续运行
                _logger.LogInformation(ex,"获取节假日数据失败");
            }

            return null;
        } finally {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// 判断是否为周末
    /// </summary>
    static bool IsWeekend(DateTime date) {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// 获取周末名称
    /// </summary>
    static string GetWeekendName(DateTime date) {
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
    public Dictionary<string,HolidayInfo>? Holiday { get; set; }
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
