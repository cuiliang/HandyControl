using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using HandyControl.Data;
using HandyControl.Interactivity;

namespace HandyControl.Controls;

/// <summary>
///     切换块
/// </summary>
public class ToggleBlock : Control
{
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(nameof(IsChecked), typeof(bool?), typeof(ToggleBlock), new FrameworkPropertyMetadata(ValueBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));
    public static readonly DependencyProperty CheckedContentProperty = DependencyProperty.Register(nameof(CheckedContent), typeof(object), typeof(ToggleBlock), new PropertyMetadata(default(object)));
    public static readonly DependencyProperty UnCheckedContentProperty = DependencyProperty.Register(nameof(UnCheckedContent), typeof(object), typeof(ToggleBlock), new PropertyMetadata(default(object)));
    public static readonly DependencyProperty IndeterminateContentProperty = DependencyProperty.Register(nameof(IndeterminateContent), typeof(object), typeof(ToggleBlock), new PropertyMetadata(default(object)));
    public static readonly DependencyProperty ToggleGestureProperty = DependencyProperty.Register(nameof(ToggleGesture), typeof(MouseGesture), typeof(ToggleBlock), new UIPropertyMetadata(new MouseGesture(MouseAction.None), OnToggleGestureChanged));

    //private MouseBinding _toggleBinding;

    [Category("Appearance")]
    [TypeConverter(typeof(NullableBoolConverter))]
    [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
    public bool? IsChecked
    {
        get
        {
            // Because Nullable<bool> unboxing is very slow (uses reflection) first we cast to bool
            var value = GetValue(IsCheckedProperty);
            // ReSharper disable once RedundantExplicitNullableCreation
            return value == null ? new bool?() : new bool?((bool) value);
        }
        set => SetValue(IsCheckedProperty, value.HasValue ? ValueBoxes.BooleanBox(value.Value) : null);
    }

    public object CheckedContent
    {
        get => GetValue(CheckedContentProperty);
        set => SetValue(CheckedContentProperty, value);
    }

    public object UnCheckedContent
    {
        get => GetValue(UnCheckedContentProperty);
        set => SetValue(UnCheckedContentProperty, value);
    }

    public object IndeterminateContent
    {
        get => GetValue(IndeterminateContentProperty);
        set => SetValue(IndeterminateContentProperty, value);
    }

    [ValueSerializer(typeof(MouseGestureValueSerializer))]
    [TypeConverter(typeof(MouseGestureConverter))]
    public MouseGesture ToggleGesture
    {
        get => (MouseGesture) GetValue(ToggleGestureProperty);
        set => SetValue(ToggleGestureProperty, value);
    }

    public ToggleBlock()
    {
        //CommandBindings.Add(new CommandBinding(ControlCommands.Toggle, OnToggled));
        OnToggleGestureChanged(ToggleGesture);


        //Unloaded += OnUnloaded;
    }

    // 是否开启鼠标按下时切换。使用此方式主要是为了解决MouseGesture触发不稳定的问题。
    private bool _toggleOnMouseDown = false;

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (_toggleOnMouseDown)
        {
            SetCurrentValue(IsCheckedProperty, IsChecked == true ? ValueBoxes.FalseBox : ValueBoxes.TrueBox);
        }
    }

    //private void OnUnloaded(object sender, RoutedEventArgs e)
    //{
    //    this.Unloaded -= OnUnloaded;
    //    //InputBindings.Clear();
    //}

    private static void OnToggleGestureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ToggleBlock) d).OnToggleGestureChanged((MouseGesture) e.NewValue);
    }

    private void OnToggleGestureChanged(MouseGesture newValue)
    {
        if (newValue.MouseAction != MouseAction.None)
        {
            _toggleOnMouseDown = true;
        }
        else
        {
            _toggleOnMouseDown = false;
        }
        /*
        InputBinding bindingToRemove = null;
        foreach (InputBinding binding in InputBindings)
        {
            if (binding is MouseBinding mouseBinding && mouseBinding.Command == ControlCommands.Toggle)
            {
                bindingToRemove = binding;
                break;
            }
        }

        if (bindingToRemove != null)
        {
            InputBindings.Remove(bindingToRemove);
        }

        if (newValue.MouseAction != MouseAction.None)
        {
            var toggleBinding = new MouseBinding(ControlCommands.Toggle, newValue);
            InputBindings.Add(toggleBinding);
        }
       */
    }


    //private void OnToggled(object sender, ExecutedRoutedEventArgs e)
    //{
    //    SetCurrentValue(IsCheckedProperty, IsChecked == true ? ValueBoxes.FalseBox : ValueBoxes.TrueBox);
    //}
}
