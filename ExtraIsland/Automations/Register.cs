using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Extensions.Registry;
using ExtraIsland.Automations.Actions;
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
            "extraisland.action.mainWindowOperator", 
            "主窗口操作器", 
            PackIconKind.DesktopWindows);
    }
    
    /// <summary>
    /// 注册处理逻辑
    /// </summary>
    /// <param name="actionService">行动服务</param>
    /// <param name="rulesetService">规则集服务</param>
    public Register(IActionService actionService, IRulesetService rulesetService) {
        //行动
        actionService.RegisterActionHandler("extraisland.action.mainWindowOperator",MainWindowOperator.Action);
        actionService.RegisterRevertHandler("extraisland.action.mainWindowOperator",MainWindowOperator.Revert);
    }
    
    public Task StartAsync(CancellationToken _) {
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken _) {
        return Task.CompletedTask;
    }
}