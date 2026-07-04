using System;
using System.IO;
using TextClipboardViewer.Models;
using TextClipboardViewer.Services;
using Xunit;

namespace TextClipboardViewer.Tests;

public class SettingsServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "TCVTest_" + Guid.NewGuid().ToString("N"));
    private string SettingsPath => Path.Combine(_dir, "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    [Fact]
    public void ファイルがなければデフォルト設定を返す()
    {
        var svc = new SettingsService(SettingsPath);
        var s = svc.Load();
        Assert.Equal("#141419", s.BackgroundColor);
        Assert.Equal(0.55, s.BackgroundOpacity);
    }

    [Fact]
    public void 保存して読み込むと値が復元される()
    {
        var svc = new SettingsService(SettingsPath);
        var s = new AppSettings
        {
            FontFamily = "MS Gothic",
            FontSize = 22,
            TextColor = "#FFD866",
            BackgroundColor = "#20304A",
            BackgroundOpacity = 0.8,
            TextShadow = true,
            WindowLeft = 50, WindowTop = 60, WindowWidth = 400, WindowHeight = 250,
            Language = "en",
        };
        svc.Save(s);

        var loaded = svc.Load();
        Assert.Equal("MS Gothic", loaded.FontFamily);
        Assert.Equal(22, loaded.FontSize);
        Assert.Equal("#FFD866", loaded.TextColor);
        Assert.Equal("#20304A", loaded.BackgroundColor);
        Assert.Equal(0.8, loaded.BackgroundOpacity);
        Assert.True(loaded.TextShadow);
        Assert.Equal(50, loaded.WindowLeft);
        Assert.Equal(60, loaded.WindowTop);
        Assert.Equal(400, loaded.WindowWidth);
        Assert.Equal(250, loaded.WindowHeight);
        Assert.Equal("en", loaded.Language);
    }

    [Fact]
    public void 壊れたファイルならデフォルト設定を返す()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(SettingsPath, "{ これはJSONではない");
        var svc = new SettingsService(SettingsPath);
        var s = svc.Load();
        Assert.Equal("#141419", s.BackgroundColor);
    }

    [Fact]
    public void 保存先がロックされていても例外を投げない()
    {
        var svc = new SettingsService(SettingsPath);
        Directory.CreateDirectory(_dir);
        using var locked = new FileStream(SettingsPath, FileMode.Create, FileAccess.Write, FileShare.None);
        var ex = Record.Exception(() => svc.Save(new AppSettings()));
        Assert.Null(ex);
    }

    [Fact]
    public void 文字列プロパティがnullや空ならデフォルト設定を返す()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(SettingsPath, """{"FontFamily":null,"TextColor":"","BackgroundColor":"#20304A"}""");
        var svc = new SettingsService(SettingsPath);
        var s = svc.Load();
        Assert.Equal("Yu Gothic UI", s.FontFamily);
        Assert.Equal("#F0F0F0", s.TextColor);
        Assert.Equal("#141419", s.BackgroundColor);
    }
}
