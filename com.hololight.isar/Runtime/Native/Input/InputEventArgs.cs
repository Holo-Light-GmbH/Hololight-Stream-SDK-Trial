/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;

namespace HoloLight.Isar.Native.Input
{
	#region InteractionManager

	public struct InteractionSourceDetectedEventArgs
	{
		public InteractionSourceState InteractionSourceState;

		public InteractionSourceDetectedEventArgs(InteractionSourceState interactionSourceState)
		{
			InteractionSourceState = interactionSourceState;
		}
	}

	public struct InteractionSourceLostEventArgs
	{
		public InteractionSourceState InteractionSourceState;

		public InteractionSourceLostEventArgs(InteractionSourceState interactionSourceState)
		{
			InteractionSourceState = interactionSourceState;
		}
	}

	public struct InteractionSourcePressedEventArgs
	{
		public InteractionSourceState InteractionSourceState;
		// TODO?: WSA has this additional PressType...
		// public InteractionSourcePressType PressType;

		public InteractionSourcePressedEventArgs(InteractionSourceState interactionSourceState)
		{
			InteractionSourceState = interactionSourceState;
		}
	}

	public struct InteractionSourceUpdatedEventArgs
	{
		public InteractionSourceState InteractionSourceState;

		public InteractionSourceUpdatedEventArgs(InteractionSourceState interactionSourceState)
		{
			InteractionSourceState = interactionSourceState;
		}
	}

	public struct InteractionSourceReleasedEventArgs
	{
		public InteractionSourceState InteractionSourceState;

		public InteractionSourceReleasedEventArgs(InteractionSourceState interactionSourceState)
		{
			InteractionSourceState = interactionSourceState;
		}
	}

	#endregion InteractionManager

	#region GestureRecognizer

	public /*readonly*/ struct TappedEventArgs
	{
		public readonly InteractionSource Source;
		public /*readonly*/ InteractionSourcePose SourcePose;
		public /*readonly*/ HeadPose HeadPose;
		public readonly int TapCount; // can only be 1 or 2, so it can be packed as 1 bit

		public TappedEventArgs(
			InteractionSource source,
			InteractionSourcePose sourcePose,
			HeadPose headPose,
			int tapCount)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
			TapCount = tapCount;
		}
	}

	public /*readonly*/ struct HoldStartedEventArgs
	{
		public readonly InteractionSource Source;
		public /*readonly*/ InteractionSourcePose SourcePose;
		public /*readonly*/ HeadPose HeadPose;

		public HoldStartedEventArgs(
			InteractionSource source,
			InteractionSourcePose sourcePose,
			HeadPose headPose)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
		}
	}

	public /*readonly*/ struct HoldCompletedEventArgs
	{
		public readonly InteractionSource Source;
		public /*readonly*/ InteractionSourcePose SourcePose;
		public /*readonly*/ HeadPose HeadPose;

		public HoldCompletedEventArgs(
			InteractionSource source,
			InteractionSourcePose sourcePose,
			HeadPose headPose)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
		}
	}

	public /*readonly*/ struct HoldCanceledEventArgs
	{
		public readonly InteractionSource Source;
		public /*readonly*/ InteractionSourcePose SourcePose;
		public /*readonly*/ HeadPose HeadPose;

		public HoldCanceledEventArgs(
			InteractionSource source,
			InteractionSourcePose sourcePose,
			HeadPose headPose)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
		}
	}

	public /*readonly*/ struct ManipulationStartedEventArgs
	{
		public readonly InteractionSource Source;
		public /*readonly*/ InteractionSourcePose SourcePose;
		public /*readonly*/ HeadPose HeadPose;

		public ManipulationStartedEventArgs(
			InteractionSource source,
			InteractionSourcePose sourcePose,
			HeadPose headPose)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
		}
	}

	public /*readonly*/ struct ManipulationUpdatedEventArgs
	{
		public readonly InteractionSource Source;
		public /*readonly*/ InteractionSourcePose SourcePose;
		public /*readonly*/ HeadPose HeadPose;
		public /*readonly*/ ManipulationDelta Delta;

		public ManipulationUpdatedEventArgs(
			InteractionSource source,
			InteractionSourcePose sourcePose,
			HeadPose headPose,
			ManipulationDelta delta)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
			Delta = delta;
		}
	}

	public /*readonly*/ struct ManipulationCompletedEventArgs
	{
		public readonly InteractionSource Source;
		public /*readonly*/ InteractionSourcePose SourcePose;
		public /*readonly*/ HeadPose HeadPose;
		public /*readonly*/ ManipulationDelta Delta;

		public ManipulationCompletedEventArgs(
			InteractionSource source,
			InteractionSourcePose sourcePose,
			HeadPose headPose,
			ManipulationDelta delta)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
			Delta = delta;
		}
	}

	public /*readonly*/ struct ManipulationCanceledEventArgs
	{
		public readonly InteractionSource Source;
		public /*readonly*/ InteractionSourcePose SourcePose;
		public /*readonly*/ HeadPose HeadPose;

		public ManipulationCanceledEventArgs(
			InteractionSource source,
			InteractionSourcePose sourcePose,
			HeadPose headPose)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
		}
	}

	[Flags]
	public enum NavigationFlags : uint
	{
		None = 0,
		IsOnRails = 1,
		IsNavX = 2,
		IsNavY = 4,
		IsNavZ = 8,
	}

	public /*readonly*/ struct NavigationStartedEventArgs
	{
		public readonly NavigationFlags Flags;

		public readonly InteractionSource Source;
		public /*readonly*/ InteractionSourcePose SourcePose;
		public /*readonly*/ HeadPose HeadPose;

		public NavigationStartedEventArgs(
			InteractionSource source,
			InteractionSourcePose sourcePose,
			HeadPose headPose,
			NavigationFlags flags)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
			Flags = flags;
		}
	}

	public /*readonly*/ struct NavigationUpdatedEventArgs
	{
		public readonly NavigationFlags Flags;
		public readonly InteractionSource Source;
		public /*readonly*/ InteractionSourcePose SourcePose;
		public /*readonly*/ HeadPose HeadPose;
		public /*readonly*/ Vector3 NormalizedOffset;

		public NavigationUpdatedEventArgs(
			InteractionSource source,
			InteractionSourcePose sourcePose,
			HeadPose headPose,
			NavigationFlags flags,
			Vector3 normalizedOffset)
		{
			Flags = flags;
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
			NormalizedOffset = normalizedOffset;
		}
	}

	public /*readonly*/ struct NavigationCompletedEventArgs
	{
		public readonly NavigationFlags Flags;
		public readonly InteractionSource Source;
		public /*readonly*/ InteractionSourcePose SourcePose;
		public /*readonly*/ HeadPose HeadPose;
		public /*readonly*/ Vector3 NormalizedOffset;

		public NavigationCompletedEventArgs(
			InteractionSource source,
			InteractionSourcePose sourcePose,
			HeadPose headPose,
			NavigationFlags flags,
			Vector3 normalizedOffset)
		{
			Flags = flags;
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
			NormalizedOffset = normalizedOffset;
		}
	}

	public /*readonly*/ struct NavigationCanceledEventArgs
	{
		public readonly NavigationFlags Flags;
		public readonly InteractionSource Source;
		public /*readonly*/ InteractionSourcePose SourcePose;
		public /*readonly*/ HeadPose HeadPose;

		public NavigationCanceledEventArgs(
			InteractionSource source,
			InteractionSourcePose sourcePose,
			HeadPose headPose,
			NavigationFlags flags)
		{
			Flags = flags;
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
		}
	}

	#endregion GestureRecognizer
}