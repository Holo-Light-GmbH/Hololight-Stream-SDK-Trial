using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Windows.Input;
using Microsoft.MixedReality.Toolkit.XRSDK.Input;
using UnityEngine.XR;

namespace HoloLight.Isar.Runtime.MRTK
{
	[MixedRealityDataProvider(
		typeof(IMixedRealityInputSystem),
		SupportedPlatforms.WindowsStandalone | SupportedPlatforms.WindowsEditor | SupportedPlatforms.WindowsUniversal,
		"ISAR XRSDK Device Manager")]
	public class IsarXRSDKDeviceManager : XRSDKDeviceManager
	{
		private MixedRealityInputAction holdAction = MixedRealityInputAction.None;
		private MixedRealityInputAction navigationAction = MixedRealityInputAction.None;
		private MixedRealityInputAction manipulationAction = MixedRealityInputAction.None;
		private MixedRealityInputAction selectAction = MixedRealityInputAction.None;

		/// <summary>
		/// Current Gesture Settings for the GestureRecognizer
		/// </summary>
		public static WindowsGestureSettings GestureSettings { get; set; }

		/// <summary>
		/// Current Navigation Gesture Recognizer Settings.
		/// </summary>
		public static WindowsGestureSettings NavigationSettings { get; set; }

		/// <summary>
		/// Current Navigation Gesture Recognizer Rails Settings.
		/// </summary>
		public static WindowsGestureSettings RailsNavigationSettings { get; set; }

		/// <summary>
		/// Should the Navigation Gesture Recognizer use Rails?
		/// </summary>
		public static bool UseRailsNavigation { get; set; }

		public IsarXRSDKDeviceManager(
			IMixedRealityInputSystem inputSystem,
			string name = null,
			uint priority = 10,
			BaseMixedRealityProfile profile = null) : base(inputSystem, name, priority, profile)
		{
		}

		public override void Enable()
		{
			if (InputSystemProfile == null) { return; }

			if (InputSystemProfile.GesturesProfile != null)
			{
				var gestureProfile = InputSystemProfile.GesturesProfile;
				GestureSettings = gestureProfile.ManipulationGestures;
				NavigationSettings = gestureProfile.NavigationGestures;
				RailsNavigationSettings = gestureProfile.RailsNavigationGestures;
				UseRailsNavigation = gestureProfile.UseRailsNavigation;

				for (int i = 0; i < gestureProfile.Gestures.Length; i++)
				{
					var gesture = gestureProfile.Gestures[i];

					switch (gesture.GestureType)
					{
						case GestureInputType.Hold:
							holdAction = gesture.Action;
							break;
						case GestureInputType.Manipulation:
							manipulationAction = gesture.Action;
							break;
						case GestureInputType.Navigation:
							navigationAction = gesture.Action;
							break;
						case GestureInputType.Select:
							selectAction = gesture.Action;
							break;
					}
				}
			}

			base.Enable();
		}

		public override bool CheckCapability(MixedRealityCapability capability)
		{
			switch (capability)
			{
				case MixedRealityCapability.ArticulatedHand:
				case MixedRealityCapability.GGVHand:
				case MixedRealityCapability.MotionController:
					return true;
				default:
					return false;
			}
		}

		public override void Update()
		{

			//hack: pass gesture input actions down to all our active controllers. Why?
			//Because gesture events are now per-controller and need to be polled, as opposed to having a single
			//GestureRecognizer instance inside the DeviceManager. However, DeviceManager has access to all 
			//the profile info needed to raise gestures (i.e. the input actions).
			foreach (var controller in ActiveControllers.Values)
			{
				if (controller is IsarXRSDKArticulatedHand)
				{
					var isarHand = (IsarXRSDKArticulatedHand)controller;
					isarHand.HoldAction = holdAction;
					isarHand.ManipulationAction = manipulationAction;
					isarHand.NavigationAction = navigationAction;
					isarHand.SelectAction = selectAction;
				}
				controller.Enabled = true;
			}

			base.Update();


		}

		protected override Type GetControllerType(SupportedControllerType supportedControllerType)
		{
			if (supportedControllerType == SupportedControllerType.OculusTouch)
			{
				return typeof(IsarXRSDKMotionController);
			}
			if (supportedControllerType == SupportedControllerType.ArticulatedHand)
			{
				return typeof(IsarXRSDKArticulatedHand);
			}
			return typeof(GenericXRSDKController);
		}

		protected override InputSourceType GetInputSourceType(SupportedControllerType supportedControllerType)
		{
			switch (supportedControllerType)
			{
				case SupportedControllerType.ArticulatedHand:
				case SupportedControllerType.GGVHand:
					return InputSourceType.Hand;
				case SupportedControllerType.OculusTouch:
				case SupportedControllerType.OculusRemote:
				case SupportedControllerType.GenericOpenVR:
				case SupportedControllerType.ViveKnuckles:
				case SupportedControllerType.ViveWand:
				case SupportedControllerType.GenericUnity:
				case SupportedControllerType.WindowsMixedReality:
				case SupportedControllerType.Xbox:
					return InputSourceType.Controller;
				default:
					return InputSourceType.Other;
			}
		}

		//copy-pasted from XRSDK WMR device manager.
		protected override SupportedControllerType GetCurrentControllerType(InputDevice inputDevice)
		{
			if (inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.HandTracking))
			{
				if (inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Left) ||
					inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Right))
				{
					// If it's a hand with a reported handedness, assume HL2 articulated hand
					return SupportedControllerType.ArticulatedHand;
				}
			}

			if (inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Controller) && inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.HeldInHand))
			{
				if (inputDevice.manufacturer == "Oculus")
				{
					return SupportedControllerType.OculusTouch;
				}
				if(inputDevice.manufacturer == "Microsoft")
				{
					return SupportedControllerType.WindowsMixedReality;
				}
				// Do other manufacturers here... we can also check the device name for better identification later on...
			}
			return base.GetCurrentControllerType(inputDevice);
		}
	}

}
