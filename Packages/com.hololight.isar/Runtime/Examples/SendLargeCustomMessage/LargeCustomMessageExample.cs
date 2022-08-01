using System;
using System.Runtime.InteropServices;
using System.Text;
using HoloLight.Isar;
using HoloLight.Isar.Native;
using UnityEngine;
using System.Linq;

public class LargeCustomMessageExample : MonoBehaviour
{
	private IsarCustomSend _isar;
	readonly string LongMessage = string.Concat(Enumerable.Repeat("Large Custom Message \n", 500000));

	void Start()
	{
		_isar = new IsarCustomSend();
		_isar.ConnectionStateChanged += OnConnectionStateChanged;
	}

	private void OnConnectionStateChanged(HlrConnectionState state)
	{

	}

	private void OnDestroy()
	{
		_isar.Dispose();
	}

	public void SendMessage()
	{
		if (_isar == null) return;

		SendToClient(LongMessage);
	}

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
