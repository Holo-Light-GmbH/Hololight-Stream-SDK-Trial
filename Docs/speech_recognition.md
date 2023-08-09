# Speech Recognition

ISAR supports [Speech Recognition](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk2/features/input/speech?view=mrtkunity-2022-05) for Hololens 2 and provides the required Speech and Dictation Providers for MRTK. Speech Recognition can be used to interact with a scene using a list of keywords. Dictation Recognition can be used to input a continuous sentence to an object in the scene, for example an input field.

## Getting Started

Recognition providers are enabled on ISAR MRTK by default. For getting started with MRTK, follow the steps in [MRTK Extension](mrtk_extension.md)

### Manual Scene Configuration

To enable Speech and Dictation, it is required to exchange the **Windows Speech Input** and **Windows Dictation Input** in the MRTK providers with **Isar Speech Input** and **Isar Dictation Input** respectively.

### Example

An example on how to use Speech and Dictation can be found in the [MRTK Examples Package](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk2/features/input/speech?view=mrtkunity-2022-05#example-scene). Guides on the respective examples can be followed and then migrated into ISAR by applying the steps above.

After following the [MRTK Extension Scene Configuration Step](mrtk_extension.md#scene-configuration), the MRTK configuration profile will be overwritten, changing the `SpeechCommandsProfile`. Before running the example, set the `Input -> Speech` configuration to `Speech.MixedRealitySpeechCommandsProfile`.
