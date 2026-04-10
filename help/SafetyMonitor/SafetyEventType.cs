
namespace SafetyMonitorInterfaceProposal
{
    /// <summary>
    /// Specifies the types of safety-related environmental events that can be monitored or reported.
    /// </summary>
    public enum SafetyEventType
    {
        /// <summary>
        /// Cloud cover is outside proscribed limits.
        /// </summary>
        CloudCover = 0,

        /// <summary>
        /// Dew point is outside proscribed limits.
        /// </summary>
        DewPoint = 1,

        /// <summary>
        /// Humidity is outside proscribed limits.
        /// </summary>
        Humidity = 2,

        /// <summary>
        /// Atmospheric pressure is outside proscribed limits.
        /// </summary>
        Pressure = 3,

        /// <summary>
        /// Rain rate is outside proscribed limits.
        /// </summary>
        RainRate = 4,

        /// <summary>
        /// Sky brightness is outside proscribed limits.
        /// </summary>
        SkyBrightness = 5,

        /// <summary>
        /// Sky quality is outside proscribed limits.
        /// </summary>
        SkyQuality = 6,

        /// <summary>
        /// Sky temperature is outside proscribed limits.
        /// </summary>
        SkyTemperature = 7,

        /// <summary>
        /// Star full width at half maximum is outside proscribed limits.
        /// </summary>
        StarFWHM = 8,

        /// <summary>
        /// Temperature is outside proscribed limits.
        /// </summary>
        Temperature = 9,

        /// <summary>
        /// Wind direction is outside proscribed limits.
        /// </summary>
        WindDirection = 10,

        /// <summary>
        /// Wind gust is outside proscribed limits.
        /// </summary>
        WindGust = 11,

        /// <summary>
        /// Wind speed is outside proscribed limits.
        /// </summary>
        WindSpeed = 12,

        /// <summary>
        /// A general safety issue has been detected.
        /// </summary>
        SafetyIssue = 13,

        /// <summary>
        /// A security-related issue has been detected.
        /// </summary>
        SecurityIssue = 14,

        /// <summary>
        /// A power-related issue has been detected.
        /// </summary>
        PowerIssue = 15,

        /// <summary>
        /// Some other type of safety event has been detected.
        /// </summary>
        /// <remarks>Please suggest a more specific type if possible so it can be added to the list at a future point.</remarks>
        Other = 1000
    }
}
