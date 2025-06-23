using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelemetryManager.Application.ContentTypeProcessing;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.OutputDtos;
using TelemetryManager.Application.Requests;
using TelemetryManager.Application.Services;
using TelemetryManager.Application.Validators;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.SensorParameter;
using TelemetryManager.Core.Data.TelemetryPackets;
using TelemetryManager.Core.Data.ValueObjects;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Infrastructure.FileReader;
using TelemetryManager.Infrastructure.JsonConfigurationLoader;
using TelemetryManager.Infrastructure.Parsing;
using TelemetryManager.Infrastructure.TelemetryPackegesGenerator;
using TelemetryManager.Persistence;
using TelemetryManager.Persistence.Repositories;

var serviceCollection = new ServiceCollection();

//serviceCollection.AddTransient<IConfigurationLoader, JsonLoader>();
serviceCollection.AddTransient<IContentDefinitionRepository, ContentDefinitionRepository>();
serviceCollection.AddTransient<IPacketStreamParser, PacketStreamParser>();
serviceCollection.AddTransient<IContentTypeParser,ContentTypeParser>();

serviceCollection.AddPersistance();
//service
serviceCollection.AddScoped<TelemetryProcessingService>();
serviceCollection.AddScoped<ParameterValidationService>();
serviceCollection.AddScoped<DeviceService>();
serviceCollection.AddScoped<IContentDefinitionService, ContentDefinitionService>();
serviceCollection.AddScoped<IFileReaderService,FileReaderService>(); 


serviceCollection.AddDbContext<TelemetryContext>(options =>
    options.UseSqlite("Data Source=TelemetryManagerSqliteDataBase.db;"));

var serviceProvider = serviceCollection.BuildServiceProvider();


using (var scope = serviceProvider.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TelemetryContext>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}



var contentDefinitionService = serviceProvider.GetRequiredService<IContentDefinitionService>();

await contentDefinitionService.RegisterAsync(
    id: 1,
    contentName: "FarengaitTemperatureSensorContent",
    parameters: new[]
    {
        ("TemperatureInFarengeit", "Temperature", "Farengait", typeof(float))
    }
);





var deviceService = serviceProvider.GetRequiredService<DeviceService>();
await deviceService.AddAsync(1,"MyFirstDevice");
await deviceService.AddSensorWithParametersAsync(1,1,1,"MyFirstTemperatureSensor");




var contentDefinitionRepository = serviceProvider.GetRequiredService<IContentDefinitionRepository>();
var res = await contentDefinitionRepository.GetAllDefinitionsAsync();

PrintContentDefinitions(res);


var deviceService1 = serviceProvider.GetRequiredService<DeviceService>();


var facade = serviceProvider.GetRequiredService<TelemetryProcessingService>();

string workingDirectory = Environment.CurrentDirectory;
string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;


string configurationFilePath = Path.Combine(projectDirectory, "DeviceConfiguration", "JsonConfigExample.json");
//facade.LoadConfiguration(configurationFilePath);


//var devices = facade.GetDevicesProfiles();
//DisplayDevices(devices);


string telemetryFilePath = Path.Combine(projectDirectory, "TelemetryPacketFiles", "telemetry1.bin");

//var generator = new TelemetryGenerator(devId: 1, totalPackets: 6, noiseRatio: 0)
//    .AddSensor(SensorType.Temperature, 1, SensorDataGenerators.GenerateTemperatureData)
//    .AddSensor(SensorType.Accelerometer, 2, SensorDataGenerators.GenerateAccelerometerData)
//    .AddSensor(SensorType.Magnetometer, 3, SensorDataGenerators.GenerateMagnetometerData)
//    .AddSensor(SensorType.FreeFall, 4, SensorDataGenerators.GenerateFreeFallData)
//    .AddSensor(SensorType.Pressure, 5, SensorDataGenerators.GeneratePressureData);

//generator.Generate(telemetryFilePath);



facade.ProcessTelemetryFile(telemetryFilePath);


PrintPagedTelemetryPackets(await facade.GetPacketsAsync(new TelemetryPacketFilterRequest()));
PrintParsingErrors(facade.GetParsingErrors());

 static void PrintContentDefinitions(IEnumerable<ContentDefinition> contentDefinitions)
{
    foreach (var contentDef in contentDefinitions)
    {
        Console.WriteLine($"ContentDefinition:");
        Console.WriteLine($"  TypeId: {contentDef.TypeId}");
        Console.WriteLine($"  Name: {contentDef.Name}");
        Console.WriteLine($"  TotalSizeBytes: {contentDef.TotalSizeBytes} bytes");
        Console.WriteLine($"  Parameters:");

        foreach (var paramDef in contentDef.Parameters)
        {
            Console.WriteLine($"    Parameter:");
            Console.WriteLine($"      Name: {paramDef.Name}");
            Console.WriteLine($"      Quantity: {paramDef.Quantity}");
            Console.WriteLine($"      Unit: {paramDef.Unit}");
            Console.WriteLine($"      DataType: {paramDef.DataType}");
            Console.WriteLine($"      ByteSize: {paramDef.ByteSize} bytes");
            Console.WriteLine();
        }

        Console.WriteLine(new string('-', 40)); // Разделитель между определениями
    }
}


void PrintPagedTelemetryPackets(PagedResponse<TelemetryPacket> pagedResponse)
{
    Console.WriteLine("================================================================================");
    Console.WriteLine($"Page: {pagedResponse.PageNumber} | Page Size: {pagedResponse.PageSize}");
    Console.WriteLine($"Total Records: {pagedResponse.TotalRecords} | Total Pages: {pagedResponse.TotalPages}");
    Console.WriteLine("================================================================================");

    if (pagedResponse.Data == null || !pagedResponse.Data.Any())
    {
        Console.WriteLine("No telemetry packets found.");
        Console.WriteLine("================================================================================");
        return;
    }

    foreach (var packet in pagedResponse.Data)
    {
        Console.WriteLine();
        Console.WriteLine("-------------------------------------------------------------------------------");
        Console.WriteLine($"Timestamp: {packet.DateTimeOfSending:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Device ID: {packet.DevId}");
        Console.WriteLine($"Sensor ID: {packet.SensorId}");
        Console.WriteLine("Content:");
        foreach (var kvp in packet.Content)
        {
            Console.WriteLine($"  - {kvp.Key}: {kvp.Value:F2}");
        }
        Console.WriteLine("-------------------------------------------------------------------------------");
    }

    Console.WriteLine();
    Console.WriteLine("================================================================================");
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

