using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using ClassIsland.Core;
using ExtraIsland.ConfigHandlers;

namespace ExtraIsland.Shared;

public class MainWindowHandler {
    public MainWindowHandler() {
        MainWindow = AppBase.Current.MainWindow!;
        AppBar = new AppBarHelper.AppBar();
        Settings = GlobalConstants.Handlers.MainConfig!.Data.Dock;
        Animator = new Animators.IslandDriftAnimator(MainWindow, Colors.Transparent, MainWindow.Height);
    }

    public MainConfigData.DockConfig Settings;
    public readonly Window MainWindow;
    public AppBarHelper.AppBar AppBar;
    public Animators.IslandDriftAnimator Animator;

    public static class TargetConfigs {
        public static bool IsMouseClickingEnabled { get; set; } = true;
        public static bool IsMouseInFadingEnabled { get; set; } = false;
        public static double Scale { get; set; }
    }
    static void ConfigDaemon(object? sender, EventArgs eventArgs) {
        dynamic app = AppBase.Current;
        dynamic settings = app.Settings;
        if (Convert.ToInt32(settings.WindowDockingLocation) > 2) {
            settings.WindowDockingLocation -= 3;
        }
        settings.IsMouseClickingEnabled = TargetConfigs.IsMouseClickingEnabled;
        if (!settings.IsIgnoreWorkAreaEnabled) {
            settings.IsIgnoreWorkAreaEnabled = true;
        }
        settings.IsMouseInFadingEnabled = TargetConfigs.IsMouseInFadingEnabled;
        if (TargetConfigs.Scale != settings.Scale) {
            GlobalConstants.Handlers.MainWindow?.SetAppBar();
        }
    }
    
    public void InitBar(Color? backColor = null, AppBarHelper.AppBarLocation? location = null, 
        AccentHelper.AccentState accentState = AccentHelper.AccentState.AccentEnableBlurbehind) {
        GlobalConstants.HostInterfaces.LessonsService!.PreMainTimerTicked -= ConfigDaemon;
        SetBar(backColor,location,accentState);
        GlobalConstants.HostInterfaces.LessonsService!.PreMainTimerTicked += ConfigDaemon;
    }
    
    public void SetBar(Color? backColor = null, AppBarHelper.AppBarLocation? location = null, 
                       AccentHelper.AccentState accentState = AccentHelper.AccentState.AccentEnableBlurbehind) {
        backColor ??= Colors.Transparent;
        dynamic app = AppBase.Current;
        dynamic settings = app.Settings;
        if (Convert.ToInt32(settings.WindowDockingLocation) > 2) {
            settings.WindowDockingLocation -= 3;
        }
        location ??= AppBarHelper.AppBarLocation.Top;
        Animator = new Animators.IslandDriftAnimator(MainWindow, backColor.Value, MainWindow.Height);
        TargetConfigs.Scale = settings.Scale;
        double topOffset = settings.Scale * 4.0;
        ((Grid)MainWindow.Content).Margin = new Thickness(0,topOffset,0,0);
        AppBar.OnFullScreenStateChanged += OnFullScreenStateChanged;
        SetAppBar(location.Value);
        UpdateAccent(accentState);
        Animator.Background(true);
    }

    public void UpdateAccent(AccentHelper.AccentState accentState) {
        AccentHelper.ChangeAccent(MainWindow, accentState);
    }

    void SetAppBar(AppBarHelper.AppBarLocation location = AppBarHelper.AppBarLocation.Top) {
        if (location == AppBar.Location) {
            AppBar = new AppBarHelper.AppBar {
                Location = location
            };
        }
        AppBar.Location = location;
        AppBarHelper.AppBarCreator.SetAppBar(MainWindow, AppBar);
    }

    void OnFullScreenStateChanged(object? sender,bool isFull) {
        UpdateAccent(isFull ? AccentHelper.AccentState.AccentDisabled : Settings.AccentState);
        TargetConfigs.IsMouseClickingEnabled = !isFull;
        TargetConfigs.IsMouseInFadingEnabled = isFull;
    }
    
    // ReSharper disable IdentifierTypo
    // ReSharper disable StringLiteralTypo
    /// <summary>
    /// AppBar操作实现封装类
    /// 由 TwilightLemon@github.com/AppBarTest 修改而来
    /// </summary>
    public static class AppBarHelper {
        public static class AppBarCreator {
            public static readonly DependencyProperty AppBarProperty =
                DependencyProperty.RegisterAttached(
                    "AppBar",
                    typeof(AppBar),
                    typeof(AppBarCreator),
                    new PropertyMetadata(null,OnAppBarChanged));
            static void OnAppBarChanged(DependencyObject d,DependencyPropertyChangedEventArgs e) {
                if (d is Window window && e.NewValue is AppBar appBar) {
                    appBar.AttachedWindow = window;
                }
            }
            public static void SetAppBar(Window element,AppBar? value) {
                if (value == null) return;
                element.SetValue(AppBarProperty,value);
            }

            public static AppBar GetAppBar(Window element) {
                return (AppBar)element.GetValue(AppBarProperty);
            }
        }

        public class AppBar : DependencyObject {
            /// <summary>
            /// 附加到的窗口
            /// </summary>
            public Window? AttachedWindow {
                get => _window;
                set {
                    if (value == null) return;
                    _window = value;
                    _window.Closing += _window_Closing;
                    _window.LocationChanged += _window_LocationChanged;
                    //获取窗口句柄hWnd
                    IntPtr handle = new WindowInteropHelper(value).Handle;
                    if (handle == IntPtr.Zero) {
                        //Win32窗口未创建
                        _window.SourceInitialized += _window_SourceInitialized;
                    } else {
                        _hWnd = handle;
                        CheckPending();
                    }
                }
            }

            void _window_LocationChanged(object? sender,EventArgs e) {
                if (_window != null)
                    Debug.WriteLine(_window.Title + " LocationChanged: Top: " + _window.Top + "  Left: " + _window.Left);
            }

            void _window_Closing(object? sender,CancelEventArgs e) {
                if (_window != null) _window.Closing -= _window_Closing;
                if (Location != AppBarLocation.None)
                    DisableAppBar();
            }

            /// <summary>
            /// 检查是否需要应用之前的Location更改
            /// </summary>
            void CheckPending() {
                //创建AppBar时提前触发的LocationChanged
                if (_locationChangePending) {
                    _locationChangePending = false;
                    LoadAppBar(Location);
                }
            }
            /// <summary>
            /// 载入AppBar
            /// </summary>
            /// <param name="e"></param>
            /// <param name="previous"></param>
            void LoadAppBar(AppBarLocation e,AppBarLocation? previous = null) {

                if (e != AppBarLocation.None) {
                    if (e == AppBarLocation.RegisterOnly) {
                        //仅注册AppBarMsg
                        //如果之前注册过有效的AppBar则先注销，以还原位置
                        if (previous.HasValue && previous.Value != AppBarLocation.RegisterOnly) {
                            if (previous.Value != AppBarLocation.None) {
                                //由生效的AppBar转为RegisterOnly，还原为普通窗口再注册空AppBar
                                DisableAppBar();
                            }
                            RegisterAppBarMsg();
                        } else {
                            //之前未注册过AppBar，直接注册
                            RegisterAppBarMsg();
                        }
                    } else {
                        if (previous.HasValue && previous.Value != AppBarLocation.None) {
                            //之前为RegisterOnly才备份窗口信息
                            if (previous.Value == AppBarLocation.RegisterOnly) {
                                BackupWindowInfo();
                            }
                            SetAppBarPosition(_originalSize);
                            ForceWindowStyles();
                        } else
                            EnableAppBar();
                    }
                } else {
                    DisableAppBar();
                }
            }
            void _window_SourceInitialized(object? sender,EventArgs e) {
                if (_window != null) {
                    _window.SourceInitialized -= _window_SourceInitialized;
                    _hWnd = new WindowInteropHelper(_window).Handle;
                }
                CheckPending();
            }

            /// <summary>
            /// 当有窗口进入或退出全屏时触发 bool参数为true时表示全屏状态
            /// </summary>
            public event EventHandler<bool>? OnFullScreenStateChanged;
            /// <summary>
            /// 期望将AppBar停靠到的位置
            /// </summary>
            public AppBarLocation Location {
                get => (AppBarLocation)GetValue(LocationProperty);
                set => SetValue(LocationProperty,value);
            }

            public static readonly DependencyProperty LocationProperty =
                DependencyProperty.Register(
                    "Location",
                    typeof(AppBarLocation),typeof(AppBar),
                    new PropertyMetadata(AppBarLocation.None,OnLocationChanged));

            bool _locationChangePending;
            static void OnLocationChanged(DependencyObject d,DependencyPropertyChangedEventArgs e) {
                if (DesignerProperties.GetIsInDesignMode(d))
                    return;
                if (d is not AppBar appBar) return;
                if (appBar.AttachedWindow == null) {
                    appBar._locationChangePending = true;
                    return;
                }
                appBar.LoadAppBar((AppBarLocation)e.NewValue,(AppBarLocation)e.OldValue);
            }

            int _callbackId;
            bool _isRegistered;
            Window? _window;
            IntPtr _hWnd;
            WindowStyle _originalStyle;
            Point _originalPosition;
            Size _originalSize = Size.Empty;
            ResizeMode _originalResizeMode;
            bool _originalTopmost;
            IntPtr WndProc(IntPtr hwnd,int msg,IntPtr wParam,
                IntPtr lParam,ref bool handled) {
                if (msg != _callbackId) return IntPtr.Zero;
                Debug.WriteLine(_window?.Title + " AppBarMsg(" + _callbackId + "): " + wParam.ToInt32() + " LParam: "
                                + lParam.ToInt32());
                switch (wParam.ToInt32()) {
                    case (int)Interop.AppBarNotify.AbnPoschanged:
                        Debug.WriteLine("AppBarNotify.ABN_POSCHANGED ! " + _window?.Title);
                        if (Location != AppBarLocation.RegisterOnly)
                            SetAppBarPosition(Size.Empty);
                        handled = true;
                        break;
                    case (int)Interop.AppBarNotify.AbnFullscreenapp:
                        OnFullScreenStateChanged?.Invoke(this,lParam.ToInt32() == 1);
                        handled = true;
                        break;
                }
                return IntPtr.Zero;
            }

            public void BackupWindowInfo() {
                _callbackId = 0;
                if (_window == null) return;
                _originalStyle = _window.WindowStyle;
                _originalSize = new Size(_window.ActualWidth,_window.ActualHeight);
                _originalPosition = new Point(_window.Left,_window.Top);
                _originalResizeMode = _window.ResizeMode;
                _originalTopmost = _window.Topmost;
            }
            public void RestoreWindowInfo() {
                if (_originalSize == Size.Empty) return;
                if (_window == null) return;
                _window.WindowStyle = _originalStyle;
                _window.ResizeMode = _originalResizeMode;
                _window.Topmost = _originalTopmost;
                _window.Left = _originalPosition.X;
                _window.Top = _originalPosition.Y;
                _window.Width = _originalSize.Width;
                _window.Height = _originalSize.Height;
            }
            public void ForceWindowStyles() {
                if (_window == null) return;
                _window.WindowStyle = WindowStyle.None;
                _window.ResizeMode = ResizeMode.NoResize;
                _window.Topmost = true;
            }

            public void RegisterAppBarMsg() {
                Interop.Appbardata data = new Interop.Appbardata();
                data.cbSize = Marshal.SizeOf(data);
                data.hWnd = _hWnd;

                _isRegistered = true;
                _callbackId = Interop.RegisterWindowMessage(Guid.NewGuid().ToString());
                data.uCallbackMessage = _callbackId;
                #pragma warning disable CA1806
                Interop.SHAppBarMessage((int)Interop.AppBarMsg.AbmNew,ref data);
                #pragma warning restore CA1806
                HwndSource? source = HwndSource.FromHwnd(_hWnd);
                Debug.WriteLineIf(source == null,"HwndSource is null!");
                source?.AddHook(WndProc);
                Debug.WriteLine(_window?.Title + " RegisterAppBarMsg: " + _callbackId);
            }
            public void EnableAppBar() {
                if (!_isRegistered) {
                    //备份窗口信息并设置窗口样式
                    BackupWindowInfo();
                    //注册成为AppBar窗口
                    RegisterAppBarMsg();
                    ForceWindowStyles();
                }
                //成为AppBar窗口之后(或已经是)只需要注册并移动窗口位置即可
                SetAppBarPosition(_originalSize);
            }
            public void SetAppBarPosition(Size windowSize) {
                Interop.Appbardata data = new Interop.Appbardata();
                data.cbSize = Marshal.SizeOf(data);
                data.hWnd = _hWnd;
                data.uEdge = (int)Location;
                data.uCallbackMessage = _callbackId;
                Debug.WriteLine("\r\nWindow: " + _window?.Title);

                //获取WPF单位与像素的转换矩阵
                if (_window != null) {
                    CompositionTarget? compositionTarget = PresentationSource.FromVisual(_window)?.CompositionTarget;
                    if (compositionTarget == null)
                        throw new Exception("居然获取不到CompositionTarget?!");
                    Matrix toPixel = compositionTarget.TransformToDevice;
                    Matrix toWpfUnit = compositionTarget.TransformFromDevice;

                    //窗口在屏幕的实际大小
                    if (windowSize == Size.Empty)
                        windowSize = new Size(_window.ActualWidth,_window.ActualHeight);
                    Vector actualSize = toPixel.Transform(new Vector(windowSize.Width,windowSize.Height));
                    //屏幕的真实像素
                    Vector workArea = toPixel.Transform(new Vector(SystemParameters.PrimaryScreenWidth,SystemParameters.PrimaryScreenHeight));
                    Debug.WriteLine("WorkArea Width: {0}, Height: {1}",workArea.X,workArea.Y);

                    if (Location is AppBarLocation.Left or AppBarLocation.Right) {
                        data.rc.top = 0;
                        data.rc.bottom = (int)workArea.Y;
                        if (Location == AppBarLocation.Left) {
                            data.rc.left = 0;
                            data.rc.right = (int)Math.Round(actualSize.X);
                        } else {
                            data.rc.right = (int)workArea.X;
                            data.rc.left = (int)workArea.X - (int)Math.Round(actualSize.X);
                        }
                    } else {
                        data.rc.left = 0;
                        data.rc.right = (int)workArea.X;
                        if (Location == AppBarLocation.Top) {
                            data.rc.top = 0;
                            data.rc.bottom = (int)Math.Round(actualSize.Y);
                        } else {
                            data.rc.bottom = (int)workArea.Y;
                            data.rc.top = (int)workArea.Y - (int)Math.Round(actualSize.Y);
                        }
                    }
                    //以上生成的是四周都没有其他AppBar时的理想位置
                    //系统将自动调整位置以适应其他AppBar
                    Debug.WriteLine("Before QueryPos: Left: {0}, Top: {1}, Right: {2}, Bottom: {3}",data.rc.left,data.rc.top,data.rc.right,
                        data.rc.bottom);
                    Interop.SHAppBarMessage((int)Interop.AppBarMsg.AbmQuerypos,ref data);
                    Debug.WriteLine("After QueryPos: Left: {0}, Top: {1}, Right: {2}, Bottom: {3}",data.rc.left,data.rc.top,data.rc.right,
                        data.rc.bottom);
                    //自定义对齐方式，确保Height和Width不会小于0
                    if (data.rc.bottom - data.rc.top < 0) {
                        if (Location == AppBarLocation.Top)
                            data.rc.bottom = data.rc.top + (int)Math.Round(actualSize.Y); //上对齐
                        else if (Location == AppBarLocation.Bottom)
                            data.rc.top = data.rc.bottom - (int)Math.Round(actualSize.Y); //下对齐
                    }
                    if (data.rc.right - data.rc.left < 0) {
                        if (Location == AppBarLocation.Left)
                            data.rc.right = data.rc.left + (int)Math.Round(actualSize.X); //左对齐
                        else if (Location == AppBarLocation.Right)
                            data.rc.left = data.rc.right - (int)Math.Round(actualSize.X); //右对齐
                    }
                    //调整完毕，设置为最终位置
                    Interop.SHAppBarMessage((int)Interop.AppBarMsg.AbmSetpos,ref data);
                    //应用到窗口
                    Point location = toWpfUnit.Transform(new Point(data.rc.left,data.rc.top));
                    Vector dimension = toWpfUnit.Transform(new Vector(data.rc.right - data.rc.left,
                        data.rc.bottom - data.rc.top));
                    Rect rect = new Rect(location,new Size(dimension.X,dimension.Y));

                    _window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle,() => {
                        _window.Left = rect.Left;
                        _window.Top = rect.Top;
                        _window.Width = rect.Width;
                        _window.Height = rect.Height;
                    });
                }

                if (_window != null) {
                    Debug.WriteLine("Set {0} Left: {1} ,Top: {2}, Width: {3}, Height: {4}",_window.Title,_window.Left,_window.Top,
                        _window.Width,_window.Height);
                }
            }
            public void DisableAppBar() {
                if (!_isRegistered) return;
                _isRegistered = false;
                Interop.Appbardata data = new Interop.Appbardata();
                data.cbSize = Marshal.SizeOf(data);
                data.hWnd = _hWnd;
                data.uCallbackMessage = _callbackId;
                Interop.SHAppBarMessage((int)Interop.AppBarMsg.AbmRemove,ref data);
                _isRegistered = false;
                RestoreWindowInfo();
                Debug.WriteLine(_window?.Title + " DisableAppBar");
            }
        }

        public enum AppBarLocation {
            Left = 0,
            Top,
            Right,
            Bottom,
            None,
            RegisterOnly = 99
        }

        internal static class Interop {
            [StructLayout(LayoutKind.Sequential)]
            internal struct Rect {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [StructLayout(LayoutKind.Sequential)]
            internal struct Appbardata {
                public int cbSize;
                public IntPtr hWnd;
                public int uCallbackMessage;
                public int uEdge;
                public Rect rc;
                public IntPtr lParam;
            }
            internal enum AppBarMsg {
                AbmNew = 0,
                AbmRemove,
                AbmQuerypos,
                AbmSetpos,
                AbmGetstate,
                AbmGettaskbarpos,
                AbmActivate,
                AbmGetautohidebar,
                AbmSetautohidebar,
                AbmWindowposchanged,
                AbmSetstate
            }
            internal enum AppBarNotify {
                AbnStatechange = 0,
                AbnPoschanged,
                AbnFullscreenapp,
                AbnWindowarrange
            }
            [DllImport("SHELL32",CallingConvention = CallingConvention.StdCall)]
            internal static extern uint SHAppBarMessage(int dwMessage,ref Appbardata pData);
            [DllImport("User32.dll", CharSet = CharSet.Unicode)]
            internal static extern int RegisterWindowMessage(string msg);
        }
    }

    public static class AccentHelper {
        [DllImport("user32.dll")]
        static extern int SetWindowCompositionAttribute(IntPtr hwnd,ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        struct WindowCompositionAttributeData {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        enum WindowCompositionAttribute {
            WcaAccentPolicy = 19
        }

        public enum AccentState {
            [Description("禁用")]
            AccentDisabled = 0,
            [Description("普通渐变")]
            AccentEnableGradient = 1,
            [Description("渐变")]
            AccentEnableTransparentgradient = 2,
            [Description("毛玻璃")]
            AccentEnableBlurbehind = 3,
            [Description("无效")]
            AccentInvalidState = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        struct AccentPolicy {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        public static void ChangeAccent(Window window, AccentState accentState) {
            WindowInteropHelper windowHelper = new WindowInteropHelper(window);
            AccentPolicy accent = new AccentPolicy();
            int accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = accentState;

            IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent,accentPtr,false);

            WindowCompositionAttributeData data = new WindowCompositionAttributeData {
                Attribute = WindowCompositionAttribute.WcaAccentPolicy,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

#pragma warning disable CA1806
            SetWindowCompositionAttribute(windowHelper.Handle,ref data);
#pragma warning restore CA1806

            Marshal.FreeHGlobal(accentPtr);
        }
    }
    // ReSharper restore IdentifierTypo
    // ReSharper restore StringLiteralTypo
}