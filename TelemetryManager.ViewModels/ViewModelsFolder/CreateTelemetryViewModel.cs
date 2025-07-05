using ReactiveUI;
using System.Reactive;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.ViewModels.ViewModelsServicesInterfaces;

namespace TelemetryManager.ViewModels.ViewModelsFolder;

public class CreateTelemetryViewModel : ReactiveObject
{
    private readonly ITelemetryGenerator _telemetryGenerator;
    private readonly IFileSelectionService _fileSelectionService;
    private readonly IFileOperationsService _fileOperationsService;

    private int _packetsCount = 10;
    private double _noiseRatio = 0.2;
    private double _validityRatio = 0.5;

    public int PacketsCount
    {
        get => _packetsCount;
        set => this.RaiseAndSetIfChanged(ref _packetsCount, value > 0 ? value : _packetsCount);
    }

    public double NoiseRatio
    {
        get => _noiseRatio;
        set => this.RaiseAndSetIfChanged(ref _noiseRatio,
               value >= 0 && value <= 1 ? value : _noiseRatio);
    }

    public double ValidityRatio
    {
        get => _validityRatio;
        set => this.RaiseAndSetIfChanged(ref _validityRatio,
               value >= 0 && value <= 1 ? value : _validityRatio);
    }

    public ReactiveCommand<Unit, Unit> CreateCommand { get; }

    public CreateTelemetryViewModel(ITelemetryGenerator telemetryGenerator,
                                  IFileSelectionService fileSelectionService,
                                  IFileOperationsService fileOperationsService)
    {
        _telemetryGenerator = telemetryGenerator;
        _fileSelectionService = fileSelectionService;
        _fileOperationsService = fileOperationsService;

        // Проверка валидности параметров перед выполнением команды
        var canExecute = this.WhenAnyValue(
            x => x.PacketsCount,
            x => x.NoiseRatio,
            x => x.ValidityRatio,
            (pc, nr, vr) => pc > 0 && nr >= 0 && nr <= 1 && vr >= 0 && vr <= 1);

        CreateCommand = ReactiveCommand.CreateFromTask(Create, canExecute);
    }

    private async Task Create()
    {
        var filePath = await _fileSelectionService.SelectFilePathForNewFileAsync(
            "Сохранить телеметрию",
            $"telemetry_{DateTime.Now:yyyyMMdd_HHmmss}.bin",
            "Бинарные файлы телеметрии",
            new[] { "*.bin" });

        if (filePath != null)
        {
            var bytes = await _telemetryGenerator.GeneratePackets(
                PacketsCount,
                NoiseRatio,
                ValidityRatio);

            await _fileOperationsService.WriteBytesToFileAsync(filePath, bytes);
        }
    }
}