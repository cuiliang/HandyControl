using System.Windows.Input;
using HandyControl.Interactivity;

namespace HandyControl.Controls;

public class TextBox : System.Windows.Controls.TextBox
{
    public TextBox()
    {
        CommandBindings.Add(new CommandBinding(ControlCommands.Clear, (s, e) =>
        {
            if (IsReadOnly)
            {
                return;
            }

            SetCurrentValue(TextProperty, string.Empty);
        }));
    }

    public bool ShowClearButton
    {
        get
        {
            return (bool) GetValue(InfoElement.ShowClearButtonProperty);
        }

        set
        {
            SetValue(InfoElement.ShowClearButtonProperty, value);
        }
    }
}
