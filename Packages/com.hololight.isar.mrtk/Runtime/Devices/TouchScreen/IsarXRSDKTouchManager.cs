using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Unity.Profiling;
using UnityEngine;
using System.Runtime.InteropServices;
using HoloLight.Isar.Native;
using HoloLight.Isar.Native.CustomMessage;
using System;
using UnityEngine.Rendering;

namespace HoloLight.Isar.Runtime.MRTK
{
	/// <summary>
	/// Manages Touch devices using unity input system.
	/// </summary>
	[MixedRealityDataProvider(
	typeof(IMixedRealityInputSystem),
	SupportedPlatforms.WindowsStandalone | SupportedPlatforms.WindowsEditor | SupportedPlatforms.WindowsUniversal,  // All platforms supported by Unity
	"ISAR XRSDK Touch Device Manager")]
	public class IsarXRSDKTouchManager : BaseInputDeviceManager
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="inputSystem">The <see cref="Microsoft.MixedReality.Toolkit.Input.IMixedRealityInputSystem"/> instance that receives data from this provider.</param>
		/// <param name="name">Friendly name of the service.</param>
		/// <param name="priority">Service priority. Used to determine order of instantiation.</param>
		/// <param name="profile">The service's configuration profile.</param>
		public IsarXRSDKTouchManager(
			IMixedRealityInputSystem inputSystem,
			string name = null,
			uint priority = DefaultPriority,
			BaseMixedRealityProfile profile = null) : base(inputSystem, name, priority, profile) { }

		private static readonly Dictionary<int, IsarXRSDKTouchController> ActiveTouches = new Dictionary<int, IsarXRSDKTouchController>();

		private List<IsarXRSDKTouchController> touchesToRemove = new List<IsarXRSDKTouchController>();

		private static readonly ProfilerMarker UpdatePerfMarker = new ProfilerMarker("[MRTK] IsarXRSDKTouchManager.Update");

		private List<Touch> _isarTouches = new List<Touch>();
		private Camera _camera;
		private IsarCustomSend _isar;
		private const int DATA_LENGTH = 5 * 4;
		private IsarViewPose _isarViewPose;
		private PipelineType _pipelineType;
		private Matrix4x4 _projectionIsar;

		private float _horizontalFactor = 1;
		private float _verticalFactor = 1;

		public override void Enable()
		{
			base.Enable();
			_pipelineType = DetectPipeline();
			_isar = new IsarCustomSend();
			_isarViewPose = new IsarViewPose();

			_isarViewPose.ViewPoseReceived += OnViewPoseReceived;
			_isar.CustomMessageReceived += OnCustomMessageReceived;
			_camera = Camera.main;
			_projectionIsar = _camera.projectionMatrix;
		}

		private void OnViewPoseReceived(in Native.Input.HlrXrPose viewPose)
		{
			if (_isarTouches.Count > 0 && (_pipelineType == PipelineType.HDPipeline || _pipelineType == PipelineType.UniversalPipeline))
			{
				// only when client is Android (using Touch) and using HDRP or URP...
				_projectionIsar = HoloLight.Isar.Utils.Convert.ToUnity(viewPose.projLeft);
			}
		}

		private void OnCustomMessageReceived(in HlrCustomMessage message)
		{
			IntPtr readPtr = message.Data;
			HoloLight.Isar.Shared.Assertions.Assert(message.Length >= Marshal.SizeOf<Int32>());
			MessageType messageType = (MessageType)Marshal.ReadInt32(readPtr);
			if (messageType != MessageType.TOUCH_DATA) return;
			readPtr = IntPtr.Add(readPtr, Marshal.SizeOf<Int32>());

			if (message.Length < DATA_LENGTH) return;

			// Creating Touch Data out of parsed Values
			byte[] data = new byte[DATA_LENGTH];
			Marshal.Copy(readPtr, data, 0, DATA_LENGTH);
			int index = 0;
			float type = BitConverter.ToSingle(data, index);
			index += sizeof(float);
			float x = BitConverter.ToSingle(data, index);
			index += sizeof(float);
			float y = BitConverter.ToSingle(data, index);
			index += sizeof(float);
			_horizontalFactor = BitConverter.ToSingle(data, index);
			index += sizeof(float);
			_verticalFactor = BitConverter.ToSingle(data, index);

			Touch touchInfo = new Touch();

			touchInfo.position = new Vector2(x, y);

			touchInfo.phase = type == 0 ? TouchPhase.Began :
									type == 2 ? TouchPhase.Moved : TouchPhase.Ended;

			touchInfo.type = TouchType.Indirect; // remote touch
			touchInfo.fingerId = 1; // in future when we also send the fingerId, we can have it here dynamic
			touchInfo.tapCount = 1;

			if (touchInfo.phase != TouchPhase.Began && _isarTouches.Count > 0)
			{
				touchInfo.deltaPosition = new Vector2(touchInfo.position.x - _isarTouches[_isarTouches.Count - 1].position.x,
													  touchInfo.position.y - _isarTouches[_isarTouches.Count - 1].position.y);
			}
			_isarTouches.Add(touchInfo);
		}

		/// <inheritdoc />
		public override void Update()
		{
			using (UpdatePerfMarker.Auto())
			{
				base.Update();

				// Ensure that touch up and source lost events are at least one frame apart.
				for (int i = 0; i < touchesToRemove.Count; i++)
				{
					IMixedRealityController controller = touchesToRemove[i];
					Service?.RaiseSourceLost(controller.InputSource, controller);
				}
				touchesToRemove.Clear();

				int touchCount = _isarTouches.Count;

				for (int i = 0; i < touchCount; i++)
				{
					Touch touch = _isarTouches[i];
					Ray ray = new Ray();
					Matrix4x4 proj;
					if (_pipelineType == PipelineType.HDPipeline || _pipelineType == PipelineType.UniversalPipeline)
					{
						proj = _projectionIsar;
					} else
					{
						proj = _camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
					}

					Matrix4x4 origProj = proj;
					proj.m00 *= _horizontalFactor;
					proj.m11 *= _verticalFactor;
					_camera.projectionMatrix = proj;

					if (_pipelineType == PipelineType.HDPipeline || _pipelineType == PipelineType.UniversalPipeline)
					{
						ray = _camera.ViewportPointToRay(touch.position, Camera.MonoOrStereoscopicEye.Mono);
					}
					else
					{
						ray = _camera.ViewportPointToRay(touch.position);
					}

					_camera.projectionMatrix = origProj;

					switch (touch.phase)
					{
						case TouchPhase.Began:
							AddTouchController(touch, ray);
							break;
						case TouchPhase.Moved:
						case TouchPhase.Stationary:
							UpdateTouchData(touch, ray);
							break;
						case TouchPhase.Ended:
						case TouchPhase.Canceled:
							RemoveTouchController(touch);
							break;
					}
				}
				_isarTouches.Clear();

			}
		}

		/// <inheritdoc />
		public override void Disable()
		{
			base.Disable();
			_isarViewPose?.Dispose();
			_isar?.Dispose();

			if (Service != null)
			{
				foreach (var controller in ActiveTouches)
				{
					if (controller.Value == null) { continue; }

					foreach (var inputSource in Service.DetectedInputSources.ToReadOnlyCollection<IMixedRealityInputSource>())
					{
						if (inputSource.SourceId == controller.Value.InputSource.SourceId)
						{
							Service.RaiseSourceLost(controller.Value.InputSource, controller.Value);
						}
					}
				}
			}
			ActiveTouches.Clear();
		}

		private static readonly ProfilerMarker AddTouchControllerPerfMarker = new ProfilerMarker("[MRTK] IsarXRSDKTouchManager.AddTouchController");

		private void AddTouchController(Touch touch, Ray ray)
		{
			using (AddTouchControllerPerfMarker.Auto())
			{
				IsarXRSDKTouchController controller;

				if (!ActiveTouches.TryGetValue(touch.fingerId, out controller))
				{
					IMixedRealityInputSource inputSource = null;

					if (Service != null)
					{
						var pointers = RequestPointers(SupportedControllerType.TouchScreen, Handedness.Any);
						inputSource = Service.RequestNewGenericInputSource($"Touch {touch.fingerId}", pointers);
					}

					controller = new IsarXRSDKTouchController(TrackingState.NotApplicable, Handedness.Any, inputSource);

					if (inputSource != null)
					{
						for (int i = 0; i < inputSource.Pointers.Length; i++)
						{
							inputSource.Pointers[i].Controller = controller;
							var touchPointer = (IMixedRealityTouchPointer)inputSource.Pointers[i];
							touchPointer.TouchRay = ray;
							touchPointer.FingerId = touch.fingerId;
						}
					}

					ActiveTouches.Add(touch.fingerId, controller);
				}

				Service?.RaiseSourceDetected(controller.InputSource, controller);

				controller.TouchData = touch;
				controller.StartTouch();
			}
		}

		private static readonly ProfilerMarker UpdateTouchDataPerfMarker = new ProfilerMarker("[MRTK] IsarXRSDKTouchManager.UpdateTouchData");

		private void UpdateTouchData(Touch touch, Ray ray)
		{
			using (UpdateTouchDataPerfMarker.Auto())
			{
				IsarXRSDKTouchController controller;

				if (!ActiveTouches.TryGetValue(touch.fingerId, out controller))
				{
					return;
				}

				controller.TouchData = touch;
				var pointer = (IMixedRealityTouchPointer)controller.InputSource.Pointers[0];
				controller.ScreenPointRay = pointer.TouchRay = ray;

				controller.Update();
			}
		}

		private static readonly ProfilerMarker RemoveTouchControllerPerfMarker = new ProfilerMarker("[MRTK] IsarXRSDKTouchManager.RemoveTouchController");

		private void RemoveTouchController(Touch touch)
		{
			using (RemoveTouchControllerPerfMarker.Auto())
			{
				IsarXRSDKTouchController controller;

				if (!ActiveTouches.TryGetValue(touch.fingerId, out controller))
				{
					return;
				}

				RecyclePointers(controller.InputSource);

				controller.TouchData = touch;
				controller.EndTouch();
				// Schedule the source lost event.
				touchesToRemove.Add(controller);
				// Remove from the active collection
				ActiveTouches.Remove(touch.fingerId);
			}
		}

		private enum PipelineType
		{
			Unsupported,
			BuiltInPipeline,
			UniversalPipeline,
			HDPipeline
		}

		/// <summary>
		/// Returns the type of renderpipeline that is currently running
		/// </summary>
		/// <returns></returns>
		private static PipelineType DetectPipeline()
		{
#if UNITY_2019_1_OR_NEWER
			if (GraphicsSettings.renderPipelineAsset != null)
			{
				// SRP
				var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
				if (srpType.Contains("HDRenderPipelineAsset"))
				{
					return PipelineType.HDPipeline;
				}
				else if (srpType.Contains("UniversalRenderPipelineAsset") || srpType.Contains("LightweightRenderPipelineAsset"))
				{
					return PipelineType.UniversalPipeline;
				}
				else return PipelineType.Unsupported;
			}
#elif UNITY_2017_1_OR_NEWER
			if (GraphicsSettings.renderPipelineAsset != null) {
				// SRP not supported before 2019
				return PipelineType.Unsupported;
			}
#endif
			// no SRP
			return PipelineType.BuiltInPipeline;
		}
	}
}
