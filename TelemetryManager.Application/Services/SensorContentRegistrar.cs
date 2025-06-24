using TelemetryManager.Application.Interfaces.Services;

namespace TelemetryManager.Application.Services;

public class SensorContentRegistrar
{
    private readonly IContentDefinitionService _contentDefinitionService;

    public SensorContentRegistrar(IContentDefinitionService contentDefinitionService)
    {
        _contentDefinitionService = contentDefinitionService;
    }

    public async Task RegisterAllSensorTypesAsync()
    {
        await RegisterTemperatureSensors();
        await RegisterPressureSensors();
        await RegisterAccelerometer();
        await RegisterMagnetometer();
        await RegisterFreeFallSensor();
        await RegisterGyroscope();
        await RegisterHumiditySensor();
    }

    private async Task RegisterTemperatureSensors()
    {
        await _contentDefinitionService.RegisterAsync(
            id: 1,
            contentName: "FahrenheitTemperatureSensorContent",
            parameters: new[]
            {
                ("TemperatureInFahrenheit", "Temperature", "Fahrenheit", typeof(float))
            });
    }

    private async Task RegisterPressureSensors()
    {
        await _contentDefinitionService.RegisterAsync(
            id: 2,
            contentName: "PressureSensorContent",
            parameters: new[]
            {
                ("Pressure", "Pressure", "kPa", typeof(float))
            });
    }

    private async Task RegisterAccelerometer()
    {
        await _contentDefinitionService.RegisterAsync(
            id: 3,
            contentName: "AccelerometerContent",
            parameters: new[]
            {
                ("X", "Acceleration", "m/s²", typeof(float)),
                ("Y", "Acceleration", "m/s²", typeof(float)),
                ("Z", "Acceleration", "m/s²", typeof(float))
            });
    }

    private async Task RegisterMagnetometer()
    {
        await _contentDefinitionService.RegisterAsync(
            id: 4,
            contentName: "MagnetometerContent",
            parameters: new[]
            {
                ("X", "MagneticField", "μT", typeof(float)),
                ("Y", "MagneticField", "μT", typeof(float)),
                ("Z", "MagneticField", "μT", typeof(float))
            });
    }

    private async Task RegisterFreeFallSensor()
    {
        await _contentDefinitionService.RegisterAsync(
            id: 5,
            contentName: "FreeFallSensorContent",
            parameters: new[]
            {
                ("IsInFreeFall", "State", "boolean", typeof(bool)),
                ("AccelerationThreshold", "Threshold", "m/s²", typeof(float))
            });
    }

    private async Task RegisterGyroscope()
    {
        await _contentDefinitionService.RegisterAsync(
            id: 6,
            contentName: "GyroscopeContent",
            parameters: new[]
            {
                ("X", "AngularVelocity", "rad/s", typeof(float)),
                ("Y", "AngularVelocity", "rad/s", typeof(float)),
                ("Z", "AngularVelocity", "rad/s", typeof(float))
            });
    }

    private async Task RegisterHumiditySensor()
    {
        await _contentDefinitionService.RegisterAsync(
            id: 7,
            contentName: "HumiditySensorContent",
            parameters: new[]
            {
                ("Humidity", "Humidity", "%", typeof(float)),
                ("Temperature", "Temperature", "Celsius", typeof(float))
            });
    }
}