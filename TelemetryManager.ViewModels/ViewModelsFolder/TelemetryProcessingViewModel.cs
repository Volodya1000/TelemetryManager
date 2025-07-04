using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.OutputDtos;
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

    public TelemetryFilterViewModel Filter { get; } = new();

    private PagedResponse<TelemetryPacketDto>? _currentPage;

    public List<TelemetryPacketDto> Packets => _currentPage?.Data ?? new List<TelemetryPacketDto>();

    [Reactive] public int CurrentPage { get; private set; } = 1;
    [Reactive] public int TotalPages { get; private set; } = 1;
    [Reactive] public string StatusMessage { get; set; } = string.Empty;
    [Reactive] public bool HasData { get; private set; }
    [Reactive] public bool HasFileLoaded { get; private set; }

    public ReactiveCommand<Unit, Unit> SelectFileCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadPacketsCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterChangedCommand { get; }

    public TelemetryProcessingViewModel(
        TelemetryProcessingService telemetryProcessingService,
        IFileSelectionService fileSelectionService,
        IFileReaderService fileReaderService)
    {
        _telemetryProcessingService = telemetryProcessingService;
        _fileSelectionService = fileSelectionService;
        _fileReaderService = fileReaderService;

        FilterChangedCommand = ReactiveCommand.CreateFromTask(HandleFilterChanged);
        SelectFileCommand = ReactiveCommand.CreateFromTask(SelectFile);
        LoadPacketsCommand = ReactiveCommand.CreateFromTask(LoadPackets);
        PreviousPageCommand = ReactiveCommand.Create(PreviousPage,
            this.WhenAnyValue(vm => vm.CurrentPage, page => page > 1));
        NextPageCommand = ReactiveCommand.Create(NextPage,
            this.WhenAnyValue(vm => vm.CurrentPage, vm => vm.TotalPages, (page, total) => page < total));

        // Реакция на изменение размера страницы
        Filter.WhenAnyValue(x => x.PageSize)
            .Skip(1)
            .InvokeCommand(this, x => x.FilterChangedCommand);

        // Реакция на изменение параметров фильтра
        Filter.WhenChanged
            .Throttle(TimeSpan.FromMilliseconds(500))
            .InvokeCommand(this, x => x.FilterChangedCommand);

        _ = LoadPackets();
    }

    private async Task HandleFilterChanged()
    {
        CurrentPage = 1;
        await LoadPackets();
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

                HasFileLoaded = true;
                StatusMessage = "Файл успешно обработан";
                CurrentPage = 1;
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

            var request = Filter.CreateRequest(CurrentPage);
            var response = await _telemetryProcessingService.GetPacketsDetailedAsync(request);

            // Создаем Subject для обработки в UI-потоке
            var updateSubject = new Subject<PagedResponse<TelemetryPacketDto>>();
            updateSubject.ObserveOn(RxApp.MainThreadScheduler).Subscribe(r =>
            {
                _currentPage = r;
                TotalPages = r.TotalPages;

                if (CurrentPage > r.TotalPages && r.TotalPages > 0)
                {
                    CurrentPage = r.TotalPages;
                    // Перезапускаем загрузку через команду
                    FilterChangedCommand.Execute().Subscribe();
                    return;
                }

                this.RaisePropertyChanged(nameof(Packets));
                HasData = r.Data?.Count > 0;
            });

            updateSubject.OnNext(response);
            updateSubject.OnCompleted();
        }
        catch (Exception ex)
        {
            var errorSubject = new Subject<Exception>();
            errorSubject.ObserveOn(RxApp.MainThreadScheduler).Subscribe(e =>
            {
                StatusMessage = $"Ошибка загрузки данных: {e.Message}";
                HasData = false;
            });

            errorSubject.OnNext(ex);
            errorSubject.OnCompleted();
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