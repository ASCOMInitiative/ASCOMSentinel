using System.Runtime.CompilerServices;

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

        public static uint ToDeviceNumber(this PropertyName propertyName)
        {
            switch (propertyName)
            {
                case PropertyName.SafetyMonitor0: return 0;
                case PropertyName.SafetyMonitor1: return 1;
                case PropertyName.SafetyMonitor2: return 2;
                case PropertyName.SafetyMonitor3: return 3;
                case PropertyName.SafetyMonitor4: return 4;
                case PropertyName.SafetyMonitor5: return 5;
                case PropertyName.SafetyMonitor6: return 6;
                case PropertyName.SafetyMonitor7: return 7;
                case PropertyName.SafetyMonitor8: return 8;
                case PropertyName.SafetyMonitor9: return 9;
                case PropertyName.CloudCover: return 10;
                case PropertyName.DewPoint: return 11;
                case PropertyName.Humidity: return 12;
                case PropertyName.Pressure: return 13;
                case PropertyName.RainRate: return 14;
                case PropertyName.SkyBrightness: return 15;
                case PropertyName.SkyQuality: return 16;
                case PropertyName.SkyTemperature: return 17;
                case PropertyName.StarFWHM: return 18;
                case PropertyName.Temperature: return 19;
                case PropertyName.WindDirection: return 20;
                case PropertyName.WindGust: return 21;
                case PropertyName.WindSpeed: return 22;

                default:
                    throw new ASCOM.InvalidValueException($"Property name: {propertyName} is not defined");
            }
        }

        public static string ToRoundedString(this double value)
        {
            double abs = Math.Abs(value);
            return abs switch
            {
                < 1.0 => value.ToString("F3"),
                <= 100.0 => value.ToString("F2"),
                _ => value.ToString("F1")
            };
        }
    }
}
