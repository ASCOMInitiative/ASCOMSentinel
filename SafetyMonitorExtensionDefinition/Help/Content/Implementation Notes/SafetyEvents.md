---
uid: SafetyEvents
title: SafetyEvents Property Implementation Notes
tocTitle: SafetyEvents
# linkText: Optional Text to Use For Links
# keywords: keyword, term 1, term 2, "term, with comma"
# alt-uid: optional-alternate-id
# summary: Optional summary abstract
---
### SafetyEvents Action (mandatory)
- **Action Name**: <code>SafetyEvents</code>
- **Action Parameters**: <code>string.Empty</code>
- **Returns**: A JSON encoded string containing an array of <code>SafetyEvent</code> objects .

<code language="JSON" title="Examples of serialised SafetyEvents object arrays">
//Example 1 - No events active
[
]
 
//Example 2 - One event active
[
    {
        "Source":"ASCOM Sentinel at My Observatory",
        "Name":"Observing conditions SkyBrightness",
        "Id":"723e775aab_SkyBrightness",
        "Type":6,
        "Trigger":3,
        "Description":"SkyBrightness rule 1 violated: Value 85.83 is greater than 0.25.",
        "EventTimeUtc":"2026-04-15T08:05:03.2139641Z"
    }
]
 
// Example 3 - Two events active
[
    {
        "Source":"ASCOM Sentinel at My Observatory",
        "Name":"Observing conditions SkyBrightness",
        "Id":"723e775aab_SkyBrightness",
        "Type":6,
        "Trigger":3,
        "Description":"SkyBrightness rule 1 violated: Value 85.83 is greater than 0.25.",
        "EventTimeUtc":"2026-04-15T08:05:03.2139641Z"
    },
    {
        "Source":"ASCOM Sentinel at My Observatory",
        "Name":"Observing conditions StarFWHM",
        "Id":"723e775aab_StarFWHM",
        "Type":9,
        "Trigger":3,
        "Description":"StarFWHM rule 1 violated: Value 1.03 is greater than 0.8.",
        "EventTimeUtc":"2026-04-15T08:05:03.2147037Z"
    }
]
</code>

*The response has been whitespace formatted for readability, but the actual response should be a single line of JSON text without unnecessary whitespace.*

> [!NOTE]
>If there are no active events, the device must return an empty JSON array (see Example 1 above) rather than an empty string or a null value. Do not return an error or throw an exception unless something is genuinely broken.

### Alpaca Clients and Devices
Alpaca clients must send an HTTP PUT request to the safety monitor's <code>action</code> endpoint as described here: 
[Alpaca Action Endpoint](https://ascom-standards.org/api/#/ASCOM%20Methods%20Common%20To%20All%20Devices/get__device_type___device_number__action) with application/x-www-form-urlencoded body parameters:
- <code>ActionName</code> = "SafetylEvents"
- <code>ActionParameters</code> = An empty string.

Alpaca devices must return a standard Alpaca response object whose <code>Value</code> field is a string containing a JSON encoded array of <code>SafetyEvent</code> objects. Or return an error if they cannot complete the request.

<code language="JSON" title="Example of an Alpaca SafetyEvents Action JSON response">
{
    "Value":"[
                {
                    "Source":"ASCOM Sentinel at My Observatory",
                    "Name":"Observing conditions StarFWHM",
                    "Id":"c25e51ed9e_StarFWHM",
                    "Type":3,
                    "Trigger":3,
                    "Description":"StarFWHM rule 1 violated: Value 1.07 is greater than 0.8.",
                    "EventTimeUtc":"2026-04-16T08:00:54.8172238Z"
                }
            ]",
    "ClientTransactionID":66,
    "ServerTransactionID":428,
    "ErrorNumber":0,
    "ErrorMessage":""
}
</code>

*The response has been whitespace formatted for readability, but the actual response should be a single line of JSON text without unnecessary whitespace.*

On receipt, the client should check the <code>ErrorNumber</code> field:
- = 0 - The <code>Value</code> field can be de-serialised to yield an enumerable collection of <code>SafetyEvent</code> objects.
- ≠ 0 - An error occurred and the <code>ErrorMessage</code> field should be examined to determine the cause of the error.

### COM Clients and Drivers
COM clients should call the safety monitor instance's Action method with <code>ActionName = "SafetyEvents"</code> and <code>ActionParameters = ""</code>.

COM devices should return a string containing a JSON array of <code>SafetyEvent</code> objects as shown above.

On return from the call, COM clients should:
- No exception - De-serialise the returned string to an enumerable collection of <code>SafetyEvent</code> objects
- Exception - Catch and handle any exception thrown by the safety monitor.