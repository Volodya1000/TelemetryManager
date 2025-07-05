using TelemetryManager.Application.Interfaces.Services;

namespace TelemetryManager.Infrastructure.FileReader;

public class FileOperationsService : IFileOperationsService
{
    public Stream OpenRead(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File {filePath} does not exist.", filePath);

        return File.OpenRead(filePath);
    }

    public async Task WriteStreamToFileAsync(string filePath, Stream data)
    {
        await using var fileStream = File.Create(filePath);
        await data.CopyToAsync(fileStream);
    }

    public async Task WriteBytesToFileAsync(string filePath, byte[] data)
    {
        await File.WriteAllBytesAsync(filePath, data);
    }

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }
}
