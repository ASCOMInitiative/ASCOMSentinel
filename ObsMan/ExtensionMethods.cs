namespace ObsMan
{
    public static class ExtensionMethods
    {
        public static LogLevel ToMSLogLevel(this ASCOM.Common.Interfaces.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case ASCOM.Common.Interfaces.LogLevel.Verbose:
                case ASCOM.Common.Interfaces.LogLevel.Debug:
                    return LogLevel.Debug;

                case ASCOM.Common.Interfaces.LogLevel.Information:
                    return LogLevel.Information;

                case ASCOM.Common.Interfaces.LogLevel.Warning:
                    return LogLevel.Warning;

                case ASCOM.Common.Interfaces.LogLevel.Error:
                    return LogLevel.Error;

                default:
                    throw new ArgumentException($"Unknown logging level: {logLevel}");
            }
        }

        public static SafetyEventType ToSafetyEventType(this PropertyName propertyName)
        {
            return propertyName switch
            {
                PropertyName.CloudCover => SafetyEventType.CloudCover,
                PropertyName.DewPoint => SafetyEventType.DewPoint,
                PropertyName.Humidity => SafetyEventType.Humidity,
                PropertyName.Pressure => SafetyEventType.Pressure,
                PropertyName.RainRate => SafetyEventType.RainRate,
                PropertyName.SkyBrightness => SafetyEventType.SkyBrightness,
                PropertyName.SkyQuality => SafetyEventType.SkyQuality,
                PropertyName.SkyTemperature => SafetyEventType.SkyTemperature,
                PropertyName.StarFWHM => SafetyEventType.StarFWHM,
                PropertyName.Temperature => SafetyEventType.Temperature,
                PropertyName.WindDirection => SafetyEventType.WindDirection,
                PropertyName.WindGust => SafetyEventType.WindGust,
                PropertyName.WindSpeed => SafetyEventType.WindSpeed,
                _ => throw new ArgumentException($"Unknown property name: {propertyName}")
            };
        }
    }
}
