using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using TelemetryManager.Application.Requests;
using System.Reactive.Linq;

namespace TelemetryManager.ViewModels.ViewModelsFolder;

public class TelemetryFilterViewModel : ReactiveObject
{
    [Reactive] public DateTimeOffset? DateFrom { get; set; }
    [Reactive] public DateTimeOffset? DateTo { get; set; }
    [Reactive] public string DeviceIdText { get; set; } = string.Empty;
    [Reactive] public string SensorTypeText { get; set; } = string.Empty;
    [Reactive] public string SensorIdText { get; set; } = string.Empty;
    [Reactive] public int PageSize { get; set; } = 10;

    public List<int> PageSizeOptions { get; } = new() { 5, 10, 20, 50, 100 };

    public TelemetryPacketFilterRequest CreateRequest(int pageNumber)
    {
        return new TelemetryPacketFilterRequest
        {
            PageNumber = pageNumber,
            PageSize = PageSize,
            DateFrom = DateFrom?.UtcDateTime,
            DateTo = DateTo?.UtcDateTime,
            DeviceId = ParseUShort(DeviceIdText),
            SensorType = ParseByte(SensorTypeText),
            SensorId = ParseByte(SensorIdText)
        };
    }

    private ushort? ParseUShort(string text) =>
        ushort.TryParse(text, out var value) ? value : null;

    private byte? ParseByte(string text) =>
        byte.TryParse(text, out var value) ? value : null;

    public IObservable<Unit> WhenChanged =>
     this.WhenAnyValue(
         x => x.DateFrom,
         x => x.DateTo,
         x => x.DeviceIdText,
         x => x.SensorTypeText,
         x => x.SensorIdText,
         x => x.PageSize)
     .Select(_ => Unit.Default);
}
