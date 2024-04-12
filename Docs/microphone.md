# Microphone Stream
Hololight Stream supports the remote capture of the client device's microphone. An example usage can be found in: `com.hololight.stream/Samples/Microphone/MicrophoneCaptureExample.cs`. Add the MicrophoneCaptureExample prefab to the Scene or add the mentioned script to an existing object in the Scene. 

## Usage
To create and use the microphone stream, create an IsarMicrophoneCapture object and make sure to have an AudioSource attached to your gameObject.
Add the AudioClip provided by calling the Start() method of IsarMicrophoneCapture and assign the AudioClip to the AudioSource.
Additionally set the AudioSource to be looping and call the Play() method.
To turn the Microphone off just call the Stop() function of IsarMicrophoneCapture. The example handles this behaviour directly when turning the object on or off.

**NOTE**
Don't forget to Dispose the IsarMicrophoneCapture object in OnDestroy().
