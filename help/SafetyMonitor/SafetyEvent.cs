using System;
using System.Collections.Generic;
using System.Text;
namespace SafetyMonitor
{

    /// <summary>
    /// Represents a safety-related event, including its condition, type, source, message, and the UTC time it occurred.
    /// </summary>
    public class SafetyEvent
    {
		/// <summary>
		/// The name of the application, device, or driver that generated the event.
		/// </summary>
		/// <exception cref="ASCOM.InvalidValueException">Thrown if the source is null or empty.</exception>
		public string Source { get; set; } = string.Empty;

		/// <summary>
		/// A short name for what is being monitored e.g. "Supply Voltage" or "Wind Speed".
		/// </summary>
		/// <exception cref="ASCOM.InvalidValueException">Thrown if the name is null or empty.</exception>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// A unique ID, defined by the event source, to identify the event that triggered this safety condition.
		/// </summary>
		/// <exception cref="ASCOM.InvalidValueException">Thrown if the message is null or empty.</exception>
		/// <remarks>
		/// <para>
		/// This field allows applications to update or remove an event that has already been sent to the safety monitor e.g. if the wind speed changes but the safety rule violation remains.
		/// This avoids the need to manage multiple events for the same condition.</para>
		/// <para>Values should be chosen to minimise the chance of replicating IDs used by other sources. A GUID is recommended.</para>
		/// </remarks>
		public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The type of safety event that occurred.
        /// </summary>
        public EventType Type { get; set; }

        /// <summary>
        /// The condition that triggered the event.
        /// </summary>
        public TriggerCondition Trigger { get; set; }

        /// <summary>
        /// A message providing additional context about the safety event.
        /// </summary>
        /// <exception cref="ASCOM.InvalidValueException">Thrown if the message is null or empty.</exception>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The UTC time at which the event occurred.
        /// </summary>
        public DateTime EventTimeUtc { get; set; }

        /// <summary>
        /// Initializes a new instance of the SafetyEvent class with the specified event source, rule name, rule ID, type, condition, message, and time.
        /// </summary>
        /// <param name="eventSource">The component that generated the event.</param>
        /// <param name="ruleName">A human-readable name for the rule that triggered this event.</param>
        /// <param name="ruleId">A unique ID, defined by the event source, to identify the rule that triggered this event.</param>
        /// <param name="eventType">The category or type of the safety event.</param>
        /// <param name="eventCondition">The condition that triggered the safety event.</param>
        /// <param name="message">A message providing additional context about the safety event.</param>
        /// <remarks>The EventTimeUtc property is automatically set to the current UTC time when the event is created.</remarks>
        public SafetyEvent(string eventSource, string ruleName, string ruleId, EventType eventType, TriggerCondition eventCondition, string message)
        {
            ArgumentNullException.ThrowIfNull(eventSource, nameof(eventSource));
            ArgumentNullException.ThrowIfNull(ruleName, nameof(ruleName));
            ArgumentNullException.ThrowIfNull(ruleId, nameof(ruleId));
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            Source = eventSource;
            Name = ruleName;
            Id = ruleId;
            Type = eventType;
            Trigger = eventCondition;
            Message = message;
            EventTimeUtc = DateTime.UtcNow;
        }
    }
}

