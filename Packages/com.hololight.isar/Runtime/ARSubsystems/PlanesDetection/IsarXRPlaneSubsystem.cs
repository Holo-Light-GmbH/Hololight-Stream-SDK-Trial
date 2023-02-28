using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HoloLight.Isar.Native;
using HoloLight.Isar.Native.CustomMessage;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
	public sealed class IsarXRPlaneSubsystem : XRPlaneSubsystem
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
			private object _lockObj = new object();
			private HlrConnectionState _connectionState;
			private PlaneDetectionMode planeDetectionMode = PlaneDetectionMode.None;
			private bool _sendConfig = false;

			public override PlaneDetectionMode requestedPlaneDetectionMode
			{
				get => planeDetectionMode;
				set
				{
					planeDetectionMode = value;
					_sendConfig = true;
				}
			}

			public override PlaneDetectionMode currentPlaneDetectionMode => planeDetectionMode;

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


			public override void GetBoundary(TrackableId trackableId, Allocator allocator, ref NativeArray<Vector2> boundary)
			{
				lock (_lockObj)
				{
					var track = TrackablesDatabase.GetTrack<IsarTrackedPlane>(trackableId);

					if(track != null)
					{
						CreateOrResizeNativeArrayIfNecessary(track.Polygon.Length, allocator, ref boundary);

						boundary.CopyFrom(track.Polygon);
					}
				}
			}

			public unsafe override TrackableChanges<BoundedPlane> GetChanges(BoundedPlane defaultPlane, Allocator allocator)
			{
				lock (_lockObj)
				{
					if (_connectionState != HlrConnectionState.Connected)
						return new TrackableChanges<BoundedPlane>();

					if (_connectionState == HlrConnectionState.Connected && _sendConfig)
					{
						bool sendPlanes = planeDetectionMode != PlaneDetectionMode.None;
						SendPlaneConfig(sendPlanes, planeDetectionMode);
						_sendConfig = false;

						if(planeDetectionMode == PlaneDetectionMode.None)
						{
							RemoveAllTracks();
						}
					}

					List<BoundedPlane> addedList = new List<BoundedPlane>();
					List<BoundedPlane> updatedList = new List<BoundedPlane>();
					List<TrackableId> removedList = new List<TrackableId>();
					var trackedPlanes = TrackablesDatabase.GetTrackableOfTypes<IsarTrackedPlane>();
					foreach (var plane in trackedPlanes)
					{
						if (plane.HasChanges)
						{
							plane.HasChanges = false;
							switch (plane.UpdateType)
							{
								case UpdateType.Added:
									addedList.Add((BoundedPlane) plane.Track);
									break;
								case UpdateType.Updated:
									updatedList.Add((BoundedPlane) plane.Track);
									break;
								case UpdateType.Removed:
									removedList.Add(plane.Track.trackableId);
									break;
							}
						}
					}

					var changes = new TrackableChanges<BoundedPlane>(addedList.Count, updatedList.Count, removedList.Count,
						allocator, defaultPlane);
					changes.added.CopyFrom(addedList.ToArray());
					changes.updated.CopyFrom(updatedList.ToArray());
					changes.removed.CopyFrom(removedList.ToArray());

					removedList.ForEach((isarId) => TrackablesDatabase.Remove(isarId));

					return changes;
				}
			}

			private void RemoveAllTracks()
			{
				var trackedPlanes = TrackablesDatabase.GetTrackableOfTypes<IsarTrackedPlane>();
				foreach (var plane in trackedPlanes)
				{
					if (plane.UpdateType > UpdateType.None && plane.UpdateType < UpdateType.Removed)
					{
						plane.Remove();
					}
				}
			}

			public override void Destroy()
			{
			}

			#region CustomSendMethods
			private void OnConnectionStateChanged(HlrConnectionState state)
			{
				lock (_lockObj)
				{
					_connectionState = state;
					if (_connectionState == HlrConnectionState.Connected) _sendConfig = true;
					else RemoveAllTracks();
				}
			}

			private void OnCustomMessageReceived(in HlrCustomMessage message)
			{
				lock (_lockObj)
				{
					if (planeDetectionMode == PlaneDetectionMode.None) return;

					IntPtr readPtr = message.Data;
					HoloLight.Isar.Shared.Assertions.Assert(message.Length >= Marshal.SizeOf<Int32>());
					MessageType messageType = (MessageType)Marshal.ReadInt32(readPtr);
					if (messageType != MessageType.AR_PLANE_DETECTION_RESULTS) return;
					readPtr = IntPtr.Add(readPtr, Marshal.SizeOf<Int32>());

					int length = (int)message.Length;
					byte[] data = new byte[length];
					Marshal.Copy(readPtr, data, 0, length);
					int index = 0;

					// Parse hit count
					int planeSize = 17 * 4;
					int planeCount = BitConverter.ToInt32(data, index);
					if (length - (2 * 4) <= planeSize * planeCount) return;
					index += 4;

					for (int i = 0; i < planeCount; i++)
					{
						int hashCode = BitConverter.ToInt32(data, index);
						index += 4;

						int stateRaw = BitConverter.ToInt32(data, index);
						TrackingState state = (TrackingState)stateRaw;
						index += 4;
						
						bool isSubsumed = BitConverter.ToInt32(data, index) == 1;
						index += 4;

						int subsumbedBy = BitConverter.ToInt32(data, index);
						index += 4;

						Vector3 position;
						position.x = BitConverter.ToSingle(data, index);
						index += 4;
						position.y = BitConverter.ToSingle(data, index);
						index += 4;
						position.z = BitConverter.ToSingle(data, index);
						index += 4;
						Utils.Convert.FlipHandedness(ref position);

						Quaternion rotation;
						rotation.w = BitConverter.ToSingle(data, index);
						index += 4;
						rotation.x = BitConverter.ToSingle(data, index);
						index += 4;
						rotation.y = BitConverter.ToSingle(data, index);    
						index += 4;
						rotation.z = BitConverter.ToSingle(data, index);		
						index += 4;
						Utils.Convert.FlipHandedness(ref rotation);

						Vector2 center;
						center.x = BitConverter.ToSingle(data, index);
						index += 4;
						center.y = BitConverter.ToSingle(data, index);
						index += 4;

						Vector2 size;
						size.x = BitConverter.ToSingle(data, index);
						index += 4;
						size.y = BitConverter.ToSingle(data, index);
						index += 4;

						int alignmentRaw = BitConverter.ToInt32(data, index);
						PlaneAlignment alignment = (PlaneAlignment)alignmentRaw;
						index += 4;

						int polygonSize = BitConverter.ToInt32(data, index) / 2;
						Vector2[] polygon = new Vector2[polygonSize];
						index += 4;

						// Boundary vertices are in right-handed coordinates and clockwise winding order. To convert
						// to left-handed, we flip the Z coordinate, but that also flips the winding, so we have to
						// flip the winding back to clockwise by reversing the polygon index.
						for (int j = polygonSize-1; j >= 0; j--)
						{
							Vector2 point;
							point.x = BitConverter.ToSingle(data, index);
							index += 4;
							point.y = BitConverter.ToSingle(data, index);
							index += 4;

							polygon[j] = Utils.Convert.FlipHandedness(new Vector2(point.x, point.y));
						}

						UpdatePlane(hashCode, isSubsumed, subsumbedBy, new Pose(position, rotation), 
							center, size, alignment, state, polygon);
					}
				}
			}

			private void UpdatePlane(int hashCode, bool isSubsumed, int subsumbedBy, Pose pose, Vector2 center, 
				Vector2 size, PlaneAlignment alignment, TrackingState state, Vector2[] polygon, PlaneClassification classification = PlaneClassification.None)
			{
				UpdateType updateType;
				var track = TrackablesDatabase.GetTrack<IsarTrackedPlane>(hashCode);

				if (track != null)
				{
					updateType = state == TrackingState.Limited ? UpdateType.Removed : UpdateType.Updated;
				}
				else
				{
					if (state != TrackingState.Tracking) return;

					track = new IsarTrackedPlane(hashCode);
					TrackablesDatabase.AddTrack(track);
					updateType = UpdateType.Added;
				}

				var subsumbedByTrack = TrackablesDatabase.GetTrack<IsarTrackedPlane>(subsumbedBy);
				TrackableId subsumbedById = isSubsumed && subsumbedByTrack != null ?
					subsumbedByTrack.Track.trackableId : TrackableId.invalidId;

				track.UpdateTrackedPlane(updateType, subsumbedById, pose, center, size, alignment, state, polygon, classification);
			}

			private void SendPlaneConfig(bool sendPlanes, PlaneDetectionMode planeDetectionMode)
			{
				if (_isar == null)
				{
					Debug.Log("Can't send custom message as object is null");
					return;
				}

				List<byte> bytes = new List<byte>();
				bytes.AddRange(BitConverter.GetBytes(sendPlanes));

				// Check for flags as direct cast is not possible due to flag nature. 
				// C# takes the highest combination of flags as a max int, i.e. Horizontal | Vertical = -1 (0xFFFFFFFF)
				int planeDetectionModeRaw = (int)planeDetectionMode;
				if (planeDetectionMode.HasFlag(PlaneDetectionMode.Horizontal) && planeDetectionMode.HasFlag(PlaneDetectionMode.Vertical))
					planeDetectionModeRaw = 3;

				bytes.AddRange(BitConverter.GetBytes(planeDetectionModeRaw));
					
				_isar.Send(MessageType.AR_PLANE_DETECTION_CONFIG, bytes.ToArray());
			}
			#endregion
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void RegisterDescriptor()
		{
			XRPlaneSubsystemDescriptor.Create(new XRPlaneSubsystemDescriptor.Cinfo
			{
				id = "ISAR Plane",
#if UNITY_2020_2_OR_NEWER
				providerType = typeof(IsarXRPlaneSubsystem.IsarProvider),
				subsystemTypeOverride = typeof(IsarXRPlaneSubsystem),
#else
				subsystemImplementationType = typeof(IsarXRPlaneSubsystem),
#endif
				supportsHorizontalPlaneDetection = true,
				supportsVerticalPlaneDetection = true,
				supportsArbitraryPlaneDetection = false,
				supportsBoundaryVertices = true
			});
		}
	}
}
