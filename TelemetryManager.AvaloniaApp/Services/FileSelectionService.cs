using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using TelemetryManager.ViewModels.ViewModelsServicesInterfaces;

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

    public async Task<string?> SelectFolderAndNameForNewFileAsync(
         string title,
         string defaultFileName,
         string fileTypeDescription,
         string[] patterns)
    {
        var folder = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        if (folder.Count == 0) return null;

        var saveDialog = new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = defaultFileName,
            FileTypeChoices = new[] { new FilePickerFileType(fileTypeDescription) { Patterns = patterns } },
            DefaultExtension = patterns.Length > 0 ? patterns[0].TrimStart('*') : null
        };

        var file = await _storageProvider.SaveFilePickerAsync(saveDialog);
        return file?.Path.AbsolutePath;
    }
}
