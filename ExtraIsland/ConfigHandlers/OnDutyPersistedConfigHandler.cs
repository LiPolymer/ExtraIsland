using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json.Serialization;
using ClassIsland.Shared.Helpers;
using ExtraIsland.Shared;

namespace ExtraIsland.ConfigHandlers;

public class OnDutyPersistedConfigHandler {
    private bool _isSaving = false; // 防止递归保存的标志

    public OnDutyPersistedConfigHandler() {
        Data = new OnDutyPersistedConfigData();
        if (!File.Exists(Path.Combine(GlobalConstants.PluginConfigFolder!,"Persisted/OnDuty.json"))) {
            if (!Directory.Exists(Path.Combine(GlobalConstants.PluginConfigFolder!,"Persisted/"))) {
                Directory.CreateDirectory(Path.Combine(GlobalConstants.PluginConfigFolder!,"Persisted/"));
            }
            ConfigureFileHelper.SaveConfig<OnDutyPersistedConfigData>(
                Path.Combine(GlobalConstants.PluginConfigFolder!,"Persisted/OnDuty.json"),
                Data);
        }
        try {
            Data = ConfigureFileHelper.LoadConfig<OnDutyPersistedConfigData>(
                Path.Combine(GlobalConstants.PluginConfigFolder!,"Persisted/OnDuty.json"));
        }
        catch {
            File.Delete(Path.Combine(GlobalConstants.PluginConfigFolder!,"Persisted/OnDuty.json"));
            ConfigureFileHelper.SaveConfig<OnDutyPersistedConfigData>(
                Path.Combine(GlobalConstants.PluginConfigFolder!,"Persisted/OnDuty.json"),
                Data);
        }
        PeoplesOnDuty = Data.GetWhoOnDuty();
        GlobalConstants.Triggers.OnLoaded += () => {
            GlobalConstants.HostInterfaces.LessonsService!.PostMainTimerTicked += Updater;
        };
        Data.PropertyChanged += Save;
    }
    
    public void Save() {
        // 防止递归保存
        if (_isSaving) return;
        
        try {
            _isSaving = true;
            ConfigureFileHelper.SaveConfig<OnDutyPersistedConfigData>(
                Path.Combine(GlobalConstants.PluginConfigFolder!,"Persisted/OnDuty.json"),
                Data);
            UpdateOnDuty();
        }
        finally {
            _isSaving = false;
        }
    }

    public void UpdateOnDuty() {
        PeoplesOnDuty = Data.GetWhoOnDuty();
        OnDutyUpdated?.Invoke();
    }
    
    public List<OnDutyPersistedConfigData.PeopleItem> PeoplesOnDuty { get; set; }

    public string PeoplesOnDutyString {
        get {
            return Data.DutyState switch {
                OnDutyPersistedConfigData.DutyStateData.Single => PeoplesOnDuty[0].Name,
                OnDutyPersistedConfigData.DutyStateData.Double => $"{PeoplesOnDuty[0].Name} {PeoplesOnDuty[1].Name}",
                OnDutyPersistedConfigData.DutyStateData.InOut => $"内:{PeoplesOnDuty[0].Name} 外:{PeoplesOnDuty[1].Name}",
                OnDutyPersistedConfigData.DutyStateData.Quadrant => $"{PeoplesOnDuty[0].Name} {PeoplesOnDuty[1].Name} {PeoplesOnDuty[2].Name} {PeoplesOnDuty[3].Name}",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public OnDutyPersistedConfigData Data { get; set; }

    public string LastUpdateString {
        get => Data.LastUpdate.ToString(CultureInfo.InvariantCulture);
    }

    public void SortCollectionByIndex() {
        ObservableCollection<OnDutyPersistedConfigData.PeopleItem> newPeoplesList = [];
        int maxIndex = Data.Peoples.Count;
        int i = 0;
        for (int l = 0; l <= maxIndex; l++) {
            while (true) {
                OnDutyPersistedConfigData.PeopleItem? item = Data.Peoples.FirstOrDefault(p => p.Index == l);
                if (item is null) break;
                Data.Peoples.Remove(item);
                newPeoplesList.Add(new OnDutyPersistedConfigData.PeopleItem {
                    Index = i,
                    Name = item.Name
                });
                i++;
            }
        }
        Data.Peoples = newPeoplesList;
    }
    
    public event Action? OnDutyUpdated;
    
    async void Updater(object? sender,EventArgs eventArgs) {
        if (EiUtils.GetDateTimeSpan(Data.LastUpdate,DateTime.Now) < Data.DutyChangeDuration) return;
        
        if (Data.IsHolidaySkipEnabled) {
            await SwapOnDutyWithHolidaySkipAsync();
        } else {
            SwapOnDuty();
        }
        
        UpdateOnDuty();
    }

    public void SwapOnDuty() {
        Data.LastUpdate = Data.IsAutoShearEnabled switch {
            false => DateTime.Now,
            true => DateTime.Today
        };
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (Data.DutyState) {
            case OnDutyPersistedConfigData.DutyStateData.Double:
                Data.CurrentPeopleIndex += 2;
                break;
            case OnDutyPersistedConfigData.DutyStateData.Quadrant:
                Data.CurrentPeopleIndex += 4;
                break;
            default:
                Data.CurrentPeopleIndex++;
                break;
        }
        if (Data.CurrentPeopleIndex >= Data.Peoples.Count & Data.IsCycled) {
            Data.CurrentPeopleIndex = 0;
        }
    }

    /// <summary>
    /// 带节假日跳过功能的值日轮换
    /// </summary>
    public async Task SwapOnDutyWithHolidaySkipAsync() {
        var lastUpdateDate = Data.LastUpdate.Date;
        var currentDate = DateTime.Today;
        
        // 如果是同一天，不需要轮换
        if (lastUpdateDate == currentDate) {
            return;
        }
        
        // 计算需要处理的日期范围
        var datesToProcess = new List<DateTime>();
        for (var date = lastUpdateDate.AddDays(1); date <= currentDate; date = date.AddDays(1)) {
            datesToProcess.Add(date);
        }
        
        // 处理每一天的轮换
        int originalIndex = Data.CurrentPeopleIndex;
        string lastSkippedHoliday = "";
        
        foreach (var date in datesToProcess) {
            bool isHoliday = await HolidayService.IsHolidayAsync(date);
            
            if (isHoliday) {
                // 获取节假日信息
                var holidayInfo = await HolidayService.GetHolidayInfoAsync(date);
                lastSkippedHoliday = holidayInfo?.Name ?? "节假日";
            } else {
                // 非节假日，执行轮换
                IncrementDutyIndex();
            }
        }
        
        // 批量更新属性，减少PropertyChanged事件触发次数
        var originalSaving = _isSaving;
        _isSaving = true; // 临时阻止保存
        
        try {
            // 更新最后跳过的节假日信息
            if (!string.IsNullOrEmpty(lastSkippedHoliday)) {
                Data.LastSkippedHoliday = lastSkippedHoliday;
                Data.LastSkippedOriginalIndex = originalIndex;
                Data.LastSkippedNewIndex = Data.CurrentPeopleIndex;
            }
            
            // 更新最后更新时间
            Data.LastUpdate = Data.IsAutoShearEnabled ? DateTime.Today : DateTime.Now;
        }
        finally {
            _isSaving = originalSaving; // 恢复保存状态
        }
        
        // 手动触发一次保存
        if (!_isSaving) {
            Save();
        }
        
        // 异步获取下一个节假日
        _ = Task.Run(async () => {
            try {
                var nextHoliday = await HolidayService.GetNextHolidayAsync(DateTime.Today);
                if (nextHoliday.HasValue) {
                    // 只更新属性，不触发保存（避免频繁保存）
                    var wasSaving = _isSaving;
                    _isSaving = true;
                    try {
                        Data.NextHolidayName = nextHoliday.Value.Name;
                    }
                    finally {
                        _isSaving = wasSaving;
                    }
                }
            }
            catch {
                // 忽略获取下一个节假日时的错误
            }
        });
    }
    
    /// <summary>
    /// 增加值日索引
    /// </summary>
    private void IncrementDutyIndex() {
        switch (Data.DutyState) {
            case OnDutyPersistedConfigData.DutyStateData.Double:
                Data.CurrentPeopleIndex += 2;
                break;
            case OnDutyPersistedConfigData.DutyStateData.Quadrant:
                Data.CurrentPeopleIndex += 4;
                break;
            default:
                Data.CurrentPeopleIndex++;
                break;
        }
        
        if (Data.CurrentPeopleIndex >= Data.Peoples.Count && Data.IsCycled) {
            Data.CurrentPeopleIndex = 0;
        }
    }
}

//TODO: 从ObservableObject继承并重构此类
public class OnDutyPersistedConfigData {

    public event Action? PropertyChanged;

    ObservableCollection<PeopleItem> _peoples = [
        new PeopleItem { Index = 0,Name = "张三" },
        new PeopleItem { Index = 1,Name = "李四" }
    ];

    public ObservableCollection<PeopleItem> Peoples {
        get => _peoples;
        set {
            _peoples = value;
            PropertyChanged?.Invoke();
        }
    }

    DateTime _lastUpdate = DateTime.Today;
    public DateTime LastUpdate {
        get => _lastUpdate;
        set {
            _lastUpdate = value;
            PropertyChanged?.Invoke();
        }
    }

    bool? _doubleState;
    public bool? DoubleState {
        get => _doubleState;
        set {
            _doubleState = value;
            PropertyChanged?.Invoke();
        }
    }

    int _currentPeopleIndex;
    public int CurrentPeopleIndex {
        get => _currentPeopleIndex;
        set {
            _currentPeopleIndex = value;
            PropertyChanged?.Invoke();
        }
    }

    bool _isCycled = true;
    public bool IsCycled {
        get => _isCycled;
        set {
            _isCycled = value;
            PropertyChanged?.Invoke();
        }
    }
    
    bool _isAutoShearEnabled = true;
    public bool IsAutoShearEnabled {
        get => _isAutoShearEnabled;
        set {
            _isAutoShearEnabled = value;
            if (value) {
                LastUpdate = LastUpdate.Date;
            }
            PropertyChanged?.Invoke();
        }
    }

    DutyStateData _dutyState = DutyStateData.Single;
    public DutyStateData DutyState {
        get => _dutyState;
        set {
            _dutyState = value;
            PropertyChanged?.Invoke();
        }
    }

    //TODO:整合为n人值日
    public enum DutyStateData {
        [Description("单人值日")] 
        Single,
        [Description("双人值日")] 
        Double,
        [Description("内/外 双人轮换值日")] 
        InOut,
        [Description("(实验性)四人值日")] 
        Quadrant
    }

    TimeSpan _dutyChangeDuration = TimeSpan.FromDays(1);
    public TimeSpan DutyChangeDuration {
        get => _dutyChangeDuration;
        set {
            _dutyChangeDuration = value;
            PropertyChanged?.Invoke();
        }
    }

    [JsonIgnore]
    public double DutyChangeDurationDays {
        get => DutyChangeDuration.TotalDays;
        set => DutyChangeDuration = TimeSpan.FromDays(value);
    }

    // 节假日跳过功能相关属性
    bool _isHolidaySkipEnabled;
    public bool IsHolidaySkipEnabled {
        get => _isHolidaySkipEnabled;
        set {
            _isHolidaySkipEnabled = value;
            PropertyChanged?.Invoke();
        }
    }

    string _lastSkippedHoliday = string.Empty;
    public string LastSkippedHoliday {
        get => _lastSkippedHoliday;
        set {
            _lastSkippedHoliday = value;
            PropertyChanged?.Invoke();
        }
    }

    int _lastSkippedOriginalIndex;
    public int LastSkippedOriginalIndex {
        get => _lastSkippedOriginalIndex;
        set {
            _lastSkippedOriginalIndex = value;
            PropertyChanged?.Invoke();
        }
    }

    int _lastSkippedNewIndex;
    public int LastSkippedNewIndex {
        get => _lastSkippedNewIndex;
        set {
            _lastSkippedNewIndex = value;
            PropertyChanged?.Invoke();
        }
    }

    string _nextHolidayName = string.Empty;
    public string NextHolidayName {
        get => _nextHolidayName;
        set {
            _nextHolidayName = value;
            PropertyChanged?.Invoke();
        }
    }

    public List<PeopleItem> GetWhoOnDuty() {
        return DutyState switch {
            DutyStateData.Single => [
                GetPeopleOnDuty(CurrentPeopleIndex)
            ],
            DutyStateData.Double => EiUtils.IsOdd(CurrentPeopleIndex) switch {
                true => [
                    GetPeopleOnDuty(CurrentPeopleIndex - 1),
                    GetPeopleOnDuty(CurrentPeopleIndex)
                ],
                false => [
                    GetPeopleOnDuty(CurrentPeopleIndex),
                    GetPeopleOnDuty(CurrentPeopleIndex + 1)
                ]
            },
            DutyStateData.InOut => EiUtils.IsOdd(CurrentPeopleIndex) switch {
                true => [
                    GetPeopleOnDuty(CurrentPeopleIndex),
                    GetPeopleOnDuty(CurrentPeopleIndex - 1)
                ],
                false => [
                    GetPeopleOnDuty(CurrentPeopleIndex),
                    GetPeopleOnDuty(CurrentPeopleIndex + 1)
                ]
            },
            DutyStateData.Quadrant => EiUtils.IsOdd(CurrentPeopleIndex) switch {
                true => [
                    GetPeopleOnDuty(CurrentPeopleIndex - 1),
                    GetPeopleOnDuty(CurrentPeopleIndex),
                    GetPeopleOnDuty(CurrentPeopleIndex + 1),
                    GetPeopleOnDuty(CurrentPeopleIndex + 2)
                ],
                false => [
                    GetPeopleOnDuty(CurrentPeopleIndex),
                    GetPeopleOnDuty(CurrentPeopleIndex + 1),
                    GetPeopleOnDuty(CurrentPeopleIndex + 2),
                    GetPeopleOnDuty(CurrentPeopleIndex + 3)
                ]
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public PeopleItem GetPeopleOnDuty(int index) {
        PeopleItem? item = Peoples.FirstOrDefault(p => p.Index == index);
        item ??= new PeopleItem {
            Index = CurrentPeopleIndex,
            Name = "无值日生"
        };
        return item;
    }

    public class PeopleItem {
        public string Name { get; set; } = string.Empty;
        public int Index { get; set; }
    }
}