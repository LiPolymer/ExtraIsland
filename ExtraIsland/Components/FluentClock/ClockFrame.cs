namespace ExtraIsland.Components;

/// <summary>
/// 流畅时钟某一时刻的期望显示状态 (纯数据, 不依赖 UI)
/// </summary>
/// <param name="Hour">时</param>
/// <param name="Minute">分</param>
/// <param name="Second">秒</param>
/// <param name="ShowSeconds">是否显示秒 (精确到秒)</param>
public readonly record struct ClockFrame(int Hour, int Minute, int Second, bool ShowSeconds) {
    /// <summary>
    /// 数字位数: HH MM SS
    /// </summary>
    public const int DigitCount = 6;

    public static ClockFrame From(DateTime time, bool showSeconds) =>
        new ClockFrame(time.Hour, time.Minute, time.Second, showSeconds);

    /// <summary>
    /// 分隔符是否可见: 精确到秒时常亮, 否则按秒奇偶闪烁 (与真实秒数对齐)
    /// </summary>
    public bool SeparatorVisible => ShowSeconds || Second % 2 == 0;

    /// <summary>
    /// 取第 <paramref name="index"/> 位数字 (0..5 分别为 时十/时个/分十/分个/秒十/秒个)
    /// </summary>
    public char DigitAt(int index) => index switch {
        0 => (char)('0' + Hour / 10),
        1 => (char)('0' + Hour % 10),
        2 => (char)('0' + Minute / 10),
        3 => (char)('0' + Minute % 10),
        4 => (char)('0' + Second / 10),
        5 => (char)('0' + Second % 10),
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    /// <summary>
    /// 该位是否属于秒
    /// </summary>
    public static bool IsSecondDigit(int index) => index >= 4;

    /// <summary>
    /// 是否为整点时刻: 小时发生变化且处于新小时的第一分钟内.
    /// 不要求恰好为第 0 秒, 以免 UI 卡顿跳帧时丢失强调; 同时排除任意时间跳变
    /// </summary>
    public static bool IsOClockMoment(ClockFrame prev, ClockFrame next) =>
        prev.Hour != next.Hour && next.Minute == 0;

    /// <summary>
    /// 秒数是否使用翻页动画.
    /// 专注模式下秒数改为淡入淡出, 但分钟翻页时同步翻页以保持整体一致
    /// </summary>
    public static bool ShouldSwapSeconds(ClockFrame prev, ClockFrame next, bool swapEnabled, bool focusedMode) =>
        swapEnabled && (!focusedMode || prev.Minute != next.Minute);
}
