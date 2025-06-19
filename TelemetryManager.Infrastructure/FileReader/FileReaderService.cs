using TelemetryManager.Application.Interfaces.Services;

namespace TelemetryManager.Infrastructure.FileReader;

public class FileReaderService : IFileReaderService
{
    public Stream OpenRead(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File {filePath} does not exist.", filePath);

        return File.OpenRead(filePath);
    }
}
