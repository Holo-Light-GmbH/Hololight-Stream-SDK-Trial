/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AOT;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;
using HoloLight.Isar.Native;
using HoloLight.Isar.Native.Input;
using HoloLight.Isar.Native.Qr;
using Version = HoloLight.Isar.Native.Version;

namespace HoloLight.Isar
{
	/// <summary>
	/// Provides access to native Server api.
	/// </summary>
	public class RemotingServer : IDisposable
	{
		#region SignalingApi Callbacks

		public event Action<SdpType, string> SdpCreated;
		//public event Action<string> SdpOfferCreated;
		//public event Action<string> SdpAnswerCreated;
		private void Callbacks_SdpCreated(SdpType type, string sdp)
		{
			Debug.Log("Callbacks_SdpCreated");
			SdpCreated?.Invoke(type, sdp);

			//if (type == SdpType.Offer)
			//	SdpOfferCreated?.Invoke(sdp);
			//else if (type == SdpType.Answer)
			//	SdpAnswerCreated?.Invoke(sdp);
		}

		/// <summary>
		/// Fired when a local ICE candidate is created.
		/// </summary>
		public event Action<string, int, string> IceCandidateCreated;

		private void Callbacks_LocalIceCandidateCreated(string sdpMLine, int mLineIndex, string iceCandidate)
		{
			Debug.Log("Callbacks_LocalIceCandidateCreated");
			IceCandidateCreated?.Invoke(sdpMLine, mLineIndex, iceCandidate);
		}

		#endregion SignalingApi Callbacks

		#region Connection Callbacks

		public event Action<ConnectionState> ConnectionStateChanged;
		private ConnectionState _connectionState = ConnectionState.Disconnected;
		private bool connectionStateChanged = false;
		public bool IsConnected
		{
			get { return _connectionState == ConnectionState.Connected; }
		}

		private void Callbacks_ConnectionStateChanged(ConnectionState state)
		{
			Debug.Log("Callbacks_ConnectionStateChanged");
			_connectionState = state;
			//ConnectionStateChanged?.Invoke(state);
			connectionStateChanged = true;
		}

		#endregion Connection Callbacks

#if !ISAR_LEGACY
#pragma warning disable CS0649
#endif
		internal ServerApi ServerApi;
#if !ISAR_LEGACY
#pragma warning restore CS0649
#endif
		public Version Version => this.ServerApi.Version;
		internal SignalingApi SignalingApi => this.ServerApi.SignalingApi;
		internal ConnectionApi ConnectionApi => this.ServerApi.ConnectionApi;

		#region input

		#region InteractionManager

		// TODO: we might need to have a bunch of multiple source states accessible by timestamp too
		// eg. for SpatialInteractionManager.GetDetectedSourcesAtTimestamp or SpatialInteractionManager.FromHistoricalTargetTime
		// which are used in WindowsMixedRealityArticulatedHand
		private readonly Dictionary<UInt32, InteractionSourceState> _interactionSourceStates =
			new Dictionary<UInt32, InteractionSourceState>();

		public InteractionSourceState[] GetCurrentReading()
		{
			return _interactionSourceStates.Values.ToArray();
		}

		public int GetCurrentReading(InteractionSourceState[] sourceStates)
		{
			if (sourceStates == null)
			{
				throw new ArgumentNullException(nameof(sourceStates));
			}

			_interactionSourceStates.Values.CopyTo(sourceStates, 0);
			return NumSourceStates;
		}

		public int NumSourceStates => _interactionSourceStates.Count;

		public delegate void InteractionSourceDetectedEventHandler(in InteractionSourceDetectedEventArgs args);
		public event InteractionSourceDetectedEventHandler InteractionSourceDetected;
		public delegate void InteractionSourceLostEventHandler(in InteractionSourceLostEventArgs args);
		public event InteractionSourceLostEventHandler InteractionSourceLost;
		public delegate void InteractionSourcePressedEventHandler(in InteractionSourcePressedEventArgs args);
		public event InteractionSourcePressedEventHandler InteractionSourcePressed;
		public delegate void InteractionSourceUpdatedEventHandler(in InteractionSourceUpdatedEventArgs args);
		public event InteractionSourceUpdatedEventHandler InteractionSourceUpdated;
		public delegate void InteractionSourceReleasedEventHandler(in InteractionSourceReleasedEventArgs args);
		public event InteractionSourceReleasedEventHandler InteractionSourceReleased;

		#endregion InteractionManager

		#region GestureRecognizer

		public event Action Selected;
		public event Action<TappedEventArgs> Tapped;

		public event Action<HoldStartedEventArgs> HoldStarted;
		public event Action<HoldCompletedEventArgs> HoldCompleted;
		public event Action<HoldCanceledEventArgs> HoldCanceled;

		public event Action<ManipulationStartedEventArgs> ManipulationStarted;
		public event Action<ManipulationUpdatedEventArgs> ManipulationUpdated;
		public event Action<ManipulationCompletedEventArgs> ManipulationCompleted;
		public event Action<ManipulationCanceledEventArgs> ManipulationCanceled;

		public event Action<NavigationStartedEventArgs> NavigationStarted;
		public event Action<NavigationUpdatedEventArgs> NavigationUpdated;
		public event Action<NavigationCompletedEventArgs> NavigationCompleted;
		public event Action<NavigationCanceledEventArgs> NavigationCanceled;

		#endregion GestureRecognizer

		#region InputEvent Dispatching

		private delegate void InputEventHandler(in InputEvent input);
		private InputEventHandler[] _inputEvents;

		private void InvokeInteractionSourceDetected(in InputEvent input)
		{
			var args = input.SourceDetectedEventArgs;
			// NOTE: this is for forcing HoloLens 1 Input (GGV) on HoloLens 2
			//args.InteractionSourceState.Source.Flags &= ~InteractionSourceFlags.SupportsPointing;
			_interactionSourceStates[args.InteractionSourceState.Source.Id] = args.InteractionSourceState;

			InteractionSourceDetected?.Invoke(args);
		}

		private void InvokeInteractionSourceLost(in InputEvent input)
		{
			var args = input.SourceLostEventArgs;
			// NOTE: this is for forcing HoloLens 1 Input (GGV) on HoloLens 2
			//args.InteractionSourceState.Source.Flags &= ~InteractionSourceFlags.SupportsPointing;
			_interactionSourceStates.Remove(args.InteractionSourceState.Source.Id);

			InteractionSourceLost?.Invoke(args);
		}

		private void InvokeInteractionSourcePressed(in InputEvent input)
		{
			var args = input.SourcePressedEventArgs;
			// NOTE: this is for forcing HoloLens 1 Input (GGV) on HoloLens 2
			//args.InteractionSourceState.Source.Flags &= ~InteractionSourceFlags.SupportsPointing;
			_interactionSourceStates[args.InteractionSourceState.Source.Id] = args.InteractionSourceState;

			InteractionSourcePressed?.Invoke(args);
		}

		private void InvokeInteractionSourceUpdated(in InputEvent input)
		{
			var args = input.SourceUpdatedEventArgs;
			// NOTE: this is for forcing HoloLens 1 Input (GGV) on HoloLens 2
			//args.InteractionSourceState.Source.Flags &= ~InteractionSourceFlags.SupportsPointing;
			_interactionSourceStates[args.InteractionSourceState.Source.Id] = args.InteractionSourceState;

			InteractionSourceUpdated?.Invoke(args);
		}

		private void InvokeInteractionSourceReleased(in InputEvent input)
		{
			var args = input.SourceReleasedEventArgs;
			// NOTE: this is for forcing HoloLens 1 Input (GGV) on HoloLens 2
			//args.InteractionSourceState.Source.Flags &= ~InteractionSourceFlags.SupportsPointing;
			_interactionSourceStates[args.InteractionSourceState.Source.Id] = args.InteractionSourceState;

			InteractionSourceReleased?.Invoke(args);
		}

		private void InvokeSelected(in InputEvent input) { Selected?.Invoke(/*userData*/); }

		private void InvokeTapped(in InputEvent input) { Tapped?.Invoke(input.TappedEventArgs); }

		private void InvokeHoldStarted(in InputEvent input) { HoldStarted?.Invoke(input.HoldStartedEventArgs); }
		private void InvokeHoldCompleted(in InputEvent input) { HoldCompleted?.Invoke(input.HoldCompletedEventArgs); }
		private void InvokeHoldCanceled(in InputEvent input) { HoldCanceled?.Invoke(input.HoldCanceledEventArgs); }

		private void InvokeManipulationStarted(in InputEvent input) { ManipulationStarted?.Invoke(input.ManipulationStartedEventArgs); }
		private void InvokeManipulationUpdated(in InputEvent input) { ManipulationUpdated?.Invoke(input.ManipulationUpdatedEventArgs); }
		private void InvokeManipulationCompleted(in InputEvent input) { ManipulationCompleted?.Invoke(input.ManipulationCompletedEventArgs); }
		private void InvokeManipulationCanceled(in InputEvent input) { ManipulationCanceled?.Invoke(input.ManipulationCanceledEventArgs); }

		private void InvokeNavigationStarted(in InputEvent input) { NavigationStarted?.Invoke(input.NavigationStartedEventArgs); }
		private void InvokeNavigationUpdated(in InputEvent input) { NavigationUpdated?.Invoke(input.NavigationUpdatedEventArgs); }
		private void InvokeNavigationCompleted(in InputEvent input) { NavigationCompleted?.Invoke(input.NavigationCompletedEventArgs); }
		private void InvokeNavigationCanceled(in InputEvent input) { NavigationCanceled?.Invoke(input.NavigationCanceledEventArgs); }

		private void InitializeInputEvents()
		{
			_inputEvents = new InputEventHandler[(int)InputEventType.Count];

			// InteractionManager
			_inputEvents[(int)InputEventType.InteractionSourceDetected] = InvokeInteractionSourceDetected;
			_inputEvents[(int)InputEventType.InteractionSourceLost] = InvokeInteractionSourceLost;
			_inputEvents[(int)InputEventType.InteractionSourcePressed] = InvokeInteractionSourcePressed;
			_inputEvents[(int)InputEventType.InteractionSourceUpdated] = InvokeInteractionSourceUpdated;
			_inputEvents[(int)InputEventType.InteractionSourceReleased] = InvokeInteractionSourceReleased;

			// GestureRecognizer
			_inputEvents[(int)InputEventType.Selected] = InvokeSelected;
			_inputEvents[(int)InputEventType.Tapped] = InvokeTapped;

			_inputEvents[(int)InputEventType.HoldStarted] = InvokeHoldStarted;
			_inputEvents[(int)InputEventType.HoldCompleted] = InvokeHoldCompleted;
			_inputEvents[(int)InputEventType.HoldCanceled] = InvokeHoldCanceled;

			_inputEvents[(int)InputEventType.ManipulationStarted] = InvokeManipulationStarted;
			_inputEvents[(int)InputEventType.ManipulationUpdated] = InvokeManipulationUpdated;
			_inputEvents[(int)InputEventType.ManipulationCompleted] = InvokeManipulationCompleted;
			_inputEvents[(int)InputEventType.ManipulationCanceled] = InvokeManipulationCanceled;

			_inputEvents[(int)InputEventType.NavigationStarted] = InvokeNavigationStarted;
			_inputEvents[(int)InputEventType.NavigationUpdated] = InvokeNavigationUpdated;
			_inputEvents[(int)InputEventType.NavigationCompleted] = InvokeNavigationCompleted;
			_inputEvents[(int)InputEventType.NavigationCanceled] = InvokeNavigationCanceled;
		}

		#endregion InputEvent Dispatching

		/// <summary>
		/// Processes all events since the previous frame
		/// </summary>
		// TODO(viktor): rename to ProcessMessages
		public void HandleEvents()
		{
			if (connectionStateChanged)
			{
				ConnectionStateChanged?.Invoke(_connectionState);
				connectionStateChanged = false;
			}

			QrApi.ProcessMessages();

			// TODO(viktor): move to c++
			while (_inputEventQueue.TryDequeue(out var input))
			{
				try
				{
					Assert.IsTrue(input.Type >= InputEventType.Min && input.Type <= InputEventType.Max);

					InputEventHandler invokeInputEvent = _inputEvents[(int)input.Type];
					invokeInputEvent(input);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		internal event Callbacks.ViewPoseHandler ViewPoseReceived;
		public void Callbacks_ViewPoseReceived(in StereoViewPose pose)
		{
			ViewPoseReceived?.Invoke(in pose);
		}

		private readonly ConcurrentQueue<InputEvent> _inputEventQueue = new ConcurrentQueue<InputEvent>();
		public void Callbacks_InputEventReceived(in InputEvent input)
		{
			// Deep copy
			InputEvent args = input;
			try
			{
				Native.Input.Convert.ToUnityCoordinateSystem(ref args);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			_inputEventQueue.Enqueue(args);
		}

		private void RegisterInputEventHandlers()
		{
			Callbacks.ViewPoseReceived += Callbacks_ViewPoseReceived;
			Callbacks.InputEventReceived += Callbacks_InputEventReceived;
		}

		private void UnregisterInputEventHandlers()
		{
			Callbacks.ViewPoseReceived -= Callbacks_ViewPoseReceived;
			Callbacks.InputEventReceived -= Callbacks_InputEventReceived;
		}

		#endregion input

		public string ConfigFilePath { get; private set; }
		public GraphicsApiConfig GraphicsApiConfig { get; private set; }

		// Is initialized when api was created successfully
		public bool IsInitialized => _apiHandle != null && !_apiHandle.IsInvalid;

		// Handle ensures native resource disposal
		private ApiHandle _apiHandle = new ApiHandle(); // ensures api creation and destruction
		private ConnectionHandle _connectionHandle = new ConnectionHandle(); // ensures connect and disconnect calls on api


		internal ServerApi CreateApi()
		{
			var api = new ServerApi();
			// TODO: ensure that this is called from the main thread
			Error error = ServerApi.Create(ref api/*, ref _apiHandle*/);
			Debug.Log($"Using ISAR version {api.Version}");

			if (error != Error.eNone)
			{
				Debug.LogError($"Unity Api creation failed with {error}");
			}

			return api;
		}

		#region GetGraphicsDeviceInfoAsync
#if ISAR_LEGACY
		/// <summary>
		/// Ask for information about D3DDevice.
		/// Raises DeviceInfoReceived Event when finished.
		/// </summary>
		/// <param name="renderTargetNativePtr"></param>
		/// <param name="remotingServerOnDeviceInfoReceived"></param>
		/// <param name="onDeviceInfoReceived"></param>
		// TODO: make static or move to native side entirely
		// TODO: make async
		private static Action<GraphicsApiConfig> _onDeviceInfoReceived;
		public void GetGraphicsDeviceInfo(IntPtr renderTargetNativePtr, Action<GraphicsApiConfig> onDeviceInfoReceived)
		{
			ServerApi = CreateApi();

			_onDeviceInfoReceived = onDeviceInfoReceived;
			// TODO: ensure that this are called from the main thread
			IntPtr cb = ConnectionApi.GetGraphicsDeviceInfo(renderTargetNativePtr,
				(GraphicsDeviceInfo info) =>
				{
					var gfxConfig = new GraphicsApiConfig(info.Device, info.DeviceContext, info.RenderTargetDesc);
					_onDeviceInfoReceived.Invoke(gfxConfig);
					_onDeviceInfoReceived = null;
				});
			GL.IssuePluginEvent(cb, (int)PluginEventId.kGetGraphicsDeviceInfo);
		}
#endif
		#endregion GetGraphicsDeviceInfoAsync

		/// <summary>
		/// Create and initialize remoting api here.
		/// When finished VideoInitCompleted Event is raised.
		/// </summary>
		/// <param name="gfxConfig"></param>
		/// <param name="configFilePath"></param>
		/// <param name="callback"></param>
		public void Initialize(GraphicsApiConfig gfxConfig, string configFilePath, Action callback)
		{
			this.GraphicsApiConfig = gfxConfig;
			this.ConfigFilePath = configFilePath;

			InitializeApi();
			InitializeVideoTrack(callback);
		}

		private void InitializeApi()
		{
			RegisterNativeEventHandlers();
			InitializeInputEvents();

			var connectionCallbacks = new ConnectionCallbacks(
					Callbacks.OnConnectionStateChanged,
					Callbacks.OnSdpCreated,
					Callbacks.OnLocalIceCandidateCreated
				);
			var error = ConnectionApi.Init(this.ConfigFilePath, this.GraphicsApiConfig, connectionCallbacks, ref _connectionHandle);
			if (error != Error.eNone)
			{
				Debug.LogError($"Api init failed with {error}");
			}

			_connectionHandle.ConnectionApi = ConnectionApi;

			var messageCallbacks = new MessageCallbacks(
					Callbacks.OnViewPoseReceived,
					Callbacks.OnInputEventReceived,
					QrApi.OnIsSupported,
					QrApi.OnRequestAccess,
					QrApi.OnAdded,
					QrApi.OnUpdated,
					QrApi.OnRemoved,
					QrApi.OnEnumerationCompleted
				);
			ConnectionApi.RegisterMessageCallbacks(_connectionHandle, ref messageCallbacks);

			QrApi.Init(ConnectionApi, _connectionHandle);
		}

		#region InitializeVideoTrackAsync

		/// <summary>
		/// Needs to be called from script thread!
		/// </summary>
		private static Action _onVideoTrackInitialized;
		private void InitializeVideoTrack(Action callback)
		{
			_onVideoTrackInitialized = callback;

			// TODO: ensure that this is called from the main thread
			IntPtr func = ConnectionApi.InitVideoTrack(_connectionHandle, GraphicsApiConfig, ConnectionApi_VideoTrackInitialized);
			GL.IssuePluginEvent(func, (int)PluginEventId.kInitVideo);
		}

		//TODO: rename VideoTrack* to VideoSource* since that's what requires init
		//and the D3D device and is important from an API usage perspective.
		//The video track is created implicitly.
		[MonoPInvokeCallback(typeof(VideoTrackInitializedCallback))]
		private static void ConnectionApi_VideoTrackInitialized(Error err)
		{
			if (err == Error.eNone)
			{
				Debug.Log("Video source init completed successfully");
				try
				{
					_onVideoTrackInitialized.Invoke();
				}
				catch(Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			else
			{
				Debug.LogError($"Video source init failed with error {err}");
			}
			_onVideoTrackInitialized = null;
		}

		#endregion InitializeVideoTrackAsync

		/// <summary>
		/// Resets the connection to a state where it is ready to go through
		/// the signaling flow again, i.e. like after calling Initialize().
		/// It is usually used in response to losing a connection.
		/// </summary>
		public /*async*/ void Reset()
		{
			// TODO: ensure not webrtc thread
			//await System.Threading.Tasks.Task.Run(() => {
			var err = ConnectionApi.Reset(_connectionHandle);
			if (err != Error.eNone)
			{
				Debug.LogError($"{nameof(RemotingServer)}: Reset failed with error {err}");
			}
			else
			{
				Debug.Log($"{nameof(RemotingServer)}: Successfully reset connection");
			}
			//});
		}

		#region SignalingApi

		/// <summary>
		/// Creates an SDP offer. This happens asynchronously inside the native lib,
		/// so the result will be available in an SdpCreatedCallback.
		/// </summary>
		public void CreateOffer()
		{
			Debug.Log($"{nameof(RemotingServer)}: Creating offer.");

			var err = SignalingApi.CreateOffer(_connectionHandle);
			if (err != Error.eNone)
			{
				Debug.LogError($"Error: {err}, ConnectionApi failed.");
			}

		}

		/// <summary>
		/// Sets the SDP answer received from a remote peer.
		/// </summary>
		/// <param name="sdpAnswer">Session description</param>
		public void SetRemoteAnswer(string sdpAnswer)
		{
			Debug.Log($"{nameof(RemotingServer)}: Setting remote answer.");

			Error err = SignalingApi.SetRemoteAnswer(_connectionHandle, sdpAnswer);
			if (err != Error.eNone)
			{
				Debug.LogError($"Error: {err}, Setting remote answer failed.");
			}
		}

		/// <summary>
		/// Adds an ICE candidate received from a remote peer.
		/// </summary>
		/// <param name="sdpMid">SDP mid</param>
		/// <param name="mlineIndex">SDP m-line index</param>
		/// <param name="sdpizedCandidate">SDPized candidate</param>
		public void AddIceCandidate(string sdpMid, int mlineIndex, string sdpizedCandidate)
		{
			Debug.Log($"{nameof(RemotingServer)}: Adding ice candidate.");

			Error err = SignalingApi.AddIceCandidate(_connectionHandle, sdpMid, mlineIndex, sdpizedCandidate);
			if (err != Error.eNone)
			{
				Debug.LogError($"Error: {err}, Adding ice candidate failed.");
			}
		}

		#endregion SignalingApi

		/// <summary>
		/// Send video frame to client.
		/// </summary>
		/// <param name="poseTimestamp"></param>
		/// <param name="renderTarget"></param>
		public void SendFrame(long poseTimestamp, IntPtr renderTarget)
		{
			var videoFrame = new GraphicsApiFrame(poseTimestamp, renderTarget);
			IntPtr cb = ConnectionApi.PushFrame(_connectionHandle, videoFrame);
			GL.IssuePluginEvent(cb, (int)PluginEventId.kPushFrame);
		}

		private void RegisterNativeEventHandlers()
		{
			Callbacks.SdpCreated += Callbacks_SdpCreated;
			Callbacks.LocalIceCandidateCreated += Callbacks_LocalIceCandidateCreated;
			Callbacks.ConnectionStateChanged += Callbacks_ConnectionStateChanged;

			RegisterInputEventHandlers();
		}

		private void UnregisterNativeEventHandlers()
		{
			UnregisterInputEventHandlers();

			Callbacks.ConnectionStateChanged -= Callbacks_ConnectionStateChanged;
			Callbacks.LocalIceCandidateCreated -= Callbacks_LocalIceCandidateCreated;
			Callbacks.SdpCreated -= Callbacks_SdpCreated;
		}

		public void Dispose()
		{
			UnregisterNativeEventHandlers();

			// Dispose handles and free api resources
			_connectionHandle?.Dispose();
			_apiHandle?.Dispose();
		}
	}
}
