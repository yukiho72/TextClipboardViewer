using System.Windows;

namespace TextClipboardViewer.Services;

/// <summary>言語リソース(Lang.*.xaml)の切り替え。DynamicResource参照が即時追従する。</summary>
public static class LanguageService
{
    private static ResourceDictionary? _currentDict;
    private static string? _currentLanguage;

    public static void Apply(string language)
    {
        // 未知の値(手編集など)は英語にフォールバック
        if (language != "ja") language = "en";
        // 設定のPropertyChangedごとに呼ばれるため、同じ言語なら何もしない
        if (language == _currentLanguage) return;

        var uri = new Uri($"pack://application:,,,/Resources/Lang/Lang.{language}.xaml");
        var dict = new ResourceDictionary { Source = uri };
        var appRes = System.Windows.Application.Current.Resources;
        if (_currentDict != null)
            appRes.MergedDictionaries.Remove(_currentDict);
        appRes.MergedDictionaries.Add(dict);
        _currentDict = dict;
        _currentLanguage = language;
    }
}

/// <summary>コードビハインドから現在言語の文字列を引くヘルパー。</summary>
public static class Loc
{
    public static string S(string key)
        => System.Windows.Application.Current.Resources[key] as string ?? $"[{key}]";
}
