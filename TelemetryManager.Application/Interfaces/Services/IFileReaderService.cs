namespace TelemetryManager.Application.Interfaces.Services;

public interface IFileReaderService
{
    public Stream OpenRead(string filePath);
}
