<#
.SYNOPSIS
    TextClipboardViewer のリリースを一括で行う。
    テスト実行 → 起動中exeの停止 → 2種類のexeをビルド → zip作成 → GitHub Release 作成。

.DESCRIPTION
    ターゲットフレームワークは csproj から読み取るため、.NET のバージョンを上げても
    このスクリプトを直す必要はない(zip名やランタイムのリンクも自動で追従する)。

.EXAMPLE
    ./scripts/release.ps1 -Version v1.0.6 -SummaryJa "○○を修正しました。" -SummaryEn "Fixed XX."

.EXAMPLE
    # リリースを作らずに、ビルドと生成されるリリースノートだけ確認する
    ./scripts/release.ps1 -Version v1.0.6 -SummaryJa "..." -SummaryEn "..." -DryRun
#>
param(
    # v1.2.3 形式
    [Parameter(Mandatory)]
    [ValidatePattern('^v\d+\.\d+\.\d+$')]
    [string]$Version,

    # リリースノート冒頭の要約(日本語 / 英語)
    [Parameter(Mandatory)][string]$SummaryJa,
    [Parameter(Mandatory)][string]$SummaryEn,

    [string]$Repo = 'yukiho72/TextClipboardViewer',

    # main がクリーン & push 済みかの確認を省略する
    [switch]$AllowDirty,
    [switch]$SkipTests,
    # ビルドとノート生成だけ行い、GitHub Release は作らない
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$csproj   = Join-Path $repoRoot 'src/TextClipboardViewer/TextClipboardViewer.csproj'
$binDir   = Join-Path $repoRoot 'src/TextClipboardViewer/bin'

function Invoke-Checked([string]$What, [scriptblock]$Command) {
    & $Command
    if ($LASTEXITCODE -ne 0) { throw "$What に失敗しました (exit $LASTEXITCODE)" }
}

# --- リリース対象が main の最新であることを確認する ---------------------------
if (-not $AllowDirty) {
    $branch = (git -C $repoRoot rev-parse --abbrev-ref HEAD).Trim()
    if ($branch -ne 'main') { throw "main 以外のブランチです: $branch (-AllowDirty で回避可)" }
    if (git -C $repoRoot status --porcelain) { throw 'コミットされていない変更があります (-AllowDirty で回避可)' }
    git -C $repoRoot fetch origin main --quiet
    if ((git -C $repoRoot rev-parse HEAD).Trim() -ne (git -C $repoRoot rev-parse origin/main).Trim()) {
        throw 'origin/main と一致していません。先に push してください (-AllowDirty で回避可)'
    }
}

# --- csproj からターゲットフレームワークを読み取る(net10.0-windows → net10 / 10) ---
$tfm = (Select-Xml -Path $csproj -XPath '//TargetFramework' | Select-Object -First 1).Node.InnerText
if ($tfm -notmatch '^net(?<major>\d+)\.\d+-windows$') { throw "TargetFramework を解釈できません: $tfm" }
$major = $Matches['major']
$fxTag = "net$major"
$runtimeUrl = "https://dotnet.microsoft.com/download/dotnet/$major.0"
Write-Host "ターゲット: $tfm (タグ: $fxTag)" -ForegroundColor Cyan

if (-not $SkipTests) {
    Write-Host 'テストを実行中...' -ForegroundColor Cyan
    Invoke-Checked 'dotnet test' { dotnet test (Join-Path $repoRoot 'TextClipboardViewer.sln') }
}

# --- 起動中だと出力ファイルがロックされ MSB3021 でビルドが失敗する ---------------
$running = Get-Process -Name 'TextClipboardViewer' -ErrorAction Ignore
if ($running) {
    Write-Host '起動中の TextClipboardViewer.exe を停止します' -ForegroundColor Yellow
    $running | Stop-Process -Force
    Start-Sleep -Milliseconds 500
}

$pubStandalone = Join-Path $binDir 'pub-standalone'
$pubFramework  = Join-Path $binDir "pub-$fxTag"

Write-Host 'standalone 版(.NET同梱)をビルド中...' -ForegroundColor Cyan
Invoke-Checked 'standalone の publish' {
    dotnet publish $csproj -c Release -r win-x64 --self-contained true `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $pubStandalone
}

Write-Host "$fxTag 版(軽量)をビルド中..." -ForegroundColor Cyan
Invoke-Checked "$fxTag の publish" {
    dotnet publish $csproj -c Release -r win-x64 --self-contained false `
        -p:PublishSingleFile=true -o $pubFramework
}

$zipStandalone = Join-Path $binDir "TextClipboardViewer-$Version-standalone-win-x64.zip"
$zipFramework  = Join-Path $binDir "TextClipboardViewer-$Version-$fxTag-win-x64.zip"
Compress-Archive -Path (Join-Path $pubStandalone 'TextClipboardViewer.exe') -DestinationPath $zipStandalone -Force
Compress-Archive -Path (Join-Path $pubFramework  'TextClipboardViewer.exe') -DestinationPath $zipFramework  -Force

$nameStandalone = Split-Path -Leaf $zipStandalone
$nameFramework  = Split-Path -Leaf $zipFramework
$sizeStandalone = [math]::Round((Get-Item $zipStandalone).Length / 1MB)
$sizeFramework  = [math]::Round((Get-Item $zipFramework).Length / 1KB)
Write-Host "$nameStandalone : 約${sizeStandalone}MB" -ForegroundColor Green
Write-Host "$nameFramework : 約${sizeFramework}KB" -ForegroundColor Green

# markdown のコードスパン用のバッククォート(here-string 内では escape 文字になるため変数にする)
$bt = [string][char]96

$notes = @"
$SummaryJa

zipを展開して ${bt}TextClipboardViewer.exe${bt} を起動してください。用途に合わせて2種類から選べます。

## ダウンロード / Download

| ファイル | サイズ | 必要環境 |
|---|---|---|
| ${bt}$nameStandalone${bt} | 約${sizeStandalone}MB | なし(.NET同梱) |
| ${bt}$nameFramework${bt} | 約${sizeFramework}KB | [.NET $major デスクトップランタイム]($runtimeUrl)が必要 |

---

$SummaryEn

Extract the zip and run ${bt}TextClipboardViewer.exe${bt}. Choose whichever suits your environment:

- ${bt}$nameStandalone${bt} (~${sizeStandalone}MB) — standalone, no .NET installation required
- ${bt}$nameFramework${bt} (~${sizeFramework}KB) — lightweight, requires the [.NET $major Desktop Runtime]($runtimeUrl)
"@

if ($DryRun) {
    Write-Host "`n--- DryRun: 以下のノートで Release $Version を作成します(実際には作成しません) ---`n" -ForegroundColor Yellow
    Write-Host $notes
    return
}

Write-Host "GitHub Release $Version を作成中..." -ForegroundColor Cyan
Invoke-Checked 'gh release create' {
    gh release create $Version $zipStandalone $zipFramework -R $Repo -t "TextClipboardViewer $Version" -n $notes
}
Write-Host "完了: https://github.com/$Repo/releases/tag/$Version" -ForegroundColor Green
