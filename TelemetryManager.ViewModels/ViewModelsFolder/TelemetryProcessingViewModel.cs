using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
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

    private PagedResponse<TelemetryPacket>? _currentPage;

    public TelemetryPacketFilterRequest Filter { get; } = new();

    public List<TelemetryPacket> Packets => _currentPage?.Data ?? new List<TelemetryPacket>();

    [Reactive] public int CurrentPage { get; private set; } = 1;
    [Reactive] public int TotalPages { get; private set; } = 1;
    [Reactive] public string StatusMessage { get; set; } = string.Empty;

    public int PageSize
    {
        get => Filter.PageSize;
        set
        {
            if (value == Filter.PageSize) return;

            Filter.PageSize = value;
            CurrentPage = 1;
            LoadPackets().ConfigureAwait(false);
        }
    }

    public List<int> PageSizeOptions { get; } = new() { 5, 10, 20, 50, 100 };

    public ReactiveCommand<Unit, Unit> SelectFileCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadPacketsCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; }

    public TelemetryProcessingViewModel(
        TelemetryProcessingService telemetryProcessingService,
        IFileSelectionService fileSelectionService)
    {
        _telemetryProcessingService = telemetryProcessingService;
        _fileSelectionService = fileSelectionService;

        // Инициализация команд
        SelectFileCommand = ReactiveCommand.CreateFromTask(SelectFile);
        LoadPacketsCommand = ReactiveCommand.CreateFromTask(LoadPackets);
        PreviousPageCommand = ReactiveCommand.Create(PreviousPage);
        NextPageCommand = ReactiveCommand.Create(NextPage);

        // Загрузка первой страницы
        LoadPackets().ConfigureAwait(false);
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
                await _telemetryProcessingService.ProcessTelemetryFile(filePath);
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
            Filter.PageNumber = CurrentPage;
            _currentPage = await _telemetryProcessingService.GetPacketsAsync(Filter);

            TotalPages = _currentPage.PageNumber;

            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
                await LoadPackets(); // Перезагружаем с корректной страницей
                return;
            }

            this.RaisePropertyChanged(nameof(Packets));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки данных: {ex.Message}";
        }
    }

    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            LoadPackets().ConfigureAwait(false);
        }
    }

    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            LoadPackets().ConfigureAwait(false);
        }
    }
}