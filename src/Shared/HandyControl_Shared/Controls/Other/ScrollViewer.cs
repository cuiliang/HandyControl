using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HandyControl.Data;
using HandyControl.Tools;

namespace HandyControl.Controls;

public class ScrollViewer : System.Windows.Controls.ScrollViewer
{
    private double _totalVerticalOffset;

    private double _totalHorizontalOffset;

    private bool _isRunning;

    /// <summary>
    ///     是否响应鼠标滚轮操作
    /// </summary>
    public static readonly DependencyProperty CanMouseWheelProperty = DependencyProperty.Register(
        nameof(CanMouseWheel), typeof(bool), typeof(ScrollViewer), new PropertyMetadata(ValueBoxes.TrueBox));

    /// <summary>
    ///     是否响应鼠标滚轮操作
    /// </summary>
    public bool CanMouseWheel
    {
        get => (bool) GetValue(CanMouseWheelProperty);
        set => SetValue(CanMouseWheelProperty, ValueBoxes.BooleanBox(value));
    }

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        // 当前滚动位置
        double offset = VerticalOffset;
        // 最大可滚动的距离
        double maxOffset = ScrollableHeight;

        // e.Delta > 0 表示向上滚动，< 0 表示向下滚动
        bool atTop = (offset <= 0);
        bool atBottom = (offset >= maxOffset);

        // 如果已经滚到顶还继续往上滚，或者已经滚到底还继续往下滚
        // 就把事件重新“扔”给外层去处理
        if ((e.Delta > 0 && atTop) || (e.Delta < 0 && atBottom))
        {
            e.Handled = true; // 阻止内层继续处理
            // 重新构造一个 MouseWheelEventArgs 并用 Bubble 的方式 Raise 到父元素
            var args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = e.Source
            };
            // 父级可能是 ListBox 的父容器，也可能是别的 UIElement
            (Parent as UIElement)?.RaiseEvent(args);
            return;
        }

        base.OnPreviewMouseWheel(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (!CanMouseWheel) return;

        if (!IsInertiaEnabled)
        {
            if (ScrollViewerAttach.GetOrientation(this) == Orientation.Vertical)
            {
                base.OnMouseWheel(e);
            }
            else
            {
                _totalHorizontalOffset = HorizontalOffset;
                CurrentHorizontalOffset = HorizontalOffset;
                _totalHorizontalOffset = Math.Min(Math.Max(0, _totalHorizontalOffset - e.Delta), ScrollableWidth);
                CurrentHorizontalOffset = _totalHorizontalOffset;
            }
            return;
        }
        e.Handled = true;

        if (ScrollViewerAttach.GetOrientation(this) == Orientation.Vertical)
        {
            if (!_isRunning)
            {
                _totalVerticalOffset = VerticalOffset;
                CurrentVerticalOffset = VerticalOffset;
            }
            _totalVerticalOffset = Math.Min(Math.Max(0, _totalVerticalOffset - e.Delta), ScrollableHeight);
            ScrollToVerticalOffsetWithAnimation(_totalVerticalOffset);
        }
        else
        {
            if (!_isRunning)
            {
                _totalHorizontalOffset = HorizontalOffset;
                CurrentHorizontalOffset = HorizontalOffset;
            }
            _totalHorizontalOffset = Math.Min(Math.Max(0, _totalHorizontalOffset - e.Delta), ScrollableWidth);
            ScrollToHorizontalOffsetWithAnimation(_totalHorizontalOffset);
        }
    }

    internal void ScrollToTopInternal(double milliseconds = 500)
    {
        if (!_isRunning)
        {
            _totalVerticalOffset = VerticalOffset;
            CurrentVerticalOffset = VerticalOffset;
        }
        ScrollToVerticalOffsetWithAnimation(0, milliseconds);
    }

    public void ScrollToVerticalOffsetWithAnimation(double offset, double milliseconds = 500)
    {
        var animation = AnimationHelper.CreateAnimation(offset, milliseconds);
        animation.EasingFunction = new CubicEase
        {
            EasingMode = EasingMode.EaseOut
        };
        animation.FillBehavior = FillBehavior.Stop;
        animation.Completed += (s, e1) =>
        {
            CurrentVerticalOffset = offset;
            _isRunning = false;
        };
        _isRunning = true;

        BeginAnimation(CurrentVerticalOffsetProperty, animation, HandoffBehavior.Compose);
    }

    public void ScrollToHorizontalOffsetWithAnimation(double offset, double milliseconds = 500)
    {
        var animation = AnimationHelper.CreateAnimation(offset, milliseconds);
        animation.EasingFunction = new CubicEase
        {
            EasingMode = EasingMode.EaseOut
        };
        animation.FillBehavior = FillBehavior.Stop;
        animation.Completed += (s, e1) =>
        {
            CurrentHorizontalOffset = offset;
            _isRunning = false;
        };
        _isRunning = true;

        BeginAnimation(CurrentHorizontalOffsetProperty, animation, HandoffBehavior.Compose);
    }

    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters) =>
        IsPenetrating ? null : base.HitTestCore(hitTestParameters);

    /// <summary>
    ///     是否支持惯性
    /// </summary>
    public static readonly DependencyProperty IsInertiaEnabledProperty = DependencyProperty.RegisterAttached(
        "IsInertiaEnabled", typeof(bool), typeof(ScrollViewer), new PropertyMetadata(ValueBoxes.FalseBox));

    public static void SetIsInertiaEnabled(DependencyObject element, bool value) => element.SetValue(IsInertiaEnabledProperty, ValueBoxes.BooleanBox(value));

    public static bool GetIsInertiaEnabled(DependencyObject element) => (bool) element.GetValue(IsInertiaEnabledProperty);

    /// <summary>
    ///     是否支持惯性
    /// </summary>
    public bool IsInertiaEnabled
    {
        get => (bool) GetValue(IsInertiaEnabledProperty);
        set => SetValue(IsInertiaEnabledProperty, ValueBoxes.BooleanBox(value));
    }

    /// <summary>
    ///     控件是否可以穿透点击
    /// </summary>
    public static readonly DependencyProperty IsPenetratingProperty = DependencyProperty.RegisterAttached(
        "IsPenetrating", typeof(bool), typeof(ScrollViewer), new PropertyMetadata(ValueBoxes.FalseBox));

    /// <summary>
    ///     控件是否可以穿透点击
    /// </summary>
    public bool IsPenetrating
    {
        get => (bool) GetValue(IsPenetratingProperty);
        set => SetValue(IsPenetratingProperty, ValueBoxes.BooleanBox(value));
    }

    public static void SetIsPenetrating(DependencyObject element, bool value) => element.SetValue(IsPenetratingProperty, ValueBoxes.BooleanBox(value));

    public static bool GetIsPenetrating(DependencyObject element) => (bool) element.GetValue(IsPenetratingProperty);

    /// <summary>
    ///     当前垂直滚动偏移
    /// </summary>
    internal static readonly DependencyProperty CurrentVerticalOffsetProperty = DependencyProperty.Register(
        nameof(CurrentVerticalOffset), typeof(double), typeof(ScrollViewer), new PropertyMetadata(ValueBoxes.Double0Box, OnCurrentVerticalOffsetChanged));

    private static void OnCurrentVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer ctl && e.NewValue is double v)
        {
            ctl.ScrollToVerticalOffset(v);
        }
    }

    /// <summary>
    ///     当前垂直滚动偏移
    /// </summary>
    internal double CurrentVerticalOffset
    {
        // ReSharper disable once UnusedMember.Local
        get => (double) GetValue(CurrentVerticalOffsetProperty);
        set => SetValue(CurrentVerticalOffsetProperty, value);
    }

    /// <summary>
    ///     当前水平滚动偏移
    /// </summary>
    internal static readonly DependencyProperty CurrentHorizontalOffsetProperty = DependencyProperty.Register(
        nameof(CurrentHorizontalOffset), typeof(double), typeof(ScrollViewer), new PropertyMetadata(ValueBoxes.Double0Box, OnCurrentHorizontalOffsetChanged));

    private static void OnCurrentHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer ctl && e.NewValue is double v)
        {
            ctl.ScrollToHorizontalOffset(v);
        }
    }

    /// <summary>
    ///     当前水平滚动偏移
    /// </summary>
    internal double CurrentHorizontalOffset
    {
        get => (double) GetValue(CurrentHorizontalOffsetProperty);
        set => SetValue(CurrentHorizontalOffsetProperty, value);
    }
}
