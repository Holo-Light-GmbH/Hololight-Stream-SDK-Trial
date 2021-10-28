# MRTK Extension

## Getting Started
### Prerequisites

- [Prerequisites](../README.md#prerequisites)
- Mixed Reality Toolkit 2.4.x - 2.7.x (tested with MRTK 2.5.4)

### First Installation

1. Follow the steps listed in [First Installation](../README.md#first-installation)
2. Import **MRTK** into the project
    - [Getting Started](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Installation.html#1-get-the-latest-mrtk-unity-packages)
3. Import **ISAR MRTK Extensions** into the project
    - Open **Package Manager** in the Unity editor (`Window -> Package Manager`)
    - Click the **+** then **Add package from disk...** and select the file `package.json` from the `com.hololight.isar.mrtk` directory contained in this repository

### Project Configuration

Follow the steps listed in [Project Configuration](../README.md#project-configuration).

### Scene Configuration

#### One-Click Scene Config

- Choose `ISAR -> Configure MRTK` to configure the scene for ISAR. This will add and configure the necessary objects to the scene.

#### Manual Config

To configure the scene manually (e.g. to integrate with existing MRTK configurations) follow the steps below.

- Configured the scene to use **MRTK** by selecting `Mixed Reality Toolkit -> Add to Scene and Configure...` in the top menu bar of Unity.
- Select **MixedRealityToolkit** from the scene and in the **Inspector** panel, change the configuration profile to **IsarXRSDKConfigurationProfile**

#### Checklist
After configuring the scene, confirm:
- [x] ... MixedRealityToolkit -> Input -> **Input Data Providers** contains **ISAR XRSDK Device Manager**
- [x] ... MixedRealityToolkit -> Input -> **Input Data Providers** contains **ISAR XRSDK Touch Device Manager**
- [x] ... MixedRealityToolkit -> Camera -> **Camera Settings Providers** contains **XR SDK Camera Settings** 

### First Run

Follow the steps listed in [First Run](../README.md#first-run).

## Troubleshooting
The below list contains specific issues that may occur when running with the ISAR MRTK Extension package installed. For all other troubleshooting issues, see [Troubleshooting](../README.md#troubleshooting).

- If several missing assembly reference errors appear, open `ISAR Core MRTK Extensions/Runtime/Hololight.Isar.Runtime.MRTK` with Unity and confirm **Override References** is checked. If not, check it.