# AR Foundation

Hololight Stream supports Unity's [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/index.html). The currently supported AR Foundation features with this package are listed in the table below.

Combined with the AR Foundation support, Hololight Stream also provides [touch input](#touch-input) for iOS Client with the new input system.

| Feature                    | HoloLens 2 | iOS/iPadOS | Quest 2 / Pro / 3 | Magic Leap 2 | Lenovo VRX | Desktop |
| :---                       | :---: | :---: | :---: | :---: | :---: | :---: |
| Collaborative participants |       |       |       |       |       |       |
| Camera                     |   ✓   |   ✓   |       |       |       |       |
| Device tracking            |   ✓   |   ✓   |   ✓   |   ✓   |   ✓   |   ✓   |
| Enviroment Probes          |       |       |       |       |       |       |
| Face tracking              |       |       |       |       |       |       |
| Human segmentation         |       |       |       |       |       |       |
| Light estimation           |       |       |       |       |       |       |
| Meshing                    |   ✓   |   ✓   |       |       |       |       |
| Occlusion                  |       |   *   |       |       |       |       |
| Plane tracking             |   ✓   |   ✓   |       |       |       |       |
| Point clouds               |       |       |       |       |       |       |
| QR Code Tracking           |   ✓   |       |       |   ✓   |       |       |
| Raycast                    |       |   ✓   |       |       |       |       |
| Session management         |       |       |       |       |       |       |
| Spatial Anchors            |   ✓   |   ✓   |   ✓   |       |       |       |
| 2D Image tracking          |       |   ✓   |       |       |       |       |
| 2D & 3D body tracking      |       |       |       |       |       |       |
| 3D Object tracking         |       |       |       |       |       |       |

\* Supported outside of AR Foundation, see [Occlusion Support](./occlusion.md)

## Getting Started

### Prerequisites

- [Prerequisites](../README.md#prerequisites)
- AR Foundation 5.0.5

### First Installation

1. Follow the steps listed in [First Installation](../README.md#first-installation)

### Scene Configuration

- In the scene, add an **AR Session Origin** object (`GameObject -> XR -> AR Session Origin`)
- Expand the **AR Session Origin** object and select the **AR Camera**
- Set the **AR Camera** background to have alpha value of `0`, within the inspector window
- In the scene, add an **AR Session** object (`GameObject -> XR -> AR Session`)

### First Run

Follow the steps listed in [First Run](../README.md#first-run).

## Features

### Camera

AR Foundation enables users to use device camera through AR Camera Manager. For the usages of camera within AR Foundation, see [Unity Documentation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/features/Camera/camera.html). For additional usages of camera through Stream, see [Camera Stream](./camera_stream.md).

### Meshing

The package provides the meshing subsystem to receive and render the real world mesh. This can either be used directly with the subsystem, through AR Foundation's [AR Mesh Manager](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/features/meshing.html) or MRTK's [Spatial Awareness](./spatial_understanding.md).

| Functionality             | HoloLens 2 | iOS | Quest 2 / Pro / 3 | Magic Leap 2 |
| :---                      | :---: | :---: | :---: | :---: |
| World Meshes              |   ✓   |   ✓   |       |       |

### Plane Tracking

The package provides the plane tracking feature to detect and visualise planes within the environment. For instructions on how to use plane tracking, see Unity's [Plane Detection](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/features/plane-detection.html) manual. To use it as a part of MRTK's Scene Understanding feature, see [Spatial Awareness](./spatial_understanding.md).

#### Supported Functionality

The plane tracking feature supports the following functionality:

| Functionality                 | HoloLens 2 | iOS | Quest 2 / Pro / 3 | Magic Leap 2 |
| :---                          | :---: | :---: | :---: | :---: |
| Arbitrary Plane Detection     |   ✓   |       |       |       |
| Boundary Vertices             |   *   |   ✓   |       |       |
| Classification                |   ✓   |   ✓   |       |       |
| Horizontal Plane Detection    |   ✓   |   ✓   |       |       |
| Vertical Plane Detection      |   ✓   |   ✓   |       |       |

\* The HoloLens 2 Client does not support Boundary Vertices therefore, the boundary vertices correspond to the 4 vertices at the plane extents.

### QR Code Tracking

Stream adds the QR Code Tracking to the features of AR Foundation. For more detailed information, see [QR Code Support](./qr_code.md).

### Raycasting

The package provides the raycast feature which will carry out hit tests on real world object, such as detected planes. For instructions on how to carry out raycasts, see Unity's [Raycast](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/features/raycasts.html) manual.

#### Supported Functionality

The raycast feature supports the following functionality. Currently, the raycast feature only supports raycasts against planes:

| Functionality             | HoloLens 2 | iOS | Quest 2 / Pro / 3 | Magic Leap 2 |
| :---                      | :---: | :---: | :---: | :---: |
| Tracked Raycasts          |       |       |       |       |
| Viewpoint Raycasts        |       |       |       |       |
| World Based Raycasts      |       |   ✓   |       |       |

### Spatial Anchors

The package provides the anchoring subsystem to add, track and remove real world anchors. For instructions on how to use them with AR Foundation, see Unity's [Anchor Manager](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/features/anchors.html) manual. Additionally, for more information about how to use the Anchor's extension methods, see [Spatial Anchors](./spatial_anchors.md).

### Touch Input

Touch data is passed to Unity via the new Input System. This data can be accessed through actions which are configured with Unity Input Actions. To find out more about the new Input system, see Unity's [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/QuickStartGuide.html) manual.

#### Supported Functionality

Currently, Hololight Stream only supports single touch with the iOS client which will always be considered the primary touch.

### 2D Image Tracking

The package provides the 2D image tracking feature to detect and track images in the environment from a supplied library. For instructions on how to use 2D image tracking, see Unity's [Image Tracking](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/features/image-tracking.html) manual.

> :warning: When running in editor play mode, the `Keep Texture at Runtime` checkbox within for the images within the `Image Library` must be checked. If not, a warning within Unity will appear due to the texture not being available.

#### Supported Functionality

The 2D image tracking feature supports the following functionality:

| Functionality             | HoloLens 2 | iOS | Quest 2 / Pro / 3 | Magic Leap 2 |
| :---                      | :---: | :---: | :---: | :---: |
| Image Validation          |       |       |       |       |
| Moving Images             |       |   ✓   |       |       |
| Mutable Library           |       |       |       |       |

## Examples

Unity provides an example project to demonstrate the functionality of AR Foundation, which can also be used with Hololight Stream. The package can be found [here](https://github.com/Unity-Technologies/arfoundation-samples) and provides a number of sample scenes for each feature. To use, load the project into Unity and follow the steps in [Getting Started](#getting-started).

### Limitations

- Some AR Foundation features are currently not supported, therefore not every sample scene will work correctly. If using, stick to the sample scenes which demonstrate the functionality listed above.
- Remember to set the **AR Camera** background to have alpha value of `0`. If this is not done, the background image will always be black.
- These samples make use of the legacy input system for touch which is not currently supported. Any scene which uses touch may need to be updated to use the new input system.

## Troubleshooting

The below list contains specific issues that may occur when running the Hololight Stream. For all other troubleshooting issues, see [Troubleshooting](../README.md#troubleshooting).

- If the background of the camera is black, make sure the `AR Session Origin -> AR Camera` background's alpha channel has been set to zero as instructed in [Scene Configuration](#scene-configuration)
- If the touch input is not triggering events, ensure that the Unity game window has window focus (actively selected). If it does not, the events will not be triggered as the game is considered not to be in focus.
