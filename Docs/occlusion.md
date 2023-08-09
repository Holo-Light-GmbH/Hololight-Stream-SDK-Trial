# Occlusion Support

## Overview
The ISAR SDK provides functionality to toggle occlusion feature and send camera information for iOS and iPadOS devices. Occlusion quality will increase depending on the sensor installed on your device and depth map resolution of your device.

**Note:** This feature is currently only available on Apple devices equipped with a LIDAR sensor. Using this with a non-LIDAR Apple device will have no effect.

Depth/alpha information must be enabled to use this feature under Project Settings → XR Plug-in Management → ISAR. The main cameras alpha value must be set to 255 to achieve non transparent blending with the background.

For a simple use, a class called OcclusionExtension has been provided. This class manages the logic required to toggle and configure occlusion functionality on the client device.

## Occlusion Extension
The occlusion manager operates on the underlying data channel for occlusion, which sends toggle and configuration messages to the client device. This manager can be found at the namespace `HoloLight.Isar`.

## Usage
The occlusion toggle functionality can be used by calling the `SendToggle()` function. This will send an integer value, interpreted as "on" for 1 and "off" for 0 on the client device.

```cs
public void SendToggleWrapper()
{
	if (_occlusionExtension != null && _occlusionExtension.IsConnected)
	{
		if (_isOcclusionEnabled == 0)
		{
			_isOcclusionEnabled = 1;
		} else
		{
			_isOcclusionEnabled = 0;
		}
			_occlusionExtension.SendToggle(_isOcclusionEnabled);
	}
}
```

The camera configuration can be sent (which includes near and far plane distance values of your camera) using the `SendCameraInfo()` function. This function requires a `Camera` object to read camera information.

**Note:** `SendCameraInfo()` function must be called on Unity main thread, otherwise main camera will not be accessible.

```cs
public void SendCameraWrapper(Camera cam)
{
	if (_occlusionExtension != null && _occlusionExtension.IsConnected)
	{
		_occlusionExtension.SendCameraInfo(cam);
	}
}
```

One important point is that you should subscribe to `OnConnectionChanged` event if you want to provide immediate occlusion information to your client device when connecting. You can achieve this using the method shown below:

```cs
private void OnEnable()
{
	_occlusionExtension = DataChannelManager.GetDataChannel<OcclusionExtension>();
	if (_occlusionExtension != null)
	{
		_occlusionExtension.Start();
		_occlusionExtension.OnConnectionChanged += OcclusionExtension_OnConnectionChanged;
	}
}

private void OcclusionExtension_OnConnectionChanged(bool connected)
{
	_readyToSend = connected;
}

private void Update()
{
	if (_readyToSend)
	{
		_occlusionExtension.SendCameraInfo(Camera.main);
		_occlusionExtension.SendToggle(_isOcclusionEnabled);
		_readyToSend = false;
	}
}
```

## Samples

For further information on how to use this feature, see the `OcclusionController` sample within the `DataChannel` folder of the `com.hololight.isar.examples` package. This sample sends configuration and toggle messages. This script can be added to an empty object in the scene to trigger the functions to enable/disable the occlusion.
