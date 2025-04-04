using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Core.Models.Plugin;
using ExtraIsland.Automations;
using ExtraIsland.Components;
using ExtraIsland.ConfigHandlers;
using ExtraIsland.LifeMode.Components;
using ExtraIsland.SettingsPages;
using ExtraIsland.Shared;
using ExtraIsland.TinyFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExtraIsland
{/*
            ___________          __                   .___         .__                       .___
            \_   _____/___  ____/  |_ _______ _____   |   |  ______|  |  _____     ____    __| _/
             |    __)_ \  \/  /\   __\\_  __ \\__  \  |   | /  ___/|  |  \__  \   /    \  / __ |
             |        \ >    <  |  |   |  | \/ / __ \_|   | \___ \ |  |__ / __ \_|   |  \/ /_/ |
            /_______  //__/\_ \ |__|   |__|   (____  /|___|/____  >|____/(____  /|___|  /\____ |
                    \/       \/                    \/           \/            \/      \/      \/
*/
    [PluginEntrance]
    // ReSharper disable once UnusedType.Global
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : PluginBase {
        public override void Initialize(HostBuilderContext context, IServiceCollection services) {
            ConsoleColor defaultColor = Console.ForegroundColor;
            //TODO: 重构早加载阶段终端处理器
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[ExIsLand][Splash]-------------------------------------------------------------------\r\n" 
                             + GlobalConstants.Assets.AsciiLogo
                             + "\r\n Copyright (C) 2024-2025 LiPolymer \r\n Licensed under GNU AGPLv3. \r\n" 
                             + "[ExIsLand][EarlyLoad]正在初始化...-------------------------------------------------");
            Console.ForegroundColor = defaultColor;
            Console.WriteLine("[ExIsLand][EarlyLoad]正在载入主设置...");
            //Initialize GlobalConstants/ConfigHandlers
            GlobalConstants.PluginConfigFolder = PluginConfigFolder;
            GlobalConstants.Handlers.MainConfig = new MainConfigHandler();
            if (GlobalConstants.Handlers.MainConfig.Data.IsTelemetryActivated) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[ExIsLand][EarlyLoad]遥测已启用! 感谢您的帮助(～￣▽￣)～");
                Console.ForegroundColor = defaultColor;
                Console.WriteLine("[ExIsLand][EarlyLoad]正在初始化Sentry...");
            
                SentrySdk.Init(o => {
                    o.Dsn = "https://0957ca91c84095acea32a5888148bb68@o4508585356165120.ingest.de.sentry.io/4508585358065744";
                    o.Release = Info.Manifest.Version;
                });
            
                AppBase.Current.DispatcherUnhandledException += (sender,e) => {
                    SentrySdk.CaptureException(e.Exception);
                };   
            }
            Console.WriteLine("[ExIsLand][EarlyLoad]正在载入其余配置...");
            GlobalConstants.Handlers.OnDuty = new OnDutyPersistedConfigHandler();
            Console.WriteLine("[ExIsLand][EarlyLoad]正在注册ClassIsland要素...");
            //Services
            services.AddHostedService<ServicesFetcherService>();
            services.AddHostedService<Register>();
            //Components
            services.AddComponent<BetterCountdown,BetterCountdownSettings>();
            services.AddComponent<FluentClock,FluentClockSettings>();
            services.AddComponent<Rhesis,RhesisSettings>();
            services.AddComponent<OnDuty,OnDutySettings>();
            services.AddComponent<LiveActivity,LiveActivitySettings>();
            //SettingsPages
            services.AddSettingsPage<MainSettingsPage>();
            services.AddSettingsPage<DutySettingsPage>();
            services.AddSettingsPage<TinyFeaturesSettingsPage>();
            //Actions
            Register.Claim(services);
            //LifeMode
            if (GlobalConstants.Handlers.MainConfig.Data.IsLifeModeActivated) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[ExIsLand][EarlyLoad]生活模式已启用!");
                Console.ForegroundColor = defaultColor;
                services.AddComponent<Sleepy,SleepySettings>();
            }
            if (GlobalConstants.Handlers.MainConfig.Data.IsExperimentalModeActivated) {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("[ExIsLand][EarlyLoad][Experiment]实验模式已启用! 若出现Bug,请勿报告!");
                Console.ForegroundColor = defaultColor;
                services.AddComponent<DebugLyricsHandler>();
                services.AddComponent<DebugSubLyricsHandler>();
            }
            #if DEBUG
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("[ExIsLand][EarlyLoad][DEBUG]这是一个调试构建! 若出现Bug,请勿报告!");
                Console.ForegroundColor = defaultColor;
                services.AddSettingsPage<DebugSettingsPage>();
            #endif
            Console.WriteLine("[ExIsLand][EarlyLoad]完成!");
            Console.WriteLine("[ExIsLand][EarlyLoad]注册事件...");
            GlobalConstants.Triggers.OnLoaded += JuniorGuide.Trigger;
            AppBase.Current.AppStarted += (_,_) => {
                GlobalConstants.Handlers.MainWindow = new MainWindowHandler();
                if (!GlobalConstants.Handlers.MainConfig.Data.Dock.Enabled) return;
                GlobalConstants.Handlers.MainWindow!.InitBar(accentState: GlobalConstants.Handlers.MainConfig.Data.Dock.AccentState);
            };
            AppBase.Current.AppStopping += (_,_) => {
                if (GlobalConstants.Handlers.LyricsIsland == null) return;
                GlobalConstants.Handlers.LyricsIsland = null;
                GlobalConstants.HostInterfaces.PluginLogger!.LogInformation("检测到内置LyricsIsland协议接口启动,5秒后将强制结束进程");
                new Thread(() => {
                    Thread.Sleep(5000);
                    GlobalConstants.HostInterfaces.PluginLogger!.LogInformation("正在关闭...");
                    Environment.Exit(0);
                }).Start();
            };
            Console.WriteLine("[ExIsLand][EarlyLoadTrigger]完成!");
            Console.WriteLine("[ExIsLand][EarlyLoad]等待服务主机启动...");
        }
    }
}
