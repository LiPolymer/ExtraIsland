using ClassIsland.Shared.Models.Action;
using System.Windows.Media;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public class ActionButtonConfig {
    public ActionSet ActionSet { get; set; } = new ActionSet();
    
    public string ContentString { get; set; } = "新按钮";
    public Color FontColor { get; set; } = Color.FromRgb(0, 0, 0);
    public Color BackgroundColor { get; set; } = Color.FromRgb(0, 200, 255);
    public Color BorderColor { get; set; } = Color.FromRgb(0, 170, 200);
}