using System.Text.Json;
using ZRScanner.Application.Ports;
using ZRScanner.Domain;

namespace ZRScanner.Infrastructure.Storage;

public sealed class LocalDocumentStorage(string rootPath) : IDocumentStorage
{
    private readonly string _root = rootPath;
    private string IndexPath => Path.Combine(_root, "documents.json");

    public async Task<Document> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_root);
        var safeName = Path.GetFileName(fileName);
        var storageKey = $"{Guid.NewGuid():N}-{safeName}";
        var path = Path.Combine(_root, storageKey);
        await using (var output = File.Create(path)) await content.CopyToAsync(output, cancellationToken);
        var info = new FileInfo(path);
        var document = Document.Create(safeName, info.Length, storageKey);
        var list = (await ReadIndexAsync(cancellationToken)).ToList();
        list.Insert(0, document);
        await File.WriteAllTextAsync(IndexPath, JsonSerializer.Serialize(list), cancellationToken);
        return document;
    }

    public async Task<IReadOnlyList<Document>> ListAsync(CancellationToken cancellationToken = default) => await ReadIndexAsync(cancellationToken);

    public Task<Stream?> OpenReadAsync(Document document, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var safeKey = Path.GetFileName(document.StorageKey);
        var path = string.IsNullOrWhiteSpace(safeKey) ? string.Empty : Path.Combine(_root, safeKey);
        if (string.IsNullOrWhiteSpace(safeKey))
        {
            // Backward compatibility for documents saved before StorageKey was added to the index.
            var safeName = Path.GetFileName(document.FileName);
            path = Directory.Exists(_root)
                ? Directory.GetFiles(_root, $"*-{safeName}").FirstOrDefault() ?? string.Empty
                : string.Empty;
        }
        return Task.FromResult<Stream?>(!string.IsNullOrEmpty(path) && File.Exists(path) ? File.OpenRead(path) : null);
    }

    private async Task<IReadOnlyList<Document>> ReadIndexAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(IndexPath)) return [];
        await using var input = File.OpenRead(IndexPath);
        return await JsonSerializer.DeserializeAsync<List<Document>>(input, cancellationToken: cancellationToken) ?? [];
    }
}