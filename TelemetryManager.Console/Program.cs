using TelemetryManager.Core;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Enums;
using TelemetryManager.Core.Interfaces;
using TelemetryManager.Core.TelemetryPackegesGenerator;
using TelemetryManager.Core.Validators;
using TelemetryManager.Infrastructure.JsonConfigurationLoader;


IConfigurationValidator configValidator = new ConfigurationValidator();
IConfigurationLoader configurationLoader = new JsonLoader();
var facade = new TelemtryManagerFacade(configurationLoader, configValidator);

//string filePath = Path.Combine(AppContext.BaseDirectory,  "JsonConfigExample.json");

string workingDirectory = Environment.CurrentDirectory;
string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;

string configurationFilePath = Path.Combine(projectDirectory, "DeviceConfiguration", "JsonConfigExample.json");

facade.LoadConfiguration(configurationFilePath);

var devices=facade.GetDevicesProfiles();

DisplayDevices(devices);


string telemetryFilePath = Path.Combine(projectDirectory, "TelemetryPacketFiles", "telemetry1.bin");


var generator = new TelemetryGenerator(devId: 1, totalPackets: 10)
    .SetNoiseRatio(0)
    //.SetNoiseRatio(0.05) // 5% шанс повреждения
    .AddSensor(SensorType.Temperature, 1, SensorDataGenerators.GenerateTemperatureData)
    .AddSensor(SensorType.Accelerometer, 2, SensorDataGenerators.GenerateAccelerometerData)
    .AddSensor(SensorType.Magnetometer, 3, SensorDataGenerators.GenerateMagnetometerData)
    .AddSensor(SensorType.FreeFall, 4, SensorDataGenerators.GenerateFreeFallData)
    .AddSensor(SensorType.Pressure, 5, SensorDataGenerators.GeneratePressureData);

generator.Generate(telemetryFilePath);


facade.ProcessTelemetryFile(telemetryFilePath);

PrintTelemetryPackets(facade.GetRecivedPackets());

static void DisplayDevices(List<DeviceProfile> devices)
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
                Console.WriteLine($"    Параметр: {param.Name}");
                Console.WriteLine($"    Единицы измерения: {param.Units}");
                Console.WriteLine($"    Диапазон: от {param.Min} до {param.Max}");
            }
        }
    }

    Console.WriteLine(new string('-', 60));
}




static void PrintTelemetryPackets(List<TelemetryPacket> packets)
{
    if (packets == null || packets.Count == 0)
    {
        Console.WriteLine("Нет данных для отображения.");
        return;
    }

    Console.WriteLine(new string('-', 80));
    Console.WriteLine($"{nameof(TelemetryPacket)} List ({packets.Count} элементов):");
    Console.WriteLine(new string('-', 80));

    foreach (var packet in packets)
    {
        Console.WriteLine("┌────────────────────────────────────────────────────────────────────────┐");
        Console.WriteLine($"│ Timestamp: {packet.Time,-10} | DevId: {packet.DevId,-5} | SourceId: {packet.SourceId} │");
        Console.WriteLine($"│ TypeId: {packet.TypeId,-12} | Content Length: {packet.Content.Length,4} bytes     │");

        // Печать ISensorData, если реализован ToString()
        if (packet.ParsedContent != null)
        {
            var values = packet.ParsedContent.GetValues();

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