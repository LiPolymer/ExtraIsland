using ClassIsland.Core.Abstractions.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Rules;
public partial class TeacherIs: RuleSettingsControlBase<TeacherIsConfig> {
    public TeacherIs() {
        InitializeComponent();
    }
    
    public static bool NextRule(object? rawConfig) {
        return GlobalConstants.HostInterfaces.LessonsService?.NextClassSubject.TeacherName
               == ((TeacherIsConfig)rawConfig!).Teacher;
    }
    
    public static bool CurrentRule(object? rawConfig) {
        return GlobalConstants.HostInterfaces.LessonsService?.CurrentSubject?.TeacherName
               == ((TeacherIsConfig)rawConfig!).Teacher;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public partial class TeacherIsConfig : ObservableRecipient {
    [ObservableProperty]
    string _teacher = "";
}