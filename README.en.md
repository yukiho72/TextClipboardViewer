# TextClipboardViewer

A resident Windows app that always shows the text on your clipboard in a borderless, always-on-top floating window.

*(日本語版: [README.md](README.md))*

## Download

Get a zip from the [Releases](https://github.com/yukiho72/TextClipboardViewer/releases) page, extract it, and run `TextClipboardViewer.exe`. Two flavors are available:

| File | Size | Requirements |
|---|---|---|
| `TextClipboardViewer-*-standalone-win-x64.zip` | ~65MB | None (.NET bundled) |
| `TextClipboardViewer-*-net8-win-x64.zip` | ~85KB | [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |

## Features

- Updates the moment you copy (event-driven, no polling)
- Drag anywhere on the borderless window to move it; drag the edges to resize
- Hover to reveal ⚙ (settings) / 🗑 (clear clipboard)
- Right-click menu for "Hide" and "Exit"
- 4 theme presets (Dark / Light / Translucent / Transparent)
- Real-time adjustment of font, font size, text color, background color, and opacity via sliders
- Japanese / English UI (follows the OS language on first run)
- Settings auto-saved to `%APPDATA%\TextClipboardViewer\settings.json`
- Runs in the system tray (show/hide, exit)

## Build and run from source

```powershell
dotnet build -c Release
.\src\TextClipboardViewer\bin\Release\net8.0-windows\TextClipboardViewer.exe
```

## Test

```powershell
dotnet test
```

## Run at startup

Put a shortcut to the exe in the folder opened by `Win+R` → `shell:startup`.

## License

[MIT License](LICENSE)
