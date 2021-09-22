# ISAR SDK

<p align="center">
    <img src="Docs/imgs/ISAR_Icon.png" width="180px">
</p>

## What is ISAR SDK?
ISAR is a Software Development Kit allowing developers to stream their entire application to an XR device with minimal integration. 
<br>
<em>ISAR SDK, built for Developers, by Developers.</em>
<br>
We’ve kicked our approach to streaming up a notch by making it *fully interactive*; capture any sensory data or telemetry and process it in your application in *real-time*. Allowing you to stay in control, crafting your own bespoke remote rendering AR solution. We diligently focused on the simplicity of integration and stability. Whether in managed environments, hosted clouds or on-premise solutions.

## Getting Started

> :warning: This **trial** will expire/renew **September 1st, 2021**.
> :warning: We don't recommend changing any source files delivered with ISAR. If you do so, we can't guarantee support.
> :warning: By downloading/using this trial, you agree to our  <a href="Licenses/ISAR.txt">license terms & conditions</a>

### Prerequisites

- Minimum Unity 2019.4.x (tested with 2019.4.18f1)
- Mixed Reality Toolkit 2.4.x or 2.5.x (tested with MRTK 2.5.4)

### First Installation 

0. Create a new Unity 3D project (skip this if you already have a project)

1. Import **MRTK** into your project
    - [Getting Started](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Installation.html#1-get-the-latest-mrtk-unity-packages)
2. Import **ISAR SDK** into your project
    - Make sure you are connected to the internet
    - Open **Package Manager** in the Unity editor (`Window -> Package Manager`)
    - Check if the package named 'Version Control' is installed and uninstall it by clicking *Remove* (if it's not there in the first place you're all good)
    - Choose **Add package from disk...** and select the file `package.json` from the `com.hololight.isar` directory contained in this repo
    - When implementing ISAR SDK for Oculus: Choose **Add package from disk...** and select the file `package.json` from the `com.hololight.isar` directory as well as the `package.json` from the `com.hololight.isar.oculus` directory

### Updating Previous Installation
This step only applies if you have a previous version of ISAR already installed/integrated in your project. If this is your first time installing ISAR, please skip to the next step.
- Open ****Package Manager**** in the Unity editor (`Window -> Package Manager`)
- Remove the current ISAR Package
- Add the new ISAR Package
- Restart Unity
- Follow the steps in [Project Configuration](#project-configuration)

### Project Configuration

1. Navigate to `Edit -> Project Settings -> XR Plug-in Management`
    - Enable **ISAR XR** for both platforms
2. Open `File -> Build Settings`
    - Ensure one of the following configurations is selected
        - Platform: **PC, Mac & Linux Standalone** + Architecture: **x86_64**
3. Restart Unity (to work around known issues)

### Scene Configuration

#### One-Click Scene Config

- Choose `ISAR -> Configure MRTK` to configure your scene to work with ISAR. It will add the necessary objects to your scene and configure them properly.

#### Manual Config

To configure your scene manually (e.g. to integrate with existing MRTK configurations) follow the steps below.

- Ensure your scene is configured to use **MRTK** by selecting `Mixed Reality Toolkit -> Add to Scene and Configure...` in the top menu bar of Unity.
- Change the configuration profile to **IsarXRSDKConfigurationProfile**

### Checklist

If ...

- [x] ... MixedRealityToolkit -> Input -> **Input Data Providers** contains **ISAR XRSDK Device Manager**, ...
- [x] ... MixedRealityToolkit -> Camera -> **Camera Settigs Providers** contains **XR SDK Camera Settings** ...
- [x] ... and you followed the steps in [Project Configuration](#project-configuration) correctly ...
- [x] ... then you have set up everything correctly! 👍

### First Run

Enter play mode in the Unity editor or build a standalone application with the following settings:

- Platform: **PC, Mac & Linux Standalone**
- Target Platform: **Windows**
- Architecture: **x86_64**

The ISAR server application will start listening on the TCP port 9999 of your computer. The ISAR client application can then use that port to establish the streaming session. Please ensure that no firewall is blocking the connection.

### HelloIsar Example

You can check out the preconfigured **HelloIsar** example as a reference or template for your project.

- Open the directory `HelloIsar` contained in this repo with Unity.
- When opening the project for the first time, you may get a warning about a wrong package reference. Click on "Continue" and let the project load.
- Then follow the steps described in the [Installation Guide](#installation-guide)

## Install and run the HoloLens client application

(also refer to <a href="Clients\HoloLens2_Client_2.1.0.0/README.md">ISAR HoloLens Client Documentation</a>)

- Install the ISAR_Client app package on a HoloLens 2 (e.g. via Device Portal) and start the application
- Insert the servers IP address in the client application and press "Connect".
- Once the client connected successfully, you should see the scene on the HoloLens.

## Install and run the Android client

(also refer to <a href="Clients\Android_Client_2.1.0.0/README.md">ISAR Android Client Documentation</a>)

- Make sure the ISAR configuration file `remoting-config.cfg` located in `Runtime/Resources` within the ISAR package has the correct number of views (1 for single screen mobiles) and resolution for your client device (in case you are not sure which resolution to choose 1920*1080 is a good starting point). Refer to [The ISAR Configuration File](#the-isar-configuration-file) below for further information on the config file.
- Install the ISAR Android client either from the Google Play Store or from the app package contained in this repository.
- Insert the servers IP address in the client application and press "Connect".
- Once the client connected successfully, you should see the scene on your Android device.

## Known Issues with Android

- Unity HDRP Project: Touch input not working
- Unity HDRP Project: Rendering has dark grey artefacts close to light sources

## Testing in the Editor

In case you want to test your application without connecting to ISAR you need to do the following steps:

- Uncheck **ISAR XR** and check **Mock HMD Loader** in XR Plug-In Management in the Unity project settings. Also, uncheck **Initialize XR on Startup**
- Make sure no ISAR-specific code will be executed to prevent unity from crashing. This means the active MRTK configuration profile is not allowed to contain any ISAR-specific `Data Providers`. So the easiest option is to switch to a default MRTK configuration profile, e.g. `DefaultMixedRealityToolkitProfile`.
- Also, ensure not to call the `Isar` class constructor anywhere. In case you use the `QrSupport` script, you should disable it.
**Congratulations! 🙌🙌🙌**

### The ISAR Configuration File

The configuration file for ISAR is located within the ISAR package in `Runtime/Resources`. When running the server another copy of the file is generated in the `Assets/StreamingAssets` directory of your project. Don't change the config in `StreamingAssets` because it will be overwritten at the next run.

- The settings `width` and `height` determine the resolution of the rendered image per view/eye. Careful: the resolution needs to be supported by the H264 hardware decoder of the client device.
- `numViews` determines the number of views/eyes rendered on the client device. Valid values are `1` for mono rendering, e.g. on Tablets, and `2` for stereo rendering, e.g. on HoloLenses.

## Additional Features

### Need QR Code Support?

Check out how to easily integrate QR code support <a href="Docs/qrcode.md">here</a>.

### Need Image Tracking?

That's as well an easy one! Check out <a href="Docs/imagetracking.md">here</a>.

### You want to use Oculus Quest 2 as Client?

We have done it for you! Check out <a href="Docs/oculusQuest.md">here</a>.

### Troubleshooting

- If the client fails to connect, check if you entered the correct IP address (e.g. 192.168.0.122 - WITHOUT the Port Number).
- Check if the Firewall is blocking the connection (you can deactivate it for a short time to check that)
- When using the **Universal Render Pipeline (URP)** please check that **Post Processing** is disabled in `Main Camera -> Rendering -> Post Processing` TODO: Philipp: should it be disabled or enabled?
- If the Unity editor crashes when trying to run the app from the Unity editor and ISAR not being used but still in the project, check if all steps stated in chapter [Testing in the Editor](#testing-in-the-editor) were successful.
- Audio doesn't work:
    1. Make sure your **Default Communication Audio Device** is the same as the **Default Audio Device** in the **Windows Sound Control Panel**
    We are working on both issues
- If the `Main Camera` is not moving and you are in an **HDRP** project, remove the `SimpleCameraController` script attached to the `Main Camera`.
- When opening the HelloIsar project for the first time, you might get a missing reference to **com.hololight.isar**.
  - That's fine, just press **Continue** and wait until the project finishes loading.
  - Once the project has loaded, open the Package Manager via `Window -> Package Manager`.
  - Click on the **+** Icon and add a package from the disk.
  - Select the `package.json` file inside the `com.hololight.isar` directory.
  - Now, the SDK should be set up successfully.
- If rainbow-colored streaks appear over the imported 3D models, configurations must be adjusted. For this you have to follow the path `C:\"your path to ISAR"\ISAR-SDK-Trial-main\Packages\com.hololight.isar\Runtime\Resources` and look for the file with the name `remoting-config`. Open the file with a text editor and enter **1920** for `width` and `height`. This step should be done before importing the ISAR package; the Oculus file does not need to be adjusted.
## License 
By downloading/using/evlatuating ISAR, you agree to our <a href="Licenses/ISAR.txt">proprietary license terms and conditions</a>.
Licensing information can be found in the folder "Licenses" of this repository
