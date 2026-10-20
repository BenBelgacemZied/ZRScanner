using ZRScanner.Domain;

namespace ZRScanner.Application.Ports;

public interface IDocumentStorage
{
    Task<Document> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> ListAsync(CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(Document document, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Document document, CancellationToken cancellationToken = default);
}