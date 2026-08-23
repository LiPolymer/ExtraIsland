namespace ExtraIsland.Shared;

/// <summary>
/// 名句来源注册表
/// </summary>
public interface IRhesisProviderRegistry {
    IReadOnlyList<IRhesisProvider> Providers { get; }
}

/// <summary>
/// 名句来源注册表:自动收集 DI 中注册的 <see cref="IRhesisProvider"/>
/// </summary>
public class RhesisProviderRegistry : IRhesisProviderRegistry {
    readonly Dictionary<string,IRhesisProvider> _registeredProviders =
        new(StringComparer.OrdinalIgnoreCase);

    public RhesisProviderRegistry(IEnumerable<IRhesisProvider> providers) {
        foreach (IRhesisProvider provider in providers) {
            if (string.IsNullOrWhiteSpace(provider.Id)) continue;
            if (provider.DefaultWeight < 0) continue;
            _registeredProviders.TryAdd(provider.Id,provider);
        }
    }

    public IReadOnlyList<IRhesisProvider> Providers {
        get {
            lock (_registeredProviders) {
                return _registeredProviders.Values.ToArray();
            }
        }
    }
}
