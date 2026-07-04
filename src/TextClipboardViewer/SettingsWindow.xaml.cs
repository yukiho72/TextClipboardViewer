using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TextClipboardViewer.Models;

namespace TextClipboardViewer;

public partial class SettingsWindow : Window
{
    private static readonly string[] TextColors =
        ["#FFFFFF", "#E8E8E8", "#222222", "#FFD866", "#8AD4FF", "#FF9E9E"];
    private static readonly string[] BackColors =
        ["#1E1E1E", "#141419", "#FAFAFA", "#20304A", "#3A1F2B"];
    private static readonly Regex HexPattern = new("^#[0-9A-Fa-f]{6}$");

    private readonly AppSettings _settings;
    private bool _loading = true;

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;

        FontCombo.ItemsSource = Fonts.SystemFontFamilies
            .Select(f => f.Source).OrderBy(s => s).ToList();
        BuildSwatches(TextColorSwatches, TextColors, hex => _settings.TextColor = hex);
        BuildSwatches(BackColorSwatches, BackColors, hex => _settings.BackgroundColor = hex);
        LoadFromSettings();
        _loading = false;
    }

    /// <summary>設定値をコントロールに反映(イベント発火は _loading で抑止)。</summary>
    private void LoadFromSettings()
    {
        _loading = true;
        OpacitySlider.Value = _settings.BackgroundOpacity * 100;
        FontSizeSlider.Value = _settings.FontSize;
        FontCombo.SelectedItem = _settings.FontFamily;
        TextColorHex.Text = _settings.TextColor;
        BackColorHex.Text = _settings.BackgroundColor;
        _loading = false;
    }

    private void BuildSwatches(WrapPanel panel, string[] colors, Action<string> apply)
    {
        foreach (var hex in colors)
        {
            var button = new Button
            {
                Width = 20,
                Height = 20,
                Margin = new Thickness(0, 0, 5, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0x60, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                Tag = hex,
            };
            button.Click += (_, _) =>
            {
                apply(hex);
                LoadFromSettings(); // Hexテキストボックス等の表示も同期する
            };
            panel.Children.Add(button);
        }
    }

    private void ApplyPreset(Action<AppSettings> preset)
    {
        preset(_settings);
        LoadFromSettings();
    }

    private void OnPresetDark(object sender, RoutedEventArgs e) => ApplyPreset(ThemePresets.ApplyDark);
    private void OnPresetLight(object sender, RoutedEventArgs e) => ApplyPreset(ThemePresets.ApplyLight);
    private void OnPresetTranslucent(object sender, RoutedEventArgs e) => ApplyPreset(ThemePresets.ApplyTranslucentDark);
    private void OnPresetTransparent(object sender, RoutedEventArgs e) => ApplyPreset(ThemePresets.ApplyTransparent);

    private void OnOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_loading) _settings.BackgroundOpacity = OpacitySlider.Value / 100.0;
    }

    private void OnFontSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_loading) _settings.FontSize = FontSizeSlider.Value;
    }

    private void OnFontChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_loading && FontCombo.SelectedItem is string font) _settings.FontFamily = font;
    }

    private void OnTextColorHexChanged(object sender, TextChangedEventArgs e)
    {
        if (!_loading && HexPattern.IsMatch(TextColorHex.Text)) _settings.TextColor = TextColorHex.Text;
    }

    private void OnBackColorHexChanged(object sender, TextChangedEventArgs e)
    {
        if (!_loading && HexPattern.IsMatch(BackColorHex.Text)) _settings.BackgroundColor = BackColorHex.Text;
    }

    private void OnLangJa(object sender, RoutedEventArgs e) => _settings.Language = "ja";
    private void OnLangEn(object sender, RoutedEventArgs e) => _settings.Language = "en";

    private void OnDragMove(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed && e.OriginalSource is not TextBox) DragMove();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
}
