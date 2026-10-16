namespace ZRScanner.Application.Ports;

/// <summary>Cloud adapter boundary. Google Drive will be called through the authenticated backend API.</summary>
public interface ICloudDocumentStorage
{
    Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken = default);
}