using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
using HoloLight.Isar.Native;
using Unity.Collections;
using System.Collections.Generic;
using HoloLight.Isar.Native.CustomMessage;

namespace HoloLight.Isar.ARSubsystems
{
	/// <summary>
	/// ISAR implementation of the <c>XRSessionSubsystem</c>. Do not create this directly. Use the <c>SubsystemManager</c> instead.
	/// </summary>
	[Preserve]
	public sealed class IsarXRImageTrackingSubsystem : XRImageTrackingSubsystem
	{
#if !UNITY_2020_2_OR_NEWER
		private IsarProvider _provider;

		protected override Provider CreateProvider()
		{
			_provider = new IsarProvider();

			return _provider;
		}

		protected override void OnStart()
		{
			base.OnStart();

			_provider.SetupCustomMessage();
		}
#endif

		class IsarProvider : Provider
		{
			private IsarCustomSend _isar;
			private bool _sendImages = false;
			private object _lockObj = new object();
			private IsarImageDatabase _database;
			private HlrConnectionState _connectionState = HlrConnectionState.Disconnected;
			
			public IsarProvider()
			{

			}

#if UNITY_2020_2_OR_NEWER
            public override void Start() 
			{
				SetupCustomMessage();
			}
            public override void Stop() { }
#endif

			internal void SetupCustomMessage()
			{
				_isar = new IsarCustomSend();
				_isar.CustomMessageReceived += OnCustomMessageReceived;
				_isar.ConnectionStateChanged += OnConnectionStateChanged;
			}

			internal void DisposeCustomMessage()
			{
				if (_isar != null)
				{
					_isar.CustomMessageReceived -= OnCustomMessageReceived;
					_isar.ConnectionStateChanged -= OnConnectionStateChanged;
					_isar.Dispose();
				}
			}

			public override RuntimeReferenceImageLibrary imageLibrary
			{
				set
				{
					lock (_lockObj)
					{
						if (value is IsarImageDatabase database)
						{
							_database = database;
							_sendImages = true;
						}
					}
				}
			}

			public override RuntimeReferenceImageLibrary CreateRuntimeLibrary(
				XRReferenceImageLibrary serializedLibrary)
			{
				return new IsarImageDatabase(serializedLibrary);
			}

			public unsafe override TrackableChanges<XRTrackedImage> GetChanges(
				XRTrackedImage defaultTrackedImage,
				Allocator allocator)
			{
				lock (_lockObj)
				{
					// If there are images to send, send them now
					if (_connectionState == HlrConnectionState.Connected && _sendImages)
					{
						foreach (var trackedImage in _database.TrackedImages)
						{
							SendImage(trackedImage.Key, trackedImage.Value);
						}

						_sendImages = false;
					}

					List<XRTrackedImage> addedList = new List<XRTrackedImage>();
					List<XRTrackedImage> updatedList = new List<XRTrackedImage>();
					List<TrackableId> removedList = new List<TrackableId>();
					var trackedPlanes = TrackablesDatabase.GetTrackableOfTypes<IsarTrackedImage>();
					foreach (var plane in trackedPlanes)
					{
						if (plane.HasChanges)
						{
							plane.HasChanges = false;
							switch (plane.UpdateType)
							{
								case UpdateType.Added:
									addedList.Add((XRTrackedImage)plane.Track);
									break;
								case UpdateType.Updated:
									updatedList.Add((XRTrackedImage)plane.Track);
									break;
								case UpdateType.Removed:
									removedList.Add(plane.Track.trackableId);
									break;
							}
						}
					}

					var changes = new TrackableChanges<XRTrackedImage>(addedList.Count, updatedList.Count, removedList.Count,
						allocator, defaultTrackedImage);
					changes.added.CopyFrom(addedList.ToArray());
					changes.updated.CopyFrom(updatedList.ToArray());
					changes.removed.CopyFrom(removedList.ToArray());

					removedList.ForEach((isarId) => TrackablesDatabase.Remove(isarId));

					return changes;
				}
			}

			private void RemoveAllTracks()
			{
				var trackedImages = TrackablesDatabase.GetTrackableOfTypes<IsarTrackedImage>();
				foreach (var image in trackedImages)
				{
					if (image.UpdateType > UpdateType.None && image.UpdateType < UpdateType.Removed)
					{
						image.Remove();
					}
				}
			}

			public override void Destroy()
			{
				DisposeCustomMessage();
			}

#region CustomMessageMethods
			private void OnConnectionStateChanged(HlrConnectionState state)
			{
				lock (_lockObj)
				{
					_connectionState = state;
					if (_connectionState == HlrConnectionState.Connected) _sendImages = true;
					else RemoveAllTracks();
				}
			}

			private void OnCustomMessageReceived(in HlrCustomMessage message)
			{
				const int expectedLength = 4 * 11;
				IntPtr readPtr = message.Data;
				HoloLight.Isar.Shared.Assertions.Assert(message.Length >= Marshal.SizeOf<Int32>());
				MessageType messageType = (MessageType)Marshal.ReadInt32(readPtr);
				readPtr = IntPtr.Add(readPtr, sizeof(Int32));

				UpdateType updateType;
				switch (messageType)
				{
					case MessageType.IMAGE_ADDED:
						updateType = UpdateType.Added;
						break;
					case MessageType.IMAGE_UPDATED:
						updateType = UpdateType.Updated;
						break;
					case MessageType.IMAGE_REMOVED:
						updateType = UpdateType.Removed;
						break;
					default:
						return;
				}
				
				if (message.Length < expectedLength) return;

				byte[] data = new byte[expectedLength];
				Marshal.Copy(readPtr, data, 0, expectedLength);
				int index = 0;

				// Parse image id
				int imageId = BitConverter.ToInt32(data, index);
				index += 4;

				// Parse position
				float px = BitConverter.ToSingle(data, index);
				index += 4;
				float py = BitConverter.ToSingle(data, index);
				index += 4;
				float pz = BitConverter.ToSingle(data, index);
				index += 4;
				Vector3 position = new Vector3(px, py, pz);
				Utils.Convert.FlipHandedness(ref position);

				// Parse rotation
				float rx = BitConverter.ToSingle(data, index);
				index += 4;
				float ry = BitConverter.ToSingle(data, index);
				index += 4;
				float rz = BitConverter.ToSingle(data, index);
				index += 4;
				float rw = BitConverter.ToSingle(data, index);
				index += 4;
				Quaternion rotation = new Quaternion(rx, ry, rz, rw);
				Utils.Convert.FlipHandedness(ref rotation);

				// Parse width
				float width = BitConverter.ToSingle(data, index);
				index += 4;

				// Parse height
				float height = BitConverter.ToSingle(data, index);

				UpdateImage(imageId, updateType, new Pose(position, rotation), width, height);
			}

			private void UpdateImage(int hashCode, UpdateType updateType, Pose pose, float width, float height)
			{
				var track = TrackablesDatabase.GetTrack<IsarTrackedImage>(hashCode);

				if(updateType == UpdateType.Added)
				{
					var referenceImage = _database.TrackedImages[hashCode];
					if (referenceImage == null) return;
					
					track = new IsarTrackedImage(referenceImage.guid, hashCode);
					TrackablesDatabase.AddTrack(track);
				}

				track.UpdateTrackedImageData(updateType, pose, width, height);
			}

			private void SendImage(int isarId, XRReferenceImage image)
			{
				if (_isar == null)
				{
					Debug.Log("Can't send custom message as object is null");
					return;
				}

				byte[] info = GetImageBytes(image);

				List<byte> bytes = new List<byte>();
				bytes.AddRange(BitConverter.GetBytes(isarId));
				bytes.AddRange(BitConverter.GetBytes((int)ImageType.MOVING));
				bytes.AddRange(BitConverter.GetBytes(image.width));
				bytes.AddRange(info);

				_isar.Send(MessageType.IMAGE_TO_TRACK, bytes.ToArray());
			}

			public byte[] GetImageBytes(XRReferenceImage referenceImage)
			{
				if (referenceImage.texture == null) return null;

				return DeCompress(referenceImage.texture).EncodeToJPG();
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

			#endregion

			public override int requestedMaxNumberOfMovingImages
			{
				get => m_RequestedMaxNumberOfMovingImages;
				set => m_RequestedMaxNumberOfMovingImages = value;
			}
			int m_RequestedMaxNumberOfMovingImages;

			public override int currentMaxNumberOfMovingImages => requestedMaxNumberOfMovingImages;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void RegisterDescriptor()
		{
			XRImageTrackingSubsystemDescriptor.Create(new XRImageTrackingSubsystemDescriptor.Cinfo
			{
				id = "ISAR Image Tracking",
#if UNITY_2020_2_OR_NEWER
                providerType = typeof(IsarXRImageTrackingSubsystem.IsarProvider),
                subsystemTypeOverride = typeof(IsarXRImageTrackingSubsystem),
#else
				subsystemImplementationType = typeof(IsarXRImageTrackingSubsystem),
#endif
				supportsMovingImages = true,
				supportsMutableLibrary = false,
				supportsImageValidation = false,
			});
		}
	}
}