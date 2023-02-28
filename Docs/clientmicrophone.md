# Client Microphone Stream
ISAR supports routing incoming audio from a connected client device to the server application directly as a byte stream or to a virtual microphone device which is added to system by "ISAR Virtual Microphone" driver. This driver basically creates two audio devices in the system, one is for rendering audio (speakers) and one is for capturing audio (microphone). In the end, audio streams that is rendered to rendering end gets automatically routed to capturing end so that client audio can be captured from Unity scene as if it is a plugged-in microphone device in the system.
By default, microphone capture of connected client is disabled in ISAR. To enable it add "MicrophoneToggleExample" prefab to your scene and make sure to toggle it to enable client microphone capturing.

## Directly access captured client microphone data
It is possible to access audio data within the server application. Please take a look into the "AudioDataExample.cs" script. The `OnAudioDataReceived` method is an example implementation that shows how to directly access the audio byte stream and prints it to the console. 

## Virtual Microphone
Alternatively a virtual microphone driver can be installed to directly forward the audio data and act as a real capturing device.

### Installation
First of all, you need to install "ISAR Virtual Microphone" driver to your system. Navigate to the VMic/Installer folder and run "install.bat". The installer requires administrator permissions. After successful installation the device is listed in the device manager as "Isar Virtual Microphone".

### Usage
As an example to test the virtual microphone featue, add "VirtualMicrophoneExample" prefab to the Unity scene. If "ISAR Virtual Microphone" driver is installed succesfully to the system previously, the "VirtualMicrophoneExample" object will play the connected client's audio.

# Client Microphone Toggle
ISAR supports toggling microphone of connected client device. By default, microphone capture of connected client is disabled.

## Usage
As an example to test the microphone toggling feature add "MicrophoneToggleExample" prefab to the scene. By toggling "Enable Microphone Capture" you can enable or disable the microphone capturing on connected client.