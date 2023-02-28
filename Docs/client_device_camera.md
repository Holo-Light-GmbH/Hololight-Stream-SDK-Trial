# Client device camera

The client device camera API enables accessing the device camera of a connected ISAR powered client, including the camera video stream, extrinsics & intrinsics. In addition, the client device camera API supports adjusting camera settings while the camera stream is running.
The currently available camera settings are:

* resolution \& framerate (via presets)
* manual \& auto exposure
* exposure composition
* white balance

## Platform Support

* Hololens 2

## Usage

The `WebCamMetadataExample` prefab demonstrates the usage of the client device camera:

`WebCamMetadataExample.cs` contains the usage of the client device camera API to access the device camera stream including their intrinsic and extrinsic data.

`WebCamMetadataExampleEditor.cs` extends `WebCamMetadataExample.cs` allowing to set camera settings via the inspector while respecting the settings' ranges & step sizes.

**_Note:_** In order to toggle the camera and for changed camera settings to apply, it is necessary to reconfigure the camera using the **Reconfigure** button in the custom inspector for `WebCamMetadataExample`.
