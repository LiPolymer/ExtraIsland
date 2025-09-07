using ClassIsland.Core.Abstractions.Services;
using ExtraIsland.ConfigHandlers;
using Microsoft.Extensions.Logging;

namespace ExtraIsland.Shared;

public static class GlobalConstants {
    public static string? PluginConfigFolder { get; set; }

    public static class Handlers {
        public static OnDutyPersistedConfigHandler? OnDuty { get; set; }
        public static MainConfigHandler? MainConfig { get; set; }
        public static LyricsIslandLyricsProvider? LyricsIsland { get; set; }
        
        public static PersistedFlagHandler? PersistedFlagHandler { get; set; }
        
        //public static MainWindowHandler? MainWindow { get; set; }
    }

    public static class Triggers {
        public static bool IsLoaded { get; private set; }
        
        /// <summary>
        /// ExtraIsland完全载入完毕后
        /// </summary>
        public static event Action? OnLoaded;
        public static void Loaded() {
            IsLoaded = true;
            OnLoaded?.Invoke();
        }
        
        /// <summary>
        /// ExtraIsland开始载入
        /// </summary>
        public static event Action? OnLoading;
        public static void Loading() {
            OnLoading?.Invoke();
        }
    }
    
    public static class HostInterfaces {
        public static ILessonsService? LessonsService;
        public static IExactTimeService? ExactTimeService;
        public static ILogger<ExtraIsland.Plugin>? PluginLogger;
    }

    public static class Assets {
        public static readonly string AsciiLogo = """
                                                  ___________          __                   .___         .__                       .___
                                                  \_   _____/___  ____/  |_ _______ _____   |   |  ______|  |  _____     ____    __| _/
                                                   |    __)_ \  \/  /\   __\\_  __ \\__  \  |   | /  ___/|  |  \__  \   /    \  / __ | 
                                                   |        \ >    <  |  |   |  | \/ / __ \_|   | \___ \ |  |__ / __ \_|   |  \/ /_/ | 
                                                  /_______  //__/\_ \ |__|   |__|   (____  /|___|/____  >|____/(____  /|___|  /\____ | 
                                                          \/       \/                    \/           \/            \/      \/      \/ 
                                                  """;
    }
}