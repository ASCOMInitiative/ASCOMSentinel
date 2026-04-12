---
uid: SafetyEvents
title: SafetyEvents Property Implementation Notes
tocTitle: SafetyEvents
# linkText: Optional Text to Use For Links
# keywords: keyword, term 1, term 2, "term, with comma"
# alt-uid: optional-alternate-id
# summary: Optional summary abstract
---

### Alpaca Devices
Alpaca devices should expect an HTTP GET request to the <code>SafetyEvents</code> endpoint and return a JSON array 
of <code>SafetyEvent</code> objects. This can be created in 
.NET by serializing a standard Alpaca response object whose <code>Value</code> property is a <code>List&lt;SafetyEvent&gt;</code> object.
The Alpaca response will be very similar to the [Supported Actions](https://ascom-standards.org/api/#/ASCOM%20Methods%20Common%20To%20All%20Devices/get__device_type___device_number__supportedactions)
response except that the JSON array will contain <code>SafetyEvent</code> objects rather than <code>string</code> objects.

### Alpaca Clients
Alpaca clients should send an HTTP GET request to the <code>SafetyEvents</code> endpoint and 
expect an Alpaca response, similar to  [Supported Actions](https://ascom-standards.org/api/#/ASCOM%20Methods%20Common%20To%20All%20Devices/get__device_type___device_number__supportedactions),
but where the <code>Value</code> property is a JSON array of &lt;SafetyEvent&gt; objects.	

### COM Drivers
COM drivers should return an <code>ArrayList</code> of <code>SafetyEvent</code> objects.

### COM Clients
COM clients must type the variable that receives the driver's response as IEnummerable or IEnumerable&lt;SafetyEvent&gt; rather 
than Arraylist, see note below.

> [!NOTE]
> .NET Framework has special support for COM objects that enables clients to detect the type of a returned object. However, this support
is not present in .NET Core and its responses appear to the client as an enumerable "COMObject" rather than as the ArrayList that the driver sent.
<br /><br />
If clients type the variable that receives the driver's response as <code>Arraylist</code>, the interface will work correctly if the driver 
is implemented in .NET Framework but not if it is implemented in .NET Core. 
If clients type the variable as IEnumerable or IEnumerable&lt;SafetyEvent&gt;, the interface will work correctly regardless 
of whether the driver is implemented in .NET Framework or .NET Core.

