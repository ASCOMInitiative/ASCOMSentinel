
namespace Sentinel
{
    /// <summary>
    /// Specifies the types of safety-related environmental events that can be monitored or reported.
    /// </summary>
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
        Other = 1000
    }
}
