using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using TextClipboardViewer.Models;
using TextClipboardViewer.Services;

namespace TextClipboardViewer;

public partial class MainWindow : Window
{
    private readonly AppSettings _settings;
    private readonly SettingsService _settingsService;
    private readonly ClipboardMonitor _monitor = new();
    private SettingsWindow? _settingsWindow;
    private readonly DispatcherTimer _saveTimer;
    private readonly DispatcherTimer _topmostTimer;

    /// <summary>トレイの「終了」からのみ true にする。false の間は Close が非表示になる。</summary>
    internal bool AllowClose { get; set; }

    /// <summary>App がトレイメニューの言語追従のために参照する。</summary>
    internal AppSettings Settings => _settings;

    public MainWindow()
    {
        InitializeComponent();
        _settingsService = new SettingsService(SettingsService.DefaultPath);
        _settings = _settingsService.Load();
        if (string.IsNullOrEmpty(_settings.Language))
        {
            // 初回起動はWindowsのUI言語に合わせる
            _settings.Language =
                System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ja" ? "ja" : "en";
        }
        Left = _settings.WindowLeft;
        Top = _settings.WindowTop;
        Width = _settings.WindowWidth;
        Height = _settings.WindowHeight;
        // 保存はデバウンス(スライダードラッグ中の連続書き込みを避ける)。見た目の反映は即時
        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _saveTimer.Tick += (_, _) =>
        {
            _saveTimer.Stop();
            _settingsService.Save(_settings);
        };
        _settings.PropertyChanged += (_, _) =>
        {
            ApplySettings();
            _saveTimer.Stop();
            _saveTimer.Start();
        };

        // WPFのTopmostは他アプリの最前面ウィンドウ・解像度変更・セッション復帰等で
        // OS側の最前面フラグを失うことがあるため、毎秒監視して失われていたら復帰する
        _topmostTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _topmostTimer.Tick += (_, _) => EnsureTopmost();
        _topmostTimer.Start();
        // 画面構成変更・ロック解除/セッション切替の直後は最前面が外れやすいので即復帰
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        SystemEvents.SessionSwitch += OnSessionSwitch;

        ApplySettings();
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e) =>
        Dispatcher.BeginInvoke(EnsureTopmost);

    private void OnSessionSwitch(object? sender, SessionSwitchEventArgs e) =>
        Dispatcher.BeginInvoke(EnsureTopmost);

    /// <summary>最前面が失われていたら、フォーカスを奪わずに最前面へ復帰させる。</summary>
    private void EnsureTopmost()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero) return; // HWND未生成
        var exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
        if (!ShouldReassertTopmost(exStyle)) return;
        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    private void ApplySettings()
    {
        LanguageService.Apply(_settings.Language);
        var bg = ParseColorOrDefault(_settings.BackgroundColor, Colors.Black);
        // 完全に透明(アルファ0)のピクセルはOSレベルでクリック透過になり
        // ドラッグ移動できなくなるため、不透明度は最低1%を確保する
        RootBorder.Background = new SolidColorBrush(bg)
        {
            Opacity = Math.Max(_settings.BackgroundOpacity, 0.01),
        };
        ClipText.Foreground = new SolidColorBrush(ParseColorOrDefault(_settings.TextColor, Colors.White));
        ClipText.FontFamily = new FontFamily(_settings.FontFamily);
        ClipText.FontSize = _settings.FontSize;
        RootBorder.ContextMenu.FontSize = _settings.FontSize;
        ClipText.Effect = _settings.TextShadow
            ? new DropShadowEffect { BlurRadius = 6, ShadowDepth = 1, Opacity = 0.8, Color = Colors.Black }
            : null;
        ApplyClickThrough(_settings.ClickThrough);
    }

    private static Color ParseColorOrDefault(string hex, Color fallback)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }
        catch (Exception e) when (e is FormatException or InvalidCastException or NullReferenceException)
        {
            return fallback;
        }
    }

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x20;
    private const int WS_EX_TOPMOST = 0x8;
    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private const uint SWP_NOSIZE = 0x1;
    private const uint SWP_NOMOVE = 0x2;
    private const uint SWP_NOACTIVATE = 0x10;

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hwnd, int index, IntPtr newLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
        int x, int y, int cx, int cy, uint flags);

    /// <summary>OSの最前面フラグ(WS_EX_TOPMOST)が失われているときだけ再アサートすべき。</summary>
    public static bool ShouldReassertTopmost(long exStyle) => (exStyle & WS_EX_TOPMOST) == 0;

    /// <summary>WS_EX_TRANSPARENT を付け外ししてマウス操作の透過を切り替える。</summary>
    private void ApplyClickThrough(bool enabled)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero) return; // HWND未生成(コンストラクタ段階)はOnSourceInitializedで適用
        long ex = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
        long updated = enabled ? ex | WS_EX_TRANSPARENT : ex & ~WS_EX_TRANSPARENT;
        SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(updated));
    }

    private void OnBorderMouseDown(object sender, MouseButtonEventArgs e)
    {
        // Previewイベントで受けることで、ScrollViewerに握られる本文領域も含め
        // ウィンドウ全面をドラッグ移動可能にする。ボタン・スクロールバーは除外
        if (e.ButtonState == MouseButtonState.Pressed && !IsOnInteractiveElement(e.OriginalSource))
        {
            DragMove();
        }
    }

    private static bool IsOnInteractiveElement(object source)
    {
        var d = source as DependencyObject;
        while (d != null)
        {
            if (d is Button or ScrollBar) return true;
            d = d is Visual ? VisualTreeHelper.GetParent(d) : LogicalTreeHelper.GetParent(d);
        }
        return false;
    }

    private void OnHideClick(object sender, RoutedEventArgs e) => Hide();

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        AllowClose = true;
        Close();
        System.Windows.Application.Current.Shutdown();
    }

    private void OnMouseEnterWindow(object sender, MouseEventArgs e) => AnimateButtons(1);

    private void OnMouseLeaveWindow(object sender, MouseEventArgs e) => AnimateButtons(0);

    private void AnimateButtons(double to) =>
        HoverButtons.BeginAnimation(OpacityProperty,
            new DoubleAnimation(to, TimeSpan.FromMilliseconds(150)));

    private void OnClearClick(object sender, RoutedEventArgs e) => ClearClipboard();

    /// <summary>クリップボードを空にする(🗑ボタン・トレイ共用)。失敗時(他プロセスがロック中)は表示を変えない。</summary>
    internal void ClearClipboard()
    {
        if (ClipboardMonitor.TryClear())
        {
            ClipText.Text = string.Empty;
        }
    }

    /// <summary>固定文＋現在時刻をクリップボードに書き込む。表示は監視イベントが更新する。</summary>
    internal void WriteTestText()
    {
        var text = Loc.S("Common_TestText") + " " + DateTime.Now.ToString("HH:mm:ss");
        ClipboardMonitor.TryWrite(text);
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        if (_settingsWindow is { IsLoaded: true })
        {
            _settingsWindow.Activate();
            return;
        }
        _settingsWindow = new SettingsWindow(_settings) { Owner = this };
        // 本体の右横に表示。画面右端からはみ出す場合は左横に出す
        _settingsWindow.Left = Left + Width + 8;
        _settingsWindow.Top = Top;
        if (_settingsWindow.Left + _settingsWindow.Width > SystemParameters.WorkArea.Right)
        {
            _settingsWindow.Left = Left - _settingsWindow.Width - 8;
        }
        _settingsWindow.Show();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        _monitor.TextCopied += text => ClipText.Text = text;
        _monitor.Start(this);
        ApplyClickThrough(_settings.ClickThrough);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!AllowClose)
        {
            // Alt+F4 等では終了せず、トレイ常駐のまま隠れる
            e.Cancel = true;
            Hide();
            return;
        }
        _saveTimer.Stop();
        _topmostTimer.Stop();
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        _settings.WindowLeft = Left;
        _settings.WindowTop = Top;
        _settings.WindowWidth = Width;
        _settings.WindowHeight = Height;
        _settingsService.Save(_settings);
        _monitor.Dispose();
        base.OnClosing(e);
    }
}
