using System.ComponentModel.DataAnnotations;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Exceptions;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core.Validators;

public class SensorDataValidator
{
    public ValidationResult Validate(SensorProfile sensorProfile, ISensorData sensorData)
    {
        if (sensorProfile == null)
            throw new ArgumentNullException(nameof(sensorProfile));
        if (sensorData == null)
            throw new ArgumentNullException(nameof(sensorData));

        var errors = new List<ValidationError>();
        var dataValues = sensorData.GetValues();

        foreach (var dataValue in dataValues)
        {
            string paramName = dataValue.Key;
            double value = dataValue.Value;

            // Поиск параметра в профиле по имени
            var paramProfile = sensorProfile.Parameters.FirstOrDefault(p => p.Name == paramName);

            if (paramProfile == null)
            {
                errors.Add(new ValidationError(
                    paramName,
                    $"Параметр '{paramName}' не найден в профиле сенсора.",
                    value
                ));
                continue;
            }

            // Проверка диапазона значения
            if (value < paramProfile.MinValue || value > paramProfile.MaxValue)
            {
                errors.Add(new ValidationError(
                    paramName,
                    $"Значение {value} выходит за допустимый диапазон [{paramProfile.MinValue}, {paramProfile.MaxValue}].",
                    value,
                    paramProfile.MinValue,
                    paramProfile.MaxValue
                ));
            }
        }

        return new ValidationResult(errors);
    }
}
