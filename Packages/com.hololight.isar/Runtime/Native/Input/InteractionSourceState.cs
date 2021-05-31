/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;

namespace HoloLight.Isar.Native.Input
{
	public struct HlrSpatialInteractionSourceProperties
	{
		public double SourceLossRisk;
		public HlrVector3 SourceLossMitigationDirection;
		public HlrSpatialInteractionSourcePose SourcePose;

		public HlrSpatialInteractionSourceProperties(
			double sourceLossRisk,
			HlrVector3 sourceLossMitigationDirection,
			HlrSpatialInteractionSourcePose sourcePose)
		{
			SourceLossRisk = sourceLossRisk;
			SourceLossMitigationDirection = sourceLossMitigationDirection;
			SourcePose = sourcePose;
		}
	}

	[Flags]
	public enum HlrSpatialInteractionSourceStateFlags : uint
	{
		None = 0,
		Grasped = 1 << 0,
		AnyPressed = 1 << 1,
		TouchpadPressed = 1 << 2,
		ThumbstickPressed = 1 << 3,
		SelectPressed = 1 << 4, // 0x00000010
		MenuPressed = 1 << 5, // 0x00000020
		TouchpadTouched = 1 << 6, // 0x00000040
	}

	public struct HlrSpatialInteractionSourceState
	{
		public HlrSpatialInteractionSourceProperties Properties;
		public HlrSpatialInteractionSource Source;
		public HlrHeadPose HeadPose; // TODO: THIS IS NEVER USED AND CAN BE THROWN AWAY
		//public Pose HeadPose;
		public HlrHandPose HandPose;
		//public Vector2 ThumbstickPosition;
		//public Vector2 TouchpadPosition;
		public float SelectPressedAmount;
		public HlrSpatialInteractionSourceStateFlags Flags;

		public HlrSpatialInteractionSourcePose SourcePose
		{
			get
			{
				return this.Properties.SourcePose;
			}
		}

		public HlrHandPose TryGetHandPose()
		{
			return HandPose;
		}

		public bool IsAnyPressed
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.AnyPressed);
				return (this.Flags & HlrSpatialInteractionSourceStateFlags.AnyPressed) != HlrSpatialInteractionSourceStateFlags.None;
			}
		}

		public bool IsSelectPressed
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.SelectPressed);
				return (this.Flags & HlrSpatialInteractionSourceStateFlags.SelectPressed) != HlrSpatialInteractionSourceStateFlags.None;
			}
		}

		public bool IsMenuPressed
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.MenuPressed);
				return (this.Flags & HlrSpatialInteractionSourceStateFlags.MenuPressed) != HlrSpatialInteractionSourceStateFlags.None;
			}
		}

		public bool IsGrasped
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.Grasped);
				return (this.Flags & HlrSpatialInteractionSourceStateFlags.Grasped) != HlrSpatialInteractionSourceStateFlags.None;
			}
		}

		public bool IsTouchpadTouched
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.TouchpadTouched);
				return (this.Flags & HlrSpatialInteractionSourceStateFlags.TouchpadTouched) != HlrSpatialInteractionSourceStateFlags.None;
			}
		}

		public bool IsTouchpadPressed
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.TouchpadPressed);
				return (this.Flags & HlrSpatialInteractionSourceStateFlags.TouchpadPressed) != HlrSpatialInteractionSourceStateFlags.None;
			}
		}

		public bool IsThumbstickPressed
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.ThumbstickPressed);
				return (this.Flags & HlrSpatialInteractionSourceStateFlags.ThumbstickPressed) != HlrSpatialInteractionSourceStateFlags.None;
			}
		}
	}
}
