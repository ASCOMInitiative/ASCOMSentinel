using ASCOM.DeviceInterface;
using System.Collections.Generic;

namespace SafetyMonitorExtension
{
    /// <summary>
    /// Proposed interface additions to the SafetyMonitor interface that can be used to track safety events.
    /// </summary>
    public interface ISafetyMonitorV4 : ISafetyMonitorV3
    {
        /// <summary>
        /// Indicates whether the safety monitor supports management of externally generated safety events via the <see cref="SetExternalEvents"/> and <see cref="ClearExternalEvents"/> methods. 
        /// </summary>
        /// <returns>True if the safety monitor supports management of external events; otherwise, false.</returns>
        /// <exception cref="ASCOM.NotConnectedException">If the device is not connected.</exception>
        /// <exception cref="ASCOM.DriverException">An error occurred that is not described by one of the more specific ASCOM exceptions. Include sufficient detail in the message text to enable the issue to be accurately diagnosed by someone other than yourself.</exception> 
        /// <remarks>
        /// <p style="color:red;"><b>This is a mandatory property and must be functionally implemented.</b></p>
        /// <para>
        /// When true, both <see cref="SetExternalEvents"/> and <see cref="ClearExternalEvents"/> must be functionally implemented.
        /// </para>
        /// <para>
        /// When false, both methods must report a not implemented error.
        /// </para>
        /// </remarks>
        bool CanManageExternalEvents { get; }

        /// <summary>
        /// True when the monitored states are safe for use.
        /// </summary>
        /// <exception cref="ASCOM.NotConnectedException">If the device is not connected.</exception>
        /// <exception cref="ASCOM.DriverException">An error occurred that is not described by one of the more specific ASCOM exceptions. Include sufficient detail in the message text to enable the issue to be accurately diagnosed by someone other than yourself.</exception> 
        /// <remarks>
        /// <p style="color:red;"><b>This is a mandatory property and must be functionally implemented.</b></p>
        /// <para><see cref="IsSafe"/> must return <see cref="System.Boolean">true</see> when the safety monitor's own monitored states are safe <b>AND</b> there are no events in the external events list
        /// (when <see cref="CanManageExternalEvents"/> is <see cref="System.Boolean">true</see>).</para>
        /// <para><see cref="IsSafe"/> must return <see cref="System.Boolean">false</see> when one or more of the safety monitor's own monitored states are unsafe <b>OR</b> there is at least one event in the external events list 
        /// (when <see cref="CanManageExternalEvents"/> is <see cref="System.Boolean">true</see>).</para>
        /// </remarks>  
        new bool IsSafe { get; }

        /// <summary>
        /// Returns a list of safety events
        /// </summary>
        /// <returns>
        /// <para>An enumerable collection of <see cref="SafetyEvent"/> objects.</para>
        /// <para>Must return an empty collection when no events are active (must not return null).</para>
        /// </returns>
        /// <exception cref="ASCOM.NotConnectedException">If the device is not connected.</exception>
        /// <exception cref="ASCOM.DriverException">An error occurred that is not described by one of the more specific ASCOM exceptions. Include sufficient detail in the message text to enable the issue to be accurately diagnosed by someone other than yourself.</exception> 
        /// <remarks>
        /// <p style="color:red;"><b>This is a mandatory property and must be functionally implemented.</b></p>
        /// <para>The returned list must only contain entries with unique <see cref="SafetyEvent.Id" /> values.</para>
        /// <para>See <see href="SafetyEvents.htm" target="_self" >Implementation Notes</see> for information on how clients, Alpaca devices and drivers should implement this method.</para>
        /// </remarks>
        IEnumerable<SafetyEvent> SafetyEvents { get; }

        /// <summary>
        /// Adds a list of <see cref="SafetyEvent"/> objects to the list returned by <see cref="SafetyEvents"/>.
        /// </summary>
        /// <param name="safetyEventList">An enumerable list of <see cref="SafetyEvent"/> objects to be added to the current safety event list.</param>
        /// <exception cref="ASCOM.MethodNotImplementedException">when the device does not support management of external events.</exception>
        /// <exception cref="ASCOM.NotConnectedException">When the device is not connected.</exception>
        /// <exception cref="ASCOM.DriverException">An error occurred that is not described by one of the more specific ASCOM exceptions. Include sufficient detail in the message text to enable the issue to be accurately diagnosed by someone other than yourself.</exception> 
        /// <remarks>
        /// <p style="color:red;"><b>This is an optional method and can report a not implemented error.</b></p>
        /// <para>
        /// If an event already exists in the safety monitor's external events list, based on its EventId, the stored value must be updated with the revised information. 
        /// i.e. the list returned by <see cref="SafetyEvents"/> must not contain entries with duplicate <see cref="SafetyEvent.Id"/> values.
        /// </para>
        /// <para>See <a href="SetExternalEvents.htm" target="_self" >Implementation Notes</a> for information on how clients, Alpaca devices and drivers should implement this method.</para>
        /// </remarks>
        void SetExternalEvents(IEnumerable<SafetyEvent> safetyEventList);

        /// <summary>
        /// Removes a list of <see cref="SafetyEvent"/> objects from the list returned by <see cref="SafetyEvents"/>.
        /// </summary>
        /// <exception cref="ASCOM.MethodNotImplementedException">When the device does not support management of external events.</exception>
        /// <exception cref="ASCOM.InvalidValueException">One or more safety event ids are not recognised.</exception>
        /// <exception cref="ASCOM.NotConnectedException">When the device is not connected.</exception>
        /// <exception cref="ASCOM.DriverException">An error occurred that is not described by one of the more specific ASCOM exceptions. Include sufficient detail in the message text to enable the issue to be accurately diagnosed by someone other than yourself.</exception> 
        /// <param name="safetyEventIdList">An enumerable list of <see cref="SafetyEvent.Id">SafetyEvent.ID</see> strings to be cleared from the current safety event list.</param>
        /// <remarks>
        /// <p style="color:red;"><b>This is an optional method and can report a not implemented error.</b></p>
        /// <para>See <see href="ClearExternalEvents.htm" target="_self" >Implementation Notes</see> for information on how clients, Alpaca devices and drivers should implement this method.</para>
        /// </remarks>
        void ClearExternalEvents(IEnumerable<string> safetyEventIdList);
    }
}
