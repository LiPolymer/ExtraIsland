using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Shared;
using ExtraIsland.Automations.Actions;
using ExtraIsland.Automations.Data;
using ExtraIsland.Automations.Rules;
using ExtraIsland.Automations.Triggers;
using ExtraIsland.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperAutoIsland.Interface.Metadata;
using SuperAutoIsland.Interface.Services;

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
        services.AddAction<SetFlagAction,SetFlag>();
        services.AddAction<UpdateRuleAction,Actions.EmptySettings>();
        if (EiUtils.IsPluginInstalled("IslandCaller.Plugin2")) {
            services.AddAction<IslandCallerAction,Actions.EmptySettings>();
            if (GlobalConstants.Handlers.MainConfig!.Data.IsExperimentalModeActivated) {
                services.AddAction<IslandCallerAdvancedAction,Actions.EmptySettings>();
            }
        }
        services.AddAction<DoSpeechAction,DoSpeechSettingsControl>();
        services.AddAction<DutyNotifyAction,DutyNotifySettingsControl>();

        // 规则
        services.AddRule<TodayIsConfig,TodayIs>
            ("extraIsland.rule.todayIs","今天是","\uE304");
        services.AddRule<LaterThanConfig,LaterThan>
            ("extraIsland.rule.laterThan","时间晚于","\uE4D4");
        services.AddRule<TeacherIsConfig,TeacherIs>
            ("extraIsland.rule.currentTeacherIs","当前教师是","\uECF9");
        services.AddRule<TeacherIsConfig,TeacherIs>
            ("extraIsland.rule.nextTeacherIs","下节课教师是","\uECF7");
        services.AddRule<FlagIsConfig,FlagIs>
            ("extraIsland.rule.flagIs","读标志","\uE844");
        services.AddRule<RulesDummyConfig,Rules.EmptySettings>
            ("extraIsland.rule.isDoubleLesson","下节课连堂","\uE2AC");
        // 触发器
        services.AddTrigger<TimePassed,TimePassedSettings>();


        //SAI Compactability
        if (EiUtils.IsPluginInstalled("lrs2187.sai")) {
            AppBase.Current.AppStarted += (_,_) => {
                GlobalConstants.HostInterfaces.PluginLogger?.LogInformation("SAI 已载入 正在注册 Blocky 元素");
                ISaiServer saiServerService = IAppHost.GetService<ISaiServer>();
                // 注册 sai 元素 (v2 接口)
                saiServerService.RegisterBlocks("ExtraIsland",it => {
                    it.AddLabel("行动")
                        .AddBlock(new BlockMetadata("extraIsland.action.setFlag") {
                            Kind = BlockKind.Action,
                            Name = "设定标志",
                            Icon = ("设标志","\uE844"),
                            Fields = new Dictionary<string,Field> {
                                ["TargetFlag"] = BasicFields.Text("ID"),
                                ["FlagContent"] = BasicFields.Text("标志"),
                                ["IsPersisted"] = BasicFields.Boolean("持久化"),
                                ["WillNotifyUpdate"] = BasicFields.Boolean("立即更新规则")
                            }
                        })
                        .AddBlock(new BlockMetadata("extraIsland.action.doSpeech") {
                            Kind = BlockKind.Action,
                            Name = "语音播报",
                            Icon = ("语音播报","\uED53"),
                            Fields = new Dictionary<string,Field> {
                                ["Text"] = BasicFields.Text("要播报的文本")
                            }
                        });
                    if (EiUtils.IsPluginInstalled("IslandCaller.Plugin2")) {
                        it.AddBlock(new BlockMetadata("extraIsland.action.islandCaller") {
                            Kind = BlockKind.Action,
                            Name = "拉起IslandCaller",
                            Icon = ("拉起IslandCaller","\uECB5"),
                            Fields = new Dictionary<string,Field>()
                        });
                        if (GlobalConstants.Handlers.MainConfig!.Data.IsExperimentalModeActivated) {
                            it.AddBlock(new BlockMetadata("extraIsland.action.islandCallerAdvanced") {
                                Kind = BlockKind.Action,
                                Name = "(实验性)拉起IslandCaller-高级",
                                Icon = ("拉起IslandCaller","\uECB5"),
                                Fields = new Dictionary<string,Field>()
                            });
                        }
                    }
                    it.AddLabel("规则")
                        .AddBlock(new BlockMetadata("extraIsland.rule.isDoubleLesson") {
                            Kind = BlockKind.Rule,
                            Name = "下节课是连堂",
                            Icon = ("连堂","\uE2AC"),
                            Fields = new Dictionary<string,Field>()
                        })
                        .AddBlock(new BlockMetadata("extraIsland.rule.currentTeacherIs") {
                            Kind = BlockKind.Rule,
                            Name = "当前教师是",
                            Icon = ("当前教师是","\uECF9"),
                            Fields = new Dictionary<string,Field> {
                                ["Teacher"] = BasicFields.Text("")
                            }
                        })
                        .AddBlock(new BlockMetadata("extraIsland.rule.nextTeacherIs") {
                            Kind = BlockKind.Rule,
                            Name = "下节课教师是",
                            Icon = ("下节课教师是","\uECF7"),
                            Fields = new Dictionary<string,Field> {
                                ["Teacher"] = BasicFields.Text("")
                            }
                        })
                        .AddBlock(new BlockMetadata("extraIsland.rule.flagIs") {
                            Kind = BlockKind.Rule,
                            Name = "标志是",
                            Icon = ("读标志","\uE844"),
                            Fields = new Dictionary<string,Field> {
                                ["TargetFlag"] = BasicFields.Text("ID"),
                                ["FlagContent"] = BasicFields.Text("期望标志")
                            }
                        });
                    it.AddLabel("数据")
                        .AddBlock<GetFlagBlock>()
                        .AddBlock<GetOnDutyBlock>()
                        .AddBlock<GetRhesisBlock>();
                });
            };
        }
    }

    /// <summary>
    /// 注册处理逻辑
    /// </summary>
    /// <param name="rulesetService">规则集服务</param>
    public Register(IRulesetService rulesetService) {
        //规则
        rulesetService.RegisterRuleHandler("extraIsland.rule.todayIs",TodayIs.Rule);
        rulesetService.RegisterRuleHandler("extraIsland.rule.laterThan",LaterThan.Rule);
        rulesetService.RegisterRuleHandler("extraIsland.rule.flagIs",FlagIs.Rule);
        rulesetService.RegisterRuleHandler("extraIsland.rule.currentTeacherIs",TeacherIs.CurrentRule);
        rulesetService.RegisterRuleHandler("extraIsland.rule.nextTeacherIs",TeacherIs.NextRule);
        rulesetService.RegisterRuleHandler("extraIsland.rule.isDoubleLesson",IsDoubleLesson.Rule);
    }

    public Task StartAsync(CancellationToken _) {
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken _) {
        return Task.CompletedTask;
    }
}