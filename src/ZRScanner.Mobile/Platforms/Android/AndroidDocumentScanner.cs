using NativeDocumentScanner = Plugin.Maui.DocumentScanner.IDocumentScanner;
using Plugin.Maui.DocumentScanner;

namespace ZRScanner.Mobile.Platforms.Android;

/// <summary>Uses ML Kit's native document scanner for edge detection and perspective correction.</summary>
public sealed class AndroidDocumentScanner(NativeDocumentScanner scanner) : ZRScanner.Application.Ports.IDocumentScanner
{
    public async Task<IReadOnlyList<Stream>> ScanPagesAsync(CancellationToken cancellationToken = default)
    {
        var paths = await scanner.ScanAsync(new DocumentScanOptions
        {
            PageLimit = 20,
            Mode = DocumentScannerMode.Full
        }, cancellationToken);

        var pages = new List<Stream>(paths.Count);
        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pages.Add(File.OpenRead(path));
        }
        return pages;
    }
}