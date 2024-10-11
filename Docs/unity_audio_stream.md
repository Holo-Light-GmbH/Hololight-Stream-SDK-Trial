# Unity Audio Stream

## Overview

Hololight Stream allows Unity audio to be streamed to a connected client device. For more information about using Audio in Unity, see [here](https://docs.unity3d.com/Manual/class-AudioSource.html).

## Getting Started

To ease integration, an example scene has been provided to illustrate how to use Hololight Stream to stream Unity audio to the client device. The example implements a single audio source with basic functionality. Multiple sources with more complex functionality can also be added for transmitting audio.

> :warning: Please note that currently audio stream system only allows usage of mono or stereo channels, 48KHZ and dsp buffer size of 256, 512 or 1024.

The following steps explain how to enable this example:

1. Add the `Hololight Stream Examples` package to the Unity Project.
2. Import the `Audio Stream` sample into the project.
3. Add an `Audio Listener` to the main camera. If one already exists, move to the next step.
4. Add the `AudioStreamer.cs` component to the main camera.
5. Add the `AudioStreamExample` prefab to the scene.
6. Expand the `AudioStreamExample` game object and open the `StaticAudioSource1` within the Inspector.
7. Set the desired `Audio Clip`.
