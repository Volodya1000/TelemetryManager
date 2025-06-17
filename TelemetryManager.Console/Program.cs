using TelemetryManager.Core;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Interfaces;
using TelemetryManager.Core.Loaders;
using TelemetryManager.Core.Validators;


IConfigurationValidator configValidator = new ConfigurationValidator();
IConfigurationLoader configurationLoader = new JsonLoader();
var facade = new TelemtryManagerFacade(configurationLoader, configValidator);

//string filePath = Path.Combine(AppContext.BaseDirectory,  "JsonConfigExample.json");

string workingDirectory = Environment.CurrentDirectory;
string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;

string filePath = Path.Combine(projectDirectory, "DevicesConfiguration", "JsonConfigExample.json");

facade.LoadConfiguration(filePath);

var devices=facade.GetDevicesProfiles();

DisplayDevices(devices);

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
