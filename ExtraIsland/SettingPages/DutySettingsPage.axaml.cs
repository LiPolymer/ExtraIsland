using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Helpers.UI;
using ClassIsland.Platforms.Abstraction;
using ExtraIsland.ConfigHandlers;
using ExtraIsland.Shared;

namespace ExtraIsland.SettingPages;

[HidePageTitle]
[SettingsPageInfo("extraisland.duty","值日","\uECDB","\uECDA")]
public partial class DutySettingsPage : SettingsPageBase {
    bool _isUpdatingHolidayInfo = false; // 防止循环更新的标志
    public OnDutyPersistedConfigHandler Settings { get; }
    
    public List<OnDutyPersistedConfigData.DutyStateData> DutyStates { get; } = [
        OnDutyPersistedConfigData.DutyStateData.Single,
        OnDutyPersistedConfigData.DutyStateData.Double,
        OnDutyPersistedConfigData.DutyStateData.InOut,
        OnDutyPersistedConfigData.DutyStateData.Quadrant
    ];

    public DutySettingsPage() {
        Settings = GlobalConstants.Handlers.OnDuty!;
        InitializeComponent();
        
        UpdateOnDuty();
        UpdateHolidayInfo();
        Settings.OnDutyUpdated += UpdateOnDuty;
        Settings.Data.PropertyChanged += OnDataPropertyChanged;
        
#if DEBUG
        DebugSwapButton.IsVisible = true;
#endif
    }
    
    void DutySettingsPage_OnUnloaded(object sender, RoutedEventArgs e) {
        Settings.OnDutyUpdated -= UpdateOnDuty;
        Settings.Data.PropertyChanged -= OnDataPropertyChanged;
        Settings.Save();
    }
    
    void OnDataPropertyChanged() {
        // 防止循环更新，只在节假日功能开启且当前没有正在更新时才执行
        if (Settings.Data.IsHolidaySkipEnabled && !_isUpdatingHolidayInfo) {
            UpdateHolidayInfo();
        }
    }

    void PeopleDataGrid_OnCurrentCellChanged(object? sender, EventArgs e) {
        Settings.Save();
    }
    
    void DeleteButton_Click(object sender, RoutedEventArgs e) {
        Button button = (sender as Button)!;
        if (button.DataContext is OnDutyPersistedConfigData.PeopleItem peopleItem) {
            Settings.Data.Peoples.Remove(peopleItem);
        }
    }

    void AddButton_Click(object sender, RoutedEventArgs e) {
        Settings.Data.Peoples.Add(new OnDutyPersistedConfigData.PeopleItem {
            Index = Settings.Data.Peoples.Count,
            Name = "新同学"
        });
        Settings.Save();
    }

    void UpdateOnDuty() {
        Dispatcher.UIThread.Invoke(() => {
            IndexOnDutyLabel.Content = Settings.Data.CurrentPeopleIndex.ToString();
            PeopleOnDutyLabel.Content = Settings.PeoplesOnDutyString;
            LastUpdateLabel.Content = Settings.LastUpdateString;
        });
    }
    
    /// <summary>
    /// 更新节假日相关信息显示
    /// </summary>
    void UpdateHolidayInfo() {
        if (!Settings.Data.IsHolidaySkipEnabled || _isUpdatingHolidayInfo) {
            return;
        }
        
        _isUpdatingHolidayInfo = true;
        
        try {
            // 异步获取下一个节假日信息
            _ = Task.Run(async () => {
                try {
                    (DateTime Date, string Name)? nextHoliday = await HolidayService.GetNextHolidayAsync(DateTime.Today);
                    
                    Dispatcher.UIThread.Invoke(() => {
                        try {
                            NextHolidayLabel.Text = nextHoliday.HasValue
                                ? $"即将跳过的节假日：{nextHoliday.Value.Name}"
                                : "即将跳过的节假日：暂无";
                        }
                        catch {
                            // UI已被释放时忽略错误
                        }
                    });
                }
                catch {
                    // 网络请求失败时的处理
                    Dispatcher.UIThread.Invoke(() => {
                        try {
                            NextHolidayLabel.Text = "即将跳过的节假日：获取失败";
                        }
                        catch {
                            // UI已被释放时忽略错误
                        }
                    });
                }
            });
            
            // 更新界面显示
            Dispatcher.UIThread.Invoke(() => {
                try {
                    // 更新上次跳过的节假日信息显示
                    LastSkippedHolidayLabel.Text = string.IsNullOrEmpty(Settings.Data.LastSkippedHoliday)
                        ? "上次跳过的节假日：暂无\n索引变化：- → -"
                        : $"上次跳过的节假日：{Settings.Data.LastSkippedHoliday}\n索引变化：{Settings.Data.LastSkippedOriginalIndex} → {Settings.Data.LastSkippedNewIndex}";
                    
                    // 更新下一个节假日信息显示
                    string nextHolidayName = string.IsNullOrEmpty(Settings.Data.NextHolidayName) ? "查询中..." : Settings.Data.NextHolidayName;
                    NextHolidayLabel.Text = $"即将跳过的节假日：{nextHolidayName}";
                }
                catch {
                    // UI已被释放时忽略错误
                }
            });
        }
        finally {
            _isUpdatingHolidayInfo = false;
        }
    }

    void ClearTimeButton_OnClick(object sender,RoutedEventArgs e) {
        Settings.Data.LastUpdate = Settings.Data.LastUpdate.Date;
    }
    
    async void ImportButton_OnClick(object sender,RoutedEventArgs e) {
        try
        {
            
            PopupHelper.DisableAllPopups();
            List<string> files = await PlatformServices.FilePickerService.OpenFilesPickerAsync(new FilePickerOpenOptions {
                FileTypeFilter = new List<FilePickerFileType> { FilePickerFileTypes.TextPlain }
            },TopLevel.GetTopLevel(this) ?? AppBase.Current.GetRootWindow());
            PopupHelper.RestoreAllPopups();
            
            if (files.Count == 0)
            {
                return;
            }
            
            string[] list = await File.ReadAllLinesAsync(files[0]);

            ObservableCollection<OnDutyPersistedConfigData.PeopleItem> peoples = [];
            int i = 0;
            foreach (string name in list) {
                peoples.Add(new OnDutyPersistedConfigData.PeopleItem {
                    Index = i,
                    Name = name
                });
                i++;
            }
            Settings.Data.Peoples = peoples;
            PeopleDataGrid.ItemsSource = Settings.Data.Peoples;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
    
    void AutoSort_OnClick(object sender,RoutedEventArgs e) {
        Settings.SortCollectionByIndex();
        PeopleDataGrid.ItemsSource = Settings.Data.Peoples;
    }
    
    void DebugButton_OnClick(object sender,RoutedEventArgs e) {
        if (Settings.Data.IsHolidaySkipEnabled) {
            // 使用带节假日跳过的轮换方法
            Task.Run(async () => {
                try {
                    await Settings.SwapOnDutyWithHolidaySkipAsync();
                    Dispatcher.UIThread.Invoke(() => {
                        Settings.UpdateOnDuty();
                        UpdateHolidayInfo();
                    });
                }
                catch {
                    // 忽略错误
                }
            });
        } else {
            // 使用传统轮换方法
            Settings.SwapOnDuty();
        }
    }
}