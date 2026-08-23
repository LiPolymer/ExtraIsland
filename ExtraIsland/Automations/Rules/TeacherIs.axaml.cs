using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Automations.Rules;
public partial class TeacherIs: RuleSettingsControlBase<TeacherIsConfig> {
    public TeacherIs() {
        InitializeComponent();
    }
    
    public static bool NextRule(object? rawConfig,ILessonsService lessonsService) {
        return lessonsService.NextClassSubject.TeacherName
               == ((TeacherIsConfig)rawConfig!).Teacher;
    }
    
    public static bool CurrentRule(object? rawConfig,ILessonsService lessonsService) {
        return lessonsService.CurrentSubject?.TeacherName
               == ((TeacherIsConfig)rawConfig!).Teacher;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public partial class TeacherIsConfig : ObservableRecipient {
    [ObservableProperty]
    string _teacher = "";
}
