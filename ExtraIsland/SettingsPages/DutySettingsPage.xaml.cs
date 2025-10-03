using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassIsland.Core.Attributes;
using ExtraIsland.ConfigHandlers;
using ExtraIsland.Shared;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace ExtraIsland.SettingsPages;

[SettingsPageInfo("extraisland.duty","ExtraIsland·值日",PackIconKind.UsersOutline,PackIconKind.Users)]
public partial class DutySettingsPage {
    private bool _isUpdatingHolidayInfo = false; // 防止循环更新的标志

    public DutySettingsPage() {
        Settings = GlobalConstants.Handlers.OnDuty!;
        InitializeComponent();
        UpdateOnDuty();
        UpdateHolidayInfo();
        Settings.OnDutyUpdated += UpdateOnDuty;
        Settings.Data.PropertyChanged += OnDataPropertyChanged;
        #if DEBUG
            DebugSwapButton.Visibility = Visibility.Visible;
        #endif
    }

    public OnDutyPersistedConfigHandler Settings { get; set; }
    
    public string PeopleOnDuty { get; set; } = string.Empty;

    void DutySettingsPage_OnUnloaded(object sender,RoutedEventArgs e) {
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
    
    void DataGrid_SelectedCellsChanged(object sender,SelectedCellsChangedEventArgs e) {
        Settings.Save();
    }
    void DeleteButton_Click(object sender,RoutedEventArgs e) {
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
        this.Invoke(() => {
            IndexOnDutyLabel.Content = Settings.Data.CurrentPeopleIndex.ToString();
            PeopleOnDutyLabel.Content = Settings.PeoplesOnDutyString;
            LastUpdateLabel.Content = Settings.LastUpdateString;
        });
    }
    
    /// <summary>
    /// 更新节假日相关信息显示
    /// </summary>
    async void UpdateHolidayInfo() {
        if (!Settings.Data.IsHolidaySkipEnabled || _isUpdatingHolidayInfo) {
            return;
        }
        
        _isUpdatingHolidayInfo = true;
        
        try {
            // 异步获取下一个节假日信息
            _ = Task.Run(async () => {
                try {
                    var nextHoliday = await HolidayService.GetNextHolidayAsync(DateTime.Today);
                    
                    this.BeginInvoke(() => {
                        try {
                            if (nextHoliday.HasValue) {
                                NextHolidayLabel.Text = $"即将跳过的节假日：{nextHoliday.Value.Name}";
                            } else {
                                NextHolidayLabel.Text = "即将跳过的节假日：暂无";
                            }
                        }
                        catch {
                            // UI已被释放时忽略错误
                        }
                    });
                }
                catch {
                    // 网络请求失败时的处理
                    this.BeginInvoke(() => {
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
            this.Invoke(() => {
                try {
                    // 更新上次跳过的节假日信息显示
                    if (string.IsNullOrEmpty(Settings.Data.LastSkippedHoliday)) {
                        LastSkippedHolidayLabel.Text = "上次跳过的节假日：暂无\n索引变化：- → -";
                    } else {
                        LastSkippedHolidayLabel.Text = $"上次跳过的节假日：{Settings.Data.LastSkippedHoliday}\n索引变化：{Settings.Data.LastSkippedOriginalIndex} → {Settings.Data.LastSkippedNewIndex}";
                    }
                    
                    // 更新下一个节假日信息显示
                    var nextHolidayName = string.IsNullOrEmpty(Settings.Data.NextHolidayName) ? "查询中..." : Settings.Data.NextHolidayName;
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
    
    [GeneratedRegex("[^0-9]+")]
    private static partial Regex NumberRegex();
    void TextBoxNumberCheck(object sender,TextCompositionEventArgs e) {
        Regex re = NumberRegex();
        e.Handled = re.IsMatch(e.Text);
    }
    
    public List<OnDutyPersistedConfigData.DutyStateData> DutyStates { get; } = [
        OnDutyPersistedConfigData.DutyStateData.Single,
        OnDutyPersistedConfigData.DutyStateData.Double,
        OnDutyPersistedConfigData.DutyStateData.InOut,
        OnDutyPersistedConfigData.DutyStateData.Quadrant
    ];

    void ClearTimeButton_OnClick(object sender,RoutedEventArgs e) {
        Settings.Data.LastUpdate = Settings.Data.LastUpdate.Date;
    }
    void ImportButton_OnClick(object sender,RoutedEventArgs e) {
        OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog {
            DefaultExt = ".txt",
            Filter = "文本文档 (.txt)|*.txt"
        };
        bool? result = dialog.ShowDialog();
        if (result != true) return;
        string[] list = File.ReadAllLines(dialog.FileName);
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
    void DebugButton_OnClick(object sender,RoutedEventArgs e) {
        if (Settings.Data.IsHolidaySkipEnabled) {
            // 使用带节假日跳过的轮换方法
            Task.Run(async () => {
                try {
                    await Settings.SwapOnDutyWithHolidaySkipAsync();
                    this.BeginInvoke(() => {
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
    void AutoSort_OnClick(object sender,RoutedEventArgs e) {
        Settings.SortCollectionByIndex();
        PeopleDataGrid.ItemsSource = Settings.Data.Peoples;
    }
}