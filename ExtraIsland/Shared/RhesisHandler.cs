using Microsoft.Extensions.Logging;

namespace ExtraIsland.Shared;

public static class RhesisHandler {
    static readonly Lock ProvidersLock = new Lock();
    static readonly Dictionary<string,IRhesisProvider> RegisteredProviders = new Dictionary<string,IRhesisProvider>(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<IRhesisProvider> Providers {
        get {
            lock (ProvidersLock) {
                return RegisteredProviders.Values.ToArray();
            }
        }
    }

    public static bool RegisterProvider(IRhesisProvider provider) {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Id)) {
            throw new ArgumentException("名句来源必须提供非空 Id。",nameof(provider));
        }
        if (provider.DefaultWeight < 0) {
            throw new ArgumentException("名句来源的默认权重不能小于 0。",nameof(provider));
        }

        lock (ProvidersLock) {
            return RegisteredProviders.TryAdd(provider.Id,provider);
        }
    }

    public static bool UnregisterProvider(string providerId) {
        lock (ProvidersLock) {
            return RegisteredProviders.Remove(providerId);
        }
    }

    public class Instance {
        public async Task<RhesisData> GetAsync(
            IReadOnlyDictionary<string,RhesisProviderConfig> providerConfigs,
            int lengthLimitation = 0,
            CancellationToken cancellationToken = default) {
            List<(IRhesisProvider Provider,RhesisProviderConfig Config)> candidates = Providers
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
                    GlobalConstants.HostInterfaces.PluginLogger?.LogWarning(
                        ex,
                        "从名句来源 {ProviderId} 获取内容时发生错误",
                        provider.Id);
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
}

public class RhesisData {
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Catalog { get; set; } = string.Empty;
}
