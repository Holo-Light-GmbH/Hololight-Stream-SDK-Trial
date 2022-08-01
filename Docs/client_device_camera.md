# Client device camera (Preview)

The client device camera API grants access to the device camera of a connected ISAR powered client, including the camera video stream, extrinsics & intrinsics.

## Platform Support

* Hololens 2

## Usage

The `CameraMetadataExample` prefab demonstrates the usage of the client device camera:
`CameraMetadataExample.cs` contains the usage of the client device camera API to access the device camera stream including their intrinsic and extrinsic data.

## Compatibility

The client device camera API is **incompatible** with the ISAR virtual camera. Do not use the client device camera API together with the ISAR virtual camera.

## Preview limitations

Due to technical limitations we currently do not provide proper extrinsic data (i.e. camera position & orientation).
