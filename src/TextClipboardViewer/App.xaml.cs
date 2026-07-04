using System.Windows;
using TextClipboardViewer.Services;

namespace TextClipboardViewer;

public partial class App : System.Windows.Application
{
    private Mutex? _mutex;
    private System.Windows.Forms.NotifyIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(initiallyOwned: true, "TextClipboardViewer_SingleInstance", out var createdNew);
        if (!createdNew)
        {
            // 既に起動中なら2つ目は黙って終了する
            Shutdown();
            return;
        }

        base.OnStartup(e);
        var main = new MainWindow();
        main.Show();
        SetupTrayIcon(main);
    }

    private void SetupTrayIcon(MainWindow main)
    {
        _trayIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = CreateAppIcon(),
            Text = "TextClipboardViewer",
            Visible = true,
        };
        var menu = new System.Windows.Forms.ContextMenuStrip();
        var toggleItem = menu.Items.Add(Loc.S("Tray_ToggleShow"), null, (_, _) =>
        {
            if (main.IsVisible) main.Hide();
            else main.Show();
        });
        var exitItem = menu.Items.Add(Loc.S("Tray_Exit"), null, (_, _) =>
        {
            main.AllowClose = true;
            main.Close();
            Shutdown();
        });
        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => main.Show();
        // WinForms のメニューは DynamicResource が効かないため、言語変更時に文言を差し替える
        main.Settings.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Models.AppSettings.Language))
            {
                toggleItem.Text = Loc.S("Tray_ToggleShow");
                exitItem.Text = Loc.S("Tray_Exit");
            }
        };
    }

    private static System.Drawing.Icon CreateAppIcon()
    {
        var bmp = new System.Drawing.Bitmap(16, 16);
        using (var g = System.Drawing.Graphics.FromImage(bmp))
        {
            g.Clear(System.Drawing.Color.Black);
            g.FillRectangle(System.Drawing.Brushes.Yellow, 0, 8, 16, 8);
            g.FillRectangle(System.Drawing.Brushes.Black, 2, 10, 12, 4);
        }
        return System.Drawing.Icon.FromHandle(bmp.GetHicon());
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
