using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TextClipboardViewer.Models;

/// <summary>アプリの全設定。デフォルト値は「半透明ダーク」プリセット相当。</summary>
public class AppSettings : INotifyPropertyChanged
{
    private string _fontFamily = "Yu Gothic UI";
    private double _fontSize = 14;
    private string _textColor = "#F0F0F0";
    private string _backgroundColor = "#141419";
    private double _backgroundOpacity = 0.55;
    private bool _textShadow;
    private double _windowLeft = 100;
    private double _windowTop = 100;
    private double _windowWidth = 320;
    private double _windowHeight = 180;
    private string _language = ""; // 空 = 未設定(初回起動時にOSのUI言語から決める)

    public string FontFamily { get => _fontFamily; set => Set(ref _fontFamily, value); }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    public string TextColor { get => _textColor; set => Set(ref _textColor, value); }
    public string BackgroundColor { get => _backgroundColor; set => Set(ref _backgroundColor, value); }
    public double BackgroundOpacity { get => _backgroundOpacity; set => Set(ref _backgroundOpacity, value); }
    public bool TextShadow { get => _textShadow; set => Set(ref _textShadow, value); }
    public double WindowLeft { get => _windowLeft; set => Set(ref _windowLeft, value); }
    public double WindowTop { get => _windowTop; set => Set(ref _windowTop, value); }
    public double WindowWidth { get => _windowWidth; set => Set(ref _windowWidth, value); }
    public double WindowHeight { get => _windowHeight; set => Set(ref _windowHeight, value); }
    public string Language { get => _language; set => Set(ref _language, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
