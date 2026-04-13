---
uid: SetExternalEvents
title: SetExternalEvents Method Implementation Notes
tocTitle: SetExternalEvents
# linkText: Optional Text to Use For Links
# keywords: keyword, term 1, term 2, "term, with comma"
# alt-uid: optional-alternate-id
# summary: Optional summary abstract
---

### Alpaca Clients
Alpaca clients should send an HTTP PUT request to the <code>SetExternalEvents</code> endpoint with 
an application/x-www-form-urlencoded <code>SafetyEventList</code> body parameter 
containing a JSON array of <code>SafetyEvent</code> classes.

### Alpaca Devices
Alpaca devices should expect an Alpaca PUT request to the <code>SetExternalEvents</code> endpoint with 
an application/x-www-form-urlencoded <code>SafetyEventList</code> body parameter 
containing a JSON array of <code>SafetyEvent</code> classes.

### COM Clients
COM clients must send an <code>ArrayList</code> of <code>SafetyEvent</code> classes.

### COM Drivers
COM drivers should expect to receive an enumerable containing <code>SafetyEvent</code> classes.

The variable receiving the client's request must be typed as <code>IEnumerable</code>
or <code>IEnumerable&lt;SafetyEvent&gt;</code> rather than as <code>ArrayList</code>, see note below.

> [!NOTE]
> .NET Framework has special support for COM objects that enables a receiving application to detect the type 
of object it has received. However, this support is not present in .NET Core. When an ArrayList is sent by .NET Core code, 
the receiver sees it as an enumerable "COMObject" type rather than as an ArrayList.
>
> If drivers type the receiving variable as <code>ArrayList</code>, the interface will work correctly for clients
implemented in .NET Framework, but not for clients implemented in .NET Core.
>
> If drivers type the variable as ,<code>IEnumerable</code> or <code>IEnumerable&lt;string&gt;</code>, the 
interface will work correctly for both .NET Framework and .NET Core clients.