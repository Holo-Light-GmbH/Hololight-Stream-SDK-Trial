using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using HoloLight.Isar.Native;
using HoloLight.Isar.Native.CustomMessage;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace HoloLight.Isar.ARSubsystems
{
	/// <summary>
	/// ISAR implementation of the <c>XRRaycastSubsystem</c>. Do not create this directly. Use the <c>SubsystemManager</c> instead.
	/// This class is a placeholder for future development. This will never be registered or created
	/// </summary>
	[Preserve]
	public sealed class IsarXRRaycastSubsystem : XRRaycastSubsystem
	{
		#if !UNITY_2020_2_OR_NEWER
		protected override Provider CreateProvider()
		{
			return new IsarProvider();
		}
#endif

		class IsarProvider : Provider
		{
			private IsarCustomSend _isar;
			private object _messagingLock = new object();
			private HlrConnectionState _connectionState;
			private AutoResetEvent _messageReceivedEvent = new AutoResetEvent(false);
			private List<XRRaycastHit> _receivedHits;
			private TrackableType _raycastAllowedTrackType;

			public IsarProvider() 
			{
			
			}

            public override void Start() 
			{
				SetupCustomMessage();
			}
            public override void Stop() 
			{
				DisposeCustomMessage();
			}

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

			public override unsafe NativeArray<XRRaycastHit> Raycast(
				XRRaycastHit defaultRaycastHit,
				Ray ray,
				TrackableType trackableTypeMask,
				Allocator allocator)
			{
				if (_isar == null) return new NativeArray<XRRaycastHit>();
				lock (_messagingLock)
				{
					_raycastAllowedTrackType = trackableTypeMask;
					SendRaycast(ray);
				}

				if (!_messageReceivedEvent.WaitOne(200)) return new NativeArray<XRRaycastHit>();

				lock (_messagingLock)
				{
					if (_receivedHits == null) return new NativeArray<XRRaycastHit>();

					return new NativeArray<XRRaycastHit>(_receivedHits.ToArray(), allocator);
				}
			}

			public override unsafe NativeArray<XRRaycastHit> Raycast(
				XRRaycastHit defaultRaycastHit,
				Vector2 screenPoint,
				TrackableType trackableTypeMask,
				Allocator allocator)
			{
				if (_isar == null) return new NativeArray<XRRaycastHit>();
				lock (_messagingLock)
				{
					_raycastAllowedTrackType = trackableTypeMask;
					SendRaycast(screenPoint);
				}

				if (!_messageReceivedEvent.WaitOne(200)) return new NativeArray<XRRaycastHit>();

				lock (_messagingLock)
				{
					if (_receivedHits == null) return new NativeArray<XRRaycastHit>();

					return new NativeArray<XRRaycastHit>(_receivedHits.ToArray(), allocator);
				}
			}

			#region CustomSendMethods
			private void OnConnectionStateChanged(HlrConnectionState state)
			{
				lock (_messagingLock)
				{
					_connectionState = state;
				}
			}

			private void OnCustomMessageReceived(in HlrCustomMessage message)
			{
				lock (_messagingLock)
				{
					IntPtr readPtr = message.Data;
					HoloLight.Isar.Shared.Assertions.Assert(message.Length >= Marshal.SizeOf<Int32>());
					MessageType messageType = (MessageType)Marshal.ReadInt32(readPtr);
					if (messageType != MessageType.AR_RAYCAST_HIT_RESULTS) return;
					readPtr = IntPtr.Add(readPtr, Marshal.SizeOf<Int32>());

					int length = (int)message.Length;
					byte[] data = new byte[length];
					Marshal.Copy(readPtr, data, 0, length);
					int index = 0;

					// Parse message type

					try
					{
						// Parse hit count
						int hitSize = 10 * 4;
						int hitCount = BitConverter.ToInt32(data, index);
						if (length - (2 * 4) != hitSize * hitCount) return;
						index += 4;

						_receivedHits = new List<XRRaycastHit>();

						for (int i = 0; i < hitCount; i++)
						{
							int trackableId = BitConverter.ToInt32(data, index);
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

							float distance = BitConverter.ToSingle(data, index);
							index += 4;

							TrackableType type = (TrackableType) BitConverter.ToInt32(data, index);
							index += 4;

							TrackableId id = TrackableId.invalidId;
							var track = TrackablesDatabase.GetTrack(trackableId);
							if (track != null)
							{
								id = track.Track.trackableId;
							}
							
							if(IsTrackCorrectType(type, _raycastAllowedTrackType)) 
								_receivedHits.Add(new XRRaycastHit(id, new Pose(position, rotation), distance, type));
						}
					}
					finally
					{
						_messageReceivedEvent.Set();
					}
				}
			}

			private bool IsTrackCorrectType(TrackableType type, TrackableType raycastAllowedTrackType)
			{
				// All
				if(raycastAllowedTrackType.HasFlag(TrackableType.All))
					return true;

				// Plane
				if (raycastAllowedTrackType.HasFlag(TrackableType.Planes) && 
					((int)type & (int)TrackableType.Planes) >= 0x01) 
					return true;
				if(raycastAllowedTrackType.HasFlag(TrackableType.PlaneEstimated) &&
					type.HasFlag(TrackableType.PlaneEstimated))
					return true;
				if (raycastAllowedTrackType.HasFlag(TrackableType.PlaneWithinInfinity) &&
					type.HasFlag(TrackableType.PlaneWithinInfinity))
					return true;
				if (raycastAllowedTrackType.HasFlag(TrackableType.PlaneWithinBounds) &&
					type.HasFlag(TrackableType.PlaneWithinBounds))
					return true;
				if (raycastAllowedTrackType.HasFlag(TrackableType.PlaneWithinPolygon) &&
					type.HasFlag(TrackableType.PlaneWithinPolygon))
					return true;

				// Feature Points
				if (raycastAllowedTrackType.HasFlag(TrackableType.FeaturePoint) &&
					type.HasFlag(TrackableType.FeaturePoint))
					return true;

				// Image
				if (raycastAllowedTrackType.HasFlag(TrackableType.Image) &&
					type.HasFlag(TrackableType.Image))
					return true;

				// Face
				if (raycastAllowedTrackType.HasFlag(TrackableType.Face) &&
					type.HasFlag(TrackableType.Face))
					return true;

				// All checks failed
				return false;
			}

			private void SendRaycast(Vector2 screenPoint)
			{
				if (_isar == null)
				{
					Debug.Log("Can't send custom message as object is null");
					return;
				}

				List<byte> bytes = new List<byte>();
				bytes.AddRange(BitConverter.GetBytes(screenPoint.x));
				bytes.AddRange(BitConverter.GetBytes(screenPoint.y));

				_isar.Send(MessageType.AR_RAYCAST_SCREEN_POINT, bytes.ToArray());
			}

			private void SendRaycast(Ray ray)
			{
				if (_isar == null)
				{
					Debug.Log("Can't send custom message as object is null");
					return;
				}

				var hlrOrigin = Utils.Convert.FlipHandedness(ray.origin);
				var hlrDirection = Utils.Convert.FlipHandedness(ray.direction);

				List<byte> bytes = new List<byte>();
				bytes.AddRange(BitConverter.GetBytes(hlrOrigin.x));
				bytes.AddRange(BitConverter.GetBytes(hlrOrigin.y));
				bytes.AddRange(BitConverter.GetBytes(hlrOrigin.z));
				bytes.AddRange(BitConverter.GetBytes(hlrDirection.x));
				bytes.AddRange(BitConverter.GetBytes(hlrDirection.y));
				bytes.AddRange(BitConverter.GetBytes(hlrDirection.z));

				_isar.Send(MessageType.AR_RAYCAST_RAY, bytes.ToArray());
			}
			#endregion
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void RegisterDescriptor()
		{
			XRRaycastSubsystemDescriptor.RegisterDescriptor(new XRRaycastSubsystemDescriptor.Cinfo
			{
				id = "ISAR Raycast",
#if UNITY_2020_2_OR_NEWER
				providerType = typeof(IsarXRRaycastSubsystem.IsarProvider),
				subsystemTypeOverride = typeof(IsarXRRaycastSubsystem),
#else
				subsystemImplementationType = typeof(IsarXRRaycastSubsystem),
#endif
				supportsViewportBasedRaycast = false,
				supportsWorldBasedRaycast = true,
				supportsTrackedRaycasts = false,
				supportedTrackableTypes = TrackableType.Planes
			});
		}
	}
}
