
using System;

namespace HoloLight.Isar.Native.Input
{
	static class Convert
	{
		public delegate void InputEventConverter(ref InputEvent input);
		public static void ToUnityCoordinateSystem(ref InputEvent input)
		{
			//Assert.IsTrue(input.Type >= InputEventType.Min &&
			//              input.Type <= InputEventType.Max);
			if (input.Type < InputEventType.Min &&
			    input.Type > InputEventType.Max)
			{
				throw new IndexOutOfRangeException(nameof(input.Type));
			}
			InputEventConverter convert = DispatchTable[(int)input.Type];
			convert(ref input);

			//_inputEventConverters[(int)input.Type](ref input);
		}

		// Look Up Table
		private static readonly InputEventConverter[] DispatchTable =
		{
			// InteractionManager
			ConvertInteractionSourceDetected,
			ConvertInteractionSourceLost,
			ConvertInteractionSourcePressed,
			ConvertInteractionSourceUpdated,
			ConvertInteractionSourceReleased,

			// GestureRecognizer
			ConvertSelected,
			ConvertTapped,

			ConvertHoldStarted,
			ConvertHoldCompleted,
			ConvertHoldCanceled,

			ConvertManipulationStarted,
			ConvertManipulationUpdated,
			ConvertManipulationCompleted,
			ConvertManipulationCanceled,

			ConvertNavigationStarted,
			ConvertNavigationUpdated,
			ConvertNavigationCompleted,
			ConvertNavigationCanceled,
		};

		#region conversion functions

		//private static void DoConversion(ref JointPose jointPose)
		//{
		//	jointPose.Position.Z    = -jointPose.Position.Z;
		//	jointPose.Orientation.X = -jointPose.Orientation.X;
		//	jointPose.Orientation.Y = -jointPose.Orientation.Y;
		//}

		private static JointPose DoConversion(JointPose jointPose)
		{
			jointPose.Position.Z    = -jointPose.Position.Z;
			jointPose.Orientation.X = -jointPose.Orientation.X;
			jointPose.Orientation.Y = -jointPose.Orientation.Y;
			return jointPose;
		}

		private static void DoConversion(ref HandPose handPose)
		{
			for (int i = 0; i < handPose.JointPoses.Length; i++)
			{
				//var pose = handPose.JointPoses[i];
				//DoConversion(ref pose);
				//handPose.JointPoses[i] = pose;
				handPose.JointPoses[i] = DoConversion(handPose.JointPoses[i]);
			}
		}

		private static void DoConversion(ref InteractionSourcePose sourcePose)
		{
			sourcePose.Velocity.Z           = -sourcePose.Velocity.Z;
			sourcePose.AngularVelocity.Z    = -sourcePose.AngularVelocity.Z;

			sourcePose.GripPosition.Z       = -sourcePose.GripPosition.Z;
			sourcePose.GripOrientation.X    = -sourcePose.GripOrientation.X;
			sourcePose.GripOrientation.Y    = -sourcePose.GripOrientation.Y;

			sourcePose.PointerPosition.Z    = -sourcePose.PointerPosition.Z;
			sourcePose.PointerOrientation.X = -sourcePose.PointerOrientation.X;
			sourcePose.PointerOrientation.Y = -sourcePose.PointerOrientation.Y;
		}

		private static void DoConversion(ref HeadPose headPose)
		{
			headPose.ForwardDirection.Z = -headPose.ForwardDirection.Z;
			headPose.UpDirection.Z      = -headPose.UpDirection.Z;
			headPose.Position.Z         = -headPose.Position.Z;
		}

		private static void DoConversion(ref InteractionSourceState state)
		{
			DoConversion(ref state.Properties.SourcePose);
			state.Properties.SourceLossMitigationDirection.Z = -state.Properties.SourceLossMitigationDirection.Z;
			DoConversion(ref state.HeadPose);
			DoConversion(ref state.HandPose);
		}

		private static void ConvertInteractionSourceDetected(ref InputEvent input)
		{
			//var args = input.SourceDetectedEventArgs;
			// HACK: convert directx coord system to unity coord system
			DoConversion(ref input.SourceDetectedEventArgs.InteractionSourceState);

			// NOTE: this is for forcing HoloLens 1 Input (GGV) on HoloLens 2
			//args.InteractionSourceState.Source.Flags &= ~InteractionSourceFlags.SupportsPointing;
		}

		private static void ConvertInteractionSourceLost(ref InputEvent input)
		{
			//var args = input.SourceLostEventArgs;
			// HACK: convert directx coord system to unity coord system
			DoConversion(ref input.SourceLostEventArgs.InteractionSourceState);

			// NOTE: this is for forcing HoloLens 1 Input (GGV) on HoloLens 2
			//args.InteractionSourceState.Source.Flags &= ~InteractionSourceFlags.SupportsPointing;
		}

		private static void ConvertInteractionSourcePressed(ref InputEvent input)
		{
			//var args = input.SourcePressedEventArgs;
			// HACK: convert directx coord system to unity coord system
			DoConversion(ref input.SourcePressedEventArgs.InteractionSourceState);

			// NOTE: this is for forcing HoloLens 1 Input (GGV) on HoloLens 2
			//args.InteractionSourceState.Source.Flags &= ~InteractionSourceFlags.SupportsPointing;
		}

		private static void ConvertInteractionSourceUpdated(ref InputEvent input)
		{
			//var args = input.SourceUpdatedEventArgs;
			// HACK: convert directx coord system to unity coord system
			DoConversion(ref input.SourceUpdatedEventArgs.InteractionSourceState);

			// NOTE: this is for forcing HoloLens 1 Input (GGV) on HoloLens 2
			//args.InteractionSourceState.Source.Flags &= ~InteractionSourceFlags.SupportsPointing;
		}

		private static void ConvertInteractionSourceReleased(ref InputEvent input)
		{
			//var args = input.SourceReleasedEventArgs;
			// HACK: convert directx coord system to unity coord system
			DoConversion(ref input.SourceReleasedEventArgs.InteractionSourceState);

			// NOTE: this is for forcing HoloLens 1 Input (GGV) on HoloLens 2
			//args.InteractionSourceState.Source.Flags &= ~InteractionSourceFlags.SupportsPointing;
		}

		private static void ConvertSelected(ref InputEvent input) { /*do nothing*/ }

		private static void ConvertTapped(ref InputEvent input)
		{
			DoConversion(ref input.TappedEventArgs.HeadPose);
		}

		private static void ConvertHoldStarted(ref InputEvent input)
		{
			DoConversion(ref input.HoldStartedEventArgs.SourcePose);
			DoConversion(ref input.HoldStartedEventArgs.HeadPose);
		}

		private static void ConvertHoldCompleted(ref InputEvent input)
		{
			DoConversion(ref input.HoldCompletedEventArgs.SourcePose);
			DoConversion(ref input.HoldCompletedEventArgs.HeadPose);
		}

		private static void ConvertHoldCanceled(ref InputEvent input)
		{
			DoConversion(ref input.HoldCanceledEventArgs.SourcePose);
			DoConversion(ref input.HoldCanceledEventArgs.HeadPose);
		}

		private static void ConvertManipulationStarted(ref InputEvent input)
		{
			DoConversion(ref input.ManipulationStartedEventArgs.SourcePose);
			DoConversion(ref input.ManipulationStartedEventArgs.HeadPose);
		}

		private static void ConvertManipulationUpdated(ref InputEvent input)
		{
			DoConversion(ref input.ManipulationUpdatedEventArgs.SourcePose);
			DoConversion(ref input.ManipulationUpdatedEventArgs.HeadPose);
			input.ManipulationUpdatedEventArgs.Delta.Translation.Z = -input.ManipulationUpdatedEventArgs.Delta.Translation.Z;
		}

		private static void ConvertManipulationCompleted(ref InputEvent input)
		{
			DoConversion(ref input.ManipulationCompletedEventArgs.SourcePose);
			DoConversion(ref input.ManipulationCompletedEventArgs.HeadPose);
			input.ManipulationCompletedEventArgs.Delta.Translation.Z = -input.ManipulationCompletedEventArgs.Delta.Translation.Z;
		}

		private static void ConvertManipulationCanceled(ref InputEvent input)
		{
			DoConversion(ref input.ManipulationCanceledEventArgs.SourcePose);
			DoConversion(ref input.ManipulationCanceledEventArgs.HeadPose);
		}

		private static void ConvertNavigationStarted(ref InputEvent input)
		{
			DoConversion(ref input.NavigationStartedEventArgs.SourcePose);
			DoConversion(ref input.NavigationStartedEventArgs.HeadPose);
		}

		private static void ConvertNavigationUpdated(ref InputEvent input)
		{
			DoConversion(ref input.NavigationUpdatedEventArgs.SourcePose);
			DoConversion(ref input.NavigationUpdatedEventArgs.HeadPose);
			input.NavigationUpdatedEventArgs.NormalizedOffset.Z = -input.NavigationUpdatedEventArgs.NormalizedOffset.Z;
		}

		private static void ConvertNavigationCompleted(ref InputEvent input)
		{
			DoConversion(ref input.NavigationCompletedEventArgs.SourcePose);
			DoConversion(ref input.NavigationCompletedEventArgs.HeadPose);
			input.NavigationCompletedEventArgs.NormalizedOffset.Z = -input.NavigationCompletedEventArgs.NormalizedOffset.Z;
		}

		private static void ConvertNavigationCanceled(ref InputEvent input)
		{
			DoConversion(ref input.NavigationCanceledEventArgs.SourcePose);
			DoConversion(ref input.NavigationCanceledEventArgs.HeadPose);
		}

		#endregion conversion functions

	}
}
