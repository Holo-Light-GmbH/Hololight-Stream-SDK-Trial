# Virtual Camera Stream
ISAR supports routing incoming audio from connected client device to a virtual microphone device which is added to system by "ISAR Virtual Microphone" driver. This driver basically creates two audio devices in the system, one is for rendering audio (speakers) and one is for capturing audio (microphone). In the end, audio streams that is rendered to rendering end gets automatically routed to capturing end so that client audio can be captured from Unity scene as if it is a plugged-in microphone device in the system. By default, microphone capture of connected client is disabled in ISAR. To enable it add "MicrophoneToggleExample" prefab to your scene and make sure to toggle it to enable client microphone.

## Installation
First of all, you need to install "ISAR Virtual Microphone" driver to your system.

## Usage
As an example to test the virtual microphone featue, add "VirtualMicrophoneExample" prefab to the Unity scene. If "ISAR Virtual Microphone" driver is installed succesfully to the system previously, the "VirtualMicrophoneExample" object will play the connected client's audio.
