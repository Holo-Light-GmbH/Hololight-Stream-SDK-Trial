/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;

namespace HoloLight.Isar.Native.Input
{
	public struct InteractionSourceProperties
	{
		public double SourceLossRisk;
		public Vector3 SourceLossMitigationDirection;
		public InteractionSourcePose SourcePose;

		public InteractionSourceProperties(
			double sourceLossRisk,
			Vector3 sourceLossMitigationDirection,
			InteractionSourcePose sourcePose)
		{
			SourceLossRisk = sourceLossRisk;
			SourceLossMitigationDirection = sourceLossMitigationDirection;
			SourcePose = sourcePose;
		}
	}

	[Flags]
	public enum InteractionSourceStateFlags : uint
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

	public struct InteractionSourceState
	{
		public InteractionSourceProperties Properties;
		public InteractionSource Source;
		public HeadPose HeadPose; // TODO: THIS IS NEVER USED AND CAN BE THROWN AWAY
		//public Pose HeadPose;
		public HandPose HandPose;
		//public Vector2 ThumbstickPosition;
		//public Vector2 TouchpadPosition;
		public float SelectPressedAmount;
		public InteractionSourceStateFlags Flags;

		public InteractionSourcePose SourcePose
		{
			get
			{
				return this.Properties.SourcePose;
			}
		}

		public HandPose TryGetHandPose()
		{
			return HandPose;
		}

		public bool IsAnyPressed
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.AnyPressed);
				return (this.Flags & InteractionSourceStateFlags.AnyPressed) != InteractionSourceStateFlags.None;
			}
		}

		public bool IsSelectPressed
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.SelectPressed);
				return (this.Flags & InteractionSourceStateFlags.SelectPressed) != InteractionSourceStateFlags.None;
			}
		}

		public bool IsMenuPressed
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.MenuPressed);
				return (this.Flags & InteractionSourceStateFlags.MenuPressed) != InteractionSourceStateFlags.None;
			}
		}

		public bool IsGrasped
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.Grasped);
				return (this.Flags & InteractionSourceStateFlags.Grasped) != InteractionSourceStateFlags.None;
			}
		}

		public bool IsTouchpadTouched
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.TouchpadTouched);
				return (this.Flags & InteractionSourceStateFlags.TouchpadTouched) != InteractionSourceStateFlags.None;
			}
		}

		public bool IsTouchpadPressed
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.TouchpadPressed);
				return (this.Flags & InteractionSourceStateFlags.TouchpadPressed) != InteractionSourceStateFlags.None;
			}
		}

		public bool IsThumbstickPressed
		{
			get
			{
				//return Flags.HasFlag(InteractionSourceStateFlags.ThumbstickPressed);
				return (this.Flags & InteractionSourceStateFlags.ThumbstickPressed) != InteractionSourceStateFlags.None;
			}
		}
	}
}
