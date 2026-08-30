using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Helpers.UI;
using ClassIsland.Platforms.Abstraction;
using ExtraIsland.Shared;
using MessageBox = System.Windows.MessageBox;

namespace ExtraIsland.Components;

public partial class RhesisSettings : ComponentBase<RhesisConfig> {
    const long MaxImportFileSizeBytes = 1024 * 1024;
    const int MaxImportEntries = 5000;
    const int MaxEntryLength = 200;
    public RhesisSettings() {
        InitializeComponent();
        AttachedToVisualTree += (_,_) => RefreshProviderItems();
    }

    public ObservableCollection<RhesisProviderSettingsItem> ProviderItems { get; } = [];

    public List<RhesisConfig.AttributesDisplayRule> AttributesRules { get; } = [
        RhesisConfig.AttributesDisplayRule.Sametime,
        RhesisConfig.AttributesDisplayRule.Separate
    ];

    void RefreshProviderItems() {
        Settings.EnsureProviderSettings(RhesisHandler.Providers);
        ProviderItems.Clear();
        foreach (IRhesisProvider provider in RhesisHandler.Providers) {
            ProviderItems.Add(new RhesisProviderSettingsItem(
                provider,
                Settings.ProviderSettings[provider.Id]));
        }
    }
    async void ImportIgnoreListButton_OnClick(object sender,RoutedEventArgs e) {
        try {
            PopupHelper.DisableAllPopups();
            List<string> files = await PlatformServices.FilePickerService.OpenFilesPickerAsync(new FilePickerOpenOptions {
                Title = "导入排除列表",
                FileTypeFilter = [FilePickerFileTypes.TextPlain]
            },TopLevel.GetTopLevel(this) ?? AppBase.Current.GetRootWindow());
            PopupHelper.RestoreAllPopups();

            if (files.Count == 0) return;

            string path = files[0];
            if (new FileInfo(path).Length > MaxImportFileSizeBytes) {
                MessageBox.Show($"导入失败: 文件大小超过 {MaxImportFileSizeBytes / 1024} KB 限制","导入排除列表");
                return;
            }

            List<string> entries = Settings.IgnoreListString
                .Split("\r\n",StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            foreach (string line in await File.ReadAllLinesAsync(path)) {
                string entry = line.Trim();
                if (entry.Length == 0) continue;
                if (entry.Length > MaxEntryLength) entry = entry[..MaxEntryLength];
                if (!entries.Contains(entry)) entries.Add(entry);
            }

            if (entries.Count > MaxImportEntries) {
                MessageBox.Show($"导入失败: 排除列表条目数超过 {MaxImportEntries} 条限制","导入排除列表");
                return;
            }

            Settings.IgnoreListString = string.Join("\r\n",entries);
        }
        catch (Exception exception) {
            PopupHelper.RestoreAllPopups();
            Console.WriteLine(exception);
        }
    }
}

public sealed class RhesisProviderSettingsItem(IRhesisProvider provider,RhesisProviderConfig configuration) {
    public string Id { get; } = provider.Id;
    public string DisplayName { get; } = provider.DisplayName;
    public string Description { get; } = provider.Description;
    public RhesisProviderConfig Configuration { get; } = configuration;
    public Control? SettingsControl { get; } = (provider as IRhesisProviderSettingsFactory)?.CreateSettingsControl(configuration);
    public bool HasSettings { get => SettingsControl != null; }
}
