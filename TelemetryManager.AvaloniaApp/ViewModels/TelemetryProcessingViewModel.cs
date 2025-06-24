using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using TelemetryManager.Application.Requests;
using TelemetryManager.Application.Services;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.TelemetryPackets;

namespace TelemetryManager.AvaloniaApp.ViewModels;

public class TelemetryProcessingViewModel : ViewModelBase
{
    private readonly TelemetryProcessingService _service;
    private PagedResponse<TelemetryPacket>? _currentPage;
    private string _statusMessage = string.Empty;
    private readonly TopLevel? _topLevel;

    public TelemetryPacketFilterRequest Filter { get; } = new TelemetryPacketFilterRequest();

    // Убедитесь, что свойство называется Items в PagedResponse
    public List<TelemetryPacket> Packets => _currentPage?.Data ?? new List<TelemetryPacket>();

    public int CurrentPage => Filter.PageNumber;
    public int TotalPages => _currentPage?.TotalPages ?? 1;

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    // Команды
    public ReactiveCommand<Unit, Unit> SelectFileCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadPacketsCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; }

    public TelemetryProcessingViewModel(TelemetryProcessingService service, TopLevel? topLevel = null)
    {
        _service = service;
        _topLevel = topLevel;

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
        if (_topLevel == null)
        {
            StatusMessage = "TopLevel недоступен";
            return;
        }

        var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите файл .bin",
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Binary Files")
                {
                    Patterns = new[] { "*.bin" },
                    MimeTypes = new[] { "application/octet-stream" }
                }
            },
            AllowMultiple = false
        });

        if (files.Count > 0 && files[0] is IStorageFile file)
        {
            try
            {
                // Получаем путь к файлу
                var filePath = file.Path.AbsolutePath;
                await _service.ProcessTelemetryFile(filePath);
                StatusMessage = "Файл успешно обработан";
                await LoadPackets();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
        }
    }

    private async Task LoadPackets()
    {
        try
        {
            _currentPage = await _service.GetPacketsAsync(Filter);
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
        if (Filter.PageNumber > 1)
        {
            Filter.PageNumber--;
            LoadPackets().ConfigureAwait(false);
        }
    }

    private void NextPage()
    {
        if (Filter.PageNumber < TotalPages)
        {
            Filter.PageNumber++;
            LoadPackets().ConfigureAwait(false);
        }
    }
}