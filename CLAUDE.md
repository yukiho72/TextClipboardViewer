# TextClipboardViewer

クリップボードのテキストを、枠なし・最前面の浮遊ウィンドウに表示する Windows 常駐アプリ（C# / WPF）。

## ビルドとテスト

```powershell
dotnet build
dotnet test
```

**ビルド前に起動中の `TextClipboardViewer.exe` を停止すること。** 常駐アプリなので出力DLLを掴んだままになり、`MSB3021`（ファイルをコピーできません）でビルドが失敗する。

```powershell
Get-Process TextClipboardViewer -ErrorAction Ignore | Stop-Process -Force
```

## リリース

**手作業でビルド・zip・`gh release create` を並べないこと。** `scripts/release.ps1` を使う。テスト実行・main の状態確認・両版（standalone / 軽量）のビルド・zip・日英リリースノート付きの Release 作成までを一括で行う。

```powershell
./scripts/release.ps1 -Version v1.0.6 -SummaryJa "○○を修正しました。" -SummaryEn "Fixed XX."
./scripts/release.ps1 -Version v1.0.6 -SummaryJa "..." -SummaryEn "..." -DryRun   # 確認のみ
```

ターゲットフレームワークは csproj から読み取るため、.NET を上げてもスクリプトの修正は不要。

## 設計ドキュメントは公開しない

`docs/` は `.gitignore` 済み。設計書・実装計画はローカル専用で、公開リポジトリには載せない。ソース・両README・LICENSE のみを公開する。

## ローカライズ

文言を足すときは **`Resources/Lang/Lang.ja.xaml` と `Lang.en.xaml` の両方**に同じキーを追加する。

- XAML 側は `{DynamicResource キー}` で書けば言語切替に自動追従する。
- **タスクトレイのメニューは WinForms のため `DynamicResource` が効かない。** `App.xaml.cs` で `AppSettings.Language` の `PropertyChanged` を購読し、各項目の `Text` を手動で差し替えている。トレイに項目を足したらここも直すこと。

## 設定

`AppSettings`（`INotifyPropertyChanged`）が唯一の状態源。プロパティを変えると `MainWindow.ApplySettings()` が即時に見た目へ反映し、500ms デバウンスで `%APPDATA%\TextClipboardViewer\settings.json` に保存される。トレイと設定画面はこのプロパティを反転させるだけでよい。

## 姉妹プロジェクト

`T:\ai\DateTimeClipper` は同じ構成の WPF トレイ常駐アプリ。最前面の再アサートなど、共通の不具合と対処を相互に流用できる。
