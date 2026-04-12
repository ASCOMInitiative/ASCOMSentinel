
namespace SafetyMonitor
{
	/// <summary>
	/// The type of environmental or operational factor that became unsafe.
	/// </summary>
	public enum SafetyEventType
	{
		/// <summary>
		/// Cloud cover is outside defined limits.
		/// </summary>
		CloudCover = 0,

		/// <summary>
		/// Dew point is outside defined limits.
		/// </summary>
		DewPoint = 1,

		/// <summary>
		/// Humidity is outside defined limits.
		/// </summary>
		Humidity = 2,

		/// <summary>
		/// Atmospheric pressure is outside defined limits.
		/// </summary>
		AtmosphericPressure = 3,

		/// <summary>
		/// Rain rate is outside defined limits.
		/// </summary>
		RainRate = 4,

		/// <summary>
		/// Sky brightness is outside defined limits.
		/// </summary>
		SkyBrightness = 5,

		/// <summary>
		/// Sky quality is outside defined limits.
		/// </summary>
		SkyQuality = 6,

		/// <summary>
		/// Sky temperature is outside defined limits.
		/// </summary>
		SkyTemperature = 7,

		/// <summary>
		/// Star full width at half maximum is outside defined limits.
		/// </summary>
		StarFWHM = 8,

		/// <summary>
		/// Ambient temperature is outside defined limits.
		/// </summary>
		AmbientTemperature = 9,

		/// <summary>
		/// Wind direction is outside defined limits.
		/// </summary>
		WindDirection = 10,

		/// <summary>
		/// Wind gust speed is outside defined limits.
		/// </summary>
		WindGust = 11,

		/// <summary>
		/// Wind speed is outside defined limits.
		/// </summary>
		WindSpeed = 12,

		/// <summary>
		/// A safety-related issue has been detected.
		/// </summary>
		Safety = 13,

		/// <summary>
		/// A security-related issue has been detected.
		/// </summary>
		Security = 14,

		/// <summary>
		/// A power-related issue has been detected.
		/// </summary>
		Power = 15,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved1=16,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved2 = 17,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved3 = 18,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved4 = 19,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved5 = 20,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved6 = 21,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved7 = 22,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved8 = 23,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved9 = 24,

		/// <summary>
		/// A reserved value for future enum growth
		/// </summary>
		Reserved10 = 25,

		/// <summary>
		/// Some other type of issue has been detected.
		/// </summary>
		Other = 1000
	}
}
