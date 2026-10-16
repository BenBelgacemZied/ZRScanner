namespace ZRScanner.Application.Ports;

public interface IPdfDocumentWriter
{
    Task<byte[]> CreatePdfAsync(IReadOnlyList<Stream> pages, CancellationToken cancellationToken = default);
}