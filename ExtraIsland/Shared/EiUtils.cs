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
    
    public static string Sha256EncryptString(string data)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        #pragma warning disable SYSLIB0021
        // ReSharper disable once AccessToStaticMemberViaDerivedType
        byte[] hash = SHA256Managed.Create().ComputeHash(bytes);
        #pragma warning restore SYSLIB0021
 
        StringBuilder builder = new StringBuilder();
        foreach (byte t in hash) {
            builder.Append(t.ToString("x2"));
        }
        return builder.ToString();
    }

    public static bool IsPluginInstalled(string pkgName) {
        return IPluginService.LoadedPlugins.Any(info => info.Manifest.Id == pkgName);
    }
    
    public static bool IsLyricsIslandInstalled() {
        return IsPluginInstalled("jiangyin14.lyrics");
    }
}