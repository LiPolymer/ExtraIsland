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
    public DutySettingsPage() {
        Settings = GlobalConstants.Handlers.OnDuty!;
        InitializeComponent();
        UpdateOnDuty();
        Settings.OnDutyUpdated += UpdateOnDuty;
        #if DEBUG
            DebugSwapButton.Visibility = Visibility.Visible;
        #endif
    }

    public OnDutyPersistedConfigHandler Settings { get; set; }
    
    public string PeopleOnDuty { get; set; } = string.Empty;

    void DutySettingsPage_OnUnloaded(object sender,RoutedEventArgs e) {
        Settings.OnDutyUpdated -= UpdateOnDuty;
        Settings.Save();
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
        Settings.SwapOnDuty();
    }
    void AutoSort_OnClick(object sender,RoutedEventArgs e) {
        Settings.SortCollectionByIndex();
        PeopleDataGrid.ItemsSource = Settings.Data.Peoples;
    }
}