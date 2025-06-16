using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core;

public class ConfigurationValidator: IConfigurationValidator
{
    /// <summary>
    /// Проверяет, что в профиле устройства для каждого TypeId все SourceId уникальны.
    /// </summary>
    /// <exception cref="ArgumentNullException">Если deviceProfile равен null</exception>
    /// <exception cref="InvalidOperationException">Если найдены дубликаты SourceId для одного TypeId</exception>
    public void Validate(DeviceProfile deviceProfile)
    {
        if (deviceProfile == null)
            throw new ArgumentNullException(nameof(deviceProfile));

        var typeIdGroups = deviceProfile.Sensors
            .GroupBy(s => s.SensorId.TypeId);

        foreach (var group in typeIdGroups)
        {
            var duplicateSourceIds = group
                .GroupBy(s => s.SensorId.SourceId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateSourceIds.Any())
            {
                throw new InvalidOperationException(
                    $"Duplicate SourceIds found for TypeId {group.Key}: " +
                    string.Join(", ", duplicateSourceIds));
            }
        }
    }

}
