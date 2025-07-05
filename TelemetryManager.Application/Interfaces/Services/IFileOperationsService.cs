namespace TelemetryManager.Application.Interfaces.Services;

public interface IFileOperationsService
{
    Stream OpenRead(string filePath);
    Task WriteStreamToFileAsync(string filePath, Stream data);
    Task WriteBytesToFileAsync(string filePath, byte[] data);
    bool FileExists(string filePath);
}
