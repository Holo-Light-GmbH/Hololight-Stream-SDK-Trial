// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.```

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.XRSDK.Input;
using UnityEngine;
using Unity.Profiling;
using UnityEngine.XR;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HoloLight.Isar.Runtime.MRTK
{
	[MixedRealityController(
		SupportedControllerType.GenericUnity,
		new[] { Handedness.Left, Handedness.Right },
		"StandardAssets/Textures/OculusControllersTouch")]
	public class IsarXRSDKMotionController : GenericXRSDKController
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public IsarXRSDKMotionController(TrackingState trackingState, Handedness controllerHandedness,
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
			new MixedRealityInteractionMapping(5, "Axis1D.PrimaryHandTrigger Press", AxisType.SingleAxis, DeviceInputType.Grip),
			new MixedRealityInteractionMapping(6, "Axis2D.PrimaryThumbstick", AxisType.DualAxis, DeviceInputType.ThumbStick),
			new MixedRealityInteractionMapping(7, "Button.PrimaryHandTrigger Press", AxisType.Digital, DeviceInputType.GripPress),
			new MixedRealityInteractionMapping(8, "Button.PrimaryThumbstick Touch", AxisType.Digital, DeviceInputType.ThumbStickTouch),
			new MixedRealityInteractionMapping(9, "Button.PrimaryThumbstick Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch),
			new MixedRealityInteractionMapping(10, "Button.PrimaryThumbstick Press", AxisType.Digital, DeviceInputType.ThumbStickPress),
			new MixedRealityInteractionMapping(11, "Button.Three Press", AxisType.Digital, DeviceInputType.PrimaryButtonPress),
			new MixedRealityInteractionMapping(12, "Button.Four Press", AxisType.Digital, DeviceInputType.SecondaryButtonPress),
			new MixedRealityInteractionMapping(13, "Button.Three Touch", AxisType.Digital, DeviceInputType.PrimaryButtonTouch),
			new MixedRealityInteractionMapping(14, "Button.Four Touch", AxisType.Digital, DeviceInputType.SecondaryButtonTouch),
			new MixedRealityInteractionMapping(15, "Button.Start Press", AxisType.Digital, DeviceInputType.Menu),
			new MixedRealityInteractionMapping(16, "Touch.PrimaryThumbRest Touch", AxisType.Digital, DeviceInputType.ThumbTouch),
			new MixedRealityInteractionMapping(17, "Touch.PrimaryThumbRest Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch)
		};

		/// <inheritdoc />
		public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => new[]
		{
			new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer),
			new MixedRealityInteractionMapping(1, "Axis1D.SecondaryIndexTrigger", AxisType.SingleAxis, DeviceInputType.Trigger),
			new MixedRealityInteractionMapping(2, "Axis1D.SecondaryIndexTrigger Touch", AxisType.Digital, DeviceInputType.TriggerTouch),
			new MixedRealityInteractionMapping(3, "Axis1D.SecondaryIndexTrigger Near Touch", AxisType.Digital, DeviceInputType.TriggerNearTouch),
			new MixedRealityInteractionMapping(4, "Axis1D.SecondaryIndexTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress),
			new MixedRealityInteractionMapping(5, "Axis1D.SecondaryHandTrigger Press", AxisType.SingleAxis, DeviceInputType.Grip),
			new MixedRealityInteractionMapping(6, "Axis2D.SecondaryThumbstick", AxisType.DualAxis, DeviceInputType.ThumbStick),
			new MixedRealityInteractionMapping(7, "Button.SecondaryHandTrigger Press", AxisType.Digital, DeviceInputType.GripPress),
			new MixedRealityInteractionMapping(8, "Button.SecondaryThumbstick Touch", AxisType.Digital, DeviceInputType.ThumbStickTouch),
			new MixedRealityInteractionMapping(9, "Button.SecondaryThumbstick Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch),
			new MixedRealityInteractionMapping(10, "Button.SecondaryThumbstick Press", AxisType.Digital, DeviceInputType.ThumbStickPress),
			new MixedRealityInteractionMapping(11, "Button.One Press", AxisType.Digital, DeviceInputType.PrimaryButtonPress),
			new MixedRealityInteractionMapping(12, "Button.Two Press", AxisType.Digital, DeviceInputType.SecondaryButtonPress),
			new MixedRealityInteractionMapping(13, "Button.One Touch", AxisType.Digital, DeviceInputType.PrimaryButtonTouch),
			new MixedRealityInteractionMapping(14, "Button.Two Touch", AxisType.Digital, DeviceInputType.SecondaryButtonTouch),
			new MixedRealityInteractionMapping(15, "Touch.SecondaryThumbRest Touch", AxisType.Digital, DeviceInputType.ThumbTouch),
			new MixedRealityInteractionMapping(16, "Touch.SecondaryThumbRest Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch)
		};


		/// <summary>
		/// We need to overwrite for the thumbrest because generic XRSDK controller doesn't look into this feature...
		/// </summary>
		private static readonly ProfilerMarker UpdateButtonDataPerfMarker = new ProfilerMarker("[MRTK] GenericXRSDKController.UpdateButtonData");

		/// <summary>
		/// Update an interaction bool data type from a bool input
		/// </summary>
		/// <remarks>
		/// Raises an Input System "Input Down" event when the key is down, and raises an "Input Up" when it is released (e.g. a Button)
		/// </remarks>
		protected override void UpdateButtonData(MixedRealityInteractionMapping interactionMapping, InputDevice inputDevice)
		{
			base.UpdateButtonData(interactionMapping, inputDevice);

			using (UpdateButtonDataPerfMarker.Auto())
			{
				Debug.Assert(interactionMapping.AxisType == AxisType.Digital);

				InputFeatureUsage<bool> buttonUsage;

				// Update the interaction data source
				switch (interactionMapping.InputType)
				{
					case DeviceInputType.ThumbTouch:
						var usages = new List<InputFeatureUsage>();
						if (inputDevice.TryGetFeatureUsages(usages))
						{
							buttonUsage = usages.FirstOrDefault(usage => usage.name == "Thumbrest").As<bool>();
						}
						break;
				}

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