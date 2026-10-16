using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using ZRScanner.Application.Ports;

namespace ZRScanner.Infrastructure.Pdf;

public sealed class PdfDocumentWriter : IPdfDocumentWriter
{
    public async Task<byte[]> CreatePdfAsync(IReadOnlyList<Stream> pages, CancellationToken cancellationToken = default)
    {
        using var document = new PdfDocument();
        foreach (var imageStream in pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var buffer = new MemoryStream();
            await imageStream.CopyToAsync(buffer, cancellationToken);
            using var image = XImage.FromStream(() => new MemoryStream(buffer.ToArray()));
            var page = document.AddPage();
            page.Width = image.PixelWidth;
            page.Height = image.PixelHeight;
            using var gfx = XGraphics.FromPdfPage(page);
            gfx.DrawImage(image, 0, 0, page.Width, page.Height);
        }
        using var output = new MemoryStream();
        document.Save(output, false);
        return output.ToArray();
    }
}