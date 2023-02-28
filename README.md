# ISAR SDK

<p align="center">
    <img src="Docs/images/ISAR_Icon.png" width="180px">
</p>

ISAR (Interactive Streaming for Augmented Reality) is a Software Development Kit that allows developers to stream their applications remotely to an XR device, with minimal integration overhead, from managed environments, hosted clouds or on-premise solutions.

The following documentation specifically references usage with the Unity Game Engine, however the ISAR SDK can be used independently of Unity. For more support regarding usage outside of Unity, see the [Support](https://support.holo-light.com/hc/en-us) section of our website.

## Getting Started

> :warning: It is recommended not to change any source files delivered with ISAR. Support cannot be guaranteed if changes are made.

### Prerequisites

- Minimum Unity 2020.3.x (tested with 2020.3.23)

### First Installation

1. Open an existing Unity 3D project or create a new one
2. Remove previous versions of ISAR from the project (skip this if the project does not currently have ISAR installed)
    - Open **Package Manager** in the Unity editor (`Window -> Package Manager`)
    - Find the **ISAR Core** package and choose **Remove**
    - Find the **ISAR MRTK Extensions** package, if installed, and choose **Remove**
	- Find the **ISAR Examples** pacakge, if installed, and choose **Remove**
3. Import **ISAR Core** into the project
    - Open **Package Manager** in the Unity editor (`Window -> Package Manager`)
    - If installed, remove the **Version Control** package from the project
    - Click the **+** then **Add package from disk...** and select the file `package.json` from the `com.hololight.isar` directory contained in this repository
4. Add the desired toolset following the instructions below
    - [MRTK](./Docs/mrtkextension.md); supports HoloLens 2, Oculus Quest 2 and Android clients
    - [XR Interaction Toolkit](./Docs/xrInteractionToolkit.md); supports Oculus Quest 2 client
    - [AR Foundation](./Docs/arfoundation.md); supports Android client

### Project Configuration

1. Navigate to `Edit -> Project Settings -> XR Plug-in Management`
    - Enable **ISAR XR**
2. Open `File -> Build Settings...`
    - Ensure the following configuration is selected:
        - Platform: **PC, Mac & Linux Standalone** + Architecture: **x86_64**

### ISAR Configuration File

Before connecting to the server, ISAR must be configured for the client device which will be used. To select the configuration required:

- Navigate to `ISAR -> Configure Rendering Settings`
- Select the client device configuration in the `Rendering Presets` dropdown box
- Click `Apply`

In addition to the rendering presets provided, it is possible to add and use self-defined presets. To do so:

- Navigate to `ISAR -> Configure Rendering Settings`
- Click `Add New Preset`
- Set the `Preset Name`
- Configure the settings for the specific device:
    - `width` and `height` determine the resolution of the rendered image per view/eye. Careful, the resolution needs to be supported by the H264 hardware decoder of the client device
    - `numViews` determines the number of views/eyes rendered on the client device. Valid values are:
        - `1` for mono rendering, e.g. Tablets
        - `2` for stereo rendering, e.g. HoloLens
    - `bandwidth` allows the developer to specify the bitrate for the encoder. If no value is set, a default value of 20Mbps is used
- Click `Add`
- Click `Apply`

The configuration file now also contains a signaling port number. It can be modified by
1. In Unity Editor navigate to `ISAR -> Configure Signaling Port` and enter any number between 1024 and 65535. By default port 9999 is used.
2. The remoting-cfg file can be modified any time (also after compilation). The port number can be replaced by any port number between 1024 and 65535.


### First Run

Enter play mode in the Unity editor or build a standalone application with the following settings:

- Platform: **PC, Mac & Linux Standalone**
- Target Platform: **Windows**
- Architecture: **x86_64**

The ISAR server application will start listening by default on the TCP port 9999 or the modified port number. The ISAR client application is using this port to establish the streaming session. If the connection fails, ensure that no firewall is blocking it.

### Disabling ISAR

The ISAR SDK can be disabled while remaining as a package in the project. To do so, follow the below steps:

- Click `Edit->Project Settings` and move to the **XR Plug-in Management** section
- Uncheck **ISAR XR**
- Check **Unity Mock HMD**
- Ensure not to call the `Isar` class, or any of the inherited classes (`IsarViewPose`, `IsarAudio`, `IsarCustomSend` or `IsarQr`), constructor within the code
- Disable the `QrSupport` script if it is enabled

## Clients

ISAR SDK supports several clients that can connect to the server. For information on installing and connecting with specific clients, refer to [Clients](./Docs/clients.md).

## Example

A preconfigured example is available within the repository. For information on how to load this example, refer to [Example](./Docs/example.md).

## Additional Features

### QR Code

To find out how to easily integrate QR code support, see [here](./Docs/qrcode.md).

### Image Tracking

To find out how to enable and use image tracking, see [here](./Docs/imagetracking.md).

## Troubleshooting

- If the client fails to connect, check the IP address is entered correctly (e.g. 192.168.0.122). If no port number is provided, the default port 9999 will be used. If the server has a different port selected, it is required to enter the corresponding port number.
- Check if the Firewall is blocking the connection (this can be deactivated for a short time to check).
- When using the **Universal Render Pipeline (URP)**, check **Post Processing** is disabled in `Main Camera -> Rendering -> Post Processing`.
- If the Unity editor crashes when trying to run the app and ISAR is not required but is still in the project, follow the steps in [Disabling ISAR](#disabling-isar) to debug the issue.
- If audio doesn't work, confirm **Default Communication Audio Device** is the same as the **Default Audio Device** in the **Windows Sound Control Panel**. Both issues are under investigation.
- If using a **HDRP** project and the `Main Camera` is not moving, remove the `SimpleCameraController` script attached to the `Main Camera`.
- When opening the HelloISAR project for the first time, a missing reference to **com.hololight.isar** may be flagged. If this occurs:
  - Press **Continue** and wait until the project finishes loading
  - Once the project has loaded, open the Package Manager via `Window -> Package Manager`
  - Click on the **+** Icon and add a package from the disk
  - Select the `package.json` file inside the `com.hololight.isar` directory
-  If rainbow-colored streaks appear over the imported 3D models, the ISAR Configurations File must be updated before importing the ISAR package. See [The ISAR Configuration File](#the-isar-configuration-file) for information about the file and [Clients](./Docs/clients.md) for suggested settings.
- The Depth-Alpha-Stream feature for better stabilization currently has some known issues with swimming effects. Improvements will come in newer versions of ISAR.

## License 
By downloading/using/evaluating ISAR, you agree to the [proprietary license terms and conditions](./Licenses/ISAR.txt). Licensing information can be found in the "Licenses" directory of this repository.
