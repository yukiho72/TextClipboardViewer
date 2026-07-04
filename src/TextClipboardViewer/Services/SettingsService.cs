using System.IO;
using System.Text.Json;
using TextClipboardViewer.Models;

namespace TextClipboardViewer.Services;

/// <summary>settings.json の読み書き。読み込み失敗・不正な内容はデフォルト設定にフォールバックする。</summary>
public class SettingsService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _path;

    public SettingsService(string path) => _path = path;

    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TextClipboardViewer", "settings.json");

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_path)) return new AppSettings();
            var loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path));
            if (loaded is null || HasInvalidStrings(loaded)) return new AppSettings();
            return loaded;
        }
        catch (Exception e) when (e is JsonException or IOException or UnauthorizedAccessException)
        {
            return new AppSettings();
        }
    }

    /// <summary>手編集などで文字列プロパティが null/空になったファイルは「壊れている」とみなす。</summary>
    private static bool HasInvalidStrings(AppSettings s) =>
        string.IsNullOrEmpty(s.FontFamily) ||
        string.IsNullOrEmpty(s.TextColor) ||
        string.IsNullOrEmpty(s.BackgroundColor);

    public void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(settings, Options));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // 保存失敗で常駐アプリを落とさない。次回の保存で回復する
        }
    }
}
