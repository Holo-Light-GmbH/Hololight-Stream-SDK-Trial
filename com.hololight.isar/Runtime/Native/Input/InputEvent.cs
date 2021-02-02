/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System.Runtime.InteropServices;

namespace HoloLight.Isar.Native.Input
{
	public enum InputEventType : uint
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
	public struct InputEvent
	{
		[FieldOffset(0)] public InputEventType Type;

		//union - https://stackoverflow.com/questions/126781/c-union-in-c-sharp
		//[FieldOffset(8)] public readonly Select select;
		// Tap has Int64 member resulting in an 8 byte alignment
		[FieldOffset(8)] public InteractionSourceDetectedEventArgs SourceDetectedEventArgs;
		[FieldOffset(8)] public InteractionSourceLostEventArgs SourceLostEventArgs;
		[FieldOffset(8)] public InteractionSourcePressedEventArgs SourcePressedEventArgs;
		[FieldOffset(8)] public InteractionSourceUpdatedEventArgs SourceUpdatedEventArgs;
		[FieldOffset(8)] public InteractionSourceReleasedEventArgs SourceReleasedEventArgs;

		[FieldOffset(8)] public TappedEventArgs TappedEventArgs;

		[FieldOffset(8)] public HoldStartedEventArgs HoldStartedEventArgs;
		[FieldOffset(8)] public HoldCompletedEventArgs HoldCompletedEventArgs;
		[FieldOffset(8)] public HoldCanceledEventArgs HoldCanceledEventArgs;

		[FieldOffset(8)] public ManipulationStartedEventArgs ManipulationStartedEventArgs;
		[FieldOffset(8)] public ManipulationUpdatedEventArgs ManipulationUpdatedEventArgs;
		[FieldOffset(8)] public ManipulationCompletedEventArgs ManipulationCompletedEventArgs;
		[FieldOffset(8)] public ManipulationCanceledEventArgs ManipulationCanceledEventArgs;

		[FieldOffset(8)] public NavigationStartedEventArgs NavigationStartedEventArgs;
		[FieldOffset(8)] public NavigationUpdatedEventArgs NavigationUpdatedEventArgs;
		[FieldOffset(8)] public NavigationCompletedEventArgs NavigationCompletedEventArgs;
		[FieldOffset(8)] public NavigationCanceledEventArgs NavigationCanceledEventArgs;
	}
}