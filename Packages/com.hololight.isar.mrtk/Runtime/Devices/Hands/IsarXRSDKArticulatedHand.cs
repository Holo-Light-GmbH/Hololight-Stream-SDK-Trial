using System;
using System.Collections.Generic;
using HoloLight.Isar.Runtime.MRTK.Devices.WindowsMixedReality;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.XRSDK.Input;
using UnityEngine;
using UnityEngine.XR;

namespace HoloLight.Isar.Runtime.MRTK
{
	[MixedRealityController(
		SupportedControllerType.ArticulatedHand,
		new[] { Handedness.Left, Handedness.Right })]
	public class IsarXRSDKArticulatedHand : GenericXRSDKController, IMixedRealityHand
	{
		public MixedRealityInputAction HoldAction = MixedRealityInputAction.None;
		public MixedRealityInputAction NavigationAction = MixedRealityInputAction.None;
		public MixedRealityInputAction ManipulationAction = MixedRealityInputAction.None;
		public MixedRealityInputAction SelectAction = MixedRealityInputAction.None;

		private static readonly HandFinger[] handFingers = Enum.GetValues(typeof(HandFinger)) as HandFinger[];

		// This is copy-pasted from WindowsMixedRealityXRSDKArticulatedHand
		// The rotation offset between the reported grip pose of a hand and the palm joint orientation.
		// These values were calculated by comparing the platform's reported grip pose and palm pose.
		private static readonly Quaternion rightPalmOffset = new Quaternion(Mathf.Sqrt(0.125f), Mathf.Sqrt(0.125f), -Mathf.Sqrt(1.5f) / 2.0f, Mathf.Sqrt(1.5f) / 2.0f);
		private static readonly Quaternion leftPalmOffset = new Quaternion(Mathf.Sqrt(0.125f), -Mathf.Sqrt(0.125f), Mathf.Sqrt(1.5f) / 2.0f, Mathf.Sqrt(1.5f) / 2.0f);

		private readonly List<Bone> fingerBones = new List<Bone>();
		private readonly Dictionary<TrackedHandJoint, MixedRealityPose> unityJointPoses = new Dictionary<TrackedHandJoint, MixedRealityPose>();
		private readonly IsarWindowsMixedRealityArticulatedHandDefinition handDefinition;

		public IsarXRSDKArticulatedHand(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
				: base(trackingState, controllerHandedness, inputSource, interactions)
		{
			handDefinition = new IsarWindowsMixedRealityArticulatedHandDefinition(inputSource, controllerHandedness);
		}

		public override MixedRealityInteractionMapping[] DefaultInteractions => handDefinition.DefaultInteractions;

		public override bool IsInPointingPose => handDefinition.IsInPointingPose;

		public override void SetupDefaultInteractions()
		{
			AssignControllerMappings(DefaultInteractions);
		}

		public override void UpdateController(InputDevice inputDevice)
		{
			base.UpdateController(inputDevice);

			//get hand data here from xrsdk.
			Hand hand;
			if (inputDevice.TryGetFeatureValue(CommonUsages.handData, out hand))
			{
				foreach (HandFinger finger in handFingers)
				{
					if (hand.TryGetFingerBones(finger, fingerBones))
					{
						for (int i = 0; i < fingerBones.Count; i++)
						{
							TrackedHandJoint trackedHandJoint = ConvertToTrackedHandJoint(finger, i);
							Bone bone = fingerBones[i];

							Vector3 position = Vector3.zero;
							Quaternion rotation = Quaternion.identity;

							// I guess this didn't work because it short-circuited out (we always have position).
							// I want both, though.
							if (bone.TryGetPosition(out position) /*|| bone.TryGetRotation(out rotation)*/)
							{
								// We want input sources to follow the playspace, so fold in the playspace transform here to
								// put the controller pose into world space.
								position = MixedRealityPlayspace.TransformPoint(position);

							}

							if (bone.TryGetRotation(out rotation))
							{
								rotation = MixedRealityPlayspace.Rotation * rotation;
							}

							unityJointPoses[trackedHandJoint] = new MixedRealityPose(position, rotation);
						}

						// Unity doesn't provide a palm joint, so we synthesize one here.
						// This is copy-pasted from WindowsMixedRealityXRSDKArticulatedHand
						MixedRealityPose palmPose = CurrentControllerPose;
						//palmPose.Rotation *= (ControllerHandedness == Handedness.Left ? leftPalmOffset : rightPalmOffset);
						unityJointPoses[TrackedHandJoint.Palm] = palmPose;
					}
				}

				handDefinition?.UpdateHandJoints(unityJointPoses);
			}

			bool dataAvailable = inputDevice.TryGetFeatureValue(GestureFeatureUsages.Tapped, out uint tapCount);
			if (dataAvailable && tapCount > 0)
			{
				CoreServices.InputSystem?.RaiseGestureCompleted(this, SelectAction);
			}

			dataAvailable = inputDevice.TryGetFeatureValue(GestureFeatureUsages.HoldStartedUsage, out bool holdStarted);
			if (dataAvailable && holdStarted)
			{
				CoreServices.InputSystem?.RaiseGestureStarted(this, HoldAction);
			}

			dataAvailable = inputDevice.TryGetFeatureValue(GestureFeatureUsages.HoldCompletedUsage, out bool holdCompleted);
			if (dataAvailable && holdCompleted)
			{
				CoreServices.InputSystem?.RaiseGestureCompleted(this, HoldAction);
			}

			dataAvailable = inputDevice.TryGetFeatureValue(GestureFeatureUsages.HoldCanceledUsage, out bool holdCanceled);
			if (dataAvailable && holdCanceled)
			{
				CoreServices.InputSystem?.RaiseGestureCanceled(this, HoldAction);
			}

			dataAvailable = inputDevice.TryGetFeatureValue(GestureFeatureUsages.ManipulationStartedUsage, out bool manipulationStarted);
			if (dataAvailable && manipulationStarted)
			{
				CoreServices.InputSystem?.RaiseGestureStarted(this, ManipulationAction);
			}

			dataAvailable = inputDevice.TryGetFeatureValue(GestureFeatureUsages.ManipulationUpdatedUsage, out Vector3 delta);
			if (dataAvailable)
			{
				if (!float.IsNaN(delta.x) && !float.IsNaN(delta.y) && !float.IsNaN(delta.z))
				{
					CoreServices.InputSystem?.RaiseGestureUpdated(this, ManipulationAction, delta);
				}
			}

			dataAvailable = inputDevice.TryGetFeatureValue(GestureFeatureUsages.ManipulationCompletedUsage, out delta);
			if (dataAvailable)
			{
				if (!float.IsNaN(delta.x) && !float.IsNaN(delta.y) && !float.IsNaN(delta.z))
				{
					CoreServices.InputSystem?.RaiseGestureCompleted(this, ManipulationAction, delta);
				}
			}

			dataAvailable = inputDevice.TryGetFeatureValue(GestureFeatureUsages.ManipulationCanceledUsage, out bool manipulationCanceled);
			if (dataAvailable && manipulationCanceled)
			{
				CoreServices.InputSystem?.RaiseGestureCanceled(this, ManipulationAction);
			}

			//dataAvailable = inputDevice.TryGetFeatureValue(GestureFeatureUsages.NavigationStartedUsage, out bool navigationStarted);
			//if (dataAvailable && navigationStarted)
			//{
			//	CoreServices.InputSystem?.RaiseGestureStarted(this, NavigationAction);
			//}

			//dataAvailable = inputDevice.TryGetFeatureValue(GestureFeatureUsages.NavigationUpdatedUsage, out Vector3 offset);
			//if (dataAvailable)
			//{
			//	CoreServices.InputSystem?.RaiseGestureUpdated(this, NavigationAction, offset);
			//}

			//dataAvailable = inputDevice.TryGetFeatureValue(GestureFeatureUsages.NavigationCompletedUsage, out offset);
			//if (dataAvailable)
			//{
			//	CoreServices.InputSystem?.RaiseGestureCompleted(this, NavigationAction, offset);
			//}

			//dataAvailable = inputDevice.TryGetFeatureValue(GestureFeatureUsages.NavigationCanceledUsage, out bool navigationCanceled);
			//if (dataAvailable && navigationCanceled)
			//{
			//	CoreServices.InputSystem?.RaiseGestureCanceled(this, NavigationAction);
			//}

			//that's what winmr articulated hand also does. I guess based on the index finger it then changes the pointer when you're near or whatever.
			for (int i = 0; i < Interactions?.Length; i++)
			{
				switch (Interactions[i].InputType)
				{
					case DeviceInputType.IndexFinger:
						handDefinition?.UpdateCurrentIndexPose(Interactions[i]);
						break;
				}
			}
		}

		public bool TryGetJoint(TrackedHandJoint joint, out MixedRealityPose pose) => unityJointPoses.TryGetValue(joint, out pose);

		static class GestureFeatureUsages
		{
			//The value here is tap count (0, 1 or 2, though MRTK doesn't seem to support double taps at the time of writing)
			public static InputFeatureUsage<uint> Tapped = new InputFeatureUsage<uint>("tapped");

			public static InputFeatureUsage<bool> HoldStartedUsage = new InputFeatureUsage<bool>("hold started");
			public static InputFeatureUsage<bool> HoldCompletedUsage = new InputFeatureUsage<bool>("hold completed");
			public static InputFeatureUsage<bool> HoldCanceledUsage = new InputFeatureUsage<bool>("hold canceled");

			public static InputFeatureUsage<bool> ManipulationStartedUsage = new InputFeatureUsage<bool>("manipulation started");
			public static InputFeatureUsage<Vector3> ManipulationUpdatedUsage = new InputFeatureUsage<Vector3>("manipulation updated");
			public static InputFeatureUsage<Vector3> ManipulationCompletedUsage = new InputFeatureUsage<Vector3>("manipulation completed");
			public static InputFeatureUsage<bool> ManipulationCanceledUsage = new InputFeatureUsage<bool>("manipulation canceled");

			public static InputFeatureUsage<bool> NavigationStartedUsage = new InputFeatureUsage<bool>("navigation started");
			public static InputFeatureUsage<Vector3> NavigationUpdatedUsage = new InputFeatureUsage<Vector3>("navigation updated");
			public static InputFeatureUsage<Vector3> NavigationCompletedUsage = new InputFeatureUsage<Vector3>("navigation completed");
			public static InputFeatureUsage<bool> NavigationCanceledUsage = new InputFeatureUsage<bool>("navigation canceled");
		}

		protected override void UpdatePoseData(MixedRealityInteractionMapping interactionMapping, InputDevice inputDevice)
		{
			//so, this doesn't work on its own, i.e. the gizmo does not change when we move our hands.
			base.UpdatePoseData(interactionMapping, inputDevice);

			//if we do stuff for spatialpointer the gizmo moves but then also the ray does and it doesn't feel the same way as in the shell.
			//what do we do now?
			Debug.Assert(interactionMapping.AxisType == AxisType.SixDof);

			// Update the interaction data source
			switch (interactionMapping.InputType)
			{
				case DeviceInputType.SpatialPointer:
					InputFeatureUsage<Vector3> pointerPosUsage = new InputFeatureUsage<Vector3>("PointerPosition"); // CustomUsages.PointerPosition
					InputFeatureUsage<Quaternion> pointerRotUsage = new InputFeatureUsage<Quaternion>("PointerRotation"); // CustomUsages.PointerRotation

					Vector3 pointerPos = Vector3.zero;
					Quaternion pointerRot = Quaternion.identity;

					bool gotPos = inputDevice.TryGetFeatureValue(pointerPosUsage, out pointerPos);
					bool gotRot = inputDevice.TryGetFeatureValue(pointerRotUsage, out pointerRot);

					if (gotPos && gotRot)
					{
						MixedRealityPose pointerPose = new MixedRealityPose(pointerPos, pointerRot);
						pointerPose.Position = MixedRealityPlayspace.TransformPoint(pointerPose.Position);
						pointerPose.Rotation = MixedRealityPlayspace.Rotation * pointerPose.Rotation;
						//do stuff w/ this

						interactionMapping.PoseData = pointerPose;

						// If our value changed raise it.
						if (interactionMapping.Changed)
						{
							// Raise input system event if it's enabled
							CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction, interactionMapping.PoseData);
						}
					}
					break;
			}
		}

		//copied from WindowsMixedRealityXRSDKArticulatedHand. If we do everything the same way it "should" work.
		private TrackedHandJoint ConvertToTrackedHandJoint(HandFinger finger, int index)
		{
			switch (finger)
			{
				case HandFinger.Thumb: return (index == 0) ? TrackedHandJoint.Wrist : TrackedHandJoint.ThumbMetacarpalJoint + index - 1;
				case HandFinger.Index: return TrackedHandJoint.IndexMetacarpal + index;
				case HandFinger.Middle: return TrackedHandJoint.MiddleMetacarpal + index;
				case HandFinger.Ring: return TrackedHandJoint.RingMetacarpal + index;
				case HandFinger.Pinky: return TrackedHandJoint.PinkyMetacarpal + index;
				default: return TrackedHandJoint.None;
			}
		}
	}

}
