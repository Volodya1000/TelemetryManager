using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Data.TelemetryPackets;

namespace TelemetryManager.Console;

internal  static class ConsoleDisplayFunctions
{
    public static void PrintContentDefinitions(IEnumerable<ContentDefinition> contentDefinitions)
    {
        foreach (var contentDef in contentDefinitions)
        {
            System.Console.WriteLine($"ContentDefinition:");
            System.Console.WriteLine($"  TypeId: {contentDef.TypeId}");
            System.Console.WriteLine($"  Name: {contentDef.Name}");
            System.Console.WriteLine($"  TotalSizeBytes: {contentDef.TotalSizeBytes} bytes");
            System.Console.WriteLine($"  Parameters:");

            foreach (var paramDef in contentDef.Parameters)
            {
                System.Console.WriteLine($"    Parameter:");
                System.Console.WriteLine($"      Name: {paramDef.Name}");
                System.Console.WriteLine($"      Quantity: {paramDef.Quantity}");
                System.Console.WriteLine($"      Unit: {paramDef.Unit}");
                System.Console.WriteLine($"      DataType: {paramDef.DataType}");
                System.Console.WriteLine($"      ByteSize: {paramDef.ByteSize} bytes");
                System.Console.WriteLine();
            }

            System.Console.WriteLine(new string('-', 40)); // Разделитель между определениями
        }
    }


    public static void PrintPagedTelemetryPackets(PagedResponse<TelemetryPacket> pagedResponse)
    {
        System.Console.WriteLine("================================================================================");
        System.Console.WriteLine($"Page: {pagedResponse.PageNumber} | Page Size: {pagedResponse.PageSize}");
        System.Console.WriteLine($"Total Records: {pagedResponse.TotalRecords} | Total Pages: {pagedResponse.TotalPages}");
        System.Console.WriteLine("================================================================================");

        if (pagedResponse.Data == null || !pagedResponse.Data.Any())
        {
            System.Console.WriteLine("No telemetry packets found.");
            System.Console.WriteLine("================================================================================");
            return;
        }

        foreach (var packet in pagedResponse.Data)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("-------------------------------------------------------------------------------");
            System.Console.WriteLine($"Timestamp: {packet.DateTimeOfSending:yyyy-MM-dd HH:mm:ss}");
            System.Console.WriteLine($"Device ID: {packet.DevId}");
            System.Console.WriteLine($"Sensor ID: {packet.SensorId}");
            System.Console.WriteLine("Content:");
            foreach (var kvp in packet.Content)
            {
                System.Console.WriteLine($"  - {kvp.Key}: {kvp.Value:F2}");
            }
            System.Console.WriteLine("-------------------------------------------------------------------------------");
        }

        System.Console.WriteLine();
        System.Console.WriteLine("================================================================================");
    }

    public static void PrintParsingErrors(IEnumerable<ParsingError> errors)
    {
        if (errors == null || !errors.Any())
        {
            System.Console.WriteLine("Ошибок нет.");
            return;
        }

        int errorIndex = 1;

        foreach (var error in errors)
        {
            System.Console.WriteLine(new string('─', 50));
            System.Console.WriteLine($"Ошибка #{errorIndex++}");
            System.Console.WriteLine("┌──────────────────────────────────────────┐");
            System.Console.WriteLine("│         Ошибка парсинга пакета           │");
            System.Console.WriteLine("└──────────────────────────────────────────┘");

            System.Console.WriteLine($"Тип ошибки:          {error.ErrorType}");
            System.Console.WriteLine($"Сообщение:           {error.Message}");
            System.Console.WriteLine($"Начало пакета:       {error.PacketStartOffset,15} байт");

            if (error.Time.HasValue)
                System.Console.WriteLine($"Время:               {new DateTime(1970, 1, 1).AddSeconds(error.Time.Value)}");

            if (error.DeviceId.HasValue)
                System.Console.WriteLine($"ID устройства:       {error.DeviceId.Value}");

            if (error.SensorType.HasValue)
                System.Console.WriteLine($"Тип датчика:         {error.SensorType.Value}");

            if (error.SourceId.HasValue)
                System.Console.WriteLine($"Источник:            {error.SourceId.Value}");

            if (error.Size.HasValue)
                System.Console.WriteLine($"Размер пакета:       {error.Size.Value} байт");

            System.Console.WriteLine(new string('─', 50));
            System.Console.WriteLine();
        }
    }

}
