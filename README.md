# TextClipboardViewer

クリップボードにコピーしたテキストを、枠なし・最前面の浮遊ウィンドウに常時表示する Windows 常駐アプリ。

*(English version: [README.en.md](README.en.md))*

## ダウンロード

[Releases](https://github.com/yukiho72/TextClipboardViewer/releases) から `TextClipboardViewer.exe` を入手してダブルクリックで起動します（.NET のインストールは不要）。

## 機能

- コピーした瞬間に表示を更新(イベント駆動、ポーリングなし)
- 枠なしウィンドウをどこを掴んでもドラッグで移動、端のドラッグでサイズ変更
- ホバーで表示される ⚙(設定)/ 🗑(クリップボードをクリア)
- 右クリックメニューから「非表示」「終了」
- テーマプリセット4種(ダーク / ライト / 半透明ダーク / 完全透明)
- フォント・文字サイズ・文字色・背景色・不透明度をスライダー等でリアルタイム調整
- 日本語 / 英語のUI切替(初回はOSの言語に追従)
- 設定は `%APPDATA%\TextClipboardViewer\settings.json` に自動保存
- タスクトレイ常駐(表示/非表示・終了)

## ソースからビルドと実行

```powershell
dotnet build -c Release
.\src\TextClipboardViewer\bin\Release\net8.0-windows\TextClipboardViewer.exe
```

## テスト

```powershell
dotnet test
```

## 自動起動したい場合

`Win+R` → `shell:startup` で開くフォルダに exe のショートカットを置く。

## ライセンス

[MIT License](LICENSE)
