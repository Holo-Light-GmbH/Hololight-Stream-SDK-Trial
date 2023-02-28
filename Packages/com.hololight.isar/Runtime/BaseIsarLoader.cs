
//#define USE_Signaling_FALLBACK

using UnityEngine;
using UnityEngine.XR.Management;
using System.Threading;
using HoloLight.Isar.Native;
using System.Runtime.InteropServices;
using System;
using System.IO;
using AOT;
using System.Threading.Tasks;
using HoloLight.Isar.Signaling;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HoloLight.Isar
{
	/// <summary>
	/// Takes care of initializing the native library and calls the display/input providers' lifecycle methods.
	/// </summary>
	public abstract class BaseIsarLoader : XRLoaderHelper
	{
		#region Callbacks

		// using ref instead of pointers.
		// https://manski.net/2012/06/pinvoke-tutorial-passing-parameters-part-3/#marshalling-structs
		// [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		// internal delegate void ViewPoseReceivedCallback(ref StereoViewPose pose, IntPtr userData);

		// [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		// internal delegate void InputEventReceivedCallback(ref InputEvent pose, IntPtr userData);

		public delegate void ConnectionStateChangedCallback(HlrConnectionState newState);
		public event ConnectionStateChangedCallback ConnectionStateChanged;
		[MonoPInvokeCallback(typeof(HlrConnectionStateChangedCallback))]
		private static void Native_OnConnectionStateChanged(HlrConnectionState newState, IntPtr userData)
		{
			try
			{
				// TODO: dispatch on unity thread eg. using UnityMainThreadDispatcher?
				var loader = (BaseIsarLoader)GCHandle.FromIntPtr(userData).Target;
				loader.ConnectionStateChanged?.Invoke(newState);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}


		/// <summary>
		/// Fired when the local session description is created.
		/// </summary>
		public delegate void SdpCreatedCallback(HlrSdpType type, string sdp);
		public event SdpCreatedCallback SdpCreated;
		[MonoPInvokeCallback(typeof(HlrSdpCreatedCallback))]
		private static void Native_OnSdpCreated(HlrSdpType type, string sdp, IntPtr userData)
		{
			try
			{
				var loader = (BaseIsarLoader)GCHandle.FromIntPtr(userData).Target;
				loader.SdpCreated?.Invoke(type, sdp);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when a local ICE candidate is created.
		/// </summary>
		public delegate void LocalIceCandidateCreatedCallback(string sdpMline, int mlineIndex, string sdpizedCandidate);
		public event LocalIceCandidateCreatedCallback LocalIceCandidateCreated;
		[MonoPInvokeCallback(typeof(HlrLocalIceCandidateCreatedCallback))]
		private static void Native_OnLocalIceCandidateCreated(string mId, int mLineIndex, string candidate, IntPtr userData)
		{
			try
			{
				var loader = (BaseIsarLoader)GCHandle.FromIntPtr(userData).Target;
				loader.LocalIceCandidateCreated?.Invoke(mId, mLineIndex, candidate);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when we receive a new XR pose from the client.
		/// </summary>
		internal delegate void ViewPoseHandler(in Native.Input.HlrXrPose viewPose);
		internal event ViewPoseHandler ViewPoseReceived;
		[MonoPInvokeCallback(typeof(HlrSvViewPoseReceivedCallback))]
		internal static void Native_OnViewPoseReceived(in Native.Input.HlrXrPose pose, IntPtr userData)
		{
			try
			{
				var loader = (BaseIsarLoader)GCHandle.FromIntPtr(userData).Target;
				loader.ViewPoseReceived?.Invoke(in pose);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when we receive a new XR pose from the client.
		/// </summary>
		internal delegate void InputEventHandler(in Native.Input.HlrInputEvent inputEvent);
		internal event InputEventHandler InputEventReceived;
		[MonoPInvokeCallback(typeof(HlrSvInputEventReceivedCallback))]
		internal static void Native_OnInputEventReceived(in Native.Input.HlrInputEvent input, IntPtr userData)
		{
			try
			{
				var loader = (BaseIsarLoader)GCHandle.FromIntPtr(userData).Target;
				loader.InputEventReceived?.Invoke(in input);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when we receive a custom message over DataChannel.
		/// </summary>
		public delegate void CustomMessageCallback(in HlrCustomMessage message);
		public event CustomMessageCallback CustomMessageReceived;
		[MonoPInvokeCallback(typeof(HlrCustomMessageCallback))]
		internal static void Native_OnCustomMessageReceived(in HlrCustomMessage message, IntPtr userData)
		{
			try
			{
				var loader = (BaseIsarLoader)GCHandle.FromIntPtr(userData).Target;
				loader.CustomMessageReceived?.Invoke(message);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when we receive audio data from the client.
		/// </summary>
		public delegate void AudioDataReceivedCallback(in HlrAudioData audioData);
		public event AudioDataReceivedCallback AudioDataReceived;
		[MonoPInvokeCallback(typeof(HlrSvAudioDataReceivedCallback))]
		internal static void Native_OnAudioDataReceived(in HlrAudioData audioData, IntPtr userData)
		{
			try
			{
				var loader = (BaseIsarLoader)GCHandle.FromIntPtr(userData).Target;
				loader.AudioDataReceived?.Invoke(audioData);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		#endregion Callbacks

		#region MemberVariables

		GCHandle thisHandle;
		IntPtr thisPtr;

		//KL: we could use Isar class instance here for init/close instead of raw API.

		//Used to post calls on Unity's main thread. Useful when we want to call into remoting lib while in a native callback.
		//Here's a lengthy read why this is useful:
		//https://docs.microsoft.com/en-us/archive/msdn-magazine/2011/february/msdn-magazine-parallel-computing-it-s-all-about-the-synchronizationcontext
		private SynchronizationContext _syncContext;

		private ISignaling _signaling;
		private HlrSvApi _serverApi;
		private HlrHandle _hlrHandle;
		internal HlrHandle HlrHandle
		{
			get { return _hlrHandle; }
		}

		public IsarRemotingConfig RemotingConfig
		{
			get;
			private set;
		}

		private HlrConnectionState _connectionState;
		private object _connectionStateLock = new object();
		public HlrConnectionState ConnectionState {
			get
			{
				HlrConnectionState state;
				lock (_connectionStateLock)
				{
					state = _connectionState;
				}
				return state;
			}
			private set {
				lock (_connectionStateLock)
				{
					_connectionState = value;
				};
			}
		}

		private object _isRunningLock = new object();
		private bool _isRunning = false;
		public bool IsRunning {
			get {
				lock (_isRunningLock) { return _isRunning; }
			}
			private set {
				lock (_isRunningLock) { _isRunning = value; }
			}
		}

		// This exists because signaling callbacks can be executed on threads other than Unity's script thread (async etc.),
		// so we can't call GetXRSettings in OnConnected.
		bool _sendDepthAlpha = false;
		#endregion

		#region Structures
		[StructLayout(LayoutKind.Sequential)]
		struct RenderSettings
		{
			public int sendDepthAlpha;
			public RenderingMode renderingMode;
			public RenderConfigStruct config;

			public RenderSettings(int sendDepthAlpha, RenderingMode renderingMode, RenderConfigStruct config)
			{
				this.sendDepthAlpha = sendDepthAlpha;
				this.renderingMode = renderingMode;
				this.config = config;
			}

			public RenderSettings(bool sendDepthAlpha, RenderingMode renderingMode, RenderConfigStruct config)
			{
				this.sendDepthAlpha = sendDepthAlpha ? 1 : 0;
				this.renderingMode = renderingMode;
				this.config = config;
			}
		}
		
		#endregion

		#region DllImports
		[DllImport("remoting_unity", CallingConvention = CallingConvention.Cdecl)]
		static extern HlrError InitialiseIsar(RenderSettings rendersettings,
			RemotingConfigStruct remotingSettings, 
			HlrConnectionCallbacks connectionCallbacks,
			IceServerSettings[] iceServerSettings,
			UInt32 iceServerSettingsSize,
			ref HlrHandle handle);

		[DllImport("remoting_unity", CallingConvention = CallingConvention.Cdecl)]
		static extern HlrError ResetIsar(ref HlrHandle handle);

		[DllImport("remoting_unity", CallingConvention = CallingConvention.Cdecl)]
		static extern HlrError DeinitaliseIsar(ref HlrHandle handle);

		[DllImport("remoting_unity")]
		public static extern void SetBitrate(Int64 bitrate);
		#endregion

		/// <summary>
		/// Get the settings
		/// </summary>
		/// <returns>The settings class</returns>
		private IsarXRSettings GetXRSettings()
		{
#if !UNITY_EDITOR
		return IsarXRSettings.s_Settings;
#else
			IsarXRSettings settingsObj = null;
			EditorBuildSettings.TryGetConfigObject(Constants.ISAR_XR_SETTINGS_KEY, out settingsObj);
			return settingsObj;
#endif
		}

		public sealed override bool Initialize()
		{
			Debug.Log("===== Initialize =====");
			_hlrHandle = new HlrHandle();
			_syncContext = SynchronizationContext.Current;

			IsarXRSettings xrSettings = GetXRSettings();
			if (xrSettings == null)
			{
				Debug.LogError("Failed to get XR settings");
				return false;
			}

			string configPath = Application.streamingAssetsPath + "/remoting-config.cfg";
			RemotingConfig = IsarRemotingConfig.CreateFromJSON(File.ReadAllText(configPath));
			RemotingConfig.GetStructs(out RenderConfigStruct renderConfigStruct, out RemotingConfigStruct remotingConfigStruct, out IceServerSettings[] iceServerSettingsArray);
			_sendDepthAlpha = xrSettings.SendDepthAlpha_Preview;
			RenderSettings renderSettings = new RenderSettings(_sendDepthAlpha, xrSettings.RenderingMode, renderConfigStruct);

			thisHandle = GCHandle.Alloc(this);
			thisPtr = GCHandle.ToIntPtr(thisHandle);
			HlrSdpCreatedCallback sdpCallback = Native_OnSdpCreated;
			HlrLocalIceCandidateCreatedCallback iceCallback = Native_OnLocalIceCandidateCreated;
			HlrConnectionCallbacks callbacks = new HlrConnectionCallbacks(null, sdpCallback, iceCallback, thisPtr);

			_serverApi = new HlrSvApi();
			HlrError err = HlrSvApi.Create(ref _serverApi);
			if (err != HlrError.eNone)
			{
				Debug.LogError("Failed to create isar server api");
				return false;
			}

			err = InitialiseIsar(renderSettings, remotingConfigStruct, callbacks, iceServerSettingsArray, (uint)iceServerSettingsArray.Length, ref _hlrHandle);
			if (err != HlrError.eNone)
			{
				Debug.LogError("Failed to initalise isar server");
				return false;
			}

			if (!CreateSubsystems())
			{
				Debug.LogError("Failed to create isar subsystems");
				return false;
			}

			_signaling = (ISignaling)Activator.CreateInstance(Type.GetType(xrSettings.SignalingImplementationType));
			_signaling.Connected += Signaling_OnConnected;
			_signaling.SdpAnswerReceived += Signaling_SdpAnswerReceived;
			_signaling.IceCandidateReceived += Signaling_IceCandidateReceived;

			_serverApi.Connection.RegisterConnectionStateHandler(HlrHandle, Native_OnConnectionStateChanged, thisPtr);
			_serverApi.Connection.RegisterViewPoseHandler(HlrHandle, Native_OnViewPoseReceived, thisPtr);
			_serverApi.Connection.RegisterCustomMessageHandler(HlrHandle, Native_OnCustomMessageReceived, thisPtr);
			_serverApi.Connection.RegisterAudioDataHandler(HlrHandle, Native_OnAudioDataReceived, thisPtr);

			SdpCreated += OnSdpCreated;
			LocalIceCandidateCreated += OnLocalIceCandidateCreated;
			ConnectionStateChanged += OnConnectionStateChanged;

			return true;
		}

		public sealed override bool Start()
		{
			if (!StartSubsystems())
			{
				return false;
			}

			_signaling.Listen();

			IsRunning = true;

			return true;
		}

		public sealed override bool Stop()
		{
			Debug.Log("===== Stop =====");

			_signaling.Dispose();

			bool retVal = StopSubsystems();

			// TODO: move to beginning of this function?
			IsRunning = false;

			return retVal;
		}

		// TODO: move signaling & flow control out of loader and into user code (allow automatic & custom xr loading)
		private bool HACK_isDeinitializing = false;
		public sealed override bool Deinitialize()
		{
			Debug.Log("===== Deinitialize =====");

			_signaling.IceCandidateReceived -= Signaling_IceCandidateReceived;
			_signaling.SdpAnswerReceived -= Signaling_SdpAnswerReceived;
			_signaling.Connected -= Signaling_OnConnected;
			//OnDeinitialized(); ?

			if (ConnectionState == HlrConnectionState.Connected)
			{
				HACK_isDeinitializing = true;
				Native_OnConnectionStateChanged(HlrConnectionState.Disconnected, thisPtr);
				HACK_isDeinitializing = false;
			}

			ConnectionStateChanged -= OnConnectionStateChanged;
			LocalIceCandidateCreated -= OnLocalIceCandidateCreated;
			SdpCreated -= OnSdpCreated;
			// TODO: we should call OnConnectionStateChanged(HlrConnectionState.Disconnected); but because of our current architecture, we can't (as opposed to the hololens client)
			//if (IsConnected)
			//{
			//	lock (_lockObj)
			//	{
			//		_connectionState = HlrConnectionState.Disconnected;
			//	}
			//	Debug.Log($"ISAR connection state changed to {HlrConnectionState.Disconnected}");
			//}

			_serverApi.Connection.UnregisterAudioDataHandler2(HlrHandle, Native_OnAudioDataReceived, thisPtr);
			_serverApi.Connection.UnregisterCustomMessageHandler(HlrHandle, Native_OnCustomMessageReceived, thisPtr);
			_serverApi.Connection.UnregisterViewPoseHandler2(HlrHandle, Native_OnViewPoseReceived, thisPtr);
			_serverApi.Connection.UnregisterConnectionStateHandler2(HlrHandle, Native_OnConnectionStateChanged, thisPtr);

			var retVal = DestroySubsystems();

			DeinitaliseIsar(ref _hlrHandle);

			thisHandle.Free();

			HlrHandle.Dispose();

			return retVal;
		}

		protected abstract bool CreateSubsystems();
		protected abstract bool StartSubsystems();
		protected abstract bool StopSubsystems();
		protected abstract bool DestroySubsystems();

		#region ServerEventHandlers
		/// <summary>
		/// Callback indicating the connection state has changed
		/// </summary>
		/// <param name="newState">States whether connected or disconnected</param>
		private void OnConnectionStateChanged(HlrConnectionState newState)
		{
			HlrConnectionState prevState;
			// atomic.exchange
			lock (_connectionStateLock)
			{
				prevState = _connectionState;
				_connectionState = newState;
			}
			Debug.Log($"ISAR connection state changed to {newState}");

			switch (newState)
			{
				case HlrConnectionState.Connected:
					_signaling.Dispose();
					//OnConnected();
					break;
				case HlrConnectionState.Disconnected:
					if (!HACK_isDeinitializing)
					{
						//Post makes an asynchronous call the next time Unity's SynchronizationContext impl does some processing.
						//Here's Unity's reference source:
						//https://github.com/Unity-Technologies/UnityCsReference/blob/2019.4/Runtime/Export/Scripting/UnitySynchronizationContext.cs
						_syncContext.Post((state) =>
						{
							lock (_isRunningLock)
							{
								if (_isRunning)
								{
									Stop();
									ResetIsar(ref _hlrHandle);
									Start();
								}
							}
						}, newState);
					}
					//OnDisconnected();
					break;
				case HlrConnectionState.Failed:
					if (prevState == HlrConnectionState.Connected)
					{
						if (!HACK_isDeinitializing)
						{
							//Post makes an asynchronous call the next time Unity's SynchronizationContext impl does some processing.
							//Here's Unity's reference source:
							//https://github.com/Unity-Technologies/UnityCsReference/blob/2019.4/Runtime/Export/Scripting/UnitySynchronizationContext.cs
							_syncContext.Post((state) =>
							{
								lock (_isRunningLock)
								{
									if (_isRunning)
									{
										Stop();
										ResetIsar(ref _hlrHandle);
										Start();
									}
								}
							}, newState);
						}
					}
					else if (prevState == HlrConnectionState.Disconnected)
					{
						_signaling.Dispose();
					}
					break;
				default:
					Debug.LogError("Unhandled ConnectionState");
					break;
			}
		}

		/// <summary>
		/// SDP Created callback
		/// </summary>
		/// <param name="type">Offer or answer</param>
		/// <param name="sdp">Connection description</param>
		/// <param name="userData"></param>
		private async void OnSdpCreated(HlrSdpType type, string sdp)
		{
			await _signaling.SendOfferAsync(sdp);
		}

		/// <summary>
		/// ICE Candidate created callback
		/// </summary>
		/// <param name="mId">I</param>
		/// <param name="mLineIndex"></param>
		/// <param name="candidate"></param>
		/// <param name="userData"></param>
		private async void OnLocalIceCandidateCreated(string mId, int mLineIndex, string candidate)
		{
			await _signaling.SendIceCandidateAsync(mId, mLineIndex, candidate);
		}
		#endregion

		#region SignalingEventHandlers
		/// <summary>
		/// ICE Candidate received callback
		/// </summary>
		/// <param name="mId"></param>
		/// <param name="mLineIndex"></param>
		/// <param name="candidate"></param>
		private void Signaling_IceCandidateReceived(string mId, int mLineIndex, string candidate)
		{
			var err = _serverApi.Signaling.AddIceCandidate(HlrHandle, mId, mLineIndex, candidate);

			if (err != HlrError.eNone)
			{
				Debug.LogError("Failed to add ICE candidate");
			}
		}

		/// <summary>
		/// SDP answer received callback
		/// </summary>
		/// <param name="sdp"></param>
		private void Signaling_SdpAnswerReceived(string sdp)
		{
			Debug.Log($"Received answer SDP: {sdp}");

			var err = _serverApi.Signaling.SetRemoteAnswer(HlrHandle, sdp);

			if (err != HlrError.eNone)
			{
				Debug.LogError("Failed to set remote answer");
			}
		}

		/// <summary>
		/// Signaling connection callback
		/// </summary>
		/// <returns></returns>
		private void Signaling_OnConnected()
		{
			Task.Run(async () => await _signaling.SendVersionAsync(_serverApi.Version.PackedValue, _sendDepthAlpha)).Wait();

			HlrError err = _serverApi.Signaling.CreateOffer(HlrHandle);

			if (err != HlrError.eNone)
			{
				Debug.LogError("Failed to create offer");
			}
		}
		#endregion
	}
}
