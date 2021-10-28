/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;

namespace HoloLight.Isar.Native.Input
{
	#region InteractionManager

	public struct HlrInputEventDataSpatialInteractionSourceDetected
	{
		public HlrSpatialInteractionSourceState InteractionSourceState;

		public HlrInputEventDataSpatialInteractionSourceDetected(HlrSpatialInteractionSourceState interactionSourceState)
		{
			InteractionSourceState = interactionSourceState;
		}
	}

	public struct HlrInputEventDataSpatialInteractionSourceLost
	{
		public HlrSpatialInteractionSourceState InteractionSourceState;

		public HlrInputEventDataSpatialInteractionSourceLost(HlrSpatialInteractionSourceState interactionSourceState)
		{
			InteractionSourceState = interactionSourceState;
		}
	}

	public struct HlrInputEventDataSpatialInteractionSourcePressed
	{
		public HlrSpatialInteractionSourceState InteractionSourceState;
		// TODO?: WSA has this additional PressType...
		// public InteractionSourcePressType PressType;

		public HlrInputEventDataSpatialInteractionSourcePressed(HlrSpatialInteractionSourceState interactionSourceState)
		{
			InteractionSourceState = interactionSourceState;
		}
	}

	public struct HlrInputEventDataSpatialInteractionSourceUpdated
	{
		public HlrSpatialInteractionSourceState InteractionSourceState;

		public HlrInputEventDataSpatialInteractionSourceUpdated(HlrSpatialInteractionSourceState interactionSourceState)
		{
			InteractionSourceState = interactionSourceState;
		}
	}

	public struct HlrInputEventDataSpatialInteractionSourceReleased
	{
		public HlrSpatialInteractionSourceState InteractionSourceState;

		public HlrInputEventDataSpatialInteractionSourceReleased(HlrSpatialInteractionSourceState interactionSourceState)
		{
			InteractionSourceState = interactionSourceState;
		}
	}

	#endregion InteractionManager

	#region GestureRecognizer

	public /*readonly*/ struct HlrInputEventDataTapped
	{
		public readonly HlrSpatialInteractionSource Source;
		public /*readonly*/ HlrSpatialInteractionSourcePose SourcePose;
		public /*readonly*/ HlrHeadPose HeadPose;
		public readonly int TapCount; // can only be 1 or 2, so it can be packed as 1 bit

		public HlrInputEventDataTapped(
			HlrSpatialInteractionSource source,
			HlrSpatialInteractionSourcePose sourcePose,
			HlrHeadPose headPose,
			int tapCount)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
			TapCount = tapCount;
		}
	}

	public /*readonly*/ struct HlrInputEventDataHoldStarted
	{
		public readonly HlrSpatialInteractionSource Source;
		public /*readonly*/ HlrSpatialInteractionSourcePose SourcePose;
		public /*readonly*/ HlrHeadPose HeadPose;

		public HlrInputEventDataHoldStarted(
			HlrSpatialInteractionSource source,
			HlrSpatialInteractionSourcePose sourcePose,
			HlrHeadPose headPose)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
		}
	}

	public /*readonly*/ struct HlrInputEventDataHoldCompleted
	{
		public readonly HlrSpatialInteractionSource Source;
		public /*readonly*/ HlrSpatialInteractionSourcePose SourcePose;
		public /*readonly*/ HlrHeadPose HeadPose;

		public HlrInputEventDataHoldCompleted(
			HlrSpatialInteractionSource source,
			HlrSpatialInteractionSourcePose sourcePose,
			HlrHeadPose headPose)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
		}
	}

	public /*readonly*/ struct HlrInputEventDataHoldCanceled
	{
		public readonly HlrSpatialInteractionSource Source;
		public /*readonly*/ HlrSpatialInteractionSourcePose SourcePose;
		public /*readonly*/ HlrHeadPose HeadPose;

		public HlrInputEventDataHoldCanceled(
			HlrSpatialInteractionSource source,
			HlrSpatialInteractionSourcePose sourcePose,
			HlrHeadPose headPose)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
		}
	}

	public /*readonly*/ struct HlrInputEventDataManipulationStarted
	{
		public readonly HlrSpatialInteractionSource Source;
		public /*readonly*/ HlrSpatialInteractionSourcePose SourcePose;
		public /*readonly*/ HlrHeadPose HeadPose;

		public HlrInputEventDataManipulationStarted(
			HlrSpatialInteractionSource source,
			HlrSpatialInteractionSourcePose sourcePose,
			HlrHeadPose headPose)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
		}
	}

	public /*readonly*/ struct HlrInputEventDataManipulationUpdated
	{
		public readonly HlrSpatialInteractionSource Source;
		public /*readonly*/ HlrSpatialInteractionSourcePose SourcePose;
		public /*readonly*/ HlrHeadPose HeadPose;
		public /*readonly*/ HlrSpatialManipulationDelta Delta;

		public HlrInputEventDataManipulationUpdated(
			HlrSpatialInteractionSource source,
			HlrSpatialInteractionSourcePose sourcePose,
			HlrHeadPose headPose,
			HlrSpatialManipulationDelta delta)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
			Delta = delta;
		}
	}

	public /*readonly*/ struct HlrInputEventDataManipulationCompleted
	{
		public readonly HlrSpatialInteractionSource Source;
		public /*readonly*/ HlrSpatialInteractionSourcePose SourcePose;
		public /*readonly*/ HlrHeadPose HeadPose;
		public /*readonly*/ HlrSpatialManipulationDelta Delta;

		public HlrInputEventDataManipulationCompleted(
			HlrSpatialInteractionSource source,
			HlrSpatialInteractionSourcePose sourcePose,
			HlrHeadPose headPose,
			HlrSpatialManipulationDelta delta)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
			Delta = delta;
		}
	}

	public /*readonly*/ struct HlrInputEventDataManipulationCanceled
	{
		public readonly HlrSpatialInteractionSource Source;
		public /*readonly*/ HlrSpatialInteractionSourcePose SourcePose;
		public /*readonly*/ HlrHeadPose HeadPose;

		public HlrInputEventDataManipulationCanceled(
			HlrSpatialInteractionSource source,
			HlrSpatialInteractionSourcePose sourcePose,
			HlrHeadPose headPose)
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

	public /*readonly*/ struct HlrInputEventDataNavigationStarted
	{
		public readonly NavigationFlags Flags;

		public readonly HlrSpatialInteractionSource Source;
		public /*readonly*/ HlrSpatialInteractionSourcePose SourcePose;
		public /*readonly*/ HlrHeadPose HeadPose;

		public HlrInputEventDataNavigationStarted(
			HlrSpatialInteractionSource source,
			HlrSpatialInteractionSourcePose sourcePose,
			HlrHeadPose headPose,
			NavigationFlags flags)
		{
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
			Flags = flags;
		}
	}

	public /*readonly*/ struct HlrInputEventDataNavigationUpdated
	{
		public readonly NavigationFlags Flags;
		public readonly HlrSpatialInteractionSource Source;
		public /*readonly*/ HlrSpatialInteractionSourcePose SourcePose;
		public /*readonly*/ HlrHeadPose HeadPose;
		public /*readonly*/ HlrVector3 NormalizedOffset;

		public HlrInputEventDataNavigationUpdated(
			HlrSpatialInteractionSource source,
			HlrSpatialInteractionSourcePose sourcePose,
			HlrHeadPose headPose,
			NavigationFlags flags,
			HlrVector3 normalizedOffset)
		{
			Flags = flags;
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
			NormalizedOffset = normalizedOffset;
		}
	}

	public /*readonly*/ struct HlrInputEventDataNavigationCompleted
	{
		public readonly NavigationFlags Flags;
		public readonly HlrSpatialInteractionSource Source;
		public /*readonly*/ HlrSpatialInteractionSourcePose SourcePose;
		public /*readonly*/ HlrHeadPose HeadPose;
		public /*readonly*/ HlrVector3 NormalizedOffset;

		public HlrInputEventDataNavigationCompleted(
			HlrSpatialInteractionSource source,
			HlrSpatialInteractionSourcePose sourcePose,
			HlrHeadPose headPose,
			NavigationFlags flags,
			HlrVector3 normalizedOffset)
		{
			Flags = flags;
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
			NormalizedOffset = normalizedOffset;
		}
	}

	public /*readonly*/ struct HlrInputEventDataNavigationCanceled
	{
		public readonly NavigationFlags Flags;
		public readonly HlrSpatialInteractionSource Source;
		public /*readonly*/ HlrSpatialInteractionSourcePose SourcePose;
		public /*readonly*/ HlrHeadPose HeadPose;

		public HlrInputEventDataNavigationCanceled(
			HlrSpatialInteractionSource source,
			HlrSpatialInteractionSourcePose sourcePose,
			HlrHeadPose headPose,
			NavigationFlags flags)
		{
			Flags = flags;
			Source = source;
			SourcePose = sourcePose;
			HeadPose = headPose;
		}
	}

	#endregion GestureRecognizer

	public struct HlrSpatialInputDataInteractionSourceDetected
	{
		HlrInteractionSourceState interactionSourceState;
	}

	public struct HlrSpatialInputDataInteractionSourceLost
	{
		HlrInteractionSourceState interactionSourceState;
	}
	public struct HlrSpatialInputDataInteractionSourcePressed
	{
		HlrInteractionSourceState interactionSourceState;
	}
	public struct HlrSpatialInputDataInteractionSourceUpdated
	{
		HlrInteractionSourceState interactionSourceState;
	}
	public struct HlrSpatialInputDataInteractionSourceReleased
	{
		HlrInteractionSourceState interactionSourceState;
	}

	public struct HlrInteractionSourceState
	{
		HlrControllerData controllerData;
	}

	struct HlrControllerData
	{
		uint controllerIdentifier;
		int handedNess; //do enums later
		HlrHeadPose headPose;
		HlrPose controllerPose;
		HlrHandPose handData;
		IntPtr buttons;
		uint buttonsLength;
		IntPtr axis1D;
		uint axis1DLength;
		IntPtr axis2D;
		uint axis2DLength;
	}
}