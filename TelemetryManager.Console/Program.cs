using TelemetryManager.Application.Interfaces;
using TelemetryManager.Application.Logger;
using TelemetryManager.Application.Services;
using TelemetryManager.Application.Validators;
using TelemetryManager.Core.Data;
using TelemetryManager.Application.OutputDtos;
using TelemetryManager.Core.Enums;
using TelemetryManager.Infrastructure.TelemetryPackegesGenerator;
using TelemetryManager.Infrastructure.JsonConfigurationLoader;
using TelemetryManager.Infrastructure.Parsing;
using TelemetryManager.Core.Interfaces.Repositories;


IConfigurationValidator configValidator = new ConfigurationValidator();
IConfigurationLoader configurationLoader = new JsonLoader();
IPacketStreamParser packetStreamParser = new PacketStreamParser();
ITelemetryRepository telemetryRepository = null;
var facade = new TelemtryService(configurationLoader, configValidator, packetStreamParser, telemetryRepository);

string workingDirectory = Environment.CurrentDirectory;
string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;

string configurationFilePath = Path.Combine(projectDirectory, "DeviceConfiguration", "JsonConfigExample.json");

facade.LoadConfiguration(configurationFilePath);

var devices=facade.GetDevicesProfiles();

DisplayDevices(devices);


string telemetryFilePath = Path.Combine(projectDirectory, "TelemetryPacketFiles", "telemetry1.bin");


var generator = new TelemetryGenerator(devId: 1, totalPackets: 6, noiseRatio: 0)
    .AddSensor(SensorType.Temperature, 1, SensorDataGenerators.GenerateTemperatureData)
    .AddSensor(SensorType.Accelerometer, 2, SensorDataGenerators.GenerateAccelerometerData)
    .AddSensor(SensorType.Magnetometer, 3, SensorDataGenerators.GenerateMagnetometerData)
    .AddSensor(SensorType.FreeFall, 4, SensorDataGenerators.GenerateFreeFallData)
    .AddSensor(SensorType.Pressure, 5, SensorDataGenerators.GeneratePressureData);

generator.Generate(telemetryFilePath);


facade.ProcessTelemetryFile(telemetryFilePath);

//DisplayDevices(devices);

PrintTelemetryPackets(facade.GetRecivedPackets());

PrintParsingErrors(facade.GetParsingErrors());



static void DisplayDevices(ICollection<DeviceProfileDto> devices)
{
    if (devices == null || devices.Count == 0)
    {
        Console.WriteLine("Список устройств пуст.");
        return;
    }

    foreach (var device in devices)
    {
        Console.WriteLine(new string('-', 60));
        Console.WriteLine($"Устройство: {device.Name}");
        Console.WriteLine($"ID устройства: {device.DeviceId}");

        var activationTime = device.ActivationTime.HasValue ? device.ActivationTime.ToString() : "Не известно";

        Console.WriteLine($"Время активации: {activationTime}");

        if (device.Sensors == null || device.Sensors.Count == 0)
        {
            Console.WriteLine("  Нет сенсоров.");
            continue;
        }

        foreach (var sensor in device.Sensors)
        {
            Console.WriteLine("  ----------------------------------------");
            Console.Write($"  Сенсор: {sensor.Name}");
            Console.Write($"  SourceId: {sensor.SourceId},");
            Console.Write($"  TypeId: {sensor.TypeId}");
            Console.WriteLine();

            if (sensor.Parameters == null || sensor.Parameters.Count == 0)
            {
                Console.WriteLine("    Нет параметров.");
                continue;
            }

            foreach (var param in sensor.Parameters)
            {
                Console.WriteLine("    ------------------------------");
                Console.WriteLine($"    Параметр: {param.ParameterName}");
                Console.WriteLine($"    Единицы измерения: {param.Units}");
                Console.WriteLine($"    Диапазон: от {param.MinValue} до {param.MaxValue}");
            }
        }
    }

    Console.WriteLine(new string('-', 60));
}




static void PrintTelemetryPackets(List<TelemetryPacketWithDate> packets)
{
    if (packets == null || packets.Count == 0)
    {
        Console.WriteLine("Нет данных для отображения.");
        return;
    }

    Console.WriteLine(new string('-', 80));
    Console.WriteLine($"{nameof(TelemetryPacketWithUIntTime)} List ({packets.Count} элементов):");
    Console.WriteLine(new string('-', 80));

    foreach (var packet in packets)
    {
        Console.WriteLine("┌────────────────────────────────────────────────────────────────────────┐");
        Console.WriteLine($"│ Sending Time: {packet.DateTimeOfSending,-10} | DevId: {packet.DevId,-5} | SourceId: {packet.SensorId.SourceId} │");
        Console.WriteLine($"│ TypeId: {packet.SensorId.TypeId,-12}      │");

        // Печать ISensorData, если реализован ToString()
        if (packet.Content != null)
        {
            var values = packet.Content;

            Console.WriteLine("├────────────────────────────────────────────────────────────────────────┤");
            Console.WriteLine("│ Parsed Data:");

            foreach (var kvp in values)
            {
                Console.WriteLine($"│   {kvp.Key}: {kvp.Value}");
            }

            Console.WriteLine("│");
        }

        Console.WriteLine("└────────────────────────────────────────────────────────────────────────┘");
        Console.WriteLine();
    }
}

static void PrintParsingErrors(IEnumerable<ParsingError> errors)
{
    if (errors == null || !errors.Any())
    {
        Console.WriteLine("Ошибок нет.");
        return;
    }

    int errorIndex = 1;

    foreach (var error in errors)
    {
        Console.WriteLine(new string('─', 50));
        Console.WriteLine($"Ошибка #{errorIndex++}");
        Console.WriteLine("┌──────────────────────────────────────────┐");
        Console.WriteLine("│         Ошибка парсинга пакета           │");
        Console.WriteLine("└──────────────────────────────────────────┘");

        Console.WriteLine($"Тип ошибки:          {error.ErrorType}");
        Console.WriteLine($"Сообщение:           {error.Message}");
        Console.WriteLine($"Начало пакета:       {error.PacketStartOffset,15} байт");

        if (error.Time.HasValue)
            Console.WriteLine($"Время:               {new DateTime(1970, 1, 1).AddSeconds(error.Time.Value)}");

        if (error.DeviceId.HasValue)
            Console.WriteLine($"ID устройства:       {error.DeviceId.Value}");

        if (error.SensorType.HasValue)
            Console.WriteLine($"Тип датчика:         {error.SensorType.Value}");

        if (error.SourceId.HasValue)
            Console.WriteLine($"Источник:            {error.SourceId.Value}");

        if (error.Size.HasValue)
            Console.WriteLine($"Размер пакета:       {error.Size.Value} байт");

        Console.WriteLine(new string('─', 50));
        Console.WriteLine();
    }
}

