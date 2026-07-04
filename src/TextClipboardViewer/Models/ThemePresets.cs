namespace TextClipboardViewer.Models;

/// <summary>色・透明度・文字影だけを変えるプリセット。フォント設定には触らない。</summary>
public static class ThemePresets
{
    public static void ApplyDark(AppSettings s)
    {
        s.BackgroundColor = "#1E1E1E";
        s.TextColor = "#E8E8E8";
        s.BackgroundOpacity = 1.0;
        s.TextShadow = false;
    }

    public static void ApplyLight(AppSettings s)
    {
        s.BackgroundColor = "#FAFAFA";
        s.TextColor = "#222222";
        s.BackgroundOpacity = 1.0;
        s.TextShadow = false;
    }

    public static void ApplyTranslucentDark(AppSettings s)
    {
        s.BackgroundColor = "#141419";
        s.TextColor = "#F0F0F0";
        s.BackgroundOpacity = 0.55;
        s.TextShadow = false;
    }

    public static void ApplyTransparent(AppSettings s)
    {
        s.BackgroundColor = "#000000";
        s.TextColor = "#FFFFFF";
        s.BackgroundOpacity = 0.0;
        s.TextShadow = true;
    }
}
