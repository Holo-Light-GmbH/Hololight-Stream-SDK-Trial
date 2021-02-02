# QR Code Support

<p align="center">
	<img src="imgs/isar_qrcode.png" width="380px">
</p>

## Overview
The QR Code Tracking supported by ISAR works differently compared to local QR Code Tracking Implementation. Based on the fact, that networking is between client and server, the API is event based. 
The important class to handle all QR Code events is **QrApi**. 

Example Implementations can be found in the script **QrSupport.cs**
This class implements and uses **QrApi** and simplyfies for you to use it.

## How To Use **QrSupport**

- Add the `QrSupport` to an GameObject in your Scene like this:
<p align="center">
	<img src="imgs/qrcode.png" width="580px">
</p>

- Once a client gets connected to the server, it will automatically call **Initialize()**.
- Call **RequestAccess()** to request Users Permission (need to do this only one time - until you reinstall the App or delete App Data). 	 
- Then you just need to call **StartWatching()**.

When QR Codes are found, the functions QrApi_OnAdded, QrApi_OnUpdated, etc. will be triggered. There you can read the content and do what ever you like. 

## How To Use **QrApi**

If you want to create your own wrapper around QrApi, feel free to do so. Check out how QrSupport is done.
It is very similar to the <a href="https://docs.microsoft.com/en-us/uwp/api/Windows.Devices.Enumeration.DeviceWatcher?view=winrt-19041">DeviceWatcher class</a> from Microsoft.

Register for these events: 
- QrApi.Added
- QrApi.Updated
- QrApi.Removed
- QrApi.EnumerationCompleted
- QrApi.IsSupportedReceived
- QrApi.AccessStatusReceived


Call **QrApi.Start()** to tell the HoloLens to start searching for QR Codes.
When it has found some, it will trigger the corresponding events.

Call **QrApi.RequestAccess()** to ask Permission from User to scan for QR Codes. 
This is needed only one time for the first use. The Status is saved through different App Session cycles. 

Call **QrApi.Stop()** if you don't need to track the QR Codes anymore.

