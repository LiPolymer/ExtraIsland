using ClassIsland.Core.Abstractions.Services;
using ExtraIsland.ConfigHandlers;

namespace ExtraIsland.Shared;

public interface IFlagService {
    /// <summary>
    /// 获取合并后的标志表(内存标志覆盖持久化标志)
    /// </summary>
    IReadOnlyDictionary<string,string> GetMergedFlags();

    /// <summary>
    /// 读取指定标志的值,未设置时返回 <paramref name="fallback"/>
    /// </summary>
    string GetValue(string key,string fallback = "[未设置值]");

    bool TryGetValue(string key,out string? value);

    /// <summary>
    /// 设置标志的值
    /// </summary>
    /// <param name="persisted">是否写入持久化存储</param>
    void SetValue(string key,string value,bool persisted = false);

    /// <summary>
    /// 移除标志
    /// </summary>
    /// <param name="persisted">是否从持久化存储移除</param>
    void RemoveValue(string key,bool persisted = false);

    /// <summary>
    /// 通知规则集状态变更
    /// </summary>
    void NotifyStatusChanged();
}

/// <summary>
/// 标志服务:统一管理内存/持久化两个标志存储及合并逻辑
/// </summary>
public class FlagService : IFlagService {
    readonly Dictionary<string,string?> _memoryFlags = [];
    readonly PersistedFlagHandler _persistedHandler;
    readonly IRulesetService _rulesetService;

    public FlagService(PersistedFlagHandler persistedHandler,IRulesetService rulesetService) {
        _persistedHandler = persistedHandler;
        _rulesetService = rulesetService;
    }

    public IReadOnlyDictionary<string,string> GetMergedFlags() {
        Dictionary<string,string> merged = new Dictionary<string,string>(_persistedHandler.FlagsTable);
        foreach (KeyValuePair<string,string?> kv in _memoryFlags) {
            if (kv.Value is null) {
                merged.Remove(kv.Key);
            } else {
                merged[kv.Key] = kv.Value;
            }
        }
        return merged;
    }

    public string GetValue(string key,string fallback = "[未设置值]") {
        return GetMergedFlags().GetValueOrDefault(key,fallback);
    }

    public bool TryGetValue(string key,out string? value) {
        return GetMergedFlags().TryGetValue(key,out value);
    }

    public void SetValue(string key,string value,bool persisted = false) {
        if (persisted) {
            _persistedHandler.FlagsTable[key] = value;
            _persistedHandler.Save();
        } else {
            _memoryFlags[key] = value;
        }
        NotifyStatusChanged();
    }

    public void RemoveValue(string key,bool persisted = false) {
        if (persisted) {
            _persistedHandler.FlagsTable.Remove(key);
            _persistedHandler.Save();
        } else {
            _memoryFlags.Remove(key);
        }
        NotifyStatusChanged();
    }

    public void NotifyStatusChanged() {
        _rulesetService.NotifyStatusChanged();
    }
}
