using System;
using System.Collections.Generic;
using System.Text;
namespace SafetyMonitorInterfaceProposal
{

    /// <summary>
    /// Represents a safety-related event, including its condition, type, source, message, and the UTC time it occurred.
    /// </summary>
    public class SafetyState
    {
        /// <summary>
        /// The human-readable name of the application, device, or driver that generated the event.
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// A short human-readable name for what is being monitored e.g. "Supply Voltage" or "Wind Speed".
        /// </summary>
        public string MonitorName { get; set; } = string.Empty;

        /// <summary>
        /// A unique ID, defined by the event source, to identify the monitor that triggered this event e.g. a GUID.
        /// </summary>
        /// <remarks>
        /// This field allows applications to update or remove an event that has already been sent to the safety monitor e.g. if the wind speed changes but the safety rule violation remains.
        /// This avoids the need to manage multiple events for the same condition.
        /// </remarks>
        public string MonitorId { get; set; } = string.Empty;

        /// <summary>
        /// The type of safety event that triggered the condition.
        /// </summary>
        public SafetyEventType EventType { get; set; }

        /// <summary>
        /// The condition that triggered the event.
        /// </summary>
        public SafetyEventCondition EventCondition { get; set; }

        /// <summary>
        /// A message providing additional context about the safety event.
        /// </summary>
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
        public SafetyState(string eventSource, string ruleName, string ruleId, SafetyEventType eventType, SafetyEventCondition eventCondition, string message)
        {
            ArgumentNullException.ThrowIfNull(eventSource, nameof(eventSource));
            ArgumentNullException.ThrowIfNull(ruleName, nameof(ruleName));
            ArgumentNullException.ThrowIfNull(ruleId, nameof(ruleId));
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            Source = eventSource;
            MonitorName = ruleName;
            MonitorId = ruleId;
            EventType = eventType;
            EventCondition = eventCondition;
            Message = message;
            EventTimeUtc = DateTime.UtcNow;
        }
    }
}

