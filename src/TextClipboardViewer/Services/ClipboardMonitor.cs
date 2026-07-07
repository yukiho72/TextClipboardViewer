using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TextClipboardViewer.Services;

/// <summary>
/// WM_CLIPBOARDUPDATE によるイベント駆動のクリップボード監視。
/// 他プロセスがクリップボードを掴んでいる瞬間の失敗は短いリトライで吸収する。
/// </summary>
public class ClipboardMonitor : IDisposable
{
    private const int WM_CLIPBOARDUPDATE = 0x031D;
    private const int RetryCount = 5;
    private const int RetryDelayMs = 50;

    private HwndSource? _source;

    /// <summary>テキストがコピーされたときに発火(UIスレッド上)。</summary>
    public event Action<string>? TextCopied;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    public void Start(Window window)
    {
        var handle = new WindowInteropHelper(window).EnsureHandle();
        _source = HwndSource.FromHwnd(handle);
        _source!.AddHook(WndProc);
        AddClipboardFormatListener(handle);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_CLIPBOARDUPDATE)
        {
            var text = TryGetText();
            if (text != null) TextCopied?.Invoke(text);
        }
        return IntPtr.Zero;
    }

    /// <summary>テキストを取得。テキスト以外・取得失敗は null。</summary>
    public static string? TryGetText()
    {
        for (var i = 0; i < RetryCount; i++)
        {
            try
            {
                return Clipboard.ContainsText() ? Clipboard.GetText() : null;
            }
            catch (COMException)
            {
                Thread.Sleep(RetryDelayMs);
            }
        }
        return null;
    }

    /// <summary>クリップボードを空にする。成功したら true。</summary>
    public static bool TryClear()
    {
        for (var i = 0; i < RetryCount; i++)
        {
            try
            {
                Clipboard.Clear();
                return true;
            }
            catch (COMException)
            {
                Thread.Sleep(RetryDelayMs);
            }
        }
        return false;
    }

    /// <summary>テキストを書き込む。成功したら true。</summary>
    public static bool TryWrite(string text)
    {
        for (var i = 0; i < RetryCount; i++)
        {
            try
            {
                Clipboard.SetText(text);
                return true;
            }
            catch (COMException)
            {
                Thread.Sleep(RetryDelayMs);
            }
        }
        return false;
    }

    public void Dispose()
    {
        if (_source == null) return;
        RemoveClipboardFormatListener(_source.Handle);
        _source.RemoveHook(WndProc);
        _source = null;
    }
}
