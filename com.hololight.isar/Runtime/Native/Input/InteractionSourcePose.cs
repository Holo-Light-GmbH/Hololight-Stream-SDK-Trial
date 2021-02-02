/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;

namespace HoloLight.Isar.Native.Input
{
	public enum InteractionSourcePositionAccuracy : int
	{
		//None,
		High = 0,
		Approximate = 1,
	}

	[Flags]
	public enum InteractionSourcePoseFlags : uint
	{
		None = 0,
		HasGripPosition = 1,
		HasGripOrientation = 2,
		HasPointerPosition = 4,
		HasPointerOrientation = 8,
		HasVelocity = 16, // 0x00000010
		HasAngularVelocity = 32, // 0x00000020
	}

	public struct InteractionSourcePose
	{
		internal Quaternion GripOrientation;
		internal Quaternion PointerOrientation;
		internal Vector3 GripPosition;
		internal Vector3 PointerPosition;
		internal Vector3 Velocity;
		internal Vector3 AngularVelocity;
		public InteractionSourcePositionAccuracy PositionAccuracy;
		public InteractionSourcePoseFlags Flags;

		public bool TryGetPosition(out Vector3 position)
		{
			return TryGetGripPosition(out position);
		}

		public bool TryGetGripPosition(out Vector3 position)
		{
			position = GripPosition;
			return Flags.HasFlag(InteractionSourcePoseFlags.HasGripPosition);
			// TODO: could Convert.ToBoolean(Flags & SpatialInteractionSourcePoseFlags.HasGripPosition); to prevent branching
			// Although, branch prediction could render this irrelevant, we would need to benchmark
			//return (Flags & InteractionSourcePoseFlags.HasGripPosition) != InteractionSourcePoseFlags.None;
		}

		public bool TryGetPointerPosition(out Vector3 position)
		{
			position = PointerPosition;
			return Flags.HasFlag(InteractionSourcePoseFlags.HasPointerPosition);
			//return (Flags & InteractionSourcePoseFlags.HasPointerPosition) != InteractionSourcePoseFlags.None;
		}

		public bool TryGetOrientation(out Quaternion orientation)
		{
			return TryGetGripOrientation(out orientation);
		}

		public bool TryGetGripOrientation(out Quaternion orientation)
		{
			orientation = GripOrientation;
			return Flags.HasFlag(InteractionSourcePoseFlags.HasGripOrientation);
			//return (Flags & InteractionSourcePoseFlags.HasGripOrientation) != InteractionSourcePoseFlags.None;
		}

		public bool TryGetPointerOrientation(out Quaternion orientation)
		{
			orientation = PointerOrientation;
			return Flags.HasFlag(InteractionSourcePoseFlags.HasPointerOrientation);
			//return (Flags & InteractionSourcePoseFlags.HasPointerOrientation) != InteractionSourcePoseFlags.None;
		}

		public bool TryGetVelocity(out Vector3 velocity)
		{
			velocity = Velocity;
			return (Flags & InteractionSourcePoseFlags.HasVelocity) != InteractionSourcePoseFlags.None;
		}

		public bool TryGetAngularVelocity(out Vector3 angularVelocity)
		{
			angularVelocity = AngularVelocity;
			return Flags.HasFlag(InteractionSourcePoseFlags.HasAngularVelocity);
			//return (Flags & InteractionSourcePoseFlags.HasAngularVelocity) != InteractionSourcePoseFlags.None;
		}
	}
}
