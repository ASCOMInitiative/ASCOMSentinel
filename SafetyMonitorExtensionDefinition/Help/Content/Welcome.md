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
and this revision is a response to that feedback.

This proposal extends the safety monitor interface in two ways:
- Provide safety event descriptions to clients about why monitored conditions are not safe.
- Enable clients and drivers to add their own safety event descriptions and make the safety monitor report <code>IsSafe = false.</code>

As ever, these changes are backward compatible with existing devices.

The proposed interface is described in the [SafetyMonitorExtension](bc4fa0b1-a8e7-4e7f-43d7-4c21a5070578.htm) section 
to the left, but it will be some time before it can be implemented in the Platform and ASCOM Library.
In the meantime, we propose an interim approach to trial the interface and gather feedback from client, driver and device authors.

## Interim Approach
We propose to trial the interface using the Action / SupportedActions mechanic that all Platform 6 interface clients, drivers and devices 
support.

The following safety monitor Action names are now reserved for this purpose:
- <b>SafetyEvents</b> - Returns a list of safety events that describe the current safety state of the device.
- <b>SetExternalEvents</b> - Accepts a list of safety events from the client and adds them to the device's list of safety events.
- <b>ClearExternalEvents</b> - Accepts a list of safety event IDs from the client and removes the corresponding events from the device's list of safety events.

Please see the [SafetyMonitorExtension](bc4fa0b1-a8e7-4e7f-43d7-4c21a5070578.htm) section opposite for class and enum definitions and behavioural information.

The Action behaviours are as follows:

### SafetyEvents Action (mandatory)
- **Action name**: <code>SafetyEvents</code>
- **Parameters**: <code>string.Empty</code>
- **Returns**: A JSON encoded string containing an array of <code>SafetyEvent</code> objects .

### SetExternalEvents Action (optional)
- **Action name**: <code>SetExternalEvents</code>
- **Parameters**: JSON encoded string containing an array of <code>SafetyEvent</code> objects to add to the device's list of external safety events.
- **Returns**: <code>string.Empty</code>

### ClearExternalEvents Action (optional)
- **Action name**: <code>ClearExternalEvents</code>
- **Parameters**: JSON encoded string containing an array of <code>SafetyEvent.Id</code> string values identifying the event IDs
to be removed from the device's list of external safety events.
- **Returns**: <code>string.Empty</code>
 
## Test Support
The new [ASCOM Sentinel](https://github.com/ASCOMInitiative/ASCOMSentinel/releases) application provides a reference implementation 
of the proposed interface and can be used to trial the interface. 

[Conform Universal](https://github.com/ASCOMInitiative/ConformU/releases) acts as a reference client 
implementation and can be used to assess operation of Sentinel as well as checking operation of any exploratory Alpaca or COM implementations.
test updates of safety monitor devices.

A NuGet package is available that contains the class and enum definitions required to implement the 
proposed interface and is available from our MyGet feed: https://www.myget.org/F/ascom-initiative/api/v3/index.json. You will need to add this URL
as a package source in your development tooling in order to install the package, which is called <code>ISafetyMonitorV4Components</code>.