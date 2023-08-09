# Unity Audio Stream

## Overview

ISAR allows Unity audio to be streamed to a connected client device. For more information about using Audio in Unity, see [here](https://docs.unity3d.com/Manual/class-AudioSource.html).

## Getting Started

To ease integration, an example scene has been provided to illustrate how to use ISAR to stream Unity audio to the client device. The example implements a single audio source with basic functionality. Multiple sources with more complex functionality can also be added for transmitting audio.

The following steps explain how to enable this example:

1. Add the `ISAR Examples` package to the Unity Project.
2. Add an `Audio Listener` to the main camera. If one already exists, move to the next step.
3. Add the component `StreamAudioWithIsarExample.cs` to the main camera. This can be found at `ISAR Examples/AudioStream/`.
4. Add the `AudioStreamExample` prefab to the scene. This can be found at `ISAR Examples/AudioStream/`.
5. Expand the `AudioStreamExample` game object and open the `StaticAudioSource1` within the Inspector.
6. Set the desired `Audio Clip`.
