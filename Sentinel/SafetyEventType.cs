
namespace Sentinel
{
    /// <summary>
    /// Specifies the types of safety-related environmental events that can be monitored or reported.
    /// </summary>
    /// <remarks>Use this enumeration to identify and categorize environmental parameters such as temperature,
    /// humidity, wind speed, and other conditions relevant to safety assessments. Each value represents a distinct
    /// measurement or status that may influence operational safety decisions. This enumeration is typically used in
    /// systems that monitor environmental conditions to determine or report safety status.</remarks>
    public enum SafetyEventType
    {
        CloudCover = 0,
        DewPoint = 1,
        Humidity = 2,
        Pressure = 3,
        RainRate = 4,
        SkyBrightness = 5,
        SkyQuality = 6,
        SkyTemperature = 7,
        StarFWHM = 8,
        Temperature = 9,
        WindDirection = 10,
        WindGust = 11,
        WindSpeed = 12,
        SafetyIssue = 13,
        SecurityIssue = 14,
        PowerIssue = 15,
        Other = 16
    }
}
