// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using HoloLight.Isar.Native;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.XRSDK.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.XR;
using static HoloLight.Isar.Runtime.MRTK.IsarTouchControllerData;

namespace HoloLight.Isar.Runtime.MRTK
{
	/// <summary>
	/// Manages XR SDK devices on the Oculus platform.
	/// </summary>
	[MixedRealityDataProvider(
		typeof(IMixedRealityInputSystem),
		SupportedPlatforms.WindowsStandalone | SupportedPlatforms.Android,
		"ISAR XRSDK Oculus Device Manager",
		"",
		"MixedRealityToolkit.Providers")]
	public class IsarXRSDKOculusDeviceManager : XRSDKDeviceManager
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="inputSystem">The <see cref="Microsoft.MixedReality.Toolkit.Input.IMixedRealityInputSystem"/> instance that receives data from this provider.</param>
		/// <param name="name">Friendly name of the service.</param>
		/// <param name="priority">Service priority. Used to determine order of instantiation.</param>
		/// <param name="profile">The service's configuration profile.</param>
		public IsarXRSDKOculusDeviceManager(
			IMixedRealityInputSystem inputSystem,
			string name = null,
			uint priority = DefaultPriority,
			BaseMixedRealityProfile profile = null) : base(inputSystem, name, priority, profile) { }

		private Dictionary<Handedness, IsarXRSDKOculusTouchController> trackedControllers = new Dictionary<Handedness, IsarXRSDKOculusTouchController>();
		private IsarCustomSend _isar;


		private IsarTouchControllerDataParent _controllerData = new IsarTouchControllerDataParent();
		private IsarTouchControllerData _leftControllerData => _controllerData.Left;
		private IsarTouchControllerData _rightControllerData => _controllerData.Right;


		/// <summary>
		/// Current Stylus Controller.
		/// </summary>
		public IsarXRSDKOculusTouchController IsarOculusController { get; private set; }

		/// <inheritdoc/>
		public override void Enable()
		{
			base.Enable();

			_isar = new IsarCustomSend();
			_isar.CustomMessageReceived += OnCustomMessageReceived;
		}

		private void OnCustomMessageReceived(in HlrCustomMessage message)
		{
			int length = (int)message.Length;
			byte[] managedData = new byte[length];

			Marshal.Copy(message.Data, managedData, 0, length);

			Debug.Assert(BitConverter.IsLittleEndian);
			int index = 0;

			// parse message type
			int clientType = BitConverter.ToInt32(managedData, index);
			if (clientType != 1) return;
			index += 4;

			// parse message type
			int messageType = BitConverter.ToInt32(managedData, index);
			if (messageType != 1) return;

			index += 4;

			string messageData = Encoding.UTF8.GetString(managedData, 8, managedData.Length - 8);

			_controllerData = JsonConvert.DeserializeObject<IsarTouchControllerDataParent>(messageData);
		}

		private static readonly ProfilerMarker UpdatePerfMarker = new ProfilerMarker("[MRTK] IsarXRSDKOculusDeviceManager.Update");

		/// <inheritdoc/>
		public override void Update()
		{
			using (UpdatePerfMarker.Auto())
			{
				base.Update();

				UpdateController(_leftControllerData);
				UpdateController(_rightControllerData);
			}
		}

		private void UpdateController(IsarTouchControllerData controllerInfo)
		{
			if (controllerInfo.IsTracked)
			{
				var controllerManager = GetOrAddIsarController(controllerInfo.Handedness);

				controllerManager.UpdateController(controllerInfo);
			}
			else
			{
				RemoveIsarOculusController(controllerInfo.Handedness);
			}
		}

		private IsarXRSDKOculusTouchController GetOrAddIsarController(Handedness handedness)
		{
			if (trackedControllers.ContainsKey(handedness))
			{
				return trackedControllers[handedness];
			}

			// Add new Controller
			var pointers = RequestPointers(SupportedControllerType.OculusTouch, handedness);
			var inputSourceType = InputSourceType.Controller;

			IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;
			var inputSource = inputSystem?.RequestNewGenericInputSource($"Oculus Quest {handedness} Controller", pointers, inputSourceType);

			IsarXRSDKOculusTouchController controllerDevice = new IsarXRSDKOculusTouchController(TrackingState.Tracked, handedness, inputSource);

			for (int i = 0; i < controllerDevice.InputSource?.Pointers?.Length; i++)
			{
				controllerDevice.InputSource.Pointers[i].Controller = controllerDevice;
			}

			inputSystem?.RaiseSourceDetected(controllerDevice.InputSource, controllerDevice);

			trackedControllers.Add(handedness, controllerDevice);

			return controllerDevice;
		}

		public override void Disable()
		{
			base.Disable();
			_isar?.Dispose();
		}


		#region IMixedRealityCapabilityCheck Implementation

		/// <inheritdoc />
		public override bool CheckCapability(MixedRealityCapability capability)
		{
			return capability == MixedRealityCapability.MotionController;
		}

		#endregion IMixedRealityCapabilityCheck Implementation

		#region Controller Utilities

		private void RemoveIsarOculusController(Handedness handedness)
		{
			if (trackedControllers.TryGetValue(handedness, out IsarXRSDKOculusTouchController controller))
			{
				RemoveIsarOculusController(controller);
			}
		}

		private void RemoveAllIsarOculusControllers()
		{
			if (trackedControllers.Count == 0) return;

			// Create a new list to avoid causing an error removing items from a list currently being iterated on.
			foreach (var controller in new List<IsarXRSDKOculusTouchController>(trackedControllers.Values))
			{
				RemoveIsarOculusController(controller);
			}
			trackedControllers.Clear();
		}

		private void RemoveIsarOculusController(IsarXRSDKOculusTouchController controllerDevice)
		{
			if (controllerDevice == null) return;

			CoreServices.InputSystem?.RaiseSourceLost(controllerDevice.InputSource, controllerDevice);
			trackedControllers.Remove(controllerDevice.ControllerHandedness);

			RecyclePointers(controllerDevice.InputSource);
		}

		/// <inheritdoc />
		protected override Type GetControllerType(SupportedControllerType supportedControllerType)
		{
			switch (supportedControllerType)
			{
				//case SupportedControllerType.ArticulatedHand:
				//	return typeof(OculusHand);
				case SupportedControllerType.OculusTouch:
					return typeof(IsarXRSDKOculusTouchController);
				default:
					return base.GetControllerType(supportedControllerType);
			}
		}

		/// <inheritdoc />
		protected override InputSourceType GetInputSourceType(SupportedControllerType supportedControllerType)
		{
			switch (supportedControllerType)
			{
				case SupportedControllerType.OculusTouch:
					return InputSourceType.Controller;
				case SupportedControllerType.ArticulatedHand:
					return InputSourceType.Hand;
				default:
					return base.GetInputSourceType(supportedControllerType);
			}
		}

		/// <inheritdoc />
		protected override SupportedControllerType GetCurrentControllerType(InputDevice inputDevice)
		{
			if (inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.HandTracking))
			{
				if (inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Left) ||
					inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Right))
				{
					// If it's a hand with a reported handedness, assume articulated hand
					return SupportedControllerType.ArticulatedHand;
				}
			}

			if (inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
			{
				return SupportedControllerType.OculusTouch;
			}

			return base.GetCurrentControllerType(inputDevice);
		}

		#endregion Controller Utilities
	}
}