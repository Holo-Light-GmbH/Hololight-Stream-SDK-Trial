**DEPRECATED:**
The virtual microphone is deprecated and will be removed. Look into: [Microphone](./microphone.md). After updating to the new API, please uninstall the Hololight Stream Virtual Microphone driver.

# Virtual Microphone Stream
Hololight Stream supports routing incoming audio from a connected client device to a virtual microphone device. This is made possible through the installable `ISAR Virtual Microphone` driver. By default, microphone capture of the connected client device is disabled. To enable it, add the `MicrophoneToggleExample` prefab to the scene and toggle the checkbox to enable the client microphone.

## Installation
The installer can be found [here](https://hololight.com/downloads/). Download and run the installer to install the `ISAR Virtual Microphone` device.

## Example
To test the virtual microphone device, add the `VirtualMicrophoneExample` prefab to the Unity scene. If the `ISAR Virtual Microphone` is installed, the audio will play on the connected client device.
