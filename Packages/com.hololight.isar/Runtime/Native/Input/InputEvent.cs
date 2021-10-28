/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System.Runtime.InteropServices;

namespace HoloLight.Isar.Native.Input
{
	public enum HlrInputEventType : uint
	{
		//Unknown = 0,
		// InteractionManager
		InteractionSourceDetected,
		InteractionSourceLost,
		InteractionSourcePressed,
		InteractionSourceUpdated,
		InteractionSourceReleased,

		// GestureRecognizer
		Selected,
		Tapped,

		HoldStarted,
		HoldCompleted,
		HoldCanceled,

		ManipulationStarted,
		ManipulationUpdated,
		ManipulationCompleted,
		ManipulationCanceled,

		NavigationStarted,
		NavigationUpdated,
		NavigationCompleted,
		NavigationCanceled,
		Count,
		Max = Count - 1,
		Min = 0,
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct HlrInputEvent
	{
		[FieldOffset(0)] public HlrInputEventType Type;

		//union - https://stackoverflow.com/questions/126781/c-union-in-c-sharp
		//[FieldOffset(8)] public readonly Select select;
		// Tap has Int64 member resulting in an 8 byte alignment
		[FieldOffset(8)] public HlrInputEventDataSpatialInteractionSourceDetected SourceDetectedEventArgs;
		[FieldOffset(8)] public HlrInputEventDataSpatialInteractionSourceLost SourceLostEventArgs;
		[FieldOffset(8)] public HlrInputEventDataSpatialInteractionSourcePressed SourcePressedEventArgs;
		[FieldOffset(8)] public HlrInputEventDataSpatialInteractionSourceUpdated SourceUpdatedEventArgs;
		[FieldOffset(8)] public HlrInputEventDataSpatialInteractionSourceReleased SourceReleasedEventArgs;

		[FieldOffset(8)] public HlrInputEventDataTapped TappedEventArgs;

		[FieldOffset(8)] public HlrInputEventDataHoldStarted HoldStartedEventArgs;
		[FieldOffset(8)] public HlrInputEventDataHoldCompleted HoldCompletedEventArgs;
		[FieldOffset(8)] public HlrInputEventDataHoldCanceled HoldCanceledEventArgs;

		[FieldOffset(8)] public HlrInputEventDataManipulationStarted ManipulationStartedEventArgs;
		[FieldOffset(8)] public HlrInputEventDataManipulationUpdated ManipulationUpdatedEventArgs;
		[FieldOffset(8)] public HlrInputEventDataManipulationCompleted ManipulationCompletedEventArgs;
		[FieldOffset(8)] public HlrInputEventDataManipulationCanceled ManipulationCanceledEventArgs;

		[FieldOffset(8)] public HlrInputEventDataNavigationStarted NavigationStartedEventArgs;
		[FieldOffset(8)] public HlrInputEventDataNavigationUpdated NavigationUpdatedEventArgs;
		[FieldOffset(8)] public HlrInputEventDataNavigationCompleted NavigationCompletedEventArgs;
		[FieldOffset(8)] public HlrInputEventDataNavigationCanceled NavigationCanceledEventArgs;
	}

	enum HlrInputType : uint
	{
		SourceDetected = 0,
		SourceLost,
		SourcePressed,
		SourceUpdated,
		SourceReleased
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct HlrSpatialInputData
	{
		[FieldOffset(0)] HlrSpatialInputDataInteractionSourceDetected sourceDetected;
		[FieldOffset(0)] HlrSpatialInputDataInteractionSourceLost sourceLost;
		[FieldOffset(0)] HlrSpatialInputDataInteractionSourcePressed sourcePressed;
		[FieldOffset(0)] HlrSpatialInputDataInteractionSourceUpdated sourceUpdated;
		[FieldOffset(0)] HlrSpatialInputDataInteractionSourceReleased sourceReleased;
	}

	public struct HlrSvSpatialInput
	{
		HlrInputType type;
		HlrSpatialInputData data;
	}
}