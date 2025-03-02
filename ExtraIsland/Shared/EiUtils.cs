using System.IO;

namespace ExtraIsland.Shared;

public static class EiUtils {
    public static bool IsOdd(int n) {
        return n % 2 == 1;
    }
    
    public static TimeSpan GetDateTimeSpan(DateTime startTime, DateTime endTime) {
        TimeSpan daysSpan = new TimeSpan(endTime.Ticks - startTime.Ticks);
        return daysSpan;
    }

    // ReSharper disable once UnusedMember.Global
    public static double ConvertVersion(string version) {
        string[] versionParts = version.Split('.');
        string major = versionParts[0] + versionParts[1] + versionParts[2];
        return Convert.ToDouble($"{major}.{versionParts[3]}");
    }
    
    public static bool IsLyricsIslandInstalled() {
        //TODO: 使用ClassIsland接口判断是否加载
        return File.Exists("./Plugins/jiangyin14.lyrics/manifest.yml");
    }
}