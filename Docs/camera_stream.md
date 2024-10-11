# Camera Stream

Hololight Stream supports the usage of the client device camera and to stream it towards the server application with the corresponding metadata.

## Platform Support

* Hololens 2
* iOS

## Usage

Open the `CameraStreamExample` scene, or drag the `CameraStreamExample` prefab into a scene. It demonstrates a potential usage of the camera stream feature, rendering the camera stream onto a canvas and providing some tools for control.

- XR View: If enabled, the augmented content is rendered into the received camera stream video.
- Display Metadata: If enabled, the received metadata is displayed in text format below.

### Camera Settings
- Enable Camera: The camera stream will only be recorded, sent and displayed, if this setting is enabled.
- Resolution and Framerate: Control the resolution and framerate of the camera stream by selecting a corresponding profile from this dropdown.
- Auto Exposure: If Auto Exposure is turned on, the Exposure slider won't affect the current streamed camera video. Otherwise the slider can be used to manipulate the Exposure values. Exposure Compensation works in both cases.

*Note* Turning the camera off, results in not recording at all on the client. On the Hololens there is a white lamp on the front of the device, that indicates a running camera.



