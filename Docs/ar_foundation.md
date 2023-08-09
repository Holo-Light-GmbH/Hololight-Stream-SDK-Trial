# AR Foundation

ISAR supports Unity's [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/manual/index.html). The currently supported AR Foundation features with this package are listed in the table below.

Combined with the AR Foundation support, ISAR also provides touch input for iOS Client with the new input system.

| Feature 					| HoloLens 2 | iOS | Quest 2 |
| :--- 						| :---: | :---: | :---: |
| Device tracking 			| ✓ | ✓ | ✓ |
| Plane tracking 			| ✓ | ✓ |   |
| Point clouds				|   |   |   |
| Anchors					| ✓ |   |   |
| Light estimation			|   |   |   |
| Enviroment Probes			|   |   |   |
| Face tracking				|   |   |   |
| 2D Image tracking			|   | ✓ |   |
| 3D Object tracking		|   |   |   |
| Meshing					| ✓ |   |   |
| 2D & 3D body tracking		|   |   |   |
| Collaborative participants|   |   |   |
| Human segmentation		|   |   |   |
| Raycast					|   | ✓ |   |
| Pass-through video		|   |   |   |
| Session management		|   |   |   |
| Occlusion					|   |   |   |

## Getting Started

### Prerequisites

- [Prerequisites](../README.md#prerequisites)
- AR Foundation 4.2.7

### First Installation

1. Follow the steps listed in [First Installation](README.md#first-installation)

### Scene Configuration

- In the scene, add an **AR Session Origin** object (`GameObject -> XR -> AR Session Origin`)
- Expand the **AR Session Origin** object and select the **AR Camera**
- Set the **AR Camera** background to have alpha value of `0`, within the inspector window
- In the scene, add an **AR Session** object (`GameObject -> XR -> AR Session`)

### First Run

Follow the steps listed in [First Run](README.md#first-run).

## Touch Input

Touch data is passed to Unity via the new Input System. This data can be accessed through actions which are configured with Unity Input Actions. To find out more about the new Input system, see Unity's [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/QuickStartGuide.html) manual.

### Supported Functionality

Currently, ISAR only supports a single touch with the iOS client and will always be considered the primary touch.

## Plane Tracking

The package provides the plane tracking feature to detect and visualise planes within the environment. For instructions on how to use plane tracking, see Unity's [Plane Detection](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/manual/plane-manager.html) manual.

### Supported Functionality

The plane tracking feature supports the following functionality:

| Functionality					| HoloLens 2 | iOS | Quest 2 |
| :--- 							| :---: | :---: | :---: |
| Horizontal Plane Detection	| ✓ | ✓ |   |
| Vertical Plane Detection		| ✓ | ✓ |   |
| Arbitrary Plane Detection		| ✓ |   |   |
| Boundary Vertices				| * | ✓ |   |
| Classification				| ✓ | ✓ |   |

* The HoloLens 2 Client does not support Boundary Vertices therefore, the boundary vertices correspond to the 4 vertices at the plane extents.

## 2D Image Tracking

The package provides the 2D image tracking feature to detect and track images in the environment from a supplied library. For instructions on how to use 2D image tracking, see Unity's [Image Tracking](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/manual/tracked-image-manager.html) manual.

> :warning: When running in editor play mode, the `Keep Texture at Runtime` checkbox within for the images within the `Image Library` must be checked. If not, a warning within Unity will appear due to the texture not being available.

### Supported Functionality

The 2D image tracking feature supports the following functionality:

| Functionality				| HoloLens 2 | iOS | Quest 2 |
| :--- 						| :---: | :---: | :---: |
| Moving Images				|   | ✓ |   |
| Mutable Library			| 	|   |   |
| Image Validation			|  	|   |   |

## Raycasting

The package provides the raycast feature which will carry out hit tests on real world object, such as detected planes. For instructions on how to carry out raycasts, see Unity's [Raycast](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/manual/raycast-manager.html) manual.

### Supported Functionality

The raycast feature supports the following functionality. Currently, the raycast feature only supports raycasts against planes:

| Functionality				| HoloLens 2 | iOS | Quest 2 |
| :--- 						| :---: | :---: | :---: |
| Viewpoint Raycasts		|   |   |   |
| World Based Raycasts		|   | ✓ |   |
| Tracked Raycasts			|  	|   |   |

## Meshing

The package provides the meshing subsystem to receive and render the real world mesh. This can either be used directly with the subsystem, through AR Foundation's [AR Mesh Manager](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/manual/mesh-manager.html) or MRTK's [Spatial Awareness](./spatial-understanding.md).

| Functionality				| HoloLens 2 | iOS | Quest 2 |
| :--- 						| :---: | :---: | :---: |
| World Meshes				| ✓ |   |   |

## Anchors

The package provides the anchoring subsystem to add, track and remove real world anchors. For instructions on how to use them with AR Foundation, see Unity's [Anchor Manager](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/manual/anchor-manager.html) manual. Additionally, for more information about how to use the Anchor's extension methods, see [Spatial Anchors](./spatial-anchors.md).

## Examples

Unity provides an example project to demonstrate the functionality of AR Foundation, which can also be used with ISAR. The package can be found [here](https://github.com/Unity-Technologies/arfoundation-samples) and provides a number of sample scenes for each feature. To use, load the project into Unity and follow the steps in [Getting Started](#getting-started).

### Limitations

- ISAR does not currently support all the features provided by AR Foundation, therefore not every sample scene will work correctly. If using, stick to the sample scenes which demonstrate the functionality listed above.
- Remember to set the **AR Camera** background to have alpha value of `0`. If this is not done, the background image will always be black.
- These samples make use of the legacy input system for touch which is not currently supported by ISAR. Any scene which uses touch may need to be updated to use the new input system.

## Troubleshooting

The below list contains specific issues that may occur when running the ISAR AR Subsystems package. For all other troubleshooting issues, see [Troubleshooting](../README.md#troubleshooting).

- If the background of the camera is black, make sure the `AR Session Origin -> AR Camera` background's alpha channel has been set to zero as instructed in [Scene Configuration](#scene-configuration)
- If the touch input is not triggering events, ensure that the Unity game window has window focus (actively selected). If it does not, the events will not be triggered as the game is considered not to be in focus.
