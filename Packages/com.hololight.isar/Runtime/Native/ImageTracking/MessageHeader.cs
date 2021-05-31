using System;
using UnityEngine;

namespace HoloLight.Isar.Native.ImageTracking
{
	class MessageHeader
	{
		int messageId;
		int totalSize;
		int position;

		public MessageHeader(int messageId, int totalSize, int position)
		{
			// Debug.Log("messageId: " + messageId + " totalSize: " + totalSize + " position: " + position);
			this.messageId = messageId;
			this.totalSize = totalSize;
			this.position = position;
		}

		public byte[] getBytes()
		{
			byte[] bytes = new byte[12];
			int offset = 0;
			Array.Copy(BitConverter.GetBytes(messageId), 0, bytes, offset, 4);
			offset += 4;

			Array.Copy(BitConverter.GetBytes(totalSize), 0, bytes, offset, 4);
			offset += 4;

			Array.Copy(BitConverter.GetBytes(position), 0, bytes, offset, 4);
			return bytes;
		}
	}
}