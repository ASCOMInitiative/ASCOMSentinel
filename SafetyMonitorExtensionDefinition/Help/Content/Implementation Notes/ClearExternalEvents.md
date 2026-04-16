---
uid: ClearExternalEvents
title: ClearExternalEvents Method Implementation Notes
tocTitle: ClearExternalEvents
# linkText: Optional Text to Use For Links
# keywords: keyword, term 1, term 2, "term, with comma"
# alt-uid: optional-alternate-id
# summary: Optional summary abstract
---
### ClearExternalEvents Action (optional)
- **Action Name**: <code>ClearExternalEvents</code>
- **Action Parameters**: A JSON encoded string containing an array of <code>SafetyEvent.Id</code> string event ID values.
- **Returns**: <code>string.Empty</code>

<code language="JSON" title="Examples of serialised SafetyEvent.Id string arrays">
// Example 1 - Clear one event
[
    "F9D431A2-3FE6-46BE-B7C0-53EA34948934"
]
  
// Example 2 - Clear two events
[
    "F9D431A2-3FE6-46BE-B7C0-53EA34948934",
    "8C528476-658E-49D3-9CD0-D772F3451DA2"
]
</code>

*The response has been whitespace formatted for readability, but the actual response should be a single line of JSON text without unnecessary whitespace.*

### Alpaca Clients and Devices
Alpaca clients must send an HTTP PUT request to the safety monitor's <code>action</code> endpoint as described here: 
[Alpaca Action Endpoint](https://ascom-standards.org/api/#/ASCOM%20Methods%20Common%20To%20All%20Devices/get__device_type___device_number__action) with application/x-www-form-urlencoded body parameters:
- <code>ActionName</code> = "ClearExternalEvents"
- <code>ActionParameters</code> = A string containing a JSON encoded array of string <code>SafetyEvent.Id</code> event ID values.

Alpaca devices must return a standard Alpaca response object with:
- <code>Value</code> = "" and <code>ErrorNumber</code> = 0 if the request was successful
- <code>Value</code> = "" and <code>ErrorNumber</code> ≠ 0 and a descriptive <code>ErrorMessage</code> if they cannot complete the request.

<code language="JSON" title="Example of an Alpaca ClearExternalEvents Action JSON response">
{
    "Value":"",
    "ClientTransactionID":66,
    "ServerTransactionID":428,
    "ErrorNumber":0,
    "ErrorMessage":""
}
</code>

*The response has been whitespace formatted for readability, but the actual response should be a single line of JSON text without unnecessary whitespace.*

On receipt, the client should check the <code>ErrorNumber</code> field:
- = 0 - The events in the supplied list of event ID values have been cleared. 
    - The state of <code>IsSafe</code> is determined by whether any events remain in the external events queue and the state of the safety monitor's own monitored conditions.
- ≠ 0 - An error occurred and the <code>ErrorMessage</code> field should be examined to determine the cause of the error.

### COM Clients and Drivers
COM clients should call the safety monitor instance's Action method with <code>ActionName = "ClearExternalEvents"</code> and <code>ActionParameters</code> = A string containing a 
JSON encoded array of string <code>SafetyEvent.Id</code> event ID values as shown above.

COM devices should return an empty string. If the device cannot complete the request, it should throw an exception with a message describing the cause of the error.

On return from the call, COM clients should:
- No exception - The values were accepted and <code>IsSafe</code> will return false.
- Exception - Catch and handle any exception thrown by the safety monitor.