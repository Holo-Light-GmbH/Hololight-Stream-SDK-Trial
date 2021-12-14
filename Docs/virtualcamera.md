# Virtual Camera Stream

## Installation
First requirement for a working camera stream solution is to install the virtual camera. Navigate to the "VCam" folder and run "install_vcam.bat". It will ask for administrator privilege. 
Uninstall works equally by running "uninstall_vcam.bat". If no virtual camera is installed the system will use the first availlable one.

## Usage
As a little example to display the virtual camera stream, drag the "WebCamTextureExample" prefab into your Scene or optionally add some plane 3d object with a mesh filter and a mesh renderer to a Unity Scene (by right click -> Add -> 3d Object -> Plane. 
Add the "WebCamTextureExample.cs" component, the "VideoToggleExample.cs" component and remove the collider. Camera stream is sent in 16/9 format. For correct display scale your plane to match this constraint and a good size. By default the camera stream is turned off. The "Toggle" field in the "CameraStreamToggle" component will turn on/off a camera stream from your client.
For a better understanding in how to receive and use a camera stream, check both components code.