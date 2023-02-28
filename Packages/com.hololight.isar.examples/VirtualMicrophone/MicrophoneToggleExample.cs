using System;
using System.Runtime.InteropServices;
using HoloLight.Isar;
using HoloLight.Isar.Native;
using UnityEngine;

public class MicrophoneToggleExample : MonoBehaviour, ISerializationCallbackReceiver
{
	private IsarCustomSend _isar;

	[SerializeField]
	private bool enableMicrophoneCapture;
	private bool togglePreDeserialized;

	private const int ENABLE_MICROPHONE_MESSAGE_IDENTIFIER = 16;
	private const int DISABLE_MICROPHONE_MESSAGE_IDENTIFIER = 17;

	public bool Toggle
	{
		get => enableMicrophoneCapture;
		set
		{
			enableMicrophoneCapture = togglePreDeserialized = value;
			ToggleAudio();
		}
	}

	void Start()
	{
		_isar = new IsarCustomSend();
		_isar.ConnectionStateChanged += OnConnectionStateChanged;
	}

	private void OnConnectionStateChanged(HlrConnectionState state)
	{
		if (state == HlrConnectionState.Connected)
		{
			if (enableMicrophoneCapture) { SendToClient(ENABLE_MICROPHONE_MESSAGE_IDENTIFIER); }
		}
	}

	private void OnDestroy()
	{
		_isar.Dispose();
	}

	public void OnAfterDeserialize()
	{
		if (enableMicrophoneCapture == togglePreDeserialized) return;
		togglePreDeserialized = enableMicrophoneCapture;
		ToggleAudio();
	}

	public void OnBeforeSerialize()
	{
	}

	[ContextMenu("Enable Microphone Capture")]
	public void AudioCaptureOn()
	{
		Toggle = true;
	}

	[ContextMenu("Disable Microphone Capture")]
	public void AudioCaptureOff()
	{
		Toggle = false;
	}

	public void ToggleAudio() 
	{ 
		if (_isar == null) 
			return; 

		if (enableMicrophoneCapture) 
		{ 
			SendToClient(ENABLE_MICROPHONE_MESSAGE_IDENTIFIER); 
		} 
		else 
		{ 
			SendToClient(DISABLE_MICROPHONE_MESSAGE_IDENTIFIER); 
		} 
	}

	public void SendToClient(int msgId)
	{
		if (!_isar.IsConnected) return;
		byte[] messageBytes = BitConverter.GetBytes(msgId);

		IntPtr unmanaged = Marshal.AllocHGlobal(messageBytes.Length);
		Marshal.Copy(messageBytes, 0, unmanaged, messageBytes.Length);

		HlrCustomMessage message = new HlrCustomMessage();
		message.Length = (uint)messageBytes.Length;
		message.Data = unmanaged;

		_isar.PushCustomMessage(message);

		Marshal.FreeHGlobal(unmanaged);
	}
}
