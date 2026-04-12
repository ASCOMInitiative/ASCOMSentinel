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
		/// <exception cref="ASCOM.InvalidValueException">Thrown if Source is null or an empty string.</exception>
		public string Source { get; set; }

		/// <summary>
		/// A short name for what is being monitored e.g. "Supply Voltage", "Perimeter Alarm", "Wind Speed".
		/// </summary>
		/// <exception cref="ASCOM.InvalidValueException">Thrown if Name is null or an empty string.</exception>
		public string Name { get; set; }

		/// <summary>
		/// A unique ID that identifies the event that triggered this safety condition.
		/// </summary>
		/// <exception cref="ASCOM.InvalidValueException">Thrown if Id is null or an empty string.</exception>
		/// <remarks>
		/// <para>
		/// This field enables client applications to safely add a new event, update an existing event or remove an event from the list provided by <see cref="ISafetyMonitorV4.SafetyEvents">ISafetyMonitorV4.SafetyEvents</see> .
		/// </para>
		/// <para>
		/// Safety monitors that implement this method should ensure that their update mechanic enforces Id as a unique key. so that, for example, if the wind speed changes but the trigger condition is still met, 
		/// the existing event is updated rather than a new event being added to the list.
		/// </para>
		/// <para>
		/// Values should be chosen to minimise the chance of replicating IDs used by other sources. A GUID is recommended.
		/// </para>
		/// </remarks>
		public string Id { get; set; }

        /// <summary>
        /// The type of safety event that occurred.
        /// </summary>
        public SafetyEventType Type { get; set; }

        /// <summary>
        /// The condition that triggered the safety event.
        /// </summary>
        public SafetyEventTrigger Trigger { get; set; }

		/// <summary>
		/// A description of the event and why it was triggered.
		/// </summary>
		/// <exception cref="ASCOM.InvalidValueException">Thrown if Description is null or an empty string.</exception>
		public string Description { get; set; }

        /// <summary>
        /// The UTC time at which the event occurred.
        /// </summary>
        /// <remarks>
        /// Should default to DateTime.UtcNow.
        /// </remarks>
        public DateTime EventTimeUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the SafetyEvent class with the specified event source, rule name, rule ID, type, condition, message, and time.
        /// </summary>
        /// <param name="source">The component that generated the event.</param>
        /// <param name="name">A human-readable name for the rule that triggered this event.</param>
        /// <param name="id">A unique ID, defined by the event source, to identify the rule that triggered this event. See <see cref="Id"/> for more details.</param>
        /// <param name="type">The type of safety event.</param>
        /// <param name="trigger">The condition that triggered the safety event.</param>
        /// <param name="description">A description providing additional context about the safety event.</param>
        /// <remarks>The EventTimeUtc property should be automatically set to the current UTC time when the event is created.</remarks>
        public SafetyEvent(string source, string name, string id, SafetyEventType type, SafetyEventTrigger trigger, string description)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(description, nameof(description));

            Source = source;
            Name = name;
            Id = id;
            Type = type;
            Trigger = trigger;
            Description = description;
            EventTimeUtc = DateTime.UtcNow;
        }
    }
}

