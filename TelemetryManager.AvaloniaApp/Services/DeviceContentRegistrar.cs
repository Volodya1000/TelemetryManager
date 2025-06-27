using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.Services;

namespace TelemetryManager.AvaloniaApp.Services;

public class DeviceContentRegistrar
{
    private readonly IContentDefinitionService _contentDefinitionService;
    private readonly SensorContentRegistrar _sensorContentRegistrar;
    private readonly DeviceService _deviceService;

    public DeviceContentRegistrar(
        IContentDefinitionService contentDefinitionService,
        SensorContentRegistrar sensorContentRegistrar,
        DeviceService deviceService)
    {
        _contentDefinitionService = contentDefinitionService;
        _sensorContentRegistrar = sensorContentRegistrar;
        _deviceService = deviceService;
    }

    public void RegisterAllContent()
    {
        _sensorContentRegistrar.RegisterAllSensorTypesAsync().Wait();

        // Устройство 1 - Метеостанция
        _deviceService.AddAsync(1, "WeatherStation").Wait();
        // Температура
        _deviceService.AddSensorWithParametersAsync(1, 1, 1, "OutdoorTempSensor").Wait();
        _deviceService.SetParameterIntervalAsync(1, 1, 1, "TemperatureInFahrenheit", -40, 120).Wait();
        // Давление
        _deviceService.AddSensorWithParametersAsync(1, 2, 1, "BarometricSensor").Wait();
        _deviceService.SetParameterIntervalAsync(1, 2, 1, "Pressure", 80, 110).Wait();
        // Влажность
        _deviceService.AddSensorWithParametersAsync(1, 7, 1, "HumiditySensor").Wait();
        _deviceService.SetParameterIntervalAsync(1, 7, 1, "Humidity", 0, 100).Wait();

        // Устройство 2 - Навигационный модуль
        _deviceService.AddAsync(2, "NavigationModule").Wait();
        // Акселерометр
        _deviceService.AddSensorWithParametersAsync(2, 3, 1, "MainAccelerometer").Wait();
        _deviceService.SetParameterIntervalAsync(2, 3, 1, "X", -16, 16).Wait();
        _deviceService.SetParameterIntervalAsync(2, 3, 1, "Y", -16, 16).Wait();
        _deviceService.SetParameterIntervalAsync(2, 3, 1, "Z", -16, 16).Wait();
        // Магнетометр
        _deviceService.AddSensorWithParametersAsync(2, 4, 1, "CompassSensor").Wait();
        _deviceService.SetParameterIntervalAsync(2, 4, 1, "X", -100, 100).Wait();
        _deviceService.SetParameterIntervalAsync(2, 4, 1, "Y", -100, 100).Wait();
        _deviceService.SetParameterIntervalAsync(2, 4, 1, "Z", -100, 100).Wait();
        // Гироскоп
        _deviceService.AddSensorWithParametersAsync(2, 6, 1, "GyroUnit").Wait();
        _deviceService.SetParameterIntervalAsync(2, 6, 1, "X", -20, 20).Wait();
        _deviceService.SetParameterIntervalAsync(2, 6, 1, "Y", -20, 20).Wait();
        _deviceService.SetParameterIntervalAsync(2, 6, 1, "Z", -20, 20).Wait();

        // Устройство 3 - Промышленный монитор
        _deviceService.AddAsync(3, "IndustrialMonitor").Wait();
        // Датчик падения
        _deviceService.AddSensorWithParametersAsync(3, 5, 1, "FallDetectionSensor").Wait();
        _deviceService.SetParameterIntervalAsync(3, 5, 1, "AccelerationThreshold", 0, 2).Wait();
        // Температура (в Цельсиях)
        _deviceService.AddSensorWithParametersAsync(3, 1, 1, "InternalTempSensor").Wait();
        _deviceService.SetParameterIntervalAsync(3, 1, 1, "TemperatureInFahrenheit", -20, 60).Wait();
        // Давление
        _deviceService.AddSensorWithParametersAsync(3, 2, 2, "VentilationPressureSensor").Wait();
        _deviceService.SetParameterIntervalAsync(3, 2, 2, "Pressure", 90, 105).Wait();
    }
}
