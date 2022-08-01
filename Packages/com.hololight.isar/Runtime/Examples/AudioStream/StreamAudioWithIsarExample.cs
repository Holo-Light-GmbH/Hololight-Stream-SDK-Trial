using HoloLight.Isar;
using System;
using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class StreamAudioWithIsarExample : MonoBehaviour
{
	private IsarCustomAudioSource _audioSource;
	private bool _running;

	void Start()
    {
		int systemSampleRate = AudioSettings.GetConfiguration().sampleRate;
		int systemDSPBufferSize = AudioSettings.GetConfiguration().dspBufferSize;
		AudioSpeakerMode speakerMode = AudioSettings.GetConfiguration().speakerMode;
		bool isAudioSettingsSupported = IsarCustomAudioSource.IsAudioSettingsSupported(systemSampleRate, 
																					   SpeakerModeToNumChannels(speakerMode));
		if (isAudioSettingsSupported)
		{
			_audioSource = new IsarCustomAudioSource(systemSampleRate,
													 systemDSPBufferSize);
			_running = true;
		}
	}

	int SpeakerModeToNumChannels(AudioSpeakerMode mode)
	{
		switch (mode)
		{
			case AudioSpeakerMode.Mono:
				return 0;
			case AudioSpeakerMode.Stereo:
				return 2;
			case AudioSpeakerMode.Quad:
				return 4;
			case AudioSpeakerMode.Surround:
				return 5;
			case AudioSpeakerMode.Mode5point1:
				return 6;
			case AudioSpeakerMode.Mode7point1:
				return 8;
			default:
				return 0;
		}
	}

	void OnAudioFilterRead(float[] data, int channels)
	{
		if (!_running)
			return;

		_audioSource.PushAudioData(data);
	}

	private void OnDestroy()
	{
		_running = false;
		if(_audioSource != null)
			_audioSource.Dispose();
	}
}
