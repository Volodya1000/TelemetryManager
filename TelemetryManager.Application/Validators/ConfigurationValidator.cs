using TelemetryManager.Application.Interfaces;
using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.Application.Validators;

public class ConfigurationValidator //: IConfigurationValidator
{
    //private HashSet<byte> _validSensorTypeIds;

    //public ConfigurationValidator()
    //{
    //    _validSensorTypeIds = Enum.GetValues(typeof(SensorType))
    //                            .Cast<byte>()
    //                            .ToHashSet();
    //}

    ///// <summary>
    ///// Проверяет валидность профилей устройств:
    ///// 1. TypeId каждого сенсора должен быть допустимым значением enum SensorType
    ///// 2. SourceId должны быть уникальны в рамках каждого TypeId
    ///// </summary>
    ///// <exception cref="ArgumentNullException">Если deviceProfile равен null</exception>
    ///// <exception cref="InvalidOperationException">
    ///// При обнаружении недопустимых TypeId или дубликатов SourceId
    ///// </exception>
    //public void Validate(List<DeviceProfile> deviceProfiles)
    //{
    //    ValidateDeviceProfilesNotNull(deviceProfiles);

    //    foreach (var deviceProfile in deviceProfiles)
    //    {
    //        ValidateSingleDeviceProfile(deviceProfile);
    //    }
    //}

    //private void ValidateDeviceProfilesNotNull(List<DeviceProfile> deviceProfiles)
    //{
    //    if (deviceProfiles == null)
    //    {
    //        throw new ArgumentNullException(nameof(deviceProfiles));
    //    }

    //    if (deviceProfiles.Any(p => p == null))
    //    {
    //        throw new ArgumentNullException(nameof(deviceProfiles),
    //            "Device profiles list contains null elements");
    //    }
    //}

    //private void ValidateSingleDeviceProfile(DeviceProfile deviceProfile)
    //{
    //    ValidateSensorTypeIds(deviceProfile);
    //    ValidateSourceIdsUniqueness(deviceProfile);
    //}

    //private void ValidateSensorTypeIds(DeviceProfile deviceProfile)
    //{
    //    var invalidSensors = deviceProfile.Sensors
    //        .Where(s => !_validSensorTypeIds.Contains((byte)s.TypeId))
    //        .ToList();

    //    if (invalidSensors.Any())
    //    {
    //        var invalidTypeIds = invalidSensors
    //            .Select(s => s.TypeId)
    //            .Distinct()
    //            .OrderBy(id => id);

    //        var allowedValues = string.Join(", ", Enum.GetNames(typeof(SensorType)));
    //        var errorMessage = $"Device {deviceProfile.DeviceId}: Invalid TypeIds detected: " +
    //                         $"{string.Join(", ", invalidTypeIds.Select(id => $"0x{id:X2}"))}. " +
    //                         $"Allowed values: {allowedValues}";

    //        throw new InvalidOperationException(errorMessage);
    //    }
    //}

    //private void ValidateSourceIdsUniqueness(DeviceProfile deviceProfile)
    //{
    //    var duplicateSourceIds = deviceProfile.Sensors
    //        .GroupBy(s => s.TypeId)
    //        .SelectMany(group => group
    //            .GroupBy(s => s.SourceId)
    //            .Where(g => g.Count() > 1)
    //            .Select(g => new { TypeId = group.Key, SourceId = g.Key }))
    //        .ToList();

    //    if (duplicateSourceIds.Any())
    //    {
    //        var errorDetails = string.Join("; ",
    //            duplicateSourceIds
    //                .GroupBy(x => x.TypeId)
    //                .Select(g => $"TypeId {g.Key}: {string.Join(", ", g.Select(x => x.SourceId))}"));

    //        throw new InvalidOperationException(
    //            $"Device {deviceProfile.DeviceId}: Duplicate SourceIds found: {errorDetails}");
    //    }
    //}
}