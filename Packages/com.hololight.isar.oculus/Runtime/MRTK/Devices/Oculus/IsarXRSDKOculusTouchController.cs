// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.```

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.XRSDK.Input;
using UnityEngine;
using Unity.Profiling;
using UnityEngine.XR;
using System;

namespace HoloLight.Isar.Runtime.MRTK
{
	[MixedRealityController(
		SupportedControllerType.OculusTouch,
		new[] { Handedness.Left, Handedness.Right },
		"StandardAssets/Textures/OculusControllersTouch")]
	public class IsarXRSDKOculusTouchController : GenericXRSDKController
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public IsarXRSDKOculusTouchController(TrackingState trackingState, Handedness controllerHandedness,
			IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
			: base(trackingState, controllerHandedness, inputSource, interactions)
		{
		}

		/// <inheritdoc />
		public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => new[]
		{
			new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer),
			new MixedRealityInteractionMapping(1, "Axis1D.PrimaryIndexTrigger", AxisType.SingleAxis, DeviceInputType.Trigger),
			new MixedRealityInteractionMapping(2, "Axis1D.PrimaryIndexTrigger Touch", AxisType.Digital, DeviceInputType.TriggerTouch),
			new MixedRealityInteractionMapping(3, "Axis1D.PrimaryIndexTrigger Near Touch", AxisType.Digital, DeviceInputType.TriggerNearTouch),
			new MixedRealityInteractionMapping(4, "Axis1D.PrimaryIndexTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress),
			new MixedRealityInteractionMapping(5, "Axis1D.PrimaryHandTrigger Press", AxisType.SingleAxis, DeviceInputType.GripPress),
			new MixedRealityInteractionMapping(6, "Axis2D.PrimaryThumbstick", AxisType.DualAxis, DeviceInputType.ThumbStick),
			new MixedRealityInteractionMapping(7, "Button.PrimaryThumbstick Touch", AxisType.Digital, DeviceInputType.ThumbStickTouch),
			new MixedRealityInteractionMapping(8, "Button.PrimaryThumbstick Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch),
			new MixedRealityInteractionMapping(9, "Button.PrimaryThumbstick Press", AxisType.Digital, DeviceInputType.ThumbStickPress),
			new MixedRealityInteractionMapping(10, "Button.Three Press", AxisType.Digital, DeviceInputType.PrimaryButtonPress),
			new MixedRealityInteractionMapping(11, "Button.Four Press", AxisType.Digital, DeviceInputType.SecondaryButtonPress),
			new MixedRealityInteractionMapping(12, "Button.Three Touch", AxisType.Digital, DeviceInputType.PrimaryButtonTouch),
			new MixedRealityInteractionMapping(13, "Button.Four Touch", AxisType.Digital, DeviceInputType.SecondaryButtonTouch),
			new MixedRealityInteractionMapping(14, "Button.Start Press", AxisType.Digital, DeviceInputType.Menu),
			new MixedRealityInteractionMapping(15, "Touch.PrimaryThumbRest Touch", AxisType.Digital, DeviceInputType.ThumbTouch),
			new MixedRealityInteractionMapping(16, "Touch.PrimaryThumbRest Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch)
		};

		/// <inheritdoc />
		public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => new[]
		{
			new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer),
			new MixedRealityInteractionMapping(1, "Axis1D.SecondaryIndexTrigger", AxisType.SingleAxis, DeviceInputType.Trigger),
			new MixedRealityInteractionMapping(2, "Axis1D.SecondaryIndexTrigger Touch", AxisType.Digital, DeviceInputType.TriggerTouch),
			new MixedRealityInteractionMapping(3, "Axis1D.SecondaryIndexTrigger Near Touch", AxisType.Digital, DeviceInputType.TriggerNearTouch),
			new MixedRealityInteractionMapping(4, "Axis1D.SecondaryIndexTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress),
			new MixedRealityInteractionMapping(5, "Axis1D.SecondaryHandTrigger Press", AxisType.SingleAxis, DeviceInputType.GripPress),
			new MixedRealityInteractionMapping(6, "Axis2D.SecondaryThumbstick", AxisType.DualAxis, DeviceInputType.ThumbStick),
			new MixedRealityInteractionMapping(7, "Button.SecondaryThumbstick Touch", AxisType.Digital, DeviceInputType.ThumbStickTouch),
			new MixedRealityInteractionMapping(8, "Button.SecondaryThumbstick Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch),
			new MixedRealityInteractionMapping(9, "Button.SecondaryThumbstick Press", AxisType.Digital, DeviceInputType.ThumbStickPress),
			new MixedRealityInteractionMapping(10, "Button.One Press", AxisType.Digital, DeviceInputType.PrimaryButtonPress),
			new MixedRealityInteractionMapping(11, "Button.Two Press", AxisType.Digital, DeviceInputType.SecondaryButtonPress),
			new MixedRealityInteractionMapping(12, "Button.One Touch", AxisType.Digital, DeviceInputType.PrimaryButtonTouch),
			new MixedRealityInteractionMapping(13, "Button.Two Touch", AxisType.Digital, DeviceInputType.SecondaryButtonTouch),
			new MixedRealityInteractionMapping(14, "Touch.SecondaryThumbRest Touch", AxisType.Digital, DeviceInputType.ThumbTouch),
			new MixedRealityInteractionMapping(15, "Touch.SecondaryThumbRest Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch)
		};


		private static readonly ProfilerMarker UpdateControllerPerfMarker = new ProfilerMarker("[MRTK] IsarXRSDKOculusTouchController.UpdateController");


		public void UpdateController(IsarTouchControllerData controllerInfo)
		{
			using (UpdateControllerPerfMarker.Auto())
			{
				IsPositionAvailable = IsRotationAvailable = controllerInfo.IsTracked;

				for (int i = 0; i < Interactions?.Length; i++)
				{
					MixedRealityInteractionMapping interactionMapping = Interactions[i];
					switch (interactionMapping.InputType)
					{
						case DeviceInputType.SpatialPointer:
							controllerInfo.PointerPose.Position = new Vector3(controllerInfo.Position.x, controllerInfo.Position.y, -controllerInfo.Position.z);
							controllerInfo.PointerPose.Rotation = new Quaternion(-controllerInfo.Rotation.x, -controllerInfo.Rotation.y, controllerInfo.Rotation.z, controllerInfo.Rotation.w);

							interactionMapping.PoseData = controllerInfo.PointerPose;
							if (interactionMapping.Changed)
							{
								CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction, interactionMapping.PoseData);
							}
							break;

						///////////////// THUMBSTICK ////////////////////////////
						case DeviceInputType.ThumbStick:
							interactionMapping.Vector2Data = new Vector2(controllerInfo.JoystickData.Position.x, controllerInfo.JoystickData.Position.y);
							if (interactionMapping.Changed)
							{
								CoreServices.InputSystem?.RaisePositionInputChanged(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction, interactionMapping.Vector2Data);
							}
							break;
						case DeviceInputType.ThumbStickPress:
							interactionMapping.BoolData = controllerInfo.JoystickData.Press;
							HandleButtonInteraction(interactionMapping);
							break;
						case DeviceInputType.ThumbStickTouch:
							interactionMapping.BoolData = controllerInfo.JoystickData.Touch; 
							HandleButtonInteraction(interactionMapping);
							break;

						///////////////// side GRIP/Middlefinger Button //////////////
						case DeviceInputType.GripPress:
							interactionMapping.FloatData = controllerInfo.GripTrigger; // evtl. 
							HandleSingleAxisInteraction(interactionMapping);
							break;

						/////////////// INDEX FINGER BUTTON //////////////////////
						case DeviceInputType.Trigger:
							interactionMapping.FloatData = controllerInfo.IndexTrigger;
							HandleSingleAxisInteraction(interactionMapping);
							break;
						case DeviceInputType.TriggerPress:
							interactionMapping.BoolData = controllerInfo.IndexTrigger.Equals(1);
							HandleButtonInteraction(interactionMapping);
							break;
						case DeviceInputType.TriggerTouch:
							interactionMapping.BoolData = controllerInfo.IndexTrigger > 0;
							HandleButtonInteraction(interactionMapping);
							break;

						/////////////// BUTTONS AB  XY PRESS AND TOUCH //////////////////////
						case DeviceInputType.PrimaryButtonPress:
							interactionMapping.BoolData = controllerInfo.Primary.Press;
							HandleButtonInteraction(interactionMapping);
							break;
						case DeviceInputType.SecondaryButtonPress:
							interactionMapping.BoolData = controllerInfo.Secondary.Press;
							HandleButtonInteraction(interactionMapping);
							break;
						case DeviceInputType.PrimaryButtonTouch:
							interactionMapping.BoolData = controllerInfo.Primary.Touch;
							HandleButtonInteraction(interactionMapping);
							break;
						case DeviceInputType.SecondaryButtonTouch:
							interactionMapping.BoolData = controllerInfo.Secondary.Touch;
							HandleButtonInteraction(interactionMapping);
							break;
					}
					// mrtk inputs triggern 
				} 
			}
		}

		private void HandleSingleAxisInteraction(MixedRealityInteractionMapping interactionMapping)
		{
			if (interactionMapping.Changed)
			{
				CoreServices.InputSystem?.RaiseFloatInputChanged(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction, interactionMapping.FloatData);
			}
		}

		private void HandleButtonInteraction(MixedRealityInteractionMapping interactionMapping)
		{
			if (interactionMapping.Changed)
			{
				// Raise input system event if it's enabled
				if (interactionMapping.BoolData)
				{
					CoreServices.InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction);
				}
				else
				{
					CoreServices.InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction);
				}
			}
		}

		private static readonly ProfilerMarker UpdateButtonDataPerfMarker = new ProfilerMarker("[MRTK] IsarXRSDKOculusTouchController.UpdateButtonData");
		protected override void UpdateButtonData(MixedRealityInteractionMapping interactionMapping, InputDevice inputDevice)
		{
			using (UpdateButtonDataPerfMarker.Auto())
			{
				Debug.Assert(interactionMapping.AxisType == AxisType.Digital);

				InputFeatureUsage<bool> buttonUsage;
				bool usingOculusButtonData = false;
				/*
								switch (interactionMapping.InputType)
								{
									case DeviceInputType.TriggerTouch:
										buttonUsage = OculusUsages.indexTouch;
										usingOculusButtonData = true;
										break;
									case DeviceInputType.TriggerNearTouch:
										buttonUsage = OculusUsages.indexTouch;
										usingOculusButtonData = true;
										break;
									case DeviceInputType.ThumbTouch:
									case DeviceInputType.ThumbNearTouch:
										buttonUsage = OculusUsages.thumbrest;
										usingOculusButtonData = true;
										break;
								}*/

				if (!usingOculusButtonData)
				{
					base.UpdateButtonData(interactionMapping, inputDevice);
				}
				else
				{
					if (inputDevice.TryGetFeatureValue(buttonUsage, out bool buttonPressed))
					{
						interactionMapping.BoolData = buttonPressed;
					}

					// If our value changed raise it.
					if (interactionMapping.Changed)
					{
						// Raise input system event if it's enabled
						if (interactionMapping.BoolData)
						{
							CoreServices.InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction);
						}
						else
						{
							CoreServices.InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction);
						}
					}
				}
			}
		}
	}
}