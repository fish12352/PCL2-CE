using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PCL.Core.App;
using PCL.Core.UI.Theme;

namespace PCL;

public static class ThemeManager
{
    private static bool _contextMenuHandlerRegistered;

    public static bool IsDarkMode => ThemeService.IsDarkMode;

    public static ResourceDictionary AppResources => System.Windows.Application.Current.Resources;

    public static ModBase.MyColor colorGray1 = new(AppResources["ColorObjectGray1"]);
    public static ModBase.MyColor colorGray4 = new(AppResources["ColorObjectGray4"]);
    public static ModBase.MyColor colorGray5 = new(AppResources["ColorObjectGray5"]);
    public static ModBase.MyColor colorSemiTransparent = new(AppResources["ColorBrushSemiTransparent"]);

    public static void ThemeRefresh(int newTheme = -1)
    {
        colorGray1 = new ModBase.MyColor(AppResources["ColorObjectGray1"]);
        colorGray4 = new ModBase.MyColor(AppResources["ColorObjectGray4"]);
        colorGray5 = new ModBase.MyColor(AppResources["ColorObjectGray5"]);
        colorSemiTransparent = new ModBase.MyColor(AppResources["ColorBrushSemiTransparent"]);
        ThemeRefreshMain();
    }

    public static void ThemeRefreshMain()
    {
        ModBase.RunInUi(() =>
        {
            if (!ModMain.frmMain.IsLoaded) return;
            RefreshBackground();
            RefreshAllContextMenuThemes();
        });
    }
    
    // 主页面背景
    private static void RefreshBackground()
    {
        if (Config.Preference.Background.BackgroundColorful || ThemeService.CurrentTheme == ColorTheme.Spectrum)
        {
            var brush = new LinearGradientBrush
            {
                EndPoint = new Point(0.1, 1),
                StartPoint = new Point(0.9, 0)
            };

            var hue = ThemeService.GetCurrentThemeArgs().Hue;
            var tone = ThemeService.CurrentTone;
            var darkLight = IsDarkMode ? 0.2d : 1d;
            if (ThemeService.CurrentTheme == ColorTheme.Spectrum)
            {
                // Use a restrained violet-to-blue gradient instead of a full rainbow.
                // Keep the accent hue independent so buttons and semantic colors stay solid violet.
                brush.GradientStops.Add(new GradientStop
                    { Offset = 0.0d, Color = LabColor.FromLch(IsDarkMode ? 0.22d : 0.96d, tone.C5 * 0.55d, 275) });
                brush.GradientStops.Add(new GradientStop
                    { Offset = 1.0d, Color = LabColor.FromLch(IsDarkMode ? 0.17d : 0.91d, tone.C5 * 0.55d, 315) });
            }
            else if (ThemeService.CurrentTheme == ColorTheme.Green)
            {
                // 青岚：保留指定的绿青双色渐变。
                brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x00, 0xB0, 0x9B), 0.0d));
                brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x96, 0xC9, 0x3D), 1.0d));
            }
            else
            {
                var hue1 = hue - 15;
                var hue2 = hue + 15;
                brush.GradientStops.Add(new GradientStop
                    { Offset = -0.1d, Color = LabColor.FromLch(0.84d * darkLight, tone.C5, hue1) });
                brush.GradientStops.Add(new GradientStop
                    { Offset = 0.4d, Color = LabColor.FromLch(0.96d * darkLight, tone.C7, hue) });
                brush.GradientStops.Add(new GradientStop
                    { Offset = 1.1d, Color = LabColor.FromLch(0.84d * darkLight, tone.C5, hue2) });
            }
            ModMain.frmMain.PanForm.Background = brush;

            if (ThemeService.CurrentTheme == ColorTheme.Spectrum)
            {
                // A stronger violet/indigo blend in the title bar, while action buttons
                // continue to use the ordinary solid accent resources.
                var titleBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0)
                };
                titleBrush.GradientStops.Add(new GradientStop(
                    LabColor.FromLch(IsDarkMode ? 0.43d : 0.48d, 0.16d, 285), 0.0d));
                titleBrush.GradientStops.Add(new GradientStop(
                    LabColor.FromLch(IsDarkMode ? 0.37d : 0.43d, 0.16d, 315), 1.0d));
                titleBrush.Freeze();
                ModMain.frmMain.PanTitleGradient.Visibility = Visibility.Collapsed;
                ModMain.frmMain.PanTitle.BackgroundBrush =
                    (SolidColorBrush)System.Windows.Application.Current.Resources["ColorBrush3"];
                ModMain.frmMain.PanTitleGradient.Background = titleBrush;
                ModMain.frmMain.PanTitleGradient.Visibility = Visibility.Visible;
            }
            else
            {
                ModMain.frmMain.PanTitle.BackgroundBrush =
                    (SolidColorBrush)System.Windows.Application.Current.Resources["ColorBrush3"];
            }
        }
        else
        {
            ModMain.frmMain.PanForm.Background = (Brush)System.Windows.Application.Current.Resources["ColorBrushBackground"];
        }

        ModMain.frmMain.PanForm.Background.Freeze();
    }

    // 通用ContextMenu主题刷新
    private static void RefreshAllContextMenuThemes()
    {
        try
        {
            if (!_contextMenuHandlerRegistered)
            {
                EventManager.RegisterClassHandler(typeof(ContextMenu), ContextMenu.OpenedEvent,
                    new RoutedEventHandler(OnContextMenuOpened));
                _contextMenuHandlerRegistered = true;
            }

            foreach (Window window in System.Windows.Application.Current.Windows)
                RefreshContextMenusInElement(window);
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "刷新ContextMenu主题时出错");
        }
    }

    private static void OnContextMenuOpened(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is ContextMenu contextMenu)
            {
                contextMenu.ClearValue(FrameworkElement.StyleProperty);
                contextMenu.UpdateDefaultStyle();
            }
        }
        catch
        {
            // 忽略个别错误
        }
    }

    private static void RefreshContextMenusInElement(DependencyObject element)
    {
        if (element is null)
            return;

        try
        {
            if (element is FrameworkElement { ContextMenu: not null } fe)
            {
                fe.ContextMenu.ClearValue(FrameworkElement.StyleProperty);
                fe.ContextMenu.UpdateDefaultStyle();
            }

            var childrenCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childrenCount; i++)
                RefreshContextMenusInElement(VisualTreeHelper.GetChild(element, i));
        }
        catch
        {
            // 忽略个别元素的错误，继续处理其他元素
        }
    }
}