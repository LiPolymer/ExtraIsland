using ClassIsland.Core.Abstractions.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static ExtraIsland.Shared.GlobalConstants;

namespace ExtraIsland.Shared;

public class ServicesFetcherService : IHostedService {
    public ServicesFetcherService(ILogger<ServicesFetcherService> selfLogger,
        ILessonsService lessonsService, 
        IExactTimeService exactTimeService, 
        ILogger<Plugin> logger, 
        IRulesetService rulesetService, 
        IProfileService profileService) {
        selfLogger.Log(LogLevel.Information, "正在获取服务...");
        HostInterfaces.LessonsService = lessonsService;
        HostInterfaces.ExactTimeService = exactTimeService;
        HostInterfaces.PluginLogger = logger;
        HostInterfaces.RulesetService = rulesetService;
        HostInterfaces.ProfileService = profileService;
        HostInterfaces.PluginLogger.Log(LogLevel.Information, "ExtraIsland 已载入!");
        Triggers.Loaded();
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}