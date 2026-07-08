# TextClipboardViewer

A resident Windows app that always shows the text on your clipboard in a borderless, always-on-top floating window.

*(日本語版: [README.md](README.md))*

## Download

Get a zip from the [Releases](https://github.com/yukiho72/TextClipboardViewer/releases) page, extract it, and run `TextClipboardViewer.exe`. Two flavors are available:

| File | Size | Requirements |
|---|---|---|
| `TextClipboardViewer-*-standalone-win-x64.zip` | ~65MB | None (.NET bundled) |
| `TextClipboardViewer-*-net10-win-x64.zip` | ~85KB | [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) |

## Features

- Updates the moment you copy (event-driven, no polling)
- Drag anywhere on the borderless window to move it; drag the edges to resize
- Hover to reveal ⚙ (settings) / 🗑 (clear clipboard)
- Right-click menu for "Hide" and "Exit"
- Click-through (when enabled, the window passes mouse input through so you can operate the app behind it). Toggle from the settings panel or the system tray's right-click menu
- 4 theme presets (Dark / Light / Translucent / Transparent)
- Real-time adjustment of font, font size, text color, background color, and opacity via sliders
- Japanese / English UI (follows the OS language on first run)
- Settings auto-saved to `%APPDATA%\TextClipboardViewer\settings.json`
- Runs in the system tray. Right-click menu: show/hide, clear clipboard, test write, toggle click-through, exit
- Test write (writes a fixed string plus the current time to the clipboard). Confirms whether the window is showing and where it is, even in click-through mode with a transparent background

## Build and run from source

```powershell
dotnet build -c Release
.\src\TextClipboardViewer\bin\Release\net10.0-windows\TextClipboardViewer.exe
```

## Test

```powershell
dotnet test
```

## Run at startup

Put a shortcut to the exe in the folder opened by `Win+R` → `shell:startup`.

## License

[MIT License](LICENSE)
