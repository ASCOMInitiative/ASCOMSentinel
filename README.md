# ASCOM Sentinel
This cross-platform application:
<ul>
   <li>Consolidates a number of real COM or Alpaca ObservingConditions devices into a single virtual device.</li>
   <li>Consolidates a number of real COM or Alpaca SafetyMonitor devices into a single virtual device.</li>
   <li>Allows the user to define rules for each virtual device ObservingConditions property that can trigger the virtual safety monitor to report <code>IsSafe</code> as false.</li> 
   <li>Employs caching on virtual device properties to support heavy client loads and protect back-end devices.</li>
   <li>Provides an optional two-level user / administrator security model to support use in community environments.</li>
   <li>Provides a graphical depiction of current observing conditions values as reported by the virtual device.</li>
    <li>Supports draft specifications for the following SupportedActions/Actions. See below for details:
   <ul>
       <li><code>GetSafetyState</code> - Returns a set of reasons identifying why the safety monitor reports <code>IsSafe</code> as false.</li>
       <li><code>SetSafetyState</code> - Enables clients and devices to add their own  safety events to the list returned by GetSafetyState.</li>
       <li><code>ClearSafetyState</code> -Enables clients and devices to clear their own safety events from the list.</li>
   </ul>
</ul>

<p style="color:darkorange"><b>This application is currently considered experimental and should only be used in conjunction with independent safety mechanics that will assure human and equipment safety.</b></p>

<p style="color:lightgreen"><b>All feedback on features, usefulness and operation is welcome, please send this as <a href=https://github.com/ASCOMInitiative/ASCOMSentinel/issues>GitHub Issues</a>.</p>

## The GetSafetyState Action
<p>The virtual safety monitor provides a <code>GetSafetyState</code> Action that returns a JSON object as a serialised string.</p>
<ul>
    <li><code style="display:inline-block; min-width:130px;">IsSafe = false</code> Returns an empty JSON array.</li>
    <li><code style="display:inline-block; min-width:130px;">IsSafe = true</code> Returns a JSON array of <code>SafetyState</code> objects.</li>
</ul>
<p>See below for the <code>SafetyState</code> class definition, its two associated enums, and an example JSON response string.</p>


## Getting Safety State Information
<p>
    ASCOM clients can check whether a safety monitor supports the GetSafetyState action by calling the 
    <a href="https://ascom-standards.org/newdocs/safetymonitor.html#SafetyMonitor.SupportedActions" target="_blank">SafetyMonitor.SupportedActions</a> method.
    The action name <code>GetSafetyState</code> will be returned in the list when the action is available.
</p>
<p>
    The serialised JSON string can be retrieved by calling the <a href="https://ascom-standards.org/newdocs/safetymonitor.html#SafetyMonitor.Action" target="_blank">SafetyMonitor.Action</a> method.
    e.g. <code>string serialisedJsonString = safetyMonitorClient.Action("GetSafetyState", "")</code>.
</p>

## SafetyState Event Class Definition
<p>
The <code>SafetyState</code> class is shown here: <a href="https://github.com/ASCOMInitiative/ASCOMSentinel/blob/main/Sentinel/SafetyState.cs" target="_blank">SafetyState.cs </a>
and its associated SafetyEventType (what type of event has happened) and SafetyEventCondition (what condition triggered the event) enums are shown here: <a href="https://github.com/ASCOMInitiative/ASCOMSentinel/blob/main/Sentinel/SafetyEventType.cs" target="_blank">SafetyEventType.cs</a> and <a href="https://github.com/ASCOMInitiative/ASCOMSentinel/blob/main/Sentinel/SafetyEventCondition.cs" target="_blank">SafetyEventCondition.cs</a>.
</p>

<h2 style="margin-bottom:0px;">Example JSON Response String</h2>
<pre>
[
    {
        "EventType":"SkyQuality",
        "EventCondition":"BelowLimit",
        "EventSource":"ASCOM Sentinel at My Observatory",
        "EventMessage":"SkyQuality rule 1 violated: Value 18.50 is less than 21.5.",
        "EventTimeUtc":"2026-03-28T16:44:14.1717927Z"
    },
    {
        "EventType":"WindGust",
        "EventCondition":"AboveLimit",
        "EventSource":"ASCOM Sentinel at My Observatory",
        "EventMessage":"WindGust rule 1 violated: Value 2.70 is greater than 2.5.",
        "EventTimeUtc":"2026-03-28T16:44:14.1756336Z"
    }
]
</pre>