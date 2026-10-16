using ZRScanner.Application.Ports;
using ZRScanner.Domain;

namespace ZRScanner.Application.UseCases;

public sealed class ScanAndSaveDocument(IDocumentScanner scanner, IPdfDocumentWriter pdfWriter, IDocumentStorage storage)
{
    public async Task<Document?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var pages = await scanner.ScanPagesAsync(cancellationToken);
        if (pages.Count == 0) return null;

        try
        {
            await using var pdf = new MemoryStream(await pdfWriter.CreatePdfAsync(pages, cancellationToken));
            return await storage.SaveAsync($"document-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf", pdf, cancellationToken);
        }
        finally
        {
            foreach (var page in pages)
                await page.DisposeAsync();
        }
    }
}