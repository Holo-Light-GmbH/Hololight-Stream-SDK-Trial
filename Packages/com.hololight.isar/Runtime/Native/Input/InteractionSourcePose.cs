/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;

namespace HoloLight.Isar.Native.Input
{
	public enum HlrSpatialInteractionSourcePositionAccuracy : int
	{
		//None,
		High = 0,
		Approximate = 1,
	}

	[Flags]
	public enum HlrSpatialInteractionSourcePoseFlags : uint
	{
		None = 0,
		HasGripPosition = 1,
		HasGripOrientation = 2,
		HasPointerPosition = 4,
		HasPointerOrientation = 8,
		HasVelocity = 16, // 0x00000010
		HasAngularVelocity = 32, // 0x00000020
	}

	public struct HlrSpatialInteractionSourcePose
	{
		internal HlrQuaternion GripOrientation;
		internal HlrQuaternion PointerOrientation;
		internal HlrVector3 GripPosition;
		internal HlrVector3 PointerPosition;
		internal HlrVector3 Velocity;
		internal HlrVector3 AngularVelocity;
		public HlrSpatialInteractionSourcePositionAccuracy PositionAccuracy;
		public HlrSpatialInteractionSourcePoseFlags Flags;

		public bool TryGetPosition(out HlrVector3 position)
		{
			return TryGetGripPosition(out position);
		}

		public bool TryGetGripPosition(out HlrVector3 position)
		{
			position = GripPosition;
			return Flags.HasFlag(HlrSpatialInteractionSourcePoseFlags.HasGripPosition);
			// TODO: could Convert.ToBoolean(Flags & SpatialInteractionSourcePoseFlags.HasGripPosition); to prevent branching
			// Although, branch prediction could render this irrelevant, we would need to benchmark
			//return (Flags & InteractionSourcePoseFlags.HasGripPosition) != InteractionSourcePoseFlags.None;
		}

		public bool TryGetPointerPosition(out HlrVector3 position)
		{
			position = PointerPosition;
			return Flags.HasFlag(HlrSpatialInteractionSourcePoseFlags.HasPointerPosition);
			//return (Flags & InteractionSourcePoseFlags.HasPointerPosition) != InteractionSourcePoseFlags.None;
		}

		public bool TryGetOrientation(out HlrQuaternion orientation)
		{
			return TryGetGripOrientation(out orientation);
		}

		public bool TryGetGripOrientation(out HlrQuaternion orientation)
		{
			orientation = GripOrientation;
			return Flags.HasFlag(HlrSpatialInteractionSourcePoseFlags.HasGripOrientation);
			//return (Flags & InteractionSourcePoseFlags.HasGripOrientation) != InteractionSourcePoseFlags.None;
		}

		public bool TryGetPointerOrientation(out HlrQuaternion orientation)
		{
			orientation = PointerOrientation;
			return Flags.HasFlag(HlrSpatialInteractionSourcePoseFlags.HasPointerOrientation);
			//return (Flags & InteractionSourcePoseFlags.HasPointerOrientation) != InteractionSourcePoseFlags.None;
		}

		public bool TryGetVelocity(out HlrVector3 velocity)
		{
			velocity = Velocity;
			return (Flags & HlrSpatialInteractionSourcePoseFlags.HasVelocity) != HlrSpatialInteractionSourcePoseFlags.None;
		}

		public bool TryGetAngularVelocity(out HlrVector3 angularVelocity)
		{
			angularVelocity = AngularVelocity;
			return Flags.HasFlag(HlrSpatialInteractionSourcePoseFlags.HasAngularVelocity);
			//return (Flags & InteractionSourcePoseFlags.HasAngularVelocity) != InteractionSourcePoseFlags.None;
		}
	}
}
