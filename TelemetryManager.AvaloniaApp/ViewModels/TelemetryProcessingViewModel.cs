using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using TelemetryManager.Application.Requests;
using TelemetryManager.Application.Services;
using TelemetryManager.AvaloniaApp.ViewModels.ViewModelsServicesInterfaces;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.TelemetryPackets;

namespace TelemetryManager.AvaloniaApp.ViewModels;

public class TelemetryProcessingViewModel : ViewModelBase
{
    private readonly TelemetryProcessingService _telemetryProcessingService;
    private readonly IFileSelectionService _fileSelectionService;

    private PagedResponse<TelemetryPacket>? _currentPage;
    private string _statusMessage = string.Empty;
    private int _currentPageNumber = 1;
    private int _totalPages = 1;

    public TelemetryPacketFilterRequest Filter { get; } = new TelemetryPacketFilterRequest();

    public List<TelemetryPacket> Packets => _currentPage?.Data ?? new List<TelemetryPacket>();

    public int CurrentPage
    {
        get => _currentPageNumber;
        private set => this.RaiseAndSetIfChanged(ref _currentPageNumber, value);
    }

    public int TotalPages
    {
        get => _totalPages;
        private set => this.RaiseAndSetIfChanged(ref _totalPages, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public int PageSize
    {
        get => Filter.PageSize;
        set
        {
            if (value != Filter.PageSize)
            {
                Filter.PageSize = value;
                CurrentPage = 1;
                this.RaisePropertyChanged(nameof(PageSize));
                LoadPackets().ConfigureAwait(false);
            }
        }
    }

    public List<int> PageSizeOptions { get; } = new List<int> { 5, 10, 20, 50, 100 };

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
            // Обновляем номер страницы в фильтре
            Filter.PageNumber = CurrentPage;

            _currentPage = await _telemetryProcessingService.GetPacketsAsync(Filter);

           

            // Обновляем свойства после получения данных
            TotalPages = _currentPage.PageNumber;

            // Корректируем текущую страницу, если она превышает общее количество
            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
                await LoadPackets(); // Перезагружаем с корректной страницей
                return;
            }

            this.RaisePropertyChanged(nameof(Packets));
            this.RaisePropertyChanged(nameof(CurrentPage));
            this.RaisePropertyChanged(nameof(TotalPages));
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