using System;
using System.Runtime.InteropServices;
using System.Text;
using HoloLight.Isar;
using HoloLight.Isar.Native;
using UnityEngine;

public class VideoToggleExample : MonoBehaviour, ISerializationCallbackReceiver
{
	private IsarCustomSend _isar;

	[SerializeField]
	private bool toggle;
	private bool togglePreDeserialized;
	public bool Toggle
	{
		get => toggle;
		set
		{
			toggle = togglePreDeserialized = value;
			ToggleVideo();
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
			if (toggle) { SendToClient("enable_video"); }
		}
	}

	private void OnDestroy()
	{
		_isar.Dispose();
	}

	public void OnAfterDeserialize()
	{
		if (toggle == togglePreDeserialized) return;
		togglePreDeserialized = toggle;
		ToggleVideo();
	}

	public void OnBeforeSerialize()
	{
	}

	[ContextMenu("Enable Camera Capture")]
	public void VideoOn()
	{
		Toggle = true;
	}

	[ContextMenu("Disable Camera Capture")]
	public void VideoOff()
	{
		Toggle = false;
	}

	public void ToggleVideo() { if (_isar == null) return; if (toggle) { SendToClient("enable_video"); } else { SendToClient("disable_video"); } }

	public void SendToClient(string messageStr)
	{
		if (!_isar.IsConnected) return;
		byte[] messageBytes = Encoding.ASCII.GetBytes(messageStr);

		IntPtr unmanaged = Marshal.AllocHGlobal(messageBytes.Length);
		Marshal.Copy(messageBytes, 0, unmanaged, messageBytes.Length);

		HlrCustomMessage message = new HlrCustomMessage();
		message.Length = (uint)messageBytes.Length;
		message.Data = unmanaged;

		_isar.PushCustomMessage(message);

		Marshal.FreeHGlobal(unmanaged);
	}
}
