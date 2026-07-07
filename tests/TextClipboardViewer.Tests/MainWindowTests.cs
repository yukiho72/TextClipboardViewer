using Xunit;

namespace TextClipboardViewer.Tests;

public class MainWindowTests
{
    private const int WsExTopmost = 0x00000008;
    private const int WsExTransparent = 0x00000020;

    [Fact]
    public void OSのTOPMOSTフラグが健在なら再アサートしない()
    {
        Assert.False(TextClipboardViewer.MainWindow.ShouldReassertTopmost(WsExTopmost));
        // 他の拡張スタイルが立っていてもTOPMOSTがあれば復帰不要
        Assert.False(TextClipboardViewer.MainWindow.ShouldReassertTopmost(WsExTopmost | WsExTransparent));
    }

    [Fact]
    public void OSのTOPMOSTフラグが失われていたら再アサートする()
    {
        Assert.True(TextClipboardViewer.MainWindow.ShouldReassertTopmost(0));
        Assert.True(TextClipboardViewer.MainWindow.ShouldReassertTopmost(WsExTransparent));
    }
}
