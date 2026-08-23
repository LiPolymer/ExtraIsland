using Microsoft.Extensions.Logging;

namespace ExtraIsland.Shared;

/// <summary>
/// 名句服务:按权重从已注册来源中获取一言
/// </summary>
public interface IRhesisService {
    Task<RhesisData> GetAsync(
        IReadOnlyDictionary<string,RhesisProviderConfig> providerConfigs,
        int lengthLimitation = 0,
        CancellationToken cancellationToken = default);
}

public class RhesisService : IRhesisService {
    readonly IRhesisProviderRegistry _registry;
    readonly ILogger<RhesisService> _logger;

    public RhesisService(IRhesisProviderRegistry registry,ILogger<RhesisService> logger) {
        _registry = registry;
        _logger = logger;
    }

    public async Task<RhesisData> GetAsync(
        IReadOnlyDictionary<string,RhesisProviderConfig> providerConfigs,
        int lengthLimitation = 0,
        CancellationToken cancellationToken = default) {
        List<(IRhesisProvider Provider,RhesisProviderConfig Config)> candidates = _registry.Providers
            .Select(provider => (
                Provider: provider,
                Config: providerConfigs.TryGetValue(provider.Id,out RhesisProviderConfig? config)
                    ? config
                    : new RhesisProviderConfig {
                        IsEnabled = provider.IsEnabledByDefault,
                        Weight = provider.DefaultWeight
                    }))
            .Where(item => item.Config.IsEnabled && item.Config.Weight > 0)
            .ToList();

        if (candidates.Count == 0) {
            return new RhesisData { Content = "未启用可用的名句来源" };
        }

        bool hasFetchError = false;
        for (int i = 0; i < 6; i++) {
            (IRhesisProvider provider,RhesisProviderConfig config) = SelectProvider(candidates);
            try {
                RhesisData dataFetched = await provider.FetchAsync(
                    config,
                    lengthLimitation,
                    cancellationToken);
                if (lengthLimitation == 0 || dataFetched.Content.Length <= lengthLimitation) {
                    return dataFetched;
                }
            } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                throw;
            } catch (Exception ex) {
                hasFetchError = true;
                _logger.LogWarning(ex,"从名句来源 {ProviderId} 获取内容时发生错误",provider.Id);
            }
        }

        return new RhesisData {
            Content = hasFetchError ? "获取时发生错误" : "满足限制时遇到困难"
        };
    }

    static (IRhesisProvider Provider,RhesisProviderConfig Config) SelectProvider(
        IReadOnlyList<(IRhesisProvider Provider,RhesisProviderConfig Config)> candidates) {
        long totalWeight = candidates.Sum(item => (long)item.Config.Weight);
        long selectedWeight = Random.Shared.NextInt64(totalWeight);
        foreach ((IRhesisProvider provider,RhesisProviderConfig config) in candidates) {
            if (selectedWeight < config.Weight) {
                return (provider,config);
            }
            selectedWeight -= config.Weight;
        }
        return candidates[^1];
    }
}
