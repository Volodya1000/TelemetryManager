using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.Requests;
using TelemetryManager.Application.Services;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.TelemetryPackets;
using TelemetryManager.ViewModels.ViewModelsServicesInterfaces;

namespace TelemetryManager.ViewModels.ViewModelsFolder;

public class TelemetryProcessingViewModel : ReactiveObject
{
    private readonly TelemetryProcessingService _telemetryProcessingService;
    private readonly IFileSelectionService _fileSelectionService;
    private readonly IFileReaderService _fileReaderService;

    private PagedResponse<TelemetryPacket>? _currentPage;

    public TelemetryPacketFilterRequest Filter { get; } = new();

    public List<TelemetryPacket> Packets => _currentPage?.Data ?? new List<TelemetryPacket>();

    [Reactive] public int CurrentPage { get; private set; } = 1;
    [Reactive] public int TotalPages { get; private set; } = 1;
    [Reactive] public string StatusMessage { get; set; } = string.Empty;
    [Reactive] public bool HasData { get; private set; }

    public int PageSize
    {
        get => Filter.PageSize;
        set
        {
            if (value == Filter.PageSize) return;

            Filter.PageSize = value;
            CurrentPage = 1;
            _ = LoadPackets();
        }
    }

    public List<int> PageSizeOptions { get; } = new() { 5, 10, 20, 50, 100 };

    public ReactiveCommand<Unit, Unit> SelectFileCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadPacketsCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; }

    public TelemetryProcessingViewModel(
        TelemetryProcessingService telemetryProcessingService,
        IFileSelectionService fileSelectionService,
        IFileReaderService fileReaderService)
    {
        _telemetryProcessingService = telemetryProcessingService;
        _fileSelectionService = fileSelectionService;
        _fileReaderService = fileReaderService;

        SelectFileCommand = ReactiveCommand.CreateFromTask(SelectFile);
        LoadPacketsCommand = ReactiveCommand.CreateFromTask(LoadPackets);
        PreviousPageCommand = ReactiveCommand.Create(PreviousPage, this.WhenAnyValue(vm => vm.CurrentPage, page => page > 1));
        NextPageCommand = ReactiveCommand.Create(NextPage, this.WhenAnyValue(vm => vm.CurrentPage, vm => vm.TotalPages, (page, total) => page < total));

        // Initial load
        _ = LoadPackets();
    }

    private async Task SelectFile()
    {
        try
        {
            var filePath = await _fileSelectionService.SelectFileAsync(
                "Выберите файл .bin",
                "Binary Files",
                new[] { "*.bin" });

            if (!string.IsNullOrEmpty(filePath))
            {
                await using var stream = _fileReaderService.OpenRead(filePath);
                await _telemetryProcessingService.ProcessTelemetryStream(stream);

                StatusMessage = "Файл успешно обработан";
                await LoadPackets();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    private async Task LoadPackets()
    {
        try
        {
            if (CurrentPage < 1) CurrentPage = 1;

            Filter.PageNumber = CurrentPage;
            _currentPage = await _telemetryProcessingService.GetPacketsAsync(Filter);

            TotalPages = _currentPage.TotalPages;

            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
                await LoadPackets();
                return;
            }

            this.RaisePropertyChanged(nameof(Packets));
            HasData = _currentPage.Data?.Count > 0;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки данных: {ex.Message}";
            HasData = false;
        }
    }

    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            _ = LoadPackets();
        }
    }

    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            _ = LoadPackets();
        }
    }
}