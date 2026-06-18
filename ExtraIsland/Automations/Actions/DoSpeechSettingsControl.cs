using Avalonia;
using Avalonia.Controls;
using ClassIsland.Core.Abstractions.Controls;

namespace ExtraIsland.Automations.Actions;

public class DoSpeechSettingsControl : ActionSettingsControlBase<DoSpeechSettings>
{
    private readonly TextBox _textBox;

    public DoSpeechSettingsControl()
    {
        var panel = new StackPanel { Spacing = 8, Margin = new Thickness(10) };

        panel.Children.Add(new TextBlock
        {
            Text = "语音播报",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            FontSize = 14
        });

        _textBox = new TextBox
        {
            Watermark = "请输入要播报的文字",
            AcceptsReturn = true,
            Height = 120,
            Width = 420
        };
        _textBox.TextChanged += (_, _) => { Settings.Text = _textBox.Text ?? string.Empty; };
        panel.Children.Add(_textBox);

        Content = panel;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _textBox.Text = Settings.Text;
    }
}

