# Haptics

Hololight Stream supports the usage of haptics through Unity Input Subsystem. For the details regarding the usage, please refer to the [Unity Reference](https://docs.unity3d.com/ScriptReference/XR.HapticCapabilities.html). [Impulse](https://docs.unity3d.com/ScriptReference/XR.InputDevice.SendHapticImpulse.html) and [Buffer](https://docs.unity3d.com/ScriptReference/XR.InputDevice.SendHapticBuffer.html) type haptics can be sent from the Input System. For additional implementations, check related classes from [XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/manual/simple-haptic-feedback.html) (for versions 3.0+) and [MRTK](https://learn.microsoft.com/en-us/dotnet/api/microsoft.mixedreality.toolkit.input.imixedrealityhapticfeedback). Haptics are only supported on Quest 2, Quest Pro, Quest 3, Magic Leap 2 and Lenovo VRX.

## Meta Haptics

Additional to the support for basic types of haptics, Stream also supports ".haptic" files created by [Meta Haptics Studio](https://developer.oculus.com/resources/haptics-overview/). Files that are exported via the Studio can be imported to Unity and sent using the extension for `InputDevice`. For an example usage, please refer to the `Meta Haptics` sample in the samples package.
