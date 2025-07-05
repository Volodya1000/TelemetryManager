namespace TelemetryManager.ViewModels.ViewModelsServicesInterfaces;

public interface IFileSelectionService
{
    Task<string?> SelectFileAsync(string title, string fileTypeDescription, string[] patterns);
    Task<string?> SelectFolderAndNameForNewFileAsync(
        string title,
        string defaultFileName,
        string fileTypeDescription,
        string[] patterns);
}