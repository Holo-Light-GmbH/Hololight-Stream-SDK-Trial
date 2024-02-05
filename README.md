# Hololight Stream SDK - Trial Version

<p align="center">
    <img src="Docs/images/STREAM icon@4x.png">
</p>

🎉 *Due to Hololight's rebranding in September 2023, ISAR SDK is now called Hololight Stream SDK. For more information about Hololight's rebranding, please* [click here](https://hololight.com/the-evolution-of-hololight-rebranding/).

> :warning: Note that for the 2024.0.0. Hololight Stream release, the Hololight Stream rebranding is going to affect the SDK code base. If you have already existing projects with an earlier version of Hololight Stream (previously ISAR SDK), you might need to change some code-related implementation.
 
Hololight Stream is a Software Development Kit that allows developers to stream their applications remotely to an XR device, with minimal integration overhead, from managed environments, hosted clouds or on-premise solutions.

## Getting Started

> :warning: It is recommended not to change any source files delivered with Hololight Stream. Support cannot be guaranteed if changes are made.

### Prerequisites

- Minimum Unity 2021.3.26

### First Installation

1. Open an existing Unity 3D project or create a new one
2. Remove previous versions of Hololight Stream from the project (skip this if the project does not currently have Hololight Stream installed)
    - Open **Package Manager** in the Unity editor (`Window -> Package Manager`)
    - Find the **Hololight Stream** package and choose **Remove**
3. Import **Hololight Stream** into the project
    - Open **Package Manager** in the Unity editor (`Window -> Package Manager`)
    - If installed, remove the **Version Control** package from the project
    - Click the **+** then **Add package from tarball...** and select the file `com.hololight.isar-<version>.tgz`, where version is the current version, from the `Packages` directory contained in this repository
4. Add the desired toolset following the instructions below
    - [MRTK](/Docs/mrtk_extension.md); supports HoloLens 2, Oculus Quest 2 and iOS clients
    - [XR Interaction Toolkit](/Docs/xr_interaction_toolkit.md); supports Oculus Quest 2 client
5. Hololight Stream SDK now offers support for the H.265 video codec. To take advantage of the enhanced video rendering capabilities offered by H.265, users may need to install additional software.
    - The extension can be downloaded from the Microsoft Store [here](https://apps.microsoft.com/store/detail/hevc-video-extensions/9NMZLZ57R3T7?hl=en-us&gl=us) for a one-time charge of € 0,99 cents.
    
### Project Configuration

1. Navigate to `Edit -> Project Settings -> XR Plug-in Management`
    - Enable **ISAR XR**
2. Open `File -> Build Settings...`
    - Ensure the following configuration is selected:
        - Platform: **PC, Mac & Linux Standalone** + Architecture: **x86_64**

### Hololight Stream Configuration

Before connecting to the server, Hololight Stream must be configured for the client device which will be used.

#### Rendering Settings

To setup the rendering configuration:

- Navigate to `ISAR -> Configure Rendering Settings`
- Select the client device configuration in the `Rendering Presets` dropdown box
- Click `Apply`

Please note that, default rendering presets provided by Hololight Stream are not modifiable.

In addition to the rendering presets provided, it is possible to add and use self-defined presets. To do so:

- Navigate to `ISAR -> Configure Rendering Settings`
- Click `Add New Preset`
- Set the `Preset Name`
- Configure the settings for the specific device:
    - `width` and `height` determine the resolution of the rendered image per view/eye. Careful, the resolution needs to be supported by the H264/H265 hardware decoder of the client device
    - `numViews` determines the number of views/eyes rendered on the client device. Valid values are:
        - `1` for mono rendering, e.g. Tablets
        - `2` for stereo rendering, e.g. HoloLens
    - `bandwidth` allows the developer to specify the bitrate for the encoder. If no value is set, a default value of 20Mbps is used
    - `framerate` set the rendering frame rate on Unity
- Click `Add`
- Click `Apply`

#### Signaling Port

To set the port to be used during signaling:

- Navigate to `ISAR -> Set Signaling Port`
- Input the described port number and click `Apply`

The port number must be between 1024 and 65535. If unset, the default port is 9999.

#### Configuration File

After building the project, the configuration can be edited, with any text editor, through the `remoting-config.cfg` file in the `StreamingAssets` folder. The configuration file is in standard JSON format, more information can be found on [JSON.org](https://www.json.org/json-en.html).

### First Run

Enter play mode in the Unity editor or build a standalone application with the following settings:

- Platform: **PC, Mac & Linux Standalone**
- Target Platform: **Windows**
- Architecture: **x86_64**

The Hololight Stream server application will start listening on the TCP port 9999. The Hololight Stream client application is using this port to establish the streaming session. If the connection fails, ensure that no firewall is blocking it.

### Disabling Hololight Stream

The Hololight Stream SDK can be disabled while remaining as a package in the project. To do so, follow the below steps:

- Click `Edit->Project Settings` and move to the **XR Plug-in Management** section
- Uncheck **ISAR XR**
- Check **Unity Mock HMD**
- Ensure not to call the `Isar` class, or any of the inherited classes (`IsarViewPose`, `IsarAudio`, etc.), constructor within the code

## Clients

Hololight Stream SDK supports several clients that can connect to the server. For information on installing and connecting with specific clients, refer to [Clients](/Docs/clients.md).

## Example

A preconfigured example is available within the repository. For information on how to load this example, refer to [Example](/Docs/example.md).

Furthermore, the `ISAR Examples` package contains example scenes, prefabs, scripts and associated resources to demonstrate Hololight Stream features within Unity.

## Additional Features

### QR Code

To find out how to easily integrate QR code support, see [here](/Docs/qr_code.md).

## Troubleshooting

- If the client fails to connect, check the IP address is entered correctly (e.g. 192.168.0.122).
- Check if the Firewall is blocking the connection (this can be deactivated for a short time to check).
- When using the **Universal Render Pipeline (URP)**, check **Post Processing** is disabled in `Main Camera -> Rendering -> Post Processing`.
- If the Unity editor crashes when trying to run the app and Hololight Stream is not required but is still in the project, follow the steps in [Disabling Hololight Stream](#disabling-hololight-stream) to debug the issue.
- If audio doesn't work, confirm **Default Communication Audio Device** is the same as the **Default Audio Device** in the **Windows Sound Control Panel**. Both issues are under investigation.
- If using a **HDRP** project and the `Main Camera` is not moving, remove the `SimpleCameraController` script attached to the `Main Camera`.
- When opening the HelloISAR project for the first time, a missing reference to **com.hololight.isar** may be flagged. If this occurs:
  - Press **Continue** and wait until the project finishes loading
  - Once the project has loaded, open the Package Manager via `Window -> Package Manager`
  - Click on the **+** Icon and add a package from the disk
  - Select the `package.json` file inside the `com.hololight.isar` directory
-  If rainbow-colored streaks appear over the imported 3D models, the Hololight Stream Configurations File must be updated before importing the ISAR package. See [The ISAR Configuration File](#the-isar-configuration-file) for information about the file and [Clients](/Docs/clients.md) for suggested settings.

## License
By downloading/using/evaluating Hololight Stream, you agree to the [proprietary license terms and conditions](/Licenses/ISAR.txt). Licensing information can be found in the "Licenses" directory of this repository.

## Usage Outside Unity
The following documentation specifically references usage with the Unity Game Engine, however the Hololight Stream SDK can be used independently of Unity. For more support regarding usage outside of Unity, please [contact us](https://holo-light.com/contact).
