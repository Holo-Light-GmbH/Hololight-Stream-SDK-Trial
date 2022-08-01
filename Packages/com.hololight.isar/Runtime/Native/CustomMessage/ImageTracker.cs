using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


namespace HoloLight.Isar.Native.CustomMessage
{
	/// <summary>
	/// This class serves both as an augmentation definition for an ImageTarget in the editor
	/// as well as a tracked image target result at runtime
	/// </summary>
	public class ImageTracker : MonoBehaviour
	{
		[Header("Image Target Settings")]
		[Tooltip("The Image your App will search for")]
		[SerializeField]
		private Sprite _imageTarget;

		[Tooltip("This object will follow the Image Target")]
		public GameObject objectToFollow;

		[Tooltip("If you give a positive number then it'll be used to improve tracking. It should be the physical width of the image in meters.")]
		[SerializeField]
		private float _imageWidth = 0;

		[Tooltip("If your physical image can be moved around, then set it to moving. If your image is fixed on a place, set it to static.")]
		[SerializeField]
		private ImageType _imageType = ImageType.MOVING;

		[Tooltip("Set it to true, if you want your Object To Follow to be scaled down/up to the physical image size.")]
		[SerializeField]
		private bool _adjustScale = false;


		private IsarCustomSend _isar;
		private bool _newPosAvailable = false;
		private bool _sendImageNow = false;

		private Image _image;
		public TrackedImage TrackedImage;


		private void Start()
		{
			if (_imageTarget == null)
			{
				Debug.LogError("Please reference a image to ImageTarget. The Texture Type should be 'Sprite (2D and UI). Image Tracker not started");
				return;
			}

			if (objectToFollow == null)
			{
				objectToFollow = GameObject.CreatePrimitive(PrimitiveType.Cube);
				objectToFollow.transform.parent = transform;
				objectToFollow.transform.localScale = new Vector3(0.02f, 0.01f, 0.02f);
				objectToFollow.transform.position = Vector3.zero;
			}

			if (objectToFollow == this.gameObject)
			{
				Debug.Log("You referenced the image tracker " + gameObject.name + " to Object To Follow. We recommend not to do that. If you disable this GameObject, Image Tracking will be stopped.");
			}
			else
			{
				objectToFollow.SetActive(false);
			}


			int imageId = Math.Abs(GetInstanceID());
			_image = new Image(_imageTarget, imageId, _imageType, _imageWidth);

			TrackedImage = new TrackedImage(objectToFollow, imageId);

			Debug.Log("ImageTracking starting for " + gameObject.name + "(" + TrackedImage.id + ")");

			_isar = new IsarCustomSend();
			_isar.CustomMessageReceived += OnCustomMessageReceived;
			_isar.ConnectionStateChanged += OnConnectionStateChanged;
		}

		private void OnConnectionStateChanged(HlrConnectionState state)
		{
			if (state == HlrConnectionState.Connected) _sendImageNow = true;
		}

		private void SendImage()
		{
			if(_isar == null)
			{
				Debug.Log("Can't send custom message as object is null");
				return;
			}

			byte[] info = DeCompress(_image.imageTexture.texture).EncodeToJPG();

			List<byte> bytes = new List<byte>();
			bytes.AddRange(BitConverter.GetBytes(_image.serverId));
			bytes.AddRange(BitConverter.GetBytes((int)_image.type));
			bytes.AddRange(BitConverter.GetBytes(_image.width));
			bytes.AddRange(info);

			_isar.Send(MessageType.IMAGE_TO_TRACK, bytes.ToArray());
		}

		private void Update()
		{
			if (_newPosAvailable)
			{
				_newPosAvailable = false;
				TrackedImage.UpdatePose();
				if (_adjustScale)
				{
					TrackedImage.UpdateScale();
				}
			}

			if (_sendImageNow)
			{
				_sendImageNow = false;
				SendImage();
			}
		}

		private void OnCustomMessageReceived(in HlrCustomMessage message)
		{
			int length = (int)message.Length;
			byte[] managedData = new byte[length];

			Marshal.Copy(message.Data, managedData, 0, length);

			Debug.Assert(BitConverter.IsLittleEndian);

			int index = 0;

			// parse message type
			int messageType = BitConverter.ToInt32(managedData, index);
			index += 4;

			MessageType messageTypeEnum = (MessageType)messageType;

			if (messageTypeEnum != MessageType.IMAGE_UPDATED)
				return;

			// parse image id
			int imageId = BitConverter.ToInt32(managedData, index); // can be used to identify which image this is

			if (imageId != _image.serverId)
			{
				return;
			}

			TrackedImage.id = imageId;

			index += 4;

			if (length != 4 * 11) return;
			// parse position
			for (int i = 0; i < 3; ++i)
			{
				TrackedImage.position[i] = BitConverter.ToSingle(managedData, index);
				index += 4;
			}
			// parse rotation
			for (int i = 0; i < 4; ++i)
			{
				TrackedImage.rotation[i] = BitConverter.ToSingle(managedData, index);
				index += 4;
			}
			// parse width
			TrackedImage.width = BitConverter.ToSingle(managedData, index);
			index += 4;

			// parse height
			TrackedImage.height = BitConverter.ToSingle(managedData, index);
			index += 4;

			// Coord adjustment
			TrackedImage.position.z *= -1;

			TrackedImage.rotation.x *= -1;
			TrackedImage.rotation.y *= -1;

			_newPosAvailable = true;
		}

		private Texture2D DeCompress(Texture2D source)
		{
			RenderTexture renderTex = RenderTexture.GetTemporary(
						source.width,
						source.height,
						0,
						RenderTextureFormat.Default,
						RenderTextureReadWrite.Linear);

			Graphics.Blit(source, renderTex);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = renderTex;
			Texture2D readableText = new Texture2D(source.width, source.height);
			readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
			readableText.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(renderTex);
			return readableText;
		}

		private void OnDestroy()
		{
			_isar.Dispose();
		}
	}
}