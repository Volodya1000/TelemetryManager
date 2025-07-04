using System.Collections.Generic;
using TelemetryManager.Application.Interfaces.Services;
using TelemetryManager.Application.Services;
using TelemetryManager.Core.Data.ValueObjects;

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
        _deviceService.AddSensorWithParametersAsync(1, 1, 1, "OutdoorTempSensor",
            new Dictionary<string, Interval>
            {
                ["TemperatureInFahrenheit"] = new Interval(-40, 120)
            }).Wait();

        // Давление
        _deviceService.AddSensorWithParametersAsync(1, 2, 1, "BarometricSensor",
            new Dictionary<string, Interval>
            {
                ["Pressure"] = new Interval(80, 110)
            }).Wait();

        // Влажность
        _deviceService.AddSensorWithParametersAsync(1, 7, 1, "HumiditySensor",
            new Dictionary<string, Interval>
            {
                ["Humidity"] = new Interval(0, 100)
            }).Wait();

        // Устройство 2 - Навигационный модуль
        _deviceService.AddAsync(2, "NavigationModule").Wait();

        // Акселерометр
        _deviceService.AddSensorWithParametersAsync(2, 3, 1, "MainAccelerometer",
            new Dictionary<string, Interval>
            {
                ["X"] = new Interval(-16, 16),
                ["Y"] = new Interval(-16, 16),
                ["Z"] = new Interval(-16, 16)
            }).Wait();

        // Магнетометр
        _deviceService.AddSensorWithParametersAsync(2, 4, 1, "CompassSensor",
            new Dictionary<string, Interval>
            {
                ["X"] = new Interval(-100, 100),
                ["Y"] = new Interval(-100, 100),
                ["Z"] = new Interval(-100, 100)
            }).Wait();

        // Гироскоп
        _deviceService.AddSensorWithParametersAsync(2, 6, 1, "GyroUnit",
            new Dictionary<string, Interval>
            {
                ["X"] = new Interval(-20, 20),
                ["Y"] = new Interval(-20, 20),
                ["Z"] = new Interval(-20, 20)
            }).Wait();

        // Устройство 3 - Промышленный монитор
        _deviceService.AddAsync(3, "IndustrialMonitor").Wait();

        // Датчик падения
        _deviceService.AddSensorWithParametersAsync(3, 5, 1, "FallDetectionSensor",
            new Dictionary<string, Interval>
            {
                ["AccelerationThreshold"] = new Interval(0, 1)
            }).Wait();

        // Температура
        _deviceService.AddSensorWithParametersAsync(3, 1, 1, "InternalTempSensor",
            new Dictionary<string, Interval>
            {
                ["TemperatureInFahrenheit"] = new Interval(-20, 60)
            }).Wait();

        // Давление
        _deviceService.AddSensorWithParametersAsync(3, 2, 2, "VentilationPressureSensor",
            new Dictionary<string, Interval>
            {
                ["Pressure"] = new Interval(90, 105)
            }).Wait();
    }
}
