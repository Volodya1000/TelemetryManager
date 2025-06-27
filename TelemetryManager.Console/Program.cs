using Microsoft.Extensions.DependencyInjection;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.Requests;
using TelemetryManager.Application.Services;
using TelemetryManager.Console;
using TelemetryManager.Core.Interfaces.Repositories;
using TelemetryManager.Infrastructure.TelemetryPackegesGenerator;

string workingDirectory = Environment.CurrentDirectory;
string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;

string configurationFilePath = Path.Combine(projectDirectory, "DeviceConfiguration", "JsonConfigExample.json");

string telemetryFilePath = Path.Combine(projectDirectory, "TelemetryPacketFiles", "telemetry1.bin");

var serviceCollection = new ServiceCollection();
serviceCollection.AddConsole();
var serviceProvider = serviceCollection.BuildServiceProvider();

//using (var scope = serviceProvider.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<TelemetryContext>();
//    context.Database.EnsureDeleted();
//    context.Database.EnsureCreated();
//}



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
await deviceService.SetParameterIntervalAsync(1, 1, 1, "TemperatureInFarengeit", -30, 30);


var contentDefinitionRepository = serviceProvider.GetRequiredService<IContentDefinitionRepository>();
var res = await contentDefinitionRepository.GetAllDefinitionsAsync();

ConsoleDisplayFunctions.PrintContentDefinitions(res);


var deviceService1 = serviceProvider.GetRequiredService<DeviceService>();


var telemetryProcessingService = serviceProvider.GetRequiredService<TelemetryProcessingService>();




var generator= serviceProvider.GetRequiredService<TelemetryGenerator>();

await generator.Generate(telemetryFilePath,1,0);

//await telemetryProcessingService.ProcessTelemetryFile(telemetryFilePath);

ConsoleDisplayFunctions.PrintPagedTelemetryPackets(await telemetryProcessingService.GetPacketsAsync(new TelemetryPacketFilterRequest()));

ConsoleDisplayFunctions.PrintParsingErrors(telemetryProcessingService.GetParsingErrors());
