# Client device camera (Preview)

The client device camera API grants access to the device camera of a connected ISAR powered client, including the camera video stream, extrinsics & intrinsics.

## Platform Support

* Hololens 2

## Usage

In your project, browse for the `CameraMetadataExample` prefab. This is an example that demonstrates how to access the camera frames and the metadata and renders 
the current camera frame with the metadata to a render texture plane.
Calls to change camera status (currently only: Off/On) can only be executed during an established connection. By default the camera is turned off. Follow the toggle logic in the example for more information. 

### Initialization
In the `Start` method a `Texture2D` is created with the given ISAR camera stream format: WIDTH = 1504, HEIGHT = 846, TextureFormat.RGBA32. Other formats are currently not supported.
In this example the texture is set as main texture to a renderer (in this case the render component on the plane gameobject).
A native unmanaged pointer must be allocated with a corresponding length in bytes: IsarClientCamera.SIZE which is calculated by: SIZE = (WIDTH * HEIGHT * BIT_COUNT) / sizeof(byte); (BIT_COUNT = 32 for RGBA32)

In the `OnEnable` function, create an `IsarClientCamera` to access ISAR native functions. Then add listener functions to `_isar.OnConnectionStateChanged` and `_isar.OnMetadataReceived`.
Within the `OnConnectionStateChanged` method everytime the state is set to connected the cached metadata is cleared again.

It's necessary to also dispose the isar object properly in `OnDisable`. In the example also cached dictionary keys and metadata is cleared.

### Getting Frames and Metadata
The camera frame and metadata are sent separate from each other and must be synchronized again after receiption.
The metadata is received by the `OnMetadataReceived` method. In this example it just adds the metadata to a dictionary to match them with a video frame using the timestamp.
In Unity's update loop the frames are then grabbed by `_isar.TryDequeueFrame` which provides a native pointer that is afterwards used as source to update the `Texture2D`,
which was created and allocated in the `Start` function.   
Metadata only provides proper Intrinsic values. Extrinsic values are currently placeholder and will be handled in a later Isar release.

## Compatibility

The client device camera API is **incompatible** with the ISAR virtual camera. Do not use the client device camera API together with the ISAR virtual camera.

## Preview limitations

Due to technical limitations we currently do not provide proper extrinsic data (i.e. camera position & orientation).
