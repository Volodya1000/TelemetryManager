using System.Threading.Tasks;

namespace TelemetryManager.AvaloniaApp.ViewModels.ViewModelsServicesInterfaces;

public interface IFileSelectionService
{
    Task<string?> SelectFileAsync(string title, string fileTypeDescription, string[] patterns);
}