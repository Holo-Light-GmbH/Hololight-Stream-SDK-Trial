# QR Code Support

<p align="center">
	<img src="images/isar_qrcode.png" width="380px">
</p>

## Overview
The ISAR SDK provides functionality to scan QR Codes in the physical environment and receive the information within Unity to be processed.

This functionality has been implemented as a tracked AR Subsystem called `XRQrCodeTrackingSubsystem`. This follows similar implementations to other Unity subsystems, such as [image tracking](https://docs.unity3d.com/Packages/com.unity.xr.arsubsystems@4.1/manual/image-tracking.html) and [anchors](https://docs.unity3d.com/Packages/com.unity.xr.arsubsystems@4.1/manual/anchor-subsystem.html).

To ease usage, an AR Foundation manager has been provided called ARQrCodeTrackerManager which interfaces with the underlying subsystem for tracking QR codes.

## AR QR Code Tracking Manager
The QR code tracking manager creates GameObjects for each detected QR code in the environment. Once detected, the QR Code information can be read and processed. This manager can be found at the namespace `HoloLight.Isar.ARFoundation`.

## Responding to detected QR codes
Subscribe to the ARQrCodeTrackerManager's `trackedQrCodesChanged` event to be notified whenever a QR code is added (i.e., first detected), updated, or removed:

```
[SerializeField]
ARQrCodeTrackerManager m_QrCodeTrackerManager;

void OnEnable() => m_QrCodeTrackerManager.trackedQrCodesChanged += OnChanged;

void OnDisable() => m_QrCodeTrackerManager.trackedQrCodesChanged -= OnChanged;

void OnChanged(ARQrCodeChangedEventArgs eventArgs)
{
    foreach (var newQrCode in eventArgs.added)
    {
        // Handle added event
    }

    foreach (var updatedQrCode in eventArgs.updated)
    {
        // Handle updated event
    }

    foreach (var removedQrCode in eventArgs.removed)
    {
        // Handle removed event
    }
}
```

You can also get all the currently tracked QR codes with the ARQrCodeTrackerManager's `trackables` property. This acts like an IEnumerable collection, so you can use it in a foreach statement:

```
void ListAllQrCodes()
{
    Debug.Log(
        $"There are {m_QrCodeTrackingManager.trackables.count} QR codes being tracked.");

    foreach (var trackedQrCode in m_QrCodeTrackingManager.trackables)
    {
        Debug.Log($"QR Code: {trackedQrCode.trackableId} is at " +
                  $"{trackedQrCode.transform.position}");
    }
}
```

Or access a specific image by its TrackableId:

```
ARQrCode GetQrCodeAt(TrackableId trackableId)
{
    return m_QrCodeTrackingManager.trackables[trackableId];
}
```

## Grabbing QR Code Data
The ARQrCode class contains a byte array property called `data`. This contains the information which has been read from the QR code. This data can be used for carrying out specific logic based on the type of QR code.

This data is specific to the QR code version and has to be read correctly. To aid in this, a helper function is provided with the ARQrCode class called `ReadData`. This function will convert the QR code data from a byte array to a string for usage.

Note, the `ReadData` function assumes that the QR code data read was in UTF8 encoding format. If this is not the case, custom parsing will be required.

## QR Code Prefab
The ARQrCodeTrackingManager has a "QR Code Prefab" field; however, this is not intended for content. When a QR code is detected, ARFoundation will create a new GameObject to represent it.

If "Qr Code Prefab" is null, then ARFoundation simply creates a GameObject with an `ARQrCode` component on it. However, if you want every tracked QR code to also include additional components, you can provide a prefab for ARFoundation to instantiate for each detected QR code. In other words, the purpose of the prefab field is to extend the default behavior of tracked QR codes; it is not the recommended way to place content in the world.

If you would like to instantiate content at the pose of the detected QR codes and have its pose updated automatically, then you should parent your content to the `ARQrCode`.

## Enabling and disabling QR code tracking
The ARQrCodeTrackingManager has an `enabled` property which can be set to enable/disable the QR code tracking. When finished tracking, it is recommended to set the `enabled` property to false to provide better performance.

## More Info
See [Unity's AR Foundation Trackable Managers Documentation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.1/manual/trackable-managers.html) for more information on usage of AR Foundation trackable managers.

