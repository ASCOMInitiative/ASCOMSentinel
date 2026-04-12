---
uid: Welcome
title: Safety Monitor Interface Proposal
toctitle: Introduction
keywords: Welcome
---
Today's safety monitor interface is basic, it simply provides a boolean IsSafe property indicating whether or not the monitored condition
is safe. No information is provided on why the condition is not safe, or what the monitored condition is. This proposal extends the 
safety monitor interface in two ways:
- Provide safety incident descriptions to clients about why the monitored condition is not safe.
- Enable clients and drivers to add their own safety incident descriptions and make the safety monitor report <code>IsSafe = false.</code>

The proposed interface is described in the SafetyMonitor section to the left, but it will be some time before it can be implemented 
in the Platform and ASCOM Library.
In the meantime, we propose an interim approach to trial the interface and gather feedback from clients and drivers as described below.

## Interim Approach
We propose that the interface be trialled using the Action / SupportedActions mechanic that all Platform 6 interface clients, drivers and devices 
support.

The following safety monitor Action names are reserved for this purpose:
- <b>SafetyEvents</b> - Returns a list of safety events that describe the current safety state of the device.
- <b>SetExternalEvents</b> - Accepts a list of safety events from the client and adds them to the device's list of safety events.
- <b>ClearExternalEvents</b> - Accepts a list of safety event IDs from the client and removes the corresponding events from the device's list of safety events.

### Safety Events Action
This will return a JSON encoded string containing an array of <code>SafetyEvent</code> objects that describe the 
current safety state of the device. 

### SetExternalEvents Action
This expects a JSON encoded string parameter containing an array of <code>SafetyEvent</code> objects that the 
client wants to add to the device's list of safety events. It returns an empty string.

### ClearExternalEvents Action
This expects a JSON encoded string parameter containing an array of string <code>SafetyEvent.Id</code> values that the 
client wants to remove from the device's list of safety events. It returns an empty string.