using System.Security.Cryptography;
using System.Text;
using ClassIsland.Core.Abstractions.Services;

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
    
    public static string Sha256EncryptString(string data) {
        ArgumentNullException.ThrowIfNull(data);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(data)));
    }

    public static bool IsPluginInstalled(string pkgName) {
        return IPluginService.LoadedPlugins.Any(info => info.Manifest.Id == pkgName && info.IsEnabled);
    }
    
    public static bool IsLyricsIslandInstalled() {
        return IsPluginInstalled("jiangyin14.lyrics");
    }
}