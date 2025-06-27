using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using TelemetryManager.AvaloniaApp.ViewModels.ViewModelsServicesInterfaces;

namespace TelemetryManager.AvaloniaApp.Services;

public class AvaloniaFileSelectionService : IFileSelectionService
{
    private readonly IStorageProvider _storageProvider;

    public AvaloniaFileSelectionService(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public async Task<string?> SelectFileAsync(string title, string fileTypeDescription, string[] patterns)
    {
        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            FileTypeFilter = new[] { new FilePickerFileType(fileTypeDescription) { Patterns = patterns } },
            AllowMultiple = false
        });

        return files.Count > 0 ? files[0].Path.AbsolutePath : null;
    }
}
