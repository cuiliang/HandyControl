using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using HandyControl.Tools;
using HandyControl.Tools.Interop;

namespace HandyControl.Controls;

public sealed class GrowlWindow : Window
{
    internal Panel GrowlPanel { get; set; }

    internal GrowlWindow()
    {
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;


        GrowlPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Top
        };

        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            IsInertiaEnabled = true,
            Content = GrowlPanel
        };
    }

    internal void Init()
    {
        var desktopWorkingArea = SystemParameters.WorkArea;
        Height = desktopWorkingArea.Height;
        Left = desktopWorkingArea.Right - Width;
        Top = 0;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        InteropMethods.IntDestroyMenu(this.GetHwndSource().CreateHandleRef());

        HideFromAltTab(this);
    }

    /// <summary>
    /// 避免在Alt+Tab中显示
    /// </summary>
    /// <param name="window"></param>
    private void HideFromAltTab(Window window)
    {
        // 设置ShowInTaskbar属性为false
        window.ShowInTaskbar = false;

        // 获取窗口句柄
        var hwnd = new WindowInteropHelper(window).Handle;

        // 获取当前窗口样式
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

        // 修改窗口样式，去掉WS_EX_APPWINDOW属性，添加WS_EX_TOOLWINDOW属性
        SetWindowLong(hwnd, GWL_EXSTYLE, (exStyle & ~WS_EX_APPWINDOW) | WS_EX_TOOLWINDOW);
    }

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_APPWINDOW = 0x00040000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}
