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
using SuperAutoIsland.Interface;
using SuperAutoIsland.Interface.MetaData;
using SuperAutoIsland.Interface.MetaData.ArgsType;
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
                // 注册 sai 元素
                RegisterData regData = new RegisterData {
                    Actions = [
                        new BlockMetadata {
                            Id = "extraIsland.action.setFlag",
                            Name = "设定标志",
                            Icon = ("设标志","\uE844"),
                            Args = new Dictionary<string,MetaArgsBase> {
                                ["TargetFlag"] = new CommonMetaArgs {
                                    Name = "ID",
                                    Type = MetaType.text
                                },
                                ["FlagContent"] = new CommonMetaArgs {
                                    Name = "标志",
                                    Type = MetaType.text
                                },
                                ["IsPersisted"] = new CommonMetaArgs {
                                    Name = "持久化",
                                    Type = MetaType.boolean
                                },
                                ["WillNotifyUpdate"] = new CommonMetaArgs {
                                    Name = "立即更新规则",
                                    Type = MetaType.boolean
                                }
                            },
                            DropdownUseNumbers = false,
                            InlineField = false,
                            InlineBlock = false
                        },
                        new BlockMetadata {
                            Id = "extraIsland.action.doSpeech",
                            Name = "语音播报",
                            Icon = ("语音播报", "\uE5C7"),
                            Args = new Dictionary<string,MetaArgsBase> {
                                ["Text"] = new CommonMetaArgs {
                                    Name = "要播报的文本",
                                    Type = MetaType.text
                                }
                            },
                            DropdownUseNumbers = false,
                            InlineField = false,
                            InlineBlock = false
                        }
                    ],
                    Rules = [
                        new BlockMetadata {
                            Id = "extraIsland.rule.isDoubleLesson",
                            Name = "下节课是连堂",
                            Icon = ("连堂","\uE2AC"),
                            Args = [],
                            DropdownUseNumbers = false,
                            InlineField = false,
                            InlineBlock = false
                        },
                        new BlockMetadata {
                            Id = "extraIsland.rule.currentTeacherIs",
                            Name = "当前教师是",
                            Icon = ("当前教师是","\uECF9"),
                            Args = new Dictionary<string,MetaArgsBase> {
                                ["Teacher"] = new CommonMetaArgs {
                                    Name = "",
                                    Type = MetaType.text
                                }
                            },
                            DropdownUseNumbers = false,
                            InlineField = false,
                            InlineBlock = false
                        },
                        new BlockMetadata {
                            Id = "extraIsland.rule.nextTeacherIs",
                            Name = "下节课教师是",
                            Icon = ("下节课教师是","\uECF7"),
                            Args = new Dictionary<string,MetaArgsBase> {
                                ["Teacher"] = new CommonMetaArgs {
                                    Name = "",
                                    Type = MetaType.text
                                }
                            },
                            DropdownUseNumbers = false,
                            InlineField = false,
                            InlineBlock = false
                        },
                        new BlockMetadata {
                            Id = "extraIsland.rule.flagIs",
                            Name = "标志是",
                            Icon = ("读标志","\uE844"),
                            Args = new Dictionary<string,MetaArgsBase> {
                                ["TargetFlag"] = new CommonMetaArgs {
                                    Name = "ID",
                                    Type = MetaType.text
                                },
                                ["FlagContent"] = new CommonMetaArgs {
                                    Name = "期望标志",
                                    Type = MetaType.text
                                }
                            },
                            DropdownUseNumbers = false,
                            InlineField = false,
                            InlineBlock = false
                        }
                    ],
                    Data = [
                        new BlockMetadata
                        {
                            Id = "extraIsland.data.getFlag",
                            Name = "读标志",
                            Icon = ("读标志","\uE844"),
                            Args = new Dictionary<string,MetaArgsBase> {
                                ["TargetFlag"] = new CommonMetaArgs {
                                    Name = "ID",
                                    Type = MetaType.text
                                }
                            },
                            DropdownUseNumbers = false,
                            InlineField = false,
                            InlineBlock = false
                        }
                    ]
                };
                if (EiUtils.IsPluginInstalled("IslandCaller.Plugin2")) {
                    regData.Actions.Add(new BlockMetadata {
                        Id = "extraIsland.action.islandCaller",
                        Name = "拉起IslandCaller",
                        Icon = ("拉起IslandCaller", "\uECB5"),
                        Args = [],
                        DropdownUseNumbers = false,
                        InlineField = false,
                        InlineBlock = false
                    });
                    if (GlobalConstants.Handlers.MainConfig!.Data.IsExperimentalModeActivated) {
                        regData.Actions.Add(new BlockMetadata {
                            Id = "extraIsland.action.islandCallerAdvanced",
                            Name = "(实验性)拉起IslandCaller-高级",
                            Icon = ("拉起IslandCaller", "\uECB5"),
                            Args = [],
                            DropdownUseNumbers = false,
                            InlineField = false,
                            InlineBlock = false
                        });
                    }
                }
                saiServerService.RegisterBlocks("ExtraIsland", regData);
                
                saiServerService.RegisterDataGetter<GetFlagConfig>("extraIsland.data.getFlag",data => {
                    if (data is not GetFlagConfig config) {
                        return Task.FromResult("???");
                    }
                    Dictionary<string,string> merged = GlobalConstants.Handlers.PersistedFlagHandler?.FlagsTable != null
                        ? new Dictionary<string, string>(GlobalConstants.Handlers.PersistedFlagHandler.FlagsTable)
                        : [];
                    foreach (KeyValuePair<string,string> kv in Flag.Flags)
                        merged[kv.Key] = kv.Value; // 内存标志覆盖持久化标志
                    return Task.FromResult(merged.GetValueOrDefault(config.TargetFlag,"[未设置值]"));
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