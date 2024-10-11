# MRTK 3 Extension

## Getting Started

### Prerequisites

- [Prerequisites](../README.md#prerequisites)
- Mixed Reality Toolkit Core 3.0.1
- Mixed Reality Toolkit Input 3.0.0
- Mixed Reality Toolkit UX Core Scripts 3.0.0
- Mixed Reality Toolkit Standard Assets 3.0.0

### Upgrade from MRTK 2

If a project is being upgraded from MRTK 2 to 3, first all components and packages regarding MRTK 2 and the Stream MRTK 2 extension must be removed. Then the installation step below can be followed. The components corresponding to the removed components can be then added back. For more details about upgrading an MRTK 2 project to MRTK 3, see [Migration Guide](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk3-overview/architecture/mrtk-v2-to-v3).

### First Installation

1. Follow the steps listed in [First Installation](../README.md#first-installation)
2. Import **MRTK** into the project
    - [Getting Started](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk3-overview/getting-started/setting-up/setup-new-project)
    - Make sure to select Core package version 3.0.1 or higher, as it includes fixes regarding Unity 2021.x support.
3. Import **Hololight Stream Extension for MRTK 3** into the project
    - Open **Package Manager** in the Unity editor (`Window -> Package Manager`)
    Click the **+** then **Add package from tarball...** and select the file `com.hololight.stream.mrtk3-<version>.tgz`, where version is the current version, from the `Packages` directory contained in this repository.

### Project Configuration

- Follow the steps listed in [Project Configuration](../README.md#project-configuration).
- Configure the MRTK profile (`Edit -> Project Settings -> MRTK3 -> Profile`) with the added Stream Subsystems. A sample profile can be found in the package.

### Scene Configuration

To configure a scene (e.g. to integrate with existing MRTK configurations) follow one of the steps below.

- Modify an existing Rig with the components from Stream Extension (see below)
or
- Replace an existing MRTK XR Rig with the one supplied in the Stream MRTK 3 extension package.
or
- Use the one-click scene configuration (see below)

#### 1. Modifications

- Controller Visualization Manager added to the XR Rig.
- `StreamInputActions` in `Packages/Hololight Stream Extension for MRTK 3/Runtime` added to the Input Action Manager on the XR Rig
- `Main Camera -> Camera Settings Manager -> Transparent Display -> Clear Color` Alpha changed to 0 for XR Mode.
- Model Prefab for MRTK Left/Right Hand Controllers changed to ones supplied in the package.
  - In an existing model prefab, Controller Visualizer should be changed to Controller Visualizer (Stream)
- For both hand controllers, input action references in `Far Ray -> MRTK Ray Interactor -> Aim Pose Source -> Pose Source List -> Element 0` changed to `Stream (Left/Right)Hand/(Tracking State/PointerPosition/PointerRotation)` from the package
- (Optional) Touch Controller Prefab added as a child to Camera Offset object for touchscreen input.
- (Optional) MRTK Spatial Mouse Controller Prefab added as a child to Camera Offset object for Desktop Client input.
  - On the prefab, `XR Controller -> Select Action` reference changed to `Stream Mouse/Select` from the package
  - On the child object, `MRTK Spatial Mouse Interactor -> Mouse Move & Mouse Scroll Action` references changed to `Stream Mouse/MouseMove` and `Stream Mouse/MouseScroll` from the package*
  - On the child object, `MRTK Spatial Mouse Interactor -> Mouse Sensitivity` set to 0.5
  - On the grand-child object `CursorVisual`, flip the sprite on X axis and scale the object for a more visible cursor
- (Optional) MRTK Speech GameObject enabled for speech input.

\* MRTK Spatial Mouse Controller has a different behavior than MRTK 2's spatial mouse. Stream passes mouse movements directly to the MRTK 3 through Input Actions. It is also possible to use these Input Actions for different spatial mouse controller implementations.

#### 2. One-Click Scene Config

- Choose `Hololight -> Stream -> Configure MRTK` to configure the scene for Hololight Stream. This will add necessary components for Stream to an existing MRTK XR Rig or a Main Camera and do the necessary modifications, or instantiate the Stream MRTK XR Rig for the scene to use.

#### 3. Additional Features

Some features are not enabled automatically with one-click scene config. Below are steps for enabling these features.

- Eye Tracking:
  - Add `Eye Tracking Manager` component to the XR Rig
  - Change `MRTK Gaze Controler -> XR Controller -> Position Action` to `Stream Gaze/Position` from the package
  - Change `MRTK Gaze Controler -> XR Controller -> Rotation Action` to `Stream Gaze/Rotation` from the package
  - Change `MRTK Gaze Controler -> XR Controller -> Tracking State Action` to `Stream Gaze/TrackingState` from the package
  - (Optional) For reticle visuals, add the `MRTK Ray Reticle Visual` component to the `MRTK Gaze Controler -> Gaze Interactor` GameObject and add `Reticle` prefab or any other custom prefab as a child to it, and add Reticle's reference to the `MRTK Ray Reticle Visual`

### First Run

Follow the steps listed in [First Run](../README.md#first-run).
