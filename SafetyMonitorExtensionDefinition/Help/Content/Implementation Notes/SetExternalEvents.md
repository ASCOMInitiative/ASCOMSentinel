---
uid: SetExternalEvents
title: SetExternalEvents Method Implementation Notes
tocTitle: SetExternalEvents
# linkText: Optional Text to Use For Links
# keywords: keyword, term 1, term 2, "term, with comma"
# alt-uid: optional-alternate-id
# summary: Optional summary abstract
---
### SetExternalEvents Action (optional)
- **Action Name**: <code>SetExternalEvents</code>
- **Action Parameters**: A JSON encoded string containing an array of <code>SafetyEvent</code> objects.
- **Returns**: <code>string.Empty</code>

<code language="JSON" title="Examples of serialised SafetyEvents object arrays">
//Example 1 - One event active
[
    {
        "Source":"ASCOM Sentinel at My Observatory",
        "Name":"Observing conditions SkyBrightness",
        "Id":"723e775aab_SkyBrightness",
        "Type":"SkyBrightness",
        "Trigger":"AboveThreshold",
        "Description":"SkyBrightness rule 1 violated: Value 85.83 is greater than 0.25.",
        "EventTimeUtc":"2026-04-15T08:05:03.2139641Z"
    }
]
 
// Example 2 - Two events active
[
    {
        "Source":"ASCOM Sentinel at My Observatory",
        "Name":"Observing conditions SkyBrightness",
        "Id":"723e775aab_SkyBrightness",
        "Type":"SkyBrightness",
        "Trigger":"AboveThreshold",
        "Description":"SkyBrightness rule 1 violated: Value 85.83 is greater than 0.25.",
        "EventTimeUtc":"2026-04-15T08:05:03.2139641Z"
    },
    {
        "Source":"ASCOM Sentinel at My Observatory",
        "Name":"Observing conditions StarFWHM",
        "Id":"723e775aab_StarFWHM",
        "Type":"StarFWHM",
        "Trigger":"AboveThreshold",
        "Description":"StarFWHM rule 1 violated: Value 1.03 is greater than 0.8.",
        "EventTimeUtc":"2026-04-15T08:05:03.2147037Z"
    }
]
</code>

*The response has been whitespace formatted for readability, but the actual response should be a single line of JSON text without unnecessary whitespace.*

### Alpaca Clients and Devices
Alpaca clients must send an HTTP PUT request to the safety monitor's <code>action</code> endpoint as described here: 
[Alpaca Action Endpoint](https://ascom-standards.org/api/#/ASCOM%20Methods%20Common%20To%20All%20Devices/get__device_type___device_number__action) with application/x-www-form-urlencoded body parameters:
- <code>ActionName</code> = "SetExternalEvents"
- <code>ActionParameters</code> = A string containing a JSON encoded array of <code>SafetyEvent</code> objects.

Alpaca devices must return a standard Alpaca response object with:
- <code>Value</code> = "" and <code>ErrorNumber</code> = 0 if the request was successful
- <code>Value</code> = "" and <code>ErrorNumber</code> ≠ 0 and a descriptive <code>ErrorMessage</code> if they cannot complete the request.

<code language="JSON" title="Example of an Alpaca SetExternalEvents Action JSON response">
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
- = 0 - The values provided have been successfully set.
    - <code>IsSafe</code> will return false.
- ≠ 0 - An error occurred and the <code>ErrorMessage</code> field should be examined to determine the cause of the error.

### COM Clients and Drivers
COM clients should call the safety monitor instance's Action method with <code>ActionName = "SetExternalEvents"</code> and <code>ActionParameters</code> = A string containing a 
JSON encoded array of <code>SafetyEvent</code> objects as shown above.

COM devices should return an empty string. If the device cannot complete the request, it should throw an exception with a message describing the cause of the error.

On return from the call, COM clients should:
- No exception - The values were accepted and <code>IsSafe</code> will return false.
- Exception - Catch and handle any exception thrown by the safety monitor.