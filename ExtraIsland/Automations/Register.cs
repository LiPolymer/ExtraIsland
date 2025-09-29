using Avalonia.Threading;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Commands;
using ClassIsland.Core.Extensions.Registry;
using ExtraIsland.Automations.Actions;
using ExtraIsland.Automations.Rules;
using ExtraIsland.Automations.Triggers;
using ExtraIsland.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExtraIsland.Automations;

/// <summary>
/// 自动化内容注册器
/// </summary>
public class Register : IHostedService {

    /// <summary>
    /// 注册ClassIsland元素
    /// </summary>
    /// <param name="services">应用服务集合</param>
    public static void Claim(IServiceCollection services) {
        // 行动
        
        services.AddAction<SetFlagConfig, SetFlag>(
            "extraIsland.action.setFlag", 
            "设标志", 
            "\uE844");
        
        if (EiUtils.IsPluginInstalled("Plugin.IslandCaller")) {
            services.AddAction<object, EmptySettings>(
                "extraIsland.action.islandCaller", 
                "拉起IslandCaller", 
                "\uECB5"); 
        }
        // 规则
        services.AddRule<TodayIsConfig, TodayIs>
            ("extraIsland.rule.todayIs", "今天是", "\uE304");
        
        services.AddRule<LaterThanConfig, LaterThan>
            ("extraIsland.rule.laterThan", "时间晚于", "\uE4D4");
        services.AddRule<TeacherIsConfig, TeacherIs>
            ("extraIsland.rule.currentTeacherIs", "当前教师是", "\uECF9");
        services.AddRule<TeacherIsConfig, TeacherIs>
            ("extraIsland.rule.nextTeacherIs", "下节课教师是", "\uECF7");
        services.AddRule<FlagIsConfig, FlagIs>
            ("extraIsland.rule.flagIs", "读标志", "\uE844");
        // 触发器
        services.AddTrigger<TimePassed,TimePassedSettings>();
    }
    
    /// <summary>
    /// 注册处理逻辑
    /// </summary>
    /// <param name="actionService">行动服务</param>
    /// <param name="rulesetService">规则集服务</param>
    public Register(IActionService actionService, IRulesetService rulesetService) {
        //行动
        actionService.RegisterActionHandler("extraIsland.action.setFlag",SetFlag.Action);
        
        if (EiUtils.IsPluginInstalled("Plugin.IslandCaller")) {
            actionService.RegisterActionHandler("extraIsland.action.islandCaller",(_,_) => {
                Dispatcher.UIThread.Invoke(() => {
                    UriNavigationCommands.UriNavigationCommand.Execute("classisland://plugins/IslandCaller/Run");
                });
            });
        }

        //规则
        rulesetService.RegisterRuleHandler("extraIsland.rule.todayIs",TodayIs.Rule);
        rulesetService.RegisterRuleHandler("extraIsland.rule.laterThan",LaterThan.Rule);
        rulesetService.RegisterRuleHandler("extraIsland.rule.flagIs",FlagIs.Rule);
        rulesetService.RegisterRuleHandler("extraIsland.rule.currentTeacherIs",TeacherIs.CurrentRule);
        rulesetService.RegisterRuleHandler("extraIsland.rule.nextTeacherIs",TeacherIs.NextRule);
    }
    
    public Task StartAsync(CancellationToken _) {
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken _) {
        return Task.CompletedTask;
    }
}