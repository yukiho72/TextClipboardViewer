using TextClipboardViewer.Models;
using Xunit;

namespace TextClipboardViewer.Tests;

public class ThemePresetsTests
{
    [Fact]
    public void デフォルト設定は半透明ダーク()
    {
        var s = new AppSettings();
        Assert.Equal("#141419", s.BackgroundColor);
        Assert.Equal("#F0F0F0", s.TextColor);
        Assert.Equal(0.55, s.BackgroundOpacity);
        Assert.False(s.TextShadow);
    }

    [Fact]
    public void ダーク適用で不透明な暗色になる()
    {
        var s = new AppSettings();
        ThemePresets.ApplyDark(s);
        Assert.Equal("#1E1E1E", s.BackgroundColor);
        Assert.Equal("#E8E8E8", s.TextColor);
        Assert.Equal(1.0, s.BackgroundOpacity);
        Assert.False(s.TextShadow);
    }

    [Fact]
    public void ライト適用で明色になる()
    {
        var s = new AppSettings();
        ThemePresets.ApplyLight(s);
        Assert.Equal("#FAFAFA", s.BackgroundColor);
        Assert.Equal("#222222", s.TextColor);
        Assert.Equal(1.0, s.BackgroundOpacity);
        Assert.False(s.TextShadow);
    }

    [Fact]
    public void 完全透明適用で背景0かつ文字影あり()
    {
        var s = new AppSettings();
        ThemePresets.ApplyTransparent(s);
        Assert.Equal(0.0, s.BackgroundOpacity);
        Assert.Equal("#FFFFFF", s.TextColor);
        Assert.True(s.TextShadow);
    }

    [Fact]
    public void プリセット適用でフォント設定は変わらない()
    {
        var s = new AppSettings { FontFamily = "MS Gothic", FontSize = 20 };
        ThemePresets.ApplyDark(s);
        Assert.Equal("MS Gothic", s.FontFamily);
        Assert.Equal(20, s.FontSize);
    }

    [Fact]
    public void プロパティ変更でPropertyChangedが発火する()
    {
        var s = new AppSettings();
        string? changed = null;
        s.PropertyChanged += (_, e) => changed = e.PropertyName;
        s.FontSize = 30;
        Assert.Equal(nameof(AppSettings.FontSize), changed);
    }
}
