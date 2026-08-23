using System.Reflection;
using System.Runtime.InteropServices;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Core.Services.Registry;
using ClassIsland.Shared;
using ExtraIsland.Automations;
using ExtraIsland.AuthorizeProvider;
using ExtraIsland.Components;
using ExtraIsland.ConfigHandlers;
using ExtraIsland.LifeMode.Components;
using ExtraIsland.SettingPages;
using ExtraIsland.Shared;
using ExtraIsland.Notification;
using LycheeLib.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExtraIsland;

/*
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
    const string AsciiLogo = """
                              ___________          __                   .___         .__                       .___
                              \_   _____/___  ____/  |_ _______ _____   |   |  ______|  |  _____     ____    __| _/
                               |    __)_ \  \/  /\   __\\_  __ \\__  \  |   | /  ___/|  |  \__  \   /    \  / __ |
                               |        \ >    <  |  |   |  | \/ / __ \_|   | \___ \ |  |__ / __ \_|   |  \/ /_/ |
                              /_______  //__/\_ \ |__|   |__|   (____  /|___|/____  >|____/(____  /|___|  /\____ |
                                      \/       \/                    \/           \/            \/      \/      \/
                              """;
    
    public override void Initialize(HostBuilderContext context, IServiceCollection services) {
        ChainedTerminal ct = new ChainedTerminal("&aExtraIsland");
        ConsoleColor defaultColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[ExIsLand][Splash]-------------------------------------------------------------------\r\n" 
                          + AsciiLogo
                          + "\r\n Copyright (C) 2024-2025 LiPolymer \r\n Licensed under GNU AGPLv3. \r\n" 
                          + "正在初始化...-------------------------------------------------------------------");
        Console.ForegroundColor = defaultColor;
        
        ChainedTerminal cct = ct.Chain("&3ConfigHandler");
        cct.WriteLine("正在载入主设置...");
        //环境与配置
        PluginEnvironment environment = new PluginEnvironment(PluginConfigFolder);
        MainConfigHandler mainConfig = new MainConfigHandler(environment);
        PersistedFlagHandler persistedFlagHandler = ConfigBase.Load<PersistedFlagHandler>(environment);
        
        services.AddSingleton(environment);
        services.AddSingleton(mainConfig);
        services.AddSingleton(persistedFlagHandler);
        
        if (mainConfig.Data.IsTelemetryActivated) {
            ChainedTerminal sct = ct.Chain("&5Sentry");
            #if DEBUG
                sct.WriteLine("这是调试构建,遥测将被禁用!",Terminal.MessageType.Debug);
            #endif
                #if !DEBUG
                // ReSharper disable once HeuristicUnreachableCode
                sct.WriteLine("&2遥测已启用! 感谢您的帮助(～￣▽￣)～");
                sct.WriteLine("正在初始化Sentry...");
            
                SentrySdk.Init(o => {
                    o.Dsn = "https://0957ca91c84095acea32a5888148bb68@o4508585356165120.ingest.de.sentry.io/4508585358065744";
                    o.Release = Info.Manifest.Version;
                    o.AutoSessionTracking = true;
                });
            
                //AppBase.Current.DispatcherUnhandledException += (_,e) => {
                //    if (e.Exception.StackTrace == null) SentrySdk.CaptureException(e.Exception);
                //    else if (e.Exception.StackTrace.Contains("ExtraIsland")) SentrySdk.CaptureException(e.Exception);
                //};
                #endif
        }
        
        cct.WriteLine("正在载入其余配置...");
        services.AddSingleton<OnDutyPersistedConfigHandler>();
        
        ct.WriteLine("正在注册ClassIsland要素...");
        //Services
        services.AddHostedService<Register>();
        
        // 标志服务
        services.AddSingleton<IFlagService,FlagService>();
        // 节假日服务
        services.AddSingleton<IHolidayService,HolidayService>();
        
        // 名句一言
        services.AddSingleton<IRhesisProvider,HitokotoRhesisProvider>();
        services.AddSingleton<IRhesisProvider,JinrishiciRhesisProvider>();
        services.AddSingleton<IRhesisProvider,SainticRhesisProvider>();
        services.AddSingleton<IRhesisProviderRegistry,RhesisProviderRegistry>();
        services.AddSingleton<IRhesisService,RhesisService>();
        
        // 歌词提供方
        services.AddSingleton<LyricsIslandLyricsProvider>();
        services.AddSingleton<LycheeLyricsProvider>();
        
        //Components
        services.AddComponent<BetterCountdown,BetterCountdownSettings>();
        services.AddComponent<FluentClock,FluentClockSettings>();
        services.AddComponent<Rhesis,RhesisSettings>();
        services.AddComponent<OnDuty,OnDutySettings>();
        services.AddComponent<LiveActivity,LiveActivitySettings>();
        services.AddComponent<DynamicLyrics,DynamicLyricsSettings>();
        services.AddComponent<ProfileInformation,ProfileInformationSettings>();
        services.AddComponent<FlagDisplay,FlagDisplaySettings>();
        
        //SettingsPages
        services.AddSettingsPage<MainSettingsPage>();
        services.AddSettingsPage<DutySettingsPage>();
        //services.AddSettingsPage<TinyFeaturesSettingsPage>();
        
        // 动态反射，实现在低 PluginSdk 上使用高版本功能
        List<SettingsPageInfo> registeredSettingsPageInfos = SettingsWindowRegistryService.Registered
            .Where(info => info.Id.StartsWith("extraisland") && info.Category == SettingsPageCategory.External)
            .ToList();
        
        if (InjectService.TryGetAddSettingsPageGroupMethod(out MethodInfo? addSettingsPageGroupMethod))
        {
            addSettingsPageGroupMethod.Invoke(typeof(SettingsWindowRegistryExtensions), [services, "extraisland.settings", "\uEA33", "ExtraIsland"]);
            
            PropertyInfo groupIdProperty = InjectService.GetSettingsPageInfoGroupIdProperty();
            foreach (SettingsPageInfo info in registeredSettingsPageInfos)
            {
                groupIdProperty.SetValue(info, "extraisland.settings");
            }
        }
        else
        {
            FieldInfo nameField = InjectService.GetSettingsPageInfoNameField();
            foreach (SettingsPageInfo info in registeredSettingsPageInfos)
            {
                nameField.SetValue(info, "ExtraIsland·" + (string)nameField.GetValue(info)!);
            }
        }
        
        //NotificationProvider
        services.AddNotificationProvider<TimeUpNotification>();
        
        //Actions / Rules / Triggers
        Register.Claim(services,mainConfig);
        
        //Authorizer
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            services.AddAuthorizeProvider<UsbDriveAuthorizer>();
        }
        
        //LifeMode
        if (mainConfig.Data.IsLifeModeActivated) {
            ct.WriteLine("&a生活模式已启用!");
            services.AddComponent<Sleepy,SleepySettings>();
        }
        
        if (mainConfig.Data.Dock.Enabled) {
            //services.AddComponent<ActionButton,ActionButtonSettings>();
        }
        
        if (mainConfig.Data.IsExperimentalModeActivated) {
            ct.WriteLine("&9实验模式已启用! &7若出现Bug,&c请勿报告&7!",Terminal.MessageType.Warn);
            services.AddComponent<DualLineContainer>();
            //services.AddComponent<DebugLyricsHandler>();
            //services.AddComponent<DebugSubLyricsHandler>();
        }
        
        #if DEBUG
        ct.WriteLine("&d这是一个调试构建! 若出现Bug,请勿报告!",Terminal.MessageType.Debug);
        //services.AddSettingsPage<DebugSettingsPage>();
        #endif
        
        ct.WriteLine("完成!");
        ct.WriteLine("注册事件...");
        
        //初始化LycheeLib
        if (EiUtils.IsPluginInstalled("ink.lipoly.ext.lychee")) {
            AppBase.Current.AppStarted += (_, _) => {
                Rendezvous.Load(IAppHost.GetService<ILycheeLyrics>());
            };
        }
        
        ct.WriteLine("完成!");
        ct.WriteLine("&a等待服务主机启动...");
    }
}
