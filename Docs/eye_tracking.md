# Eye Tracking

Hololight Stream offers seamless integration with eye and gaze tracking devices. Currently, HoloLens 2 supports gaze interaction, while Quest Pro supports both gaze and eye tracking functionalities. To enable these features, users must grant the necessary permissions on their devices. Additionally, for optimal performance, it's essential to calibrate the eye tracking system.

## Usage with MRTK 2

To enable gaze interaction with MRTK 2, start by configuring Stream for MRTK 2 using the provided [setup instructions](mrtk2_extension.md). Make sure to also follow the steps for additional features. Once the Eye Gaze Data Provider is configured and tracking is enabled, basic gaze interaction functionalities within MRTK can be utilized. For detailed usage guidelines, please refer to the official Mixed Reality Toolkit [documentation](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk2/features/input/eye-tracking/eye-tracking-main?view=mrtkunity-2022-05).

## Usage with MRTK 3

For MRTK 3 integration, refer to the additional feature [setup instructions](mrtk3_extension.md) for the MRTK XR Rig for Stream. If a custom rig is required, create a Gaze Controller using the input actions provided in the Stream MRTK 3 extension package. Additionally, ensure the Eye Tracking Manager, responsible for starting and stopping tracking, is added to the rig or to the scene. Detailed instructions for using the Gaze Controller and Interactor with MRTK 3 can be found in their respective [documentation](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk3-input/packages/input/eye-tracking).

## Usage with Unity Input System

Eye and gaze tracking functionalities are also compatible with Unity's Input System. An illustrative example of such integration is available in the `Eye Tracking` sample included in the Stream's samples package.
