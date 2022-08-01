using UnityEngine;

public class VirtualMicrophoneExample : MonoBehaviour
{
	private static string ISAR_VIRTUAL_MICROPHONE_IDENTIFIER = "ISAR";

	private AudioSource _audioSource;
	private bool _isIsarDeviceFound;
	private string _isarDeviceName;

	void Start()
	{
		_isIsarDeviceFound = false;
		_audioSource = GetComponent<AudioSource>();

		foreach (var device in Microphone.devices)
		{
			//Check virtual device that contains ISAR in its name
			if (device.Contains(ISAR_VIRTUAL_MICROPHONE_IDENTIFIER))
			{
				// Get exact name of device
				_isarDeviceName = device;
				_isIsarDeviceFound = true;
				break;
			}
		}

		if (!_isIsarDeviceFound)
		{
			Debug.Log("ISARVirtualMicrophone device is not found on system.");
		}

		//Start capturing initially
		StartVirtualMicrophoneCapturing();
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
			StopVirtualMicrophoneCapturing();
		else
			StartVirtualMicrophoneCapturing();
	}


	private void OnDisable()
	{
		StopVirtualMicrophoneCapturing();
	}

	private void OnEnable()
	{
		StartVirtualMicrophoneCapturing();
	}

	private void OnDestroy()
	{
		if (_audioSource.isPlaying)
			_audioSource.Stop();
	}

	void StartVirtualMicrophoneCapturing()
	{
		if (_isIsarDeviceFound)
		{ 		
			//Capture frequency of virtual microphone should be 48hz as in last parameter in this example
			_audioSource.clip = Microphone.Start(_isarDeviceName, true, 60, 48000);
			_audioSource.loop = true;
			while (!(Microphone.GetPosition(_isarDeviceName) > 0)) { }
			_audioSource.Play();
		}
	}

	void StopVirtualMicrophoneCapturing()
	{
		if (_audioSource.isPlaying)
		{
			_audioSource.Stop();

			if (_isIsarDeviceFound)
			{
				Microphone.End(_isarDeviceName);
			}
		}
	}
}
