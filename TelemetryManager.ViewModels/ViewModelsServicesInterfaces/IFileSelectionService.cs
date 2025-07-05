namespace TelemetryManager.ViewModels.ViewModelsServicesInterfaces;

public interface IFileSelectionService
{
    Task<string?> SelectFileAsync(string title, string fileTypeDescription, string[] patterns);
    public Task<string?> SelectFilePathForNewFileAsync(
        string title,
        string defaultFileName,
        string fileTypeDescription,
        string[] patterns);
}