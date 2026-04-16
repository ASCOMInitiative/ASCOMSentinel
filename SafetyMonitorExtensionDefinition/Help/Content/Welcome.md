---
uid: Welcome
title: Safety Monitor Interface Proposal
toctitle: Introduction
keywords: Welcome
---
## Introduction
Today's safety monitor interface is basic, it simply provides a boolean IsSafe property indicating whether or not a monitored condition
is safe. No information is provided about why the condition is unsafe, or what the monitored condition is. 

We have received community feedback about enabling SafetyMonitor devices to provide context about the source of the unsafe condition, 
and this interface revision is a response to that feedback.

This proposal extends the safety monitor interface in two ways:
- Provides safety event descriptions to clients about why monitored conditions are not safe. *(Must be implemented.)*
- Enables clients and drivers/devices to add their own safety event descriptions and make the safety monitor report <code>IsSafe = false.</code>
*(Optional implementation.)*

The <code>IsSafe</code> property is unchanged so these additions are backward compatible with existing clients.

The proposed interface is described in the [SafetyMonitorExtension](bc4fa0b1-a8e7-4e7f-43d7-4c21a5070578.htm) section 
to the left and may change in the light of feedback. 

**To enable the interface to be trialled immediately,
we propose an interim approach implemented through the Action / SupportedActions mechanic that all Platform 6 and later clients, 
drivers and devices support.**

## Interim Approach using Action / SupportedActions

The following safety monitor Action names are reserved to implement the three new members:
- <div style="display:inline-block;min-width:160px"><b>SafetyEvents</b></div> - Returns a list of safety events that describe the current safety state of the device.
- <div style="display:inline-block;min-width:160px"><b>SetExternalEvents</b></div> - Accepts a list of safety events and adds them to the device's list of safety events.
- <div style="display:inline-block;min-width:160px"><b>ClearExternalEvents</b></div> - Accepts a list of safety event IDs and removes the corresponding events from the device's list of safety events.

Please see the [SafetyMonitorExtension](bc4fa0b1-a8e7-4e7f-43d7-4c21a5070578.htm) section opposite for class and enum definitions and behavioural information.

### SupportedActions

The <code>SupportedActions</code> property should return these Action names:
- <code>SafetyEvents</code> (mandatory)
- <code>SetExternalEvents</code> (only if implemented)
- <code>ClearExternalEvents</code> (only if implemented)

Please note that there is no need to implement the <code>CanManageExternalEvents</code> property in this interim implementation because 
the presence of the <code>SetExternalEvents</code> and <code>ClearExternalEvents</code> action names in the <code>SupportedActions</code> list is sufficient to 
indicate that external events can be managed.

The Action behaviours are as follows:

### SafetyEvents Action (mandatory)
- **Action Name**: <code>SafetyEvents</code>
- **Action Parameters**: <code>string.Empty</code>
- **Returns**: A string containing a JSON encoded array of <code>SafetyEvent</code> objects .

See [SafetyEvents Action Implementation Notes](SafetyEvents.htm) for more information.

### SetExternalEvents Action (optional)
- **Action Name**: <code>SetExternalEvents</code>
- **Action Parameters**: A string containing a JSON encoded array of <code>SafetyEvent</code> objects to add to the device's list of external safety events. 
- **Returns**: ""

See [SetExternalEvents Action Implementation Notes](SetExternalEvents.htm) for more information.

### ClearExternalEvents Action (optional)
- **Action Name**: <code>ClearExternalEvents</code>
- **Action Parameters**: A string containing a JSON encoded array of <code>SafetyEvent.Id</code> string values that identify the events to be removed from the device's list of external safety events.
- **Returns**: ""

See [ClearExternalEvents Action Implementation Notes](ClearExternalEvents.htm) for more information.

## Test Support
Three areas of support are available to enable the proposed interface to be trialled immediately:

- **Testing Clients** - The new [ASCOM Sentinel](https://github.com/ASCOMInitiative/ASCOMSentinel) 
(link to [Latest Release](https://github.com/ASCOMInitiative/ASCOMSentinel/releases)) application provides a reference implementation 
of the revised SafetyMonitor interface that clients can use to trial the interface. 
- **Testing Alpaca Devices and COM Drivers** - The latest version of [Conform Universal](https://github.com/ASCOMInitiative/ConformU/releases) 
behaves as a reference client and displays safety events returned by Alpaca and COM driver implementations through <code>SafetyEvents</code>. 
It also exercises the <code>SetExternalEvents</code> and <code>ClearExternalEvents</code> actions to allow testing of these optional features.
- **Code Definitions** - A NuGet package containing the class and enum definitions required to implement the 
proposed interface is available from our MyGet feed. You will need to add this URL: https://www.myget.org/F/ascom-initiative/api/v3/index.json
as a package source in your development tooling in order to see the package, which is named <code>ISafetyMonitorV4Components</code>.