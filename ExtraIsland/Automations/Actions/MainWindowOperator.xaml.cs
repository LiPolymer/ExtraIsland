using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;
using MahApps.Metro.Controls;

namespace ExtraIsland.Automations.Actions;

public partial class MainWindowOperator {
    public MainWindowOperator() {
        InitializeComponent();
    }

    public List<OperationType> OperationTypes { get; } = [
        OperationType.DirectlyHide,
        OperationType.DirectlyShow
    ];
    
    public static void Action(object? config, string _) {
        GlobalConstants.Handlers.MainWindow!.MainWindow.BeginInvoke(() => {
            switch (((MainWindowOperatorConfig)config!).OperationType) {
                case OperationType.DirectlyHide:
                    GlobalConstants.Handlers.MainWindow!.MainWindow.Hide();
                    break;
                case OperationType.DirectlyShow:
                    GlobalConstants.Handlers.MainWindow!.MainWindow.Show();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            } 
        });
    }

    public static void Revert(object? config, string _) {
        GlobalConstants.Handlers.MainWindow!.MainWindow.BeginInvoke(() => {
            switch (((MainWindowOperatorConfig)config!).OperationType) {
                case OperationType.DirectlyHide:
                    GlobalConstants.Handlers.MainWindow!.MainWindow.Show();
                    break;
                case OperationType.DirectlyShow:
                    GlobalConstants.Handlers.MainWindow!.MainWindow.Hide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        });
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class MainWindowOperatorConfig : ObservableRecipient {
    public OperationType OperationType { get; set; } = OperationType.DirectlyHide;
}

public enum OperationType {
    [Description("窗口级隐藏")]
    DirectlyHide,
    [Description("窗口级显示")]
    DirectlyShow
}