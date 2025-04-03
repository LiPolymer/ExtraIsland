﻿using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Extensions.Registry;
using ExtraIsland.Automations.Actions;
using ExtraIsland.Automations.Rules;
using ExtraIsland.Automations.Triggers;
using MaterialDesignThemes.Wpf;
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
        services.AddAction<MainWindowOperatorConfig, MainWindowOperator>(
            "extraIsland.action.mainWindowOperator", 
            "主窗口操作器", 
            PackIconKind.DesktopWindows);
        // 规则
        services.AddRule<TodayIsConfig, TodayIs>
            ("extraIsland.rule.todayIs", "今天是", PackIconKind.Calendar);
        // 触发器
        services.AddTrigger<PostMainTimerTicked>();
    }
    
    /// <summary>
    /// 注册处理逻辑
    /// </summary>
    /// <param name="actionService">行动服务</param>
    /// <param name="rulesetService">规则集服务</param>
    public Register(IActionService actionService, IRulesetService rulesetService) {
        //行动
        actionService.RegisterActionHandler("extraIsland.action.mainWindowOperator",MainWindowOperator.Action);
        actionService.RegisterRevertHandler("extraIsland.action.mainWindowOperator",MainWindowOperator.Revert);
        //规则
        rulesetService.RegisterRuleHandler("extraIsland.rule.todayIs",TodayIs.Rule);
    }
    
    public Task StartAsync(CancellationToken _) {
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken _) {
        return Task.CompletedTask;
    }
}