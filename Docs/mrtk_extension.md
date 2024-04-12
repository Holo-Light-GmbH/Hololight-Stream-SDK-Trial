# MRTK Extension

## Getting Started
### Prerequisites

- [Prerequisites](README.md#prerequisites)
- Mixed Reality Toolkit 2.7.x - 2.8.x

### First Installation

1. Follow the steps listed in [First Installation](README.md#first-installation)
2. Import **MRTK** into the project
    - [Getting Started](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Installation.html#1-get-the-latest-mrtk-unity-packages)
3. Import **ISAR MRTK Extensions** into the project
    - Open **Package Manager** in the Unity editor (`Window -> Package Manager`)
    Click the **+** then **Add package from tarball...** and select the file `com.hololight.isar.mrtk-<version>.tgz`, where version is the current version, from the `Packages` directory contained in this repository

### Project Configuration

Follow the steps listed in [Project Configuration](README.md#project-configuration).

### Scene Configuration

#### One-Click Scene Config

- Choose `Hololight -> Stream -> Configure MRTK` to configure the scene for Hololight Stream. This will add and configure the necessary objects to the scene.

#### Manual Config

To configure the scene manually (e.g. to integrate with existing MRTK configurations) follow the steps below.

- Configured the scene to use **MRTK** by selecting `Mixed Reality Toolkit -> Add to Scene and Configure...` in the top menu bar of Unity.
- Select **MixedRealityToolkit** from the scene and in the **Inspector** panel, change the configuration profile to **HololightStreamConfigurationProfile**

#### Checklist
After configuring the scene, confirm:
- [x] ... MixedRealityToolkit -> Camera -> **Camera Settings Providers** contains **XR SDK Camera Settings**
- [x] ... MixedRealityToolkit -> Input -> **Input Data Providers** contains **Hand Joint Server**
- [x] ... MixedRealityToolkit -> Input -> **Input Data Providers** contains **Hololight Stream Device Manager**
- [x] ... MixedRealityToolkit -> Input -> **Input Data Providers** contains **Hololight Stream Touch Device Manager**
- [x] ... MixedRealityToolkit -> Input -> **Input Data Providers** contains **Hololight Stream Dictation Input**
- [x] ... MixedRealityToolkit -> Input -> **Input Data Providers** contains **Hololight Stream Speech Input**

### First Run

Follow the steps listed in [First Run](README.md#first-run).

## Troubleshooting
The below list contains specific issues that may occur when running with the Hololight Stream MRTK Extension package installed. For all other troubleshooting issues, see [Troubleshooting](README.md#troubleshooting).

- If several missing assembly reference errors appear, open `Hololight Stream Extension for MRTK/Runtime/Hololight.Stream.Runtime.MRTK` with Unity and confirm **Override References** is checked. If not, check it.
