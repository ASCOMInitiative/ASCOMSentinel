
namespace SafetyMonitorExtension
{
    /// <summary>
    /// The type of environmental or operational factor that became unsafe.
    /// </summary>
    public enum SafetyEventType
    {
        /// <summary>
		/// Uninitialised value placeholder. This value should never be used in practice and is reserved to help identify uninitialised variables.
		/// </summary>
		ValueNotSet = 0,

        /// <summary>
        /// Cloud cover is outside defined limits.
        /// </summary>
        CloudCover = 1,

        /// <summary>
        /// Dew point is outside defined limits.
        /// </summary>
        DewPoint = 2,

        /// <summary>
        /// Humidity is outside defined limits.
        /// </summary>
        Humidity = 3,

        /// <summary>
        /// Atmospheric pressure is outside defined limits.
        /// </summary>
        AtmosphericPressure = 4,

        /// <summary>
        /// Rain rate is outside defined limits.
        /// </summary>
        RainRate = 5,

        /// <summary>
        /// Sky brightness is outside defined limits.
        /// </summary>
        SkyBrightness = 6,

        /// <summary>
        /// Sky quality is outside defined limits.
        /// </summary>
        SkyQuality = 7,

        /// <summary>
        /// Sky temperature is outside defined limits.
        /// </summary>
        SkyTemperature = 8,

        /// <summary>
        /// Star full width at half maximum is outside defined limits.
        /// </summary>
        StarFWHM = 9,

        /// <summary>
        /// Ambient temperature is outside defined limits.
        /// </summary>
        AmbientTemperature = 10,

        /// <summary>
        /// Wind direction is outside defined limits.
        /// </summary>
        WindDirection = 11,

        /// <summary>
        /// Wind gust speed is outside defined limits.
        /// </summary>
        WindGust = 12,

        /// <summary>
        /// Wind speed is outside defined limits.
        /// </summary>
        WindSpeed = 13,

        /// <summary>
        /// A safety-related issue has been detected.
        /// </summary>
        Safety = 14,

        /// <summary>
        /// A security-related issue has been detected.
        /// </summary>
        Security = 15,

        /// <summary>
        /// A power-related issue has been detected.
        /// </summary>
        Power = 16,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved1 = 17,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved2 = 18,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved3 = 19,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved4 = 20,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved5 = 21,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved6 = 22,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved7 = 23,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved8 = 24,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved9 = 25,

        /// <summary>
        /// A reserved value for future enum growth
        /// </summary>
        Reserved10 = 26,

        /// <summary>
        /// A user-defined event for local use within an observatory.
        /// </summary>
        LocallyDefined1 = 27,

        /// <summary>
        /// A user-defined event for local use within an observatory.
        /// </summary>
        LocallyDefined2 = 28,
        /// <summary>
        /// A user-defined event for local use within an observatory.
        /// </summary>
        LocallyDefined3 = 29,
        /// <summary>
        /// A user-defined event for local use within an observatory.
        /// </summary>
        LocallyDefined4 = 30,
        /// <summary>
        /// A user-defined event for local use within an observatory.
        /// </summary>
        LocallyDefined5 = 31,
        /// <summary>
        /// A user-defined event for local use within an observatory.
        /// </summary>
        LocallyDefined6 = 32,
        /// <summary>
        /// A user-defined event for local use within an observatory.
        /// </summary>
        LocallyDefined7 = 33,
        /// <summary>
        /// A user-defined event for local use within an observatory.
        /// </summary>
        LocallyDefined8 = 34,
        /// <summary>
        /// A user-defined event for local use within an observatory.
        /// </summary>
        LocallyDefined9 = 35,
        /// <summary>
        /// A user-defined event for local use within an observatory.
        /// </summary>
        LocallyDefined10 = 36,
    }
}
