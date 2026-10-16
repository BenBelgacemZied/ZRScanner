namespace ZRScanner.Application.Ports;

public interface IDocumentScanner
{
    Task<IReadOnlyList<Stream>> ScanPagesAsync(CancellationToken cancellationToken = default);
}