namespace ZRScanner.Domain;

public sealed record Document(Guid Id, string FileName, DateTimeOffset CreatedAt, long SizeBytes, string StorageKey = "")
{
    public static Document Create(string fileName, long sizeBytes, string storageKey) =>
        new(Guid.NewGuid(), fileName, DateTimeOffset.UtcNow, sizeBytes, storageKey);
}