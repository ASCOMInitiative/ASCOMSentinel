
namespace SafetyMonitor
{
	/// <summary>
	/// Specifies the types of safety-related environmental events that can be monitored or reported.
	/// </summary>
	/// <summary>
	/// Identifies the environmental or operational factor that has caused a safety condition
	/// to be evaluated as outside acceptable limits.
	/// </summary>
	public enum EventType
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
		/// Wind gust is outside defined limits.
		/// </summary>
		WindGust = 11,

		/// <summary>
		/// Wind speed is outside defined limits.
		/// </summary>
		WindSpeed = 12,

		/// <summary>
		/// A safety issue has been detected.
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
